using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VCS_Analyzer.Services;
using VCS_Analyzer.Entities;
using LibGit2Sharp.Handlers;
using VCS_Analyzer;
using System.Configuration;
using System.Threading;

namespace VCSAnalyzer.FDroid
{
    public class FDroid : IProcess
    {
        private string downloadDirectory,repositoryPath;

        public event EventHandler<NotificationArgs> NotificationIssued;

        public FDroid(bool update)
        {
            downloadDirectory = ConfigurationManager.AppSettings.Get("FDroidDownloadPath");
            repositoryPath = ConfigurationManager.AppSettings.Get("FDroidGitRepository");
        }

        public void StartProcess()
        {
            Thread startProcess = new Thread(CloneRepository);
            startProcess.Start();

        }

        public void UpdateRepository()
        {
            throw new NotImplementedException();

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

        protected virtual void OnMessageIssued(NotificationArgs e)
        {
            NotificationIssued?.Invoke(this, e);
        }


        
    }
}
