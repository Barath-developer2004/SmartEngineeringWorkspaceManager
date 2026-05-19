using System;

namespace SmartEngineeringWorkspaceManager.Models
{
    public class ActivityLog
    {
        public int ActivityId { get; set; }
        public int UserId { get; set; }
        public string ActivityType { get; set; }
        public string Description { get; set; }
        public DateTime ActivityDate { get; set; }
    }
}