using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using VCSAnalyzer.Entities;
using System.IO;

namespace VCSAnalyzer.Services
{
    class Database
    {
        string connectionString;

        public Database()
        {
            connectionString = ConfigurationManager.AppSettings.Get("DatabaseConnectionString");
        }

        public void CreateDatabase(string path)
        {
            SQLiteConnection.CreateFile(path);
        }

        public void CreateTable(string SQLStatement)
        {
            using (var dbConnection = new SQLiteConnection(connectionString))
            {
                dbConnection.Open();
                using (SQLiteCommand command = new SQLiteCommand(dbConnection))
                {
                    using (var transaction = dbConnection.BeginTransaction())
                    {
                        command.CommandText = SQLStatement;
                        command.ExecuteNonQuery();
                        transaction.Commit();
                    }
                }

                dbConnection.Close();
            }
        }



        public void BatchInsertApps(List<App> apps)
        {
            string commandText;

            using (var dbConnection = new SQLiteConnection(connectionString))
            {
                dbConnection.Open();
                using (SQLiteCommand command = new SQLiteCommand(dbConnection))
                {
                    using (var transaction = dbConnection.BeginTransaction())
                    {
                        foreach (var app in apps)
                        {
                            commandText = string.Format("INSERT INTO APP " +
                                "(NAME, FRIENDLY_NAME, SUMMARY, CATEGORY, WEBSITE, LICENSE, REPO_TYPE, ISSUE_TRACKER, SOURCE) " +
                                "VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}');",
                                app.Name.Replace("'", "''"),
                                app.FriendlyName.Replace("'", "''"),
                                app.Summary.Replace("'", "''"),
                                app.Category.Replace("'", "''"),
                                app.Website,
                                app.License.Replace("'", "''"),
                                app.RepoType,
                                app.IssueTracker,
                                app.Source);
                            command.CommandText = commandText;
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                }

                dbConnection.Close();
            }
        }

        public List<App> GetApps()
        {
            List<App> apps = new List<App>();
            using (var dbConnection = new SQLiteConnection(connectionString))
            {
                using (SQLiteCommand command = new SQLiteCommand(dbConnection))
                {
                    dbConnection.Open();
                    using (var transaction = dbConnection.BeginTransaction())
                    {

                        command.CommandText = "SELECT* FROM APP WHERE REPO_TYPE = 'git'";
                        command.CommandType = System.Data.CommandType.Text;
                        SQLiteDataReader reader = command.ExecuteReader();
                        App app;
                        while (reader.Read())
                        {
                            app = new App();
                            app.Name = reader["NAME"].ToString();
                            app.FriendlyName = reader["FRIENDLY_NAME"].ToString();
                            app.Summary = reader["SUMMARY"].ToString();
                            app.Category = reader["CATEGORY"].ToString();
                            app.Website = reader["WEBSITE"].ToString();
                            app.License = reader["LICENSE"].ToString();
                            app.RepoType = reader["REPO_TYPE"].ToString();
                            app.IssueTracker = reader["ISSUE_TRACKER"].ToString();
                            app.Source = reader["SOURCE"].ToString();
                            app.Id = Convert.ToInt64(reader["ID"]);

                            apps.Add(app);
                        }
                    }
                    dbConnection.Close();
                }
            }

            return apps;
        }

        public void UpsertAppDonwload(long appId, DateTime dowloadDate)
        {
            string UPSERT_TABLE_APP_CLONE = "INSERT OR REPLACE INTO APP_CLONE (APPID, LAST_DOWNLOAD_DATE, LAST_DOWNLOAD_DATE_TICKS, ID) " +
               "VALUES (  " +
               "{0}, " +
               "'{1}', " +
               "{2}, " +
               "(SELECT ID FROM APP_CLONE WHERE APPID = {0})" +
               ");";
            string commandText = string.Format(UPSERT_TABLE_APP_CLONE, appId, dowloadDate.ToString(), dowloadDate.Ticks);

            using (var dbConnection = new SQLiteConnection(connectionString))
            {
                using (SQLiteCommand command = new SQLiteCommand(dbConnection))
                {
                    dbConnection.Open();
                    using (var transaction = dbConnection.BeginTransaction())
                    {
                        command.CommandText = commandText;
                        command.ExecuteNonQuery();
                        transaction.Commit();
                    }
                    dbConnection.Close();
                }
            }
        }
        /*
                public void BatchInsertManifest(List<Manifest> manifests)
                {
                    using (SQLiteCommand command = new SQLiteCommand(dbConnection))
                    {
                        dbConnection.Open();
                        string commandText;
                        using (var transaction = dbConnection.BeginTransaction())
                        {
                            foreach (var manifest in manifests)
                            {
                                commandText = string.Format(Constants.INSERT_TABLE_MANIFEST,
                                    manifest.AppID,
                                    manifest.CommitGUID,
                                    manifest.CommitID,
                                    manifest.Content.Replace("'", "''"),
                                    manifest.AuthorName.Replace("'", "''"),
                                    manifest.AuthorEmail,
                                    manifest.CommitDate.ToString(),
                                    manifest.CommitDate.Ticks);
                                command.CommandText = commandText;
                                command.ExecuteNonQuery();

                                commandText = string.Format(Constants.INSERT_TABLE_MANIFEST_SDK,
                                    manifest.AppID,
                                    manifest.CommitGUID,
                                    manifest.CommitID,
                                    manifest.MinSdkVersion == 0 ? "null" : manifest.MinSdkVersion.ToString(),
                                    manifest.TargetSdkVersion == 0 ? "null" : manifest.TargetSdkVersion.ToString(),
                                    manifest.AuthorName.Replace("'", "''"),
                                    manifest.AuthorEmail,
                                    manifest.CommitDate.ToString(),
                                    manifest.CommitDate.Ticks);
                                command.CommandText = commandText;
                                command.ExecuteNonQuery();

                                foreach (var permission in manifest.Permission)
                                {
                                    commandText = string.Format(Constants.INSERT_TABLE_MANIFEST_PERMISSION,
                                        manifest.AppID,
                                        manifest.CommitGUID,
                                        manifest.CommitID,
                                        permission,
                                        manifest.AuthorName.Replace("'", "''"),
                                        manifest.AuthorEmail,
                                        manifest.CommitDate.ToString(),
                                        manifest.CommitDate.Ticks);
                                    command.CommandText = commandText;
                                    command.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                        }

                        dbConnection.Close();
                    }
                }

                public void BatchInsertTag(List<Tag> Tags, long AppID)
                {
                    using (SQLiteCommand command = new SQLiteCommand(dbConnection))
                    {
                        dbConnection.Open();
                        string commandText;
                        try
                        {
                            using (var transaction = dbConnection.BeginTransaction())
                            {
                                foreach (var tag in Tags)
                                {
                                    commandText = string.Format(Constants.INSERT_TABLE_TAG,
                                        tag.Name,
                                        tag.Id,
                                        tag.Message.Replace("'", "''"),
                                        tag.AuthorEmail,
                                        tag.Date.ToString(),
                                        tag.Date.Ticks,
                                        AppID);
                                    command.CommandText = commandText;
                                    command.ExecuteNonQuery();
                                }

                                transaction.Commit();
                            }
                        }
                        catch (Exception error)
                        {
                            dbConnection.Close();
                            throw error;
                        }

                        dbConnection.Close();
                    }
                }

                public void BatchInsertCommits(List<Commit> Commits, long AppID)
                {
                    using (SQLiteCommand command = new SQLiteCommand(dbConnection))
                    {
                        dbConnection.Open();
                        string commandText;
                        try
                        {
                            using (var transaction = dbConnection.BeginTransaction())
                            {
                                foreach (var commit in Commits)
                                {
                                    commandText = string.Format(Constants.INSERT_TABLE_COMMIT_LOG,
                                        commit.GUID,
                                        commit.Message.Replace("'", "''"),
                                        commit.AuthorName.Replace("'", "''"),
                                        commit.AuthorEmail,
                                        commit.Date.ToString(),
                                        commit.Date.Ticks,
                                        AppID);
                                    command.CommandText = commandText;
                                    //command.ExecuteNonQuery();
                                    object obj = command.ExecuteScalar();

                                    foreach (var file in commit.CommitFiles)
                                    {
                                        commandText = string.Format(Constants.INSERT_TABLE_COMMIT_LOG_FILE,
                                            file.Path.Replace("'", "''"),
                                            file.Operation,
                                            commit.GUID,
                                            (long)obj,
                                            AppID);
                                        command.CommandText = commandText;
                                        command.ExecuteNonQuery();
                                    }
                                }

                                transaction.Commit();
                            }
                        }
                        catch (Exception error)
                        {
                            dbConnection.Close();
                            throw error;
                        }

                        dbConnection.Close();
                    }
                }

                public App GetAppByName(string Name)
                {
                    App app = new App();
                    using (SQLiteCommand command = new SQLiteCommand(dbConnection))
                    {
                        dbConnection.Open();
                        using (var transaction = dbConnection.BeginTransaction())
                        {

                            command.CommandText = string.Format(Constants.SELECT_APP, Name);
                            command.CommandType = System.Data.CommandType.Text;
                            SQLiteDataReader reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                app = new App();
                                app.Name = reader[Constants.COLUMN_APP_NAME].ToString();
                                app.FriendlyName = reader[Constants.COLUMN_APP_FRIENDLY_NAME].ToString();
                                app.Summary = reader[Constants.COLUMN_APP_SUMMARY].ToString();
                                app.Category = reader[Constants.COLUMN_APP_CATEGORY].ToString();
                                app.Website = reader[Constants.COLUMN_APP_WEBSITE].ToString();
                                app.License = reader[Constants.COLUMN_APP_LICENSE].ToString();
                                app.RepoType = reader[Constants.COLUMN_APP_REPO_TYPE].ToString();
                                app.IssueTracker = reader[Constants.COLUMN_APP_ISSUE_TRACKER].ToString();
                                app.Source = reader[Constants.COLUMN_APP_SOURCE].ToString();
                                app.Id = Convert.ToInt64(reader[Constants.COLUMN_APP_ID]);
                            }
                        }

                        dbConnection.Close();
                    }

                    return app;
                }

                public long GetCommitId(long AppID, string CommitGUID)
                {
                    long id = new long();
                    using (SQLiteCommand command = new SQLiteCommand(dbConnection))
                    {
                        dbConnection.Open();
                        using (var transaction = dbConnection.BeginTransaction())
                        {

                            command.CommandText = string.Format(Constants.SELECT_COMMIT, AppID, CommitGUID);
                            command.CommandType = System.Data.CommandType.Text;
                            SQLiteDataReader reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                id = Convert.ToInt64(reader[Constants.COLUMN_COMMIT_LOG_ID].ToString());
                            }
                        }

                        dbConnection.Close();
                    }

                    return id;
                }

                public List<Commit> GetAllCommits()
                {
                    List<Commit> commitList = new List<Commit>();
                    Commit commit;
                    using (SQLiteCommand command = new SQLiteCommand(dbConnection))
                    {
                        dbConnection.Open();
                        using (var transaction = dbConnection.BeginTransaction())
                        {
                            command.CommandText = string.Format(Constants.SELECT_ALL_COMMIT);
                            command.CommandType = System.Data.CommandType.Text;
                            SQLiteDataReader reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                commit = new Commit();
                                commit.AuthorEmail = reader[Constants.COLUMN_COMMIT_LOG_AUTHOR_EMAIL].ToString();
                                commit.AuthorName = reader[Constants.COLUMN_COMMIT_LOG_AUTHOR_NAME].ToString();
                                commit.Date = new DateTime(Convert.ToInt64(reader[Constants.COLUMN_COMMIT_LOG_DATE_TICKS]));
                                commit.GUID = reader[Constants.COLUMN_COMMIT_LOG_GUID].ToString();
                                commit.Message = reader[Constants.COLUMN_COMMIT_LOG_MESSAGE].ToString();
                                commit.AppID = Convert.ToInt64(reader[Constants.COLUMN_COMMIT_LOG_APPID]);

                                commitList.Add(commit);
                            }
                        }

                        dbConnection.Close();
                    }

                    return commitList;
                }

                public void UpsertAppDonwload(long appId, DateTime dowloadDate)
                {
                    string commandText = string.Format(Constants.UPSERT_TABLE_APP_CLONE, appId, dowloadDate.ToString(), dowloadDate.Ticks);

                    using (SQLiteCommand command = new SQLiteCommand(dbConnection))
                    {
                        dbConnection.Open();

                        using (var transaction = dbConnection.BeginTransaction())
                        {
                            command.CommandText = commandText;
                            command.ExecuteNonQuery();
                            transaction.Commit();
                        }

                        dbConnection.Close();
                    }

                }

                public List<App> GetApps()
                {
                    List<App> apps = new List<App>();

                    using (SQLiteCommand command = new SQLiteCommand(dbConnection))
                    {
                        dbConnection.Open();
                        using (var transaction = dbConnection.BeginTransaction())
                        {

                            command.CommandText = Constants.SELECT_ALL_APP;
                            command.CommandType = System.Data.CommandType.Text;
                            SQLiteDataReader reader = command.ExecuteReader();
                            App app;
                            while (reader.Read())
                            {
                                app = new App();
                                app.Name = reader[Constants.COLUMN_APP_NAME].ToString();
                                app.FriendlyName = reader[Constants.COLUMN_APP_FRIENDLY_NAME].ToString();
                                app.Summary = reader[Constants.COLUMN_APP_SUMMARY].ToString();
                                app.Category = reader[Constants.COLUMN_APP_CATEGORY].ToString();
                                app.Website = reader[Constants.COLUMN_APP_WEBSITE].ToString();
                                app.License = reader[Constants.COLUMN_APP_LICENSE].ToString();
                                app.RepoType = reader[Constants.COLUMN_APP_REPO_TYPE].ToString();
                                app.IssueTracker = reader[Constants.COLUMN_APP_ISSUE_TRACKER].ToString();
                                app.Source = reader[Constants.COLUMN_APP_SOURCE].ToString();
                                app.Id = Convert.ToInt64(reader[Constants.COLUMN_APP_ID]);

                                apps.Add(app);
                            }
                        }

                        dbConnection.Close();
                    }

                    return apps;
                }

                public List<long> GetDistinctManifestApps()
                {
                    List<long> idList = new List<long>();

                    using (SQLiteCommand command = new SQLiteCommand(dbConnection))
                    {
                        dbConnection.Open();
                        using (var transaction = dbConnection.BeginTransaction())
                        {
                            command.CommandText = Constants.SELECT_DISTINCT_MANIFEST_APPS;
                            command.CommandType = System.Data.CommandType.Text;
                            SQLiteDataReader reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                idList.Add(Convert.ToInt64(reader[0]));
                            }
                        }

                        dbConnection.Close();
                    }


                    return idList;
                }


                public List<long> GetCommitLogApps()
                {
                    List<long> idList = new List<long>();

                    using (SQLiteCommand command = new SQLiteCommand(dbConnection))
                    {
                        dbConnection.Open();
                        using (var transaction = dbConnection.BeginTransaction())
                        {
                            command.CommandText = Constants.SELECT_COMMITLOG_APPS;
                            command.CommandType = System.Data.CommandType.Text;
                            SQLiteDataReader reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                idList.Add(Convert.ToInt64(reader[0]));
                            }
                        }

                        dbConnection.Close();
                    }


                    return idList;
                }

                public List<Permission> GetPermissions()
                {
                    List<Permission> permissionList = new List<Permission>();

                    using (SQLiteCommand command = new SQLiteCommand(dbConnection))
                    {
                        dbConnection.Open();
                        using (var transaction = dbConnection.BeginTransaction())
                        {

                            command.CommandText = Constants.SELECT_ALL_PERMISSION_HISTORY;
                            command.CommandType = System.Data.CommandType.Text;
                            SQLiteDataReader reader = command.ExecuteReader();
                            Permission permission;
                            while (reader.Read())
                            {
                                permission = new Permission();
                                permission.AppID = Convert.ToInt64(reader[Constants.COLUMN_MANIFEST_PERMISSION_APPID]);
                                permission.CommitGUID = reader[Constants.COLUMN_MANIFEST_PERMISSION_COMMIT_GUID].ToString();
                                permission.CommitID = Convert.ToInt64(reader[Constants.COLUMN_MANIFEST_PERMISSION_COMMITID]);
                                permission.PermissionName = reader[Constants.COLUMN_MANIFEST_PERMISSION_PERMISSION].ToString();
                                permission.AuthorName = reader[Constants.COLUMN_MANIFEST_PERMISSION_AUTHOR_NAME].ToString();
                                permission.AuthorEmail = reader[Constants.COLUMN_MANIFEST_PERMISSION_AUTHOR_EMAIL].ToString();
                                permission.Date = new DateTime(Convert.ToInt64(reader[Constants.COLUMN_MANIFEST_PERMISSION_DATE_TICKS]));

                                permissionList.Add(permission);
                            }
                        }

                        dbConnection.Close();
                    }

                    return permissionList;
                }
                */
    }

}
