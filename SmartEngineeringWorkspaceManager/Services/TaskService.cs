using System;
using System.Collections.Generic;
using System.Data.SQLite;
using SmartEngineeringWorkspaceManager.Models;

namespace SmartEngineeringWorkspaceManager.Services
{
    public class TaskService
    {
        private readonly DatabaseService _dbService;

        public TaskService()
        {
            _dbService = new DatabaseService();
        }

        // CREATE
        public void AddTask(TaskItem task)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                string query = @"
                    INSERT INTO Tasks (Title, Description, Status, Deadline, ProjectId, AssignedToUserId) 
                    VALUES (@Title, @Desc, @Status, @Deadline, @ProjId, @AssignId)";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Title", task.TaskName);
                    command.Parameters.AddWithValue("@Desc", task.Description);
                    command.Parameters.AddWithValue("@Status", task.Status);
                    command.Parameters.AddWithValue("@Deadline", task.Deadline);
                    command.Parameters.AddWithValue("@ProjId", task.ProjectId);
                    command.Parameters.AddWithValue("@AssignId", task.AssignedTo);

                    command.ExecuteNonQuery();
                }
            }
        }

        // READ
        public List<TaskItem> GetAllTasks()
        {
            var tasks = new List<TaskItem>();

            using (var connection = _dbService.GetConnection())
            {
                connection.Open();

                // JOIN across three tables (Tasks, Projects, Users) to resolve IDs into clean readable names
                string query = @"
                    SELECT t.*, p.ProjectName, u.FullName 
                    FROM Tasks t
                    LEFT JOIN Projects p ON t.ProjectId = p.ProjectId
                    LEFT JOIN Users u ON t.AssignedToUserId = u.UserId
                    ORDER BY t.Deadline ASC";

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var task = new TaskItem
                        {
                            TaskId = Convert.ToInt32(reader["TaskId"]),
                            ProjectId = Convert.ToInt32(reader["ProjectId"]),
                            TaskName = reader["Title"].ToString(),
                            Description = reader["Description"].ToString(),
                            Status = reader["Status"]?.ToString() ?? "Pending",
                            ProjectName = reader["ProjectName"]?.ToString() ?? "N/A",
                            AssignedToName = reader["FullName"]?.ToString() ?? "Unassigned"
                        };

                        if (reader["AssignedToUserId"] != DBNull.Value)
                            task.AssignedTo = Convert.ToInt32(reader["AssignedToUserId"]);

                        if (reader["Deadline"] != DBNull.Value)
                            task.Deadline = Convert.ToDateTime(reader["Deadline"]);

                        if (reader["CreatedAt"] != DBNull.Value)
                            task.CreatedDate = Convert.ToDateTime(reader["CreatedAt"]);

                        tasks.Add(task);
                    }
                }
            }
            return tasks;
        }

        // UPDATE (Full entity update)
        public void UpdateTask(TaskItem task)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                string query = @"
                    UPDATE Tasks 
                    SET Title = @Title, Description = @Desc, Status = @Status, 
                        Deadline = @Deadline, ProjectId = @ProjId, AssignedToUserId = @AssignId
                    WHERE TaskId = @TaskId";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Title", task.TaskName);
                    command.Parameters.AddWithValue("@Desc", task.Description);
                    command.Parameters.AddWithValue("@Status", task.Status);
                    command.Parameters.AddWithValue("@Deadline", task.Deadline);
                    command.Parameters.AddWithValue("@ProjId", task.ProjectId);
                    command.Parameters.AddWithValue("@AssignId", task.AssignedTo);
                    command.Parameters.AddWithValue("@TaskId", task.TaskId);

                    command.ExecuteNonQuery();
                }
            }
        }

        // UPDATE STATUS ONLY (Fast completion toggle)
        public void UpdateTaskStatus(int taskId, string newStatus)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = new SQLiteCommand("UPDATE Tasks SET Status = @Status WHERE TaskId = @TaskId", connection))
                {
                    command.Parameters.AddWithValue("@Status", newStatus);
                    command.Parameters.AddWithValue("@TaskId", taskId);
                    command.ExecuteNonQuery();

                    if(AuthenticationService.CurrentUser != null && newStatus == "Completed")
                    {
                        var logService = new ActivityLogService();
                        logService.LogActivity(AuthenticationService.CurrentUser.UserId, "Task Completed", $"Task ID {taskId} marked as Completed.");

                        // Notify current user so it shows up
                        var noteService = new NotificationService();
                        noteService.CreateNotification(AuthenticationService.CurrentUser.UserId, "Task Completed", $"Task {taskId} was finished by {AuthenticationService.CurrentUser.Username}");
                    }
                }
            }
        }

        // DELETE
        public void DeleteTask(int taskId)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = new SQLiteCommand("DELETE FROM Tasks WHERE TaskId = @TaskId", connection))
                {
                    command.Parameters.AddWithValue("@TaskId", taskId);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}