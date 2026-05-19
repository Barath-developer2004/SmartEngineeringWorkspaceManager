using System;
using System.Collections.Generic;
using System.Data.SQLite;
using SmartEngineeringWorkspaceManager.Models;

namespace SmartEngineeringWorkspaceManager.Services
{
    public class ActivityLogService
    {
        private readonly DatabaseService _dbService;

        public ActivityLogService()
        {
            _dbService = new DatabaseService();
        }

        public void LogActivity(int userId, string activityType, string description)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                string query = "INSERT INTO ActivityLogs (UserId, ActivityType, Description, ActivityDate) VALUES (@UserId, @Type, @Desc, @Date)";
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@Type", activityType);
                    cmd.Parameters.AddWithValue("@Desc", description);
                    cmd.Parameters.AddWithValue("@Date", DateTime.Now); // Ensures your local Windows system time is explicitly written to SQLite!
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<ActivityLog> GetRecentActivities(int count = 20)
        {
            var logs = new List<ActivityLog>();
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                // Get most recent first
                string query = "SELECT * FROM ActivityLogs ORDER BY ActivityDate DESC LIMIT @Count";
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Count", count);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            logs.Add(new ActivityLog
                            {
                                ActivityId = Convert.ToInt32(reader["ActivityId"]),
                                UserId = Convert.ToInt32(reader["UserId"]),
                                ActivityType = reader["ActivityType"].ToString(),
                                Description = reader["Description"].ToString(),
                                ActivityDate = Convert.ToDateTime(reader["ActivityDate"])
                            });
                        }
                    }
                }
            }
            return logs;
        }
    }
}