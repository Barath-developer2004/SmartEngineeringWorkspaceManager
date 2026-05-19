using System;
using System.Collections.Generic;

namespace SmartEngineeringWorkspaceManager.Models
{
    public class ReportSnapshot
    {
        public string Title { get; set; }
        public DateTime GeneratedAt { get; set; }
        public int TotalProjects { get; set; }
        public int OpenTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int DocumentsUploaded { get; set; }

        // Tabular data for exporting
        public List<string> CSVLines { get; set; }

        public ReportSnapshot()
        {
            CSVLines = new List<string>();
        }
    }
}