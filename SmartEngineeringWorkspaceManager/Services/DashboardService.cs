using System;
using System.Collections.Generic;
using System.Data.SQLite;
using SmartEngineeringWorkspaceManager.Models;
using System.Linq;

namespace SmartEngineeringWorkspaceManager.Services
{
    public class DashboardService
    {
        private readonly DatabaseService _dbService;

        public DashboardService()
        {
            _dbService = new DatabaseService();
        }

        // Aggregate Data for the Top Cards
        public DashboardStats GetDashboardStats()
        {
            var stats = new DashboardStats();

            using (var connection = _dbService.GetConnection())
            {
                connection.Open();

                // 1. We use aggregate SQL functions like COUNT(*) to let the database do the math really fast
                // rather than downloading 10,000 rows into C# and using a .Count property.
                using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Projects", connection))
                    stats.TotalProjects = Convert.ToInt32(cmd.ExecuteScalar());

                // 2. We can add simple WHERE conditions to filter our counts dynamically
                using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Projects WHERE Status = 'Active'", connection))
                    stats.ActiveProjects = Convert.ToInt32(cmd.ExecuteScalar());

                using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Documents", connection))
                    stats.TotalDocuments = Convert.ToInt32(cmd.ExecuteScalar());

                using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Tasks WHERE Status != 'Completed'", connection))
                    stats.PendingTasks = Convert.ToInt32(cmd.ExecuteScalar());

                using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Tasks WHERE Status = 'Completed'", connection))
                    stats.CompletedTasks = Convert.ToInt32(cmd.ExecuteScalar());
            }

            return stats;
        }

        // Fetch recent chronologically sorted activity
        public List<ActivityItem> GetRecentActivity(int maxItems = 6)
        {
            var activityLog = new List<ActivityItem>();

            using (var connection = _dbService.GetConnection())
            {
                connection.Open();

                // 1. Get recent Tasks
                string taskQuery = "SELECT Title, CreatedAt, Status FROM Tasks ORDER BY CreatedAt DESC LIMIT @Limit";
                using (var cmd = new SQLiteCommand(taskQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@Limit", maxItems);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string title = reader["Title"].ToString();
                            string status = reader["Status"]?.ToString() ?? "Created";

                            activityLog.Add(new ActivityItem
                            {
                                ActivityDescription = $"Task '{title}' was marked as {status}.",
                                Timestamp = reader["CreatedAt"] != DBNull.Value ? Convert.ToDateTime(reader["CreatedAt"]) : DateTime.Now
                            });
                        }
                    }
                }

                // 2. Get recent Documents
                string docQuery = @"
                    SELECT d.FileName, d.UploadedAt, p.ProjectName 
                    FROM Documents d 
                    LEFT JOIN Projects p ON d.ProjectId = p.ProjectId 
                    ORDER BY d.UploadedAt DESC LIMIT @Limit";

                using (var cmd = new SQLiteCommand(docQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@Limit", maxItems);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string fileName = reader["FileName"].ToString();
                            string projectName = reader["ProjectName"]?.ToString() ?? "Unknown Project";

                            activityLog.Add(new ActivityItem
                            {
                                ActivityDescription = $"Document '{fileName}' uploaded to {projectName}.",
                                Timestamp = reader["UploadedAt"] != DBNull.Value ? Convert.ToDateTime(reader["UploadedAt"]) : DateTime.Now
                            });
                        }
                    }
                }
            }

            // Combine both lists in C#, sort them chronologically (newest first), and return only top 6
            return activityLog
                    .OrderByDescending(a => a.Timestamp)
                    .Take(maxItems)
                    .ToList();
        }
    }
}