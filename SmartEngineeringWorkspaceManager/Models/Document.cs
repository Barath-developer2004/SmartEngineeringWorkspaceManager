using System;

namespace SmartEngineeringWorkspaceManager.Models
{
    public class Document
    {
        public int DocumentId { get; set; }
        public int ProjectId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public DateTime UploadDate { get; set; }
        public int UploadedBy { get; set; }

        // This is an extra/helper property. It's not stored in the Documents table directly
        // but can be joined from the Projects table so the DataGrid displays a friendly name.
        public string ProjectName { get; set; }
    }
}