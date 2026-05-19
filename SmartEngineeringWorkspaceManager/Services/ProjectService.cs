using System;
using System.Collections.Generic;
using System.Data.SQLite;
using SmartEngineeringWorkspaceManager.Models;

namespace SmartEngineeringWorkspaceManager.Services
{
    public class ProjectService
    {
        private readonly DatabaseService _dbService;

        public ProjectService()
        {
            _dbService = new DatabaseService();
        }

        // CREATE: Add a new project
        public void AddProject(Project project)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();

                // We use parameters (@ProjectName, @Description, etc) to prevent SQL Injection attacks!
                string query = @"
                    INSERT INTO Projects (ProjectName, Description, Deadline, Status, CreatedAt) 
                    VALUES (@ProjectName, @Description, @Deadline, @Status, @CreatedAt)";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProjectName", project.ProjectName);
                    command.Parameters.AddWithValue("@Description", project.Description ?? "");
                    command.Parameters.AddWithValue("@Deadline", project.Deadline);
                    command.Parameters.AddWithValue("@Status", project.Status ?? "Active");
                    command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                    command.ExecuteNonQuery();

                    if(AuthenticationService.CurrentUser != null)
                    {
                        var logService = new ActivityLogService();
                        logService.LogActivity(AuthenticationService.CurrentUser.UserId, "Project Created", $"Created project: {project.ProjectName}");
                    }
                }
            }
        }

        // READ: Get all projects
        public List<Project> GetAllProjects()
        {
            var projects = new List<Project>();

            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Projects ORDER BY CreatedAt DESC";

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var project = new Project
                        {
                            ProjectId = Convert.ToInt32(reader["ProjectId"]),
                            ProjectName = reader["ProjectName"].ToString(),
                            Description = reader["Description"].ToString(),
                            Status = reader["Status"].ToString()
                        };

                        // Date fields might be null if manually inserted, so we check first
                        if (reader["Deadline"] != DBNull.Value)
                            project.Deadline = Convert.ToDateTime(reader["Deadline"]);

                        if (reader["CreatedAt"] != DBNull.Value)
                            project.CreatedDate = Convert.ToDateTime(reader["CreatedAt"]);

                        projects.Add(project);
                    }
                }
            }

            return projects;
        }

        // UPDATE: Modify an existing project
        public void UpdateProject(Project project)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                string query = @"
                    UPDATE Projects 
                    SET ProjectName = @ProjectName, 
                        Description = @Description, 
                        Deadline = @Deadline, 
                        Status = @Status 
                    WHERE ProjectId = @ProjectId";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProjectName", project.ProjectName);
                    command.Parameters.AddWithValue("@Description", project.Description);
                    command.Parameters.AddWithValue("@Deadline", project.Deadline);
                    command.Parameters.AddWithValue("@Status", project.Status);
                    command.Parameters.AddWithValue("@ProjectId", project.ProjectId);

                    command.ExecuteNonQuery();

                    // If the project was just marked completed, generate a notification!
                    if (project.Status == "Completed" && AuthenticationService.CurrentUser != null)
                    {
                        var noteService = new NotificationService();
                        // Instead of hardcoding 1, we notify the actual user or all admins
                        // For now we just notify the current user so it shows up in their feed
                        noteService.CreateNotification(AuthenticationService.CurrentUser.UserId, "Project Completed", $"Project '{project.ProjectName}' was completed by {AuthenticationService.CurrentUser.Username}");

                        var logService = new ActivityLogService();
                        logService.LogActivity(AuthenticationService.CurrentUser.UserId, "Project Terminated", $"Project '{project.ProjectName}' marked as Completed.");
                    }
                }
            }
        }

        // DELETE: Remove a project entirely
        public void DeleteProject(int projectId)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                string query = "DELETE FROM Projects WHERE ProjectId = @ProjectId";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProjectId", projectId);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}