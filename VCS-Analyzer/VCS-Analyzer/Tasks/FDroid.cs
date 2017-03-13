using LibGit2Sharp;
using System;
using System.Collections.Generic;
using VCSAnalyzer.Services;
using VCSAnalyzer.Entities;
using LibGit2Sharp.Handlers;
using System.Configuration;
using System.Threading;

namespace VCSAnalyzer.Tasks
{
    public class FDroid : IProcess
    {
        private string downloadDirectory, repositoryPath;
        private bool onlyUpdate;

        public event EventHandler<NotificationArgs> NotificationIssued;

        public FDroid(bool update)
        {
            downloadDirectory = ConfigurationManager.AppSettings.Get("FDroidDownloadPath");
            repositoryPath = ConfigurationManager.AppSettings.Get("FDroidGitRepository");
            onlyUpdate = update;
        }

        public void StartProcess()
        {
            if (onlyUpdate)
            {
                Thread startProcess = new Thread(CloneRepository);
                startProcess.Start();
            }
            else
            {
                Thread startProcess = new Thread(UpdateRepository);
                startProcess.Start();
            }
        }

        public void UpdateRepository()
        {
            NotificationArgs notificationArgs = new NotificationArgs("Started - Update F-Droid Repositroy", DateTime.Now, NotificationType.INFORMATION);
            OnMessageIssued(notificationArgs);

            using (var repo = new Repository(downloadDirectory))
            {
                PullOptions options = new PullOptions();
                options.FetchOptions = new FetchOptions();
                options.FetchOptions.CredentialsProvider = new CredentialsHandler(
                    (url, usernameFromUrl, types) =>
                    new UsernamePasswordCredentials()
                    {
                        Username = "xxx",
                        Password = "xxx"
                    });

                Commands.Pull(repo, new Signature("xxx", "xxx", new DateTimeOffset(DateTime.Now)), options);
            }

            notificationArgs = new NotificationArgs("Completed - Update F-Droid Repositroy", DateTime.Now, NotificationType.INFORMATION);
            OnMessageIssued(notificationArgs);
        }

        public void CloneRepository()
        {
            NotificationArgs notificationArgs = new NotificationArgs("Started - Clone F-Droid Repositroy", DateTime.Now, NotificationType.INFORMATION);
            OnMessageIssued(notificationArgs);

            CloneOptions options = new CloneOptions();
            options.BranchName = "master";
            options.Checkout = true;
            string repo = Repository.Clone(repositoryPath, downloadDirectory, options);

            notificationArgs = new NotificationArgs("Completed - Clone F-Droid Repositroy", DateTime.Now, NotificationType.SUCCESS);
            OnMessageIssued(notificationArgs);



            notificationArgs = new NotificationArgs("Started - Analyzing Metadata Files", DateTime.Now, NotificationType.INFORMATION);
            OnMessageIssued(notificationArgs);

            SourceFileParser sp = new SourceFileParser(downloadDirectory + @"\metadata");
            List<App> apps = sp.ParseFiles();

            notificationArgs = new NotificationArgs("Completed - Analyzing Metadata Files", DateTime.Now, NotificationType.SUCCESS);
            OnMessageIssued(notificationArgs);



            notificationArgs = new NotificationArgs("Started - Saving To Database", DateTime.Now, NotificationType.INFORMATION);
            OnMessageIssued(notificationArgs);

            Database db = new Database();
            db.BatchInsertApps(apps);

            notificationArgs = new NotificationArgs("Completed - Saving To Database", DateTime.Now, NotificationType.SUCCESS);
            OnMessageIssued(notificationArgs);
        }

        public void OnMessageIssued(NotificationArgs e)
        {
            NotificationIssued?.Invoke(this, e);
        }



    }
}
