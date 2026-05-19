using System;
using System.Collections.Generic;
using System.Data.SQLite;
using SmartEngineeringWorkspaceManager.Models;

namespace SmartEngineeringWorkspaceManager.Services
{
    public class DocumentService
    {
        private readonly DatabaseService _dbService;

        public DocumentService()
        {
            _dbService = new DatabaseService();
        }

        // CREATE: Add a new document record.
        public void AddDocument(Document document)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();

                string query = @"
                    INSERT INTO Documents (ProjectId, FileName, FilePath, FileType, UploadedAt, UploadedBy) 
                    VALUES (@ProjectId, @FileName, @FilePath, @FileType, @UploadedAt, @UploadedBy)";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProjectId", document.ProjectId);
                    command.Parameters.AddWithValue("@FileName", document.FileName);
                    command.Parameters.AddWithValue("@FilePath", document.FilePath);
                    command.Parameters.AddWithValue("@FileType", document.FileType ?? "");
                    command.Parameters.AddWithValue("@UploadedAt", DateTime.Now);
                    command.Parameters.AddWithValue("@UploadedBy", document.UploadedBy);

                    command.ExecuteNonQuery();
                }

                // Immediately create the Base "v1.0" Revision for this newly uploaded document
                // Get the generated DocumentId from the database using last_insert_rowid()
                long newDocumentId;
                using (var idCmd = new SQLiteCommand("SELECT last_insert_rowid()", connection))
                {
                    newDocumentId = (long)idCmd.ExecuteScalar();
                }

                string revQuery = @"
                    INSERT INTO Revisions (DocumentId, VersionNumber, FilePath, ModifiedBy, ModifiedDate, RevisionComment) 
                    VALUES (@DocId, 'v1.0', @FilePath, @ModBy, @ModDate, 'Initial Document Upload')";

                using (var revCmd = new SQLiteCommand(revQuery, connection))
                {
                    revCmd.Parameters.AddWithValue("@DocId", newDocumentId);
                    revCmd.Parameters.AddWithValue("@FilePath", document.FilePath);
                    revCmd.Parameters.AddWithValue("@ModBy", document.UploadedBy);
                    revCmd.Parameters.AddWithValue("@ModDate", DateTime.Now);
                    revCmd.ExecuteNonQuery();
                }

                if(AuthenticationService.CurrentUser != null)
                {
                    var logService = new ActivityLogService();
                    logService.LogActivity(AuthenticationService.CurrentUser.UserId, "Document Uploaded", $"Uploaded {document.FileName}");
                }
            }
        }

        // READ: Get all documents, potentially joining with Projects to get ProjectName
        public List<Document> GetAllDocuments()
        {
            var documents = new List<Document>();

            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                // SQL JOIN: We join Projects and Documents where the ProjectId matches
                // so we can display "Bridge Blueprint" mapped to "Project Alpha" in UI.
                string query = @"
                    SELECT d.*, p.ProjectName 
                    FROM Documents d
                    LEFT JOIN Projects p ON d.ProjectId = p.ProjectId
                    ORDER BY d.UploadedAt DESC";

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var doc = new Document
                        {
                            DocumentId = Convert.ToInt32(reader["DocumentId"]),
                            ProjectId = Convert.ToInt32(reader["ProjectId"]),
                            FileName = reader["FileName"].ToString(),
                            FilePath = reader["FilePath"].ToString(),
                            FileType = reader["FileType"]?.ToString(),
                            ProjectName = reader["ProjectName"]?.ToString()
                        };

                        if (reader["UploadedAt"] != DBNull.Value)
                            doc.UploadDate = Convert.ToDateTime(reader["UploadedAt"]);

                        if (reader["UploadedBy"] != DBNull.Value)
                            doc.UploadedBy = Convert.ToInt32(reader["UploadedBy"]);

                        documents.Add(doc);
                    }
                }
            }

            return documents;
        }

        // DELETE: Remove a document record from Database
        public void DeleteDocument(int documentId)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();

                // First delete all linked Revisions (Cascade Delete pattern)
                string delRevQuery = "DELETE FROM Revisions WHERE DocumentId = @DocId";
                using (var revCmd = new SQLiteCommand(delRevQuery, connection))
                {
                    revCmd.Parameters.AddWithValue("@DocId", documentId);
                    revCmd.ExecuteNonQuery();
                }

                // Then delete the master Document
                string query = "DELETE FROM Documents WHERE DocumentId = @DocumentId";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@DocumentId", documentId);
                    command.ExecuteNonQuery();
                }
            }
        }

        // ---------- REVISION SPECIFIC METHODS ----------

        // CREATE: Add a new revision version to an existing document
        public void AddRevision(Revision revision)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                string revQuery = @"
                    INSERT INTO Revisions (DocumentId, VersionNumber, FilePath, ModifiedBy, ModifiedDate, RevisionComment) 
                    VALUES (@DocId, @VerNum, @FilePath, @ModBy, @ModDate, @Comment)";

                using (var revCmd = new SQLiteCommand(revQuery, connection))
                {
                    revCmd.Parameters.AddWithValue("@DocId", revision.DocumentId);
                    revCmd.Parameters.AddWithValue("@VerNum", revision.VersionNumber);
                    revCmd.Parameters.AddWithValue("@FilePath", revision.FilePath);
                    revCmd.Parameters.AddWithValue("@ModBy", revision.ModifiedBy);
                    revCmd.Parameters.AddWithValue("@ModDate", DateTime.Now);
                    revCmd.Parameters.AddWithValue("@Comment", revision.RevisionComment);

                    revCmd.ExecuteNonQuery();

                    if(AuthenticationService.CurrentUser != null)
                    {
                        var logService = new ActivityLogService();
                        logService.LogActivity(AuthenticationService.CurrentUser.UserId, "Document Revised", $"Revision {revision.VersionNumber} created for Document ID {revision.DocumentId}");
                    }
                }
            }
        }

        // READ: Get the history of revisions for a specific Document ID
        public List<Revision> GetRevisionsForDocument(int documentId)
        {
            var history = new List<Revision>();

            using (var connection = _dbService.GetConnection())
            {
                connection.Open();

                string query = @"
                    SELECT r.*, u.FullName 
                    FROM Revisions r
                    LEFT JOIN Users u ON r.ModifiedBy = u.UserId
                    WHERE r.DocumentId = @DocId
                    ORDER BY r.ModifiedDate DESC";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@DocId", documentId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var rev = new Revision
                            {
                                RevisionId = Convert.ToInt32(reader["RevisionId"]),
                                DocumentId = Convert.ToInt32(reader["DocumentId"]),
                                VersionNumber = reader["VersionNumber"].ToString(),
                                FilePath = reader["FilePath"].ToString(),
                                RevisionComment = reader["RevisionComment"]?.ToString(),
                                ModifierName = reader["FullName"]?.ToString() ?? "System"
                            };

                            if (reader["ModifiedBy"] != DBNull.Value)
                                rev.ModifiedBy = Convert.ToInt32(reader["ModifiedBy"]);

                            if (reader["ModifiedDate"] != DBNull.Value)
                                rev.ModifiedDate = Convert.ToDateTime(reader["ModifiedDate"]);

                            history.Add(rev);
                        }
                    }
                }
            }

            return history;
        }
    }
}