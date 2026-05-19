using System;

namespace SmartEngineeringWorkspaceManager.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedDate { get; set; }
        public int UserId { get; set; } // Who this notification is for
    }
}