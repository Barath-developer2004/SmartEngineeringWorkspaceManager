using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Win32;
using SmartEngineeringWorkspaceManager.Models;
using SmartEngineeringWorkspaceManager.Services;
using System.Threading.Tasks;

namespace SmartEngineeringWorkspaceManager.Pages
{
    public partial class ReportsPage : UserControl
    {
        private ReportSnapshot _currentReport;
        private ExportService _exportService;

        public ReportsPage()
        {
            InitializeComponent();
            _exportService = new ExportService();
        }

        private void BtnPreview_Click(object sender, RoutedEventArgs e)
        {
            overlayEmpty.Visibility = Visibility.Collapsed;
            txtStatus.Text = "";

            // Build the data model
            _currentReport = new ReportSnapshot
            {
                Title = (cbReportType.SelectedItem as ComboBoxItem)?.Content.ToString(),
                GeneratedAt = DateTime.Now
            };

            // Grab real metrics
            var projService = new ProjectService();
            var tasksService = new TaskService();
            var docService = new DocumentService();

            var allProjects = projService.GetAllProjects();
            var allTasks = tasksService.GetAllTasks();
            var allDocs = docService.GetAllDocuments();

            _currentReport.TotalProjects = allProjects.Count;
            _currentReport.OpenTasks = allTasks.Count(t => t.Status != "Completed");
            _currentReport.CompletedTasks = allTasks.Count(t => t.Status == "Completed");
            _currentReport.DocumentsUploaded = allDocs.Count;

            // Populate UI Scorecards
            valProj.Text = _currentReport.TotalProjects.ToString();
            valPending.Text = _currentReport.OpenTasks.ToString();
            valDone.Text = _currentReport.CompletedTasks.ToString();

            // Set up Grid Data Columns dynamically based on Report Selection
            dgPreview.Columns.Clear();
            _currentReport.CSVLines.Clear();

            if (_currentReport.Title == "Activity Audit Trail")
            {
                dgPreview.Columns.Add(new DataGridTextColumn { Header = "Date", Binding = new Binding("ActivityDate") { StringFormat = "g" } });
                dgPreview.Columns.Add(new DataGridTextColumn { Header = "Type", Binding = new Binding("ActivityType") });
                dgPreview.Columns.Add(new DataGridTextColumn { Header = "Description", Binding = new Binding("Description") });

                var acts = new ActivityLogService().GetRecentActivities(100);
                dgPreview.ItemsSource = acts;

                // Pack text rows for export
                _currentReport.CSVLines.Add("Date,Type,Description");
                foreach(var a in acts) {
                    _currentReport.CSVLines.Add($"\"{a.ActivityDate}\",\"{a.ActivityType}\",\"{a.Description.Replace("\"", "\\\"")}\"");
                }
            }
            else // Default System Overview / Tasks
            {
                dgPreview.Columns.Add(new DataGridTextColumn { Header = "Task Name", Binding = new Binding("TaskName") });
                dgPreview.Columns.Add(new DataGridTextColumn { Header = "Status", Binding = new Binding("Status") });
                dgPreview.Columns.Add(new DataGridTextColumn { Header = "Project", Binding = new Binding("ProjectName") });

                dgPreview.ItemsSource = allTasks;

                // Pack text rows for export
                _currentReport.CSVLines.Add("Task Name,Status,Project Name");
                foreach(var t in allTasks) {
                    _currentReport.CSVLines.Add($"\"{t.TaskName}\",\"{t.Status}\",\"{t.ProjectName}\"");
                }
            }
        }

        private void BtnExportCsv_Click(object sender, RoutedEventArgs e)
        {
            if (_currentReport == null) {
                txtStatus.Text = "Please Preview a report first!";
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "CSV Excel file (*.csv)|*.csv",
                FileName = $"Report_{DateTime.Now:yyyyMMdd_HHmm}.csv"
            };

            if (sfd.ShowDialog() == true)
            {
                bool success = _exportService.ExportToCsv(_currentReport, sfd.FileName);
                if (success) {
                    txtStatus.Text = $"Success! Exported to {sfd.FileName}";
                    LogExportActivity();
                } else {
                    txtStatus.Text = "Failed to export CSV. File might be open.";
                }
            }
        }

        private void BtnExportPrint_Click(object sender, RoutedEventArgs e)
        {
             if (_currentReport == null) {
                txtStatus.Text = "Please Preview a report first!";
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Text Document (*.txt)|*.txt",
                FileName = $"Printable_Report_{DateTime.Now:yyyyMMdd}.txt"
            };

            if (sfd.ShowDialog() == true)
            {
                bool success = _exportService.ExportToPrintableText(_currentReport, sfd.FileName);
                if (success) {
                    txtStatus.Text = $"Success! Document saved to {sfd.FileName}";
                    LogExportActivity();
                } else {
                    txtStatus.Text = "Failed to save Document.";
                }
            }
        }

        private void LogExportActivity()
        {
             if(AuthenticationService.CurrentUser != null)
             {
                   new ActivityLogService().LogActivity(AuthenticationService.CurrentUser.UserId, "Export Generated", $"Exported {_currentReport.Title}");
             }
        }
    }
}