using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VCSAnalyzer.Entities;
using VCSAnalyzer.Services;

namespace VCSAnalyzer.Tasks
{
    public class AppTags : IProcess
    {
        public event EventHandler<NotificationArgs> NotificationIssued;
        private string downloadDirectory, tagDownloadDirectory;
        private bool extractArchive;

        public void OnMessageIssued(NotificationArgs e)
        {
            NotificationIssued?.Invoke(this, e);
        }

        public void StartProcess()
        {
            Thread startProcess = new Thread(DownloadTags);
            startProcess.Start();
        }

        public AppTags(bool ExtractArchive)
        {
            extractArchive = ExtractArchive;
            downloadDirectory = ConfigurationManager.AppSettings.Get("AppDownloadPath");
            tagDownloadDirectory = ConfigurationManager.AppSettings.Get("AppTagDownloadPath");
        }

        private void DownloadTags()
        {
            Database db = new Database();
            Repository repo;
            //List<Entities.Tag> tagList;
            //Entities.Tag tag;
            NotificationArgs notificationArgs;

            var directories = Directory.GetDirectories(downloadDirectory);

            //int counter = 0;
            //int groupSize = Convert.ToInt32(Math.Ceiling(directories.Count() / 3.0));
            //var result = directories.GroupBy(s => counter++ / groupSize).Select(g => g.ToArray()).ToList();
            //int threadCount = result.Count() + 1;


            string repoURL, archiveURL, tagDownloadLocation, tagArchive;
            foreach (var directory in directories)
            {
                string lastFolderName = Path.GetFileName(directory);
                long appId = db.GetAppByName(lastFolderName).Id;
                try
                {
                    repo = new Repository(directory);
                    List<Entities.Tag> tagList = new List<Entities.Tag>();
                    repoURL = repo.Config.GetValueOrDefault<string>("remote.origin.url");

                    int tagCount = repo.Tags.Count();

                    notificationArgs = new NotificationArgs(string.Format("Started - Tags for {0} - Total tags: {1}", lastFolderName, tagCount), DateTime.Now, NotificationType.INFORMATION);
                    OnMessageIssued(notificationArgs);

                    var xxx = repo.Tags.OrderBy(b => ((LibGit2Sharp.Commit)b.PeeledTarget).Author.When.LocalDateTime);

                    LibGit2Sharp.Commit tg;
                    foreach (var t in xxx)
                    {
                        tg = (LibGit2Sharp.Commit)t.PeeledTarget;
                        Entities.Tag tag = new Entities.Tag(t.FriendlyName, tg.MessageShort, tg.Author.Email, tg.Id.ToString(), tg.Author.When.LocalDateTime);
                        tagList.Add(tag);

                        using (var client = new WebClient())
                        {
                            if (repoURL.Substring(repoURL.Length - 4).Equals(".git", StringComparison.CurrentCultureIgnoreCase))
                                archiveURL = string.Format("{0}/archive/{1}.zip", repoURL.Substring(0, repoURL.Length - 4), t.FriendlyName);
                            else
                                archiveURL = string.Format("{0}/archive/{1}.zip", repoURL, t.FriendlyName);
                            tagDownloadLocation = string.Format(@"{0}\{1}\{2}", tagDownloadDirectory, lastFolderName, t.FriendlyName);
                            tagArchive = string.Format(@"{0}\{1}\{2}.zip", tagDownloadDirectory, lastFolderName, t.FriendlyName);
                            Directory.CreateDirectory(tagDownloadLocation);

                            client.Credentials = CredentialCache.DefaultCredentials;
                            client.DownloadFile(archiveURL, tagArchive);

                            if (true)
                            {
                                lock (locker)
                                {
                                    ZipFile.ExtractToDirectory(tagArchive, tagDownloadLocation);
                                    File.Delete(tagArchive);
                                }
                            }

                        }
                    }

                    db.BatchInsertTag(tagList, appId);

                    notificationArgs = new NotificationArgs(string.Format("Completed - Tags for {0}", lastFolderName), DateTime.Now, NotificationType.SUCCESS);
                    OnMessageIssued(notificationArgs);
                }
                catch (Exception error)
                {
                    LogFailure(string.Format("Failed - Tags for {0} ; {1}", lastFolderName, error.Message));
                    notificationArgs = new NotificationArgs("Failed -  Tags for " + lastFolderName, DateTime.Now, NotificationType.FAILURE);
                    OnMessageIssued(notificationArgs);
                    continue;
                }
            }



            //Parallel.For(0, threadCount, i =>
            //{
            //    var dataSet = result.ElementAtOrDefault(i);
            //    if (dataSet != null)
            //    {
            //        Console.WriteLine("Processing dataset: " + i + "; Count: " + dataSet.Count());
            //        string repoURL, archiveURL, tagDownloadLocation, tagArchive;
            //        foreach (var directory in dataSet)
            //        {
            //            string lastFolderName = Path.GetFileName(directory);
            //            long appId = db.GetAppByName(lastFolderName).Id;
            //            try
            //            {
            //                repo = new Repository(directory);
            //                List<Entities.Tag> tagList = new List<Entities.Tag>();
            //                repoURL = repo.Config.GetValueOrDefault<string>("remote.origin.url");

            //                int tagCount = repo.Tags.Count();

            //                notificationArgs = new NotificationArgs(string.Format("Started - Tags for {0} - Total tags: {1}", lastFolderName, tagCount), DateTime.Now, NotificationType.INFORMATION);
            //                OnMessageIssued(notificationArgs);

            //                var xxx = repo.Tags.OrderBy(b => ((LibGit2Sharp.Commit)b.PeeledTarget).Author.When.LocalDateTime);

            //                LibGit2Sharp.Commit tg;
            //                foreach (var t in xxx)
            //                {
            //                    tg = (LibGit2Sharp.Commit)t.PeeledTarget;
            //                    Entities.Tag tag = new Entities.Tag(t.FriendlyName, tg.MessageShort, tg.Author.Email, tg.Id.ToString(), tg.Author.When.LocalDateTime);
            //                    tagList.Add(tag);

            //                    using (var client = new WebClient())
            //                    {
            //                        if (repoURL.Substring(repoURL.Length - 4).Equals(".git", StringComparison.CurrentCultureIgnoreCase))
            //                            archiveURL = string.Format("{0}/archive/{1}.zip", repoURL.Substring(0, repoURL.Length - 4), t.FriendlyName);
            //                        else
            //                            archiveURL = string.Format("{0}/archive/{1}.zip", repoURL, t.FriendlyName);
            //                        tagDownloadLocation = string.Format(@"{0}\{1}\{2}", tagDownloadDirectory, lastFolderName, t.FriendlyName);
            //                        tagArchive = string.Format(@"{0}\{1}\{2}.zip", tagDownloadDirectory, lastFolderName, t.FriendlyName);
            //                        Directory.CreateDirectory(tagDownloadLocation);

            //                        client.Credentials = CredentialCache.DefaultCredentials;
            //                        client.DownloadFile(archiveURL, tagArchive);

            //                        if (extractArchive)
            //                        {
            //                            ZipFile.ExtractToDirectory(tagArchive, tagDownloadLocation);
            //                            File.Delete(tagArchive);
            //                        }

            //                    }
            //                }

            //                db.BatchInsertTag(tagList, appId);

            //                notificationArgs = new NotificationArgs(string.Format("Completed - Tags for {0}", lastFolderName), DateTime.Now, NotificationType.SUCCESS);
            //                OnMessageIssued(notificationArgs);
            //            }
            //            catch (Exception error)
            //            {
            //                LogFailure(string.Format("Failed - Tags for {0} ; {1}", lastFolderName, error.Message));
            //                notificationArgs = new NotificationArgs("Failed -  Tags for " + lastFolderName, DateTime.Now, NotificationType.FAILURE);
            //                OnMessageIssued(notificationArgs);
            //                continue;
            //            }
            //        }
            //    }
            //});

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
