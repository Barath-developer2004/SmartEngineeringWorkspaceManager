using System;
using System.IO;
using System.Text;
using SmartEngineeringWorkspaceManager.Models;

namespace SmartEngineeringWorkspaceManager.Services
{
    public class ExportService
    {
        // Generates a simple, highly portable CSV rather than forcing complex massive external COM Interop for Excel
        // CSV files open perfectly in Excel while maintaining modularity and stability.
        public bool ExportToCsv(ReportSnapshot report, string filePath)
        {
            try
            {
                var sb = new StringBuilder();

                // Add header info
                sb.AppendLine("SMART ENGINEERING WORKSPACE MANAGER");
                sb.AppendLine($"Report Type: {report.Title}");
                sb.AppendLine($"Generated: {report.GeneratedAt}");
                sb.AppendLine();
                sb.AppendLine("--- OVERVIEW ---");
                sb.AppendLine($"Total Projects,{report.TotalProjects}");
                sb.AppendLine($"Pending Tasks,{report.OpenTasks}");
                sb.AppendLine($"Completed Tasks,{report.CompletedTasks}");
                sb.AppendLine($"Documents Uploaded,{report.DocumentsUploaded}");
                sb.AppendLine();

                // Add detailed grid data
                sb.AppendLine("--- DETAIL LOG ---");
                foreach (var line in report.CSVLines)
                {
                    sb.AppendLine(line);
                }

                File.WriteAllText(filePath, sb.ToString());
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // We will generate a basic text-based readable printout to act as a lightweight PDF alternative 
        // to avoid complex graphics drawing requirements which break beginner-friendliness.
        public bool ExportToPrintableText(ReportSnapshot report, string filePath)
        {
            try
            {
                var sb = new StringBuilder();

                sb.AppendLine("=".PadRight(50, '='));
                sb.AppendLine("       OFFICIAL SYSTEM REPORT       ");
                sb.AppendLine("=".PadRight(50, '='));
                sb.AppendLine();
                sb.AppendLine($"  TITLE: {report.Title}");
                sb.AppendLine($"  DATE:  {report.GeneratedAt}");
                sb.AppendLine();
                sb.AppendLine("- SUMMARY -");
                sb.AppendLine($"  Projects Tracked: {report.TotalProjects}");
                sb.AppendLine($"  Active Tasks:     {report.OpenTasks}");
                sb.AppendLine($"  Finished Tasks:   {report.CompletedTasks}");
                sb.AppendLine($"  Files Indexed:    {report.DocumentsUploaded}");
                sb.AppendLine();
                sb.AppendLine("- DETAIL LOG -");

                foreach (var line in report.CSVLines)
                {
                    // Clean up CSV format for visual reading
                    sb.AppendLine("  > " + line.Replace(",", " | "));
                }

                sb.AppendLine();
                sb.AppendLine("=".PadRight(50, '='));
                sb.AppendLine("        END OF REPORT");

                File.WriteAllText(filePath, sb.ToString());
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}