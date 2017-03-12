using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VCSAnalyzer;
using VCSAnalyzer.Entities;
using VCSAnalyzer.Services;

namespace VCS_Analyzer.Tasks
{
    class AppRepos : IProcess
    {
        public event EventHandler<NotificationArgs> NotificationIssued;
        private string downloadDirectory;

        public void OnMessageIssued(NotificationArgs e)
        {
            NotificationIssued?.Invoke(this, e);
        }

        public void StartProcess()
        {
            Thread startProcess = new Thread(StartRepoDownload);
            startProcess.Start();
        }

        public AppRepos()
        {
            downloadDirectory = ConfigurationManager.AppSettings.Get("AppDownloadPath");
        }

        private void StartRepoDownload()
        {
            string repoLocation;
            NotificationArgs notificationArgs;
            Database db = new Database();

            List<App> apps = db.GetApps();
            foreach (var app in apps)
            {
                repoLocation = string.Format(@"{0}\{1}", downloadDirectory, app.Name);
                if (Directory.Exists(repoLocation))
                    continue;
                try
                {
                    notificationArgs = new NotificationArgs("Started - Clone "+app.Name, DateTime.Now, NotificationType.INFORMATION);
                    OnMessageIssued(notificationArgs);

                    CloneOptions options = new CloneOptions();
                    options.BranchName = "master";
                    options.Checkout = true;
                    Repository.Clone(app.Source, repoLocation, options);

                    db.UpsertAppDonwload(app.Id, DateTime.Now);

                    notificationArgs = new NotificationArgs("Completed - Clone " + app.Name, DateTime.Now, NotificationType.SUCCESS);
                    OnMessageIssued(notificationArgs);
                }
                catch (Exception error)
                {
                    LogFailure(string.Format("Failed - Clone {0} ; {1}", app.Name, error.Message));
                    notificationArgs = new NotificationArgs("Failed - Clone " + app.Name, DateTime.Now, NotificationType.FAILURE);
                    OnMessageIssued(notificationArgs);
                    continue;
                }
            }
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
