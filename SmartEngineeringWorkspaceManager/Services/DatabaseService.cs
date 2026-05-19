using System;
using System.Data.SQLite;
using System.IO;

namespace SmartEngineeringWorkspaceManager.Services
{
    public class DatabaseService
    {
        // Define the name of the database file
        private const string DatabaseFileName = "WorkspaceData.sqlite";

        // Build the connection string telling SQLite where to find the file
        // To keep things simple for now, we save it right next to the .exe file
        private readonly string _connectionString = $"Data Source={DatabaseFileName};Version=3;";

        /// <summary>
        /// Gets a new active connection to the SQLite database.
        /// Ensure you use 'using' blocks when calling this to close the connection automatically.
        /// </summary>
        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(_connectionString);
        }

        /// <summary>
        /// Creates the database file and necessary structure if they do not exist.
        /// </summary>
        public void InitializeDatabase()
        {
            // Create the file if it doesn't exist yet
            if (!File.Exists(DatabaseFileName))
            {
                SQLiteConnection.CreateFile(DatabaseFileName);
            }

            // Open a connection to execute our table creation commands
            using (var connection = GetConnection())
            {
                connection.Open();

                // 1. Users Table
                // Primary Key ensures every user gets a unique, auto-incrementing ID.
                string createUsersTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        UserId INTEGER PRIMARY KEY AUTOINCREMENT,
                        Username TEXT NOT NULL UNIQUE,
                        FullName TEXT,
                        Email TEXT,
                        Password TEXT NOT NULL,
                        Role TEXT NOT NULL DEFAULT 'Engineer',
                        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                    );";

                // 2. Projects Table
                // Note: We added the Deadline column based on our new Project model.
                // If you already ran the app previously, SQLite won't update the schema of an existing table automatically 
                // with IF NOT EXISTS. So, for the sake of simplicity right now we'll dynamically alter it if missing, 
                // or just drop and recreate it if we are in dev. We will use a safe approach by altering the table if needed.
                string createProjectsTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Projects (
                        ProjectId INTEGER PRIMARY KEY AUTOINCREMENT,
                        ProjectName TEXT NOT NULL,
                        Description TEXT,
                        Deadline DATETIME,
                        Status TEXT DEFAULT 'Active',
                        CreatedBy INTEGER,
                        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY(CreatedBy) REFERENCES Users(UserId)
                    );";

                // 3. Documents Table
                // Documents belong to a Project. This is a 1-to-Many relationship (1 Project has Many Documents).
                string createDocumentsTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Documents (
                        DocumentId INTEGER PRIMARY KEY AUTOINCREMENT,
                        FileName TEXT NOT NULL,
                        FilePath TEXT NOT NULL,
                        FileType TEXT,
                        UploadedBy INTEGER,
                        ProjectId INTEGER NOT NULL,
                        UploadedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY(ProjectId) REFERENCES Projects(ProjectId),
                        FOREIGN KEY(UploadedBy) REFERENCES Users(UserId)
                    );";

                // 4. Tasks Table
                // Note: We've upgraded this table to include Deadline and Status replacing the simple IsCompleted bool
                string createTasksTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Tasks (
                        TaskId INTEGER PRIMARY KEY AUTOINCREMENT,
                        Title TEXT NOT NULL,
                        Description TEXT,
                        Status TEXT DEFAULT 'Pending',
                        Deadline DATETIME,
                        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                        ProjectId INTEGER NOT NULL,
                        AssignedToUserId INTEGER,
                        FOREIGN KEY(ProjectId) REFERENCES Projects(ProjectId),
                        FOREIGN KEY(AssignedToUserId) REFERENCES Users(UserId)
                    );";

                // 5. Revisions Table
                // This tracks versions of Documents. A classic One-to-Many relationship (1 Document -> Many Revisions).
                string createRevisionsTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Revisions (
                        RevisionId INTEGER PRIMARY KEY AUTOINCREMENT,
                        DocumentId INTEGER NOT NULL,
                        VersionNumber TEXT NOT NULL,
                        FilePath TEXT NOT NULL,
                        ModifiedBy INTEGER,
                        ModifiedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                        RevisionComment TEXT,
                        FOREIGN KEY(DocumentId) REFERENCES Documents(DocumentId),
                        FOREIGN KEY(ModifiedBy) REFERENCES Users(UserId)
                    );";

                // 6. Activity Logs Table
                // Used as an audit trail for important workflow actions
                string createActivityLogsTableQuery = @"
                    CREATE TABLE IF NOT EXISTS ActivityLogs (
                        ActivityId INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserId INTEGER NOT NULL,
                        ActivityType TEXT NOT NULL,
                        Description TEXT NOT NULL,
                        ActivityDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY(UserId) REFERENCES Users(UserId)
                    );";

                // 7. Notifications Table
                // For targeted system messages to specific users
                string createNotificationsTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Notifications (
                        NotificationId INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserId INTEGER NOT NULL,
                        Title TEXT NOT NULL,
                        Message TEXT NOT NULL,
                        IsRead INTEGER DEFAULT 0,
                        CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY(UserId) REFERENCES Users(UserId)
                    );";

                // Execute the commands one by one
                using (var command = new SQLiteCommand(createUsersTableQuery, connection)) command.ExecuteNonQuery();
                using (var command = new SQLiteCommand(createProjectsTableQuery, connection)) command.ExecuteNonQuery();
                using (var command = new SQLiteCommand(createDocumentsTableQuery, connection)) command.ExecuteNonQuery();
                using (var command = new SQLiteCommand(createTasksTableQuery, connection)) command.ExecuteNonQuery();
                using (var command = new SQLiteCommand(createRevisionsTableQuery, connection)) command.ExecuteNonQuery();
                using (var command = new SQLiteCommand(createActivityLogsTableQuery, connection)) command.ExecuteNonQuery();
                using (var command = new SQLiteCommand(createNotificationsTableQuery, connection)) command.ExecuteNonQuery();

                connection.Close();
            }

            // Quick fix to add Deadline if the DB already exists from previous runs:
            using (var connection = GetConnection())
            {
                connection.Open();
                try
                {
                    using (var command = new SQLiteCommand("ALTER TABLE Projects ADD COLUMN Deadline DATETIME;", connection))
                        command.ExecuteNonQuery();
                }
                catch (Exception) { }

                try
                {
                    using (var command = new SQLiteCommand("ALTER TABLE Documents ADD COLUMN FileType TEXT;", connection))
                        command.ExecuteNonQuery();
                }
                catch (Exception) { }

                try
                {
                    using (var command = new SQLiteCommand("ALTER TABLE Documents ADD COLUMN UploadedBy INTEGER;", connection))
                        command.ExecuteNonQuery();
                }
                catch (Exception) { }

                try
                {
                    using (var command = new SQLiteCommand("ALTER TABLE Tasks ADD COLUMN Deadline DATETIME;", connection))
                        command.ExecuteNonQuery();
                }
                catch (Exception) { }

                try
                {
                    using (var command = new SQLiteCommand("ALTER TABLE Tasks ADD COLUMN Status TEXT DEFAULT 'Pending';", connection))
                        command.ExecuteNonQuery();
                }
                catch (Exception) { }

                try
                {
                    using (var command = new SQLiteCommand("ALTER TABLE Tasks ADD COLUMN CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP;", connection))
                        command.ExecuteNonQuery();
                }
                catch (Exception) { }

                connection.Close();
            }
        }

        /// <summary>
        /// A simple test method to verify that we can connect, open, and close the DB successfully.
        /// </summary>
        public bool TestConnection()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open(); // If file or permissions are bad, this throws an error
                    return true;
                }
            }
            catch (SQLiteException ex)
            {
                // Rethrow safe critical SQL errors so App.xaml can handle them instead of dying quietly
                throw new Exception($"SQLite Database core engine failed to respond. Code: {ex.ErrorCode}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Database inaccessible. Ensure the application has write permissions inside its install directory.", ex);
            }
        }
    }
}
