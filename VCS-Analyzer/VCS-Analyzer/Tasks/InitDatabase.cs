using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VCSAnalyzer.Services;

namespace VCSAnalyzer.Tasks
{
    public class InitDatabase : IProcess
    {
        private string dbFilePath;
        public event EventHandler<NotificationArgs> NotificationIssued;

        public void OnMessageIssued(NotificationArgs e)
        {
            NotificationIssued?.Invoke(this, e);
        }

        public void StartProcess()
        {
            Thread startProcess = new Thread(CreateDatabase);
            startProcess.Start();
        }

        public InitDatabase()
        {
            dbFilePath = ConfigurationManager.AppSettings.Get("DatabaseFilePath");
        }

        private void CreateDatabase()
        {
            Database db = new Database();

            NotificationArgs notificationArgs = new NotificationArgs("Started - Create Database", DateTime.Now, NotificationType.INFORMATION);
            OnMessageIssued(notificationArgs);

            db.CreateDatabase(dbFilePath);

            notificationArgs = new NotificationArgs("Completed - Create Database", DateTime.Now, NotificationType.INFORMATION);
            OnMessageIssued(notificationArgs);



            notificationArgs = new NotificationArgs("Started - Create Tables", DateTime.Now, NotificationType.INFORMATION);
            OnMessageIssued(notificationArgs);

            List<string> sqlStatements = new List<string>();

            sqlStatements.Add("CREATE TABLE IF NOT EXISTS APP(" +
            "ID INTEGER PRIMARY KEY AUTOINCREMENT," +
            "NAME TEXT NOT NULL," +
            "FRIENDLY_NAME TEXT," +
            "SUMMARY TEXT," +
            "CATEGORY TEXT," +
            "WEBSITE TEXT," +
            "LICENSE TEXT," +
            "REPO_TYPE TEXT," +
            "ISSUE_TRACKER TEXT," +
            "SOURCE TEXT NOT NULL" +
            ");"
            );

            sqlStatements.Add("CREATE TABLE IF NOT EXISTS COMMIT_LOG(" +
            "ID INTEGER PRIMARY KEY AUTOINCREMENT," +
            "GUID TEXT NOT NULL," +
            "MESSAGE TEXT," +
            "AUTHOR_NAME TEXT," +
            "AUTHOR_EMAIL TEXT," +
            "DATE_TEXT TEXT," +
            "DATE_TICKS REAL," +
            "APPID INTEGER NOT NULL" +
            ");"
            );

            sqlStatements.Add("CREATE TABLE IF NOT EXISTS TAG(" +
            "ID INTEGER PRIMARY KEY AUTOINCREMENT," +
            "NAME TEXT NOT NULL," +
            "GUID TEXT NOT NULL," +
            "MESSAGE TEXT," +
            "AUTHOR_EMAIL TEXT," +
            "DATE_TEXT TEXT," +
            "DATE_TICKS REAL," +
            "APPID INTEGER NOT NULL" +
            ");"
            );

            sqlStatements.Add("CREATE TABLE IF NOT EXISTS COMMIT_LOG_FILE(" +
            "ID INTEGER PRIMARY KEY AUTOINCREMENT, " +
            "PATH TEXT, " +
            "OPERATION TEXT, " +
            "COMMIT_GUID TEXT, " +
            "COMMITID INTEGER NOT NULL, " +
            "APPID INTEGER NOT NULL" +
            ");"
            );

            sqlStatements.Add("CREATE TABLE IF NOT EXISTS APP_CLONE(" +
            "ID INTEGER PRIMARY KEY AUTOINCREMENT," +
            "APPID INTEGER NOT NULL," +
            "LAST_DOWNLOAD_DATE TEXT," +
            "LAST_DOWNLOAD_DATE_TICKS REAL" +
            ");"
            );

            sqlStatements.Add("CREATE TABLE IF NOT EXISTS MANIFEST(" +
            "ID INTEGER PRIMARY KEY AUTOINCREMENT," +
            "APPID INTEGER NOT NULL," +
            "COMMIT_GUID TEXT, " +
            "COMMITID INTEGER NOT NULL, " +
            "CONTENT TEXT, " +
            "AUTHOR_NAME TEXT, " +
            "AUTHOR_EMAIL, " +
            "DATE_TEXT TEXT," +
            "DATE_TICKS REAL " +
            ");"
            );

            sqlStatements.Add("CREATE TABLE IF NOT EXISTS MANIFEST_PERMISSION(" +
            "ID INTEGER PRIMARY KEY AUTOINCREMENT," +
            "APPID INTEGER NOT NULL," +
            "COMMIT_GUID TEXT, " +
            "COMMITID INTEGER NOT NULL, " +
            "PERMISSION TEXT, " +
            "AUTHOR_NAME TEXT, " +
            "AUTHOR_EMAIL, " +
            "DATE_TEXT TEXT," +
            "DATE_TICKS REAL " +
            ");"
            );

            sqlStatements.Add("CREATE TABLE IF NOT EXISTS MANIFEST_SDK(" +
            "ID INTEGER PRIMARY KEY AUTOINCREMENT," +
            "APPID INTEGER NOT NULL," +
            "COMMIT_GUID TEXT, " +
            "COMMITID INTEGER NOT NULL, " +
            "MIN_SDK INTEGER, " +
            "TARGET_SDK INTEGER, " +
            "AUTHOR_NAME TEXT, " +
            "AUTHOR_EMAIL, " +
            "DATE_TEXT TEXT," +
            "DATE_TICKS REAL " +
            ");"
            );

            foreach (var str in sqlStatements)
                db.CreateTable(str);


            notificationArgs = new NotificationArgs("Completed - Create Tables", DateTime.Now, NotificationType.SUCCESS);
            OnMessageIssued(notificationArgs);
        }
    }
}
