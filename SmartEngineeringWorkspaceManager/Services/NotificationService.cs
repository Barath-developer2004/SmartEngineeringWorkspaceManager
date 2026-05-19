using System;
using System.Collections.Generic;
using System.Data.SQLite;
using SmartEngineeringWorkspaceManager.Models;

namespace SmartEngineeringWorkspaceManager.Services
{
    public class NotificationService
    {
        private readonly DatabaseService _dbService;

        public NotificationService()
        {
            _dbService = new DatabaseService();
        }

        public void CreateNotification(int targetUserId, string title, string message)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                string query = "INSERT INTO Notifications (UserId, Title, Message, CreatedDate) VALUES (@User, @Title, @Msg, @Date)";
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@User", targetUserId);
                    cmd.Parameters.AddWithValue("@Title", title);
                    cmd.Parameters.AddWithValue("@Msg", message);
                    cmd.Parameters.AddWithValue("@Date", DateTime.Now); // Override default UTC behavior with local PC time
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<Notification> GetUserNotifications(int userId, bool onlyUnread = false)
        {
            var notes = new List<Notification>();
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Notifications WHERE UserId = @User ";
                if(onlyUnread)
                {
                    query += "AND IsRead = 0 ";
                }
                query += "ORDER BY CreatedDate DESC";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@User", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            notes.Add(new Notification
                            {
                                NotificationId = Convert.ToInt32(reader["NotificationId"]),
                                UserId = Convert.ToInt32(reader["UserId"]),
                                Title = reader["Title"].ToString(),
                                Message = reader["Message"].ToString(),
                                IsRead = Convert.ToBoolean(reader["IsRead"]),
                                CreatedDate = Convert.ToDateTime(reader["CreatedDate"])
                            });
                        }
                    }
                }
            }
            return notes;
        }

        public void MarkAsRead(int notificationId)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                string query = "UPDATE Notifications SET IsRead = 1 WHERE NotificationId = @Id";
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Id", notificationId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}