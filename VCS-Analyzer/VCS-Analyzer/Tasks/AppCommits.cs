using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VCSAnalyzer.Entities;
using VCSAnalyzer.Services;

namespace VCSAnalyzer.Tasks
{
    public class AppCommits : IProcess
    {
        public event EventHandler<NotificationArgs> NotificationIssued;
        private string downloadDirectory;
        bool includeFiles;

        public void OnMessageIssued(NotificationArgs e)
        {
            NotificationIssued?.Invoke(this, e);
        }

        public void StartProcess()
        {
            Thread startProcess = new Thread(CommitHistory);
            startProcess.Start();
        }

        public AppCommits(bool IncludeCommitFiles)
        {
            downloadDirectory = ConfigurationManager.AppSettings.Get("AppDownloadPath");
            includeFiles = IncludeCommitFiles;
        }


        private void CommitHistory()
        {
            Database db = new Database();
            Repository repo;
            //List<Entities.Commit> commiList 
            //Entities.Commit commit;
            //CommitFile commitFile;
            NotificationArgs notificationArgs;

            var directories = Directory.GetDirectories(downloadDirectory);

            int counter = 0;
            int groupSize = Convert.ToInt32(Math.Ceiling(directories.Count() / 3.0));

            var result = directories.GroupBy(s => counter++ / groupSize).Select(g => g.ToArray()).ToList();
            int threadCount = result.Count() + 1;

            Parallel.For(0, threadCount, i =>
            {
                var dataSet = result.ElementAtOrDefault(i);
                if (dataSet != null)
                {
                    Console.WriteLine("Processing dataset: " + i + "; Count: " + dataSet.Count());
                    foreach (var directory in dataSet)
                    {
                        string lastFolderName = Path.GetFileName(directory);
                        long appId = db.GetAppByName(lastFolderName).Id;
                        try
                        {
                            repo = new Repository(directory);
                            List<Entities.Commit> commiList = new List<Entities.Commit>();

                            int commitCount = repo.Commits.Count();

                            notificationArgs = new NotificationArgs(string.Format("Started - Commit Histroy for {0} - Total Commits: {1}", lastFolderName, commitCount), DateTime.Now, NotificationType.INFORMATION);
                            OnMessageIssued(notificationArgs);

                            int j = repo.Commits.Count() - 1;
                            foreach (var cx in repo.Commits)
                            // for (int i = commitCount - 1; i >= 0; i--)
                            {
                                Entities.Commit commit = new Entities.Commit();
                                commit.AuthorEmail = cx.Author.Email;
                                commit.AuthorEmail = cx.Author.Email;
                                commit.AuthorName = cx.Author.Name;
                                commit.Date = cx.Author.When.LocalDateTime;
                                commit.Message = cx.Message;
                                commit.GUID = cx.Sha;

                                if (includeFiles)
                                {
                                    if (j == commitCount - 1)
                                    {
                                        Tree firstCommit = repo.Lookup<Tree>(repo.Commits.ElementAt(j).Tree.Sha);
                                        Tree lastCommit = repo.Lookup<Tree>(repo.Commits.ElementAt(0).Tree.Sha);

                                        var changes = repo.Diff.Compare<TreeChanges>(lastCommit, firstCommit);
                                        foreach (var item in changes)
                                        {
                                            if (item.Status != ChangeKind.Deleted)
                                            {
                                                CommitFile commitFile = new CommitFile(item.Path, ChangeKind.Added.ToString());
                                                commit.CommitFiles.Add(commitFile);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var changes = repo.Diff.Compare<TreeChanges>(repo.Commits.ElementAt(j + 1).Tree, repo.Commits.ElementAt(j).Tree);
                                        foreach (var item in changes)
                                        {
                                            CommitFile commitFile = new CommitFile(item.Path, item.Status.ToString());
                                            commit.CommitFiles.Add(commitFile);
                                        }
                                    }
                                }

                                commiList.Add(commit);

                                j--;
                            }

                            db.BatchInsertCommits(commiList, appId);

                            notificationArgs = new NotificationArgs(string.Format("Completed - Commit Histroy for {0}", lastFolderName), DateTime.Now, NotificationType.SUCCESS);
                            OnMessageIssued(notificationArgs);
                        }
                        catch (Exception error)
                        {
                            LogFailure(string.Format("Failed - Commit Histroy for {0} ; {1}", lastFolderName, error.Message));
                            notificationArgs = new NotificationArgs("Failed -  Commit Histroy for " + lastFolderName, DateTime.Now, NotificationType.FAILURE);
                            OnMessageIssued(notificationArgs);
                            continue;
                        }
                    }
                }
            });
        }

        private object locker = new object();
        private void LogFailure(string message)
        {
            lock (locker)
            {
                using (StreamWriter w = File.AppendText("error.txt"))
                {
                    w.WriteLine(message);
                }
            }
        }
    }
}
