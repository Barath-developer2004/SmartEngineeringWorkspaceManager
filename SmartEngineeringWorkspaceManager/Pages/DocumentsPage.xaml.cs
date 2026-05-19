using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using System.Diagnostics;
using SmartEngineeringWorkspaceManager.Models;
using SmartEngineeringWorkspaceManager.Services;

namespace SmartEngineeringWorkspaceManager.Pages
{
    public partial class DocumentsPage : UserControl
    {
        private readonly DocumentService _documentService;
        private readonly ProjectService _projectService;

        public DocumentsPage()
        {
            InitializeComponent();
            _documentService = new DocumentService();
            _projectService = new ProjectService();

            LoadProjectsToComboBox();
            LoadDocuments();
        }

        private void LoadProjectsToComboBox()
        {
            // We need a list of active projects so the user can tag the document to a specific project.
            var projects = _projectService.GetAllProjects();
            cbProjects.ItemsSource = projects;
            if (projects.Count > 0) cbProjects.SelectedIndex = 0;
        }

        private void LoadDocuments(string searchQuery = "")
        {
            var docs = _documentService.GetAllDocuments();

            if (!string.IsNullOrWhiteSpace(searchQuery) && searchQuery != "Search documents...")
            {
                docs = docs.Where(d => 
                    d.FileName.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (d.ProjectName != null && d.ProjectName.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
                ).ToList();
            }

            dgDocuments.ItemsSource = docs;
        }

        // --- File Upload Logic ---

        private void BtnUpload_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validation
            if (cbProjects.SelectedValue == null)
            {
                MessageBox.Show("Please select a Project first before uploading a document.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Open File Dialog mapping to Windows OS
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select Engineering Document";
            openFileDialog.Filter = "All Files (*.*)|*.*|PDFs (*.pdf)|*.pdf|CAD Files (*.dwg)|*.dwg|Word (*.docx)|*.docx";

            // If the user clicks "OK" in the file explorer dialog
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Grab file details dynamically from the selected file
                    string selectedFilePath = openFileDialog.FileName;
                    string fileName = Path.GetFileName(selectedFilePath); // gets "blueprint.pdf"
                    string fileExt = Path.GetExtension(selectedFilePath).ToUpper(); // gets ".PDF"

                    // Create the Document Model. 
                    // Note: We are storing the string PATH string to the file, NOT the massive file binary itself (BLOB).
                    // This keeps SQLite blazing fast and avoids database bloat.
                    var newDoc = new Document
                    {
                        ProjectId = (int)cbProjects.SelectedValue,
                        FileName = fileName,
                        FilePath = selectedFilePath, 
                        FileType = fileExt,
                        UploadedBy = AuthenticationService.CurrentUser?.UserId ?? 1 
                    };

                    _documentService.AddDocument(newDoc);
                    LoadDocuments(); // Refresh Grid

                    MessageBox.Show($"File '{fileName}' successfully registered into the system.", "Upload Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"File registration failed. The database may be locked.\nError: {ex.Message}", "Database Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // --- File Management Actions ---

        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Document docToOpen)
            {
                if (File.Exists(docToOpen.FilePath))
                {
                    try {
                        Process.Start(new ProcessStartInfo { FileName = docToOpen.FilePath, UseShellExecute = true });
                    } catch (Exception) {
                        MessageBox.Show("Cannot open file. Windows blocked the application or no default program is assigned.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Error: The original file has been moved or deleted from its location on your hard drive.", 
                        "File Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Document docToDelete)
            {
                var result = MessageBox.Show($"WARNING: Are you absolutely certain you wish to delete\n'{docToDelete.FileName}'?\n\nThis will permanently delete all associated revisions. This action cannot be undone.", 
                    "Confirm Critical Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);

                if (result == MessageBoxResult.Yes)
                {
                    try {
                        _documentService.DeleteDocument(docToDelete.DocumentId);
                        LoadDocuments();
                    } catch (Exception ex) {
                        MessageBox.Show($"Failed to delete document: {ex.Message}", "Database Lock", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        // --- Revision Tracking Logic ---

        private int _currentRevisionDocumentId = 0;

        private void BtnViewRevisions_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Document selectedDoc)
            {
                _currentRevisionDocumentId = selectedDoc.DocumentId;
                RevTitleText.Text = $"Revision History: {selectedDoc.FileName}";

                LoadRevisions(_currentRevisionDocumentId);

                // Show the Revision pop-out UI and hide main DataGrid
                RevisionPanel.Visibility = Visibility.Visible;
                dgDocuments.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadRevisions(int documentId)
        {
            var revisions = _documentService.GetRevisionsForDocument(documentId);
            dgRevisions.ItemsSource = revisions;
        }

        private void BtnCloseRevisions_Click(object sender, RoutedEventArgs e)
        {
            // Close the panel and show the Documents grid again
            RevisionPanel.Visibility = Visibility.Collapsed;
            dgDocuments.Visibility = Visibility.Visible;

            // Clear inputs
            txtNewVersion.Text = string.Empty;
            txtRevComment.Text = string.Empty;
            _currentRevisionDocumentId = 0;
        }

        private void BtnOpenRevision_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Revision revToOpen)
            {
                if (File.Exists(revToOpen.FilePath))
                {
                    Process.Start(new ProcessStartInfo { FileName = revToOpen.FilePath, UseShellExecute = true });
                }
                else
                {
                    MessageBox.Show("Error: Cannot locate this physical file version.", "File Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnUploadRevision_Click(object sender, RoutedEventArgs e)
        {
            if (_currentRevisionDocumentId == 0) return;

            if (string.IsNullOrWhiteSpace(txtNewVersion.Text))
            {
                MessageBox.Show("Please enter a version number (ex: v2.0).", "Missing Version", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select New Revision Document";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var newRev = new Revision
                    {
                        DocumentId = _currentRevisionDocumentId,
                        VersionNumber = txtNewVersion.Text,
                        FilePath = openFileDialog.FileName, 
                        RevisionComment = txtRevComment.Text,
                        ModifiedBy = 1 // Hardcoded User 1
                    };

                    _documentService.AddRevision(newRev);
                    LoadRevisions(_currentRevisionDocumentId); // Refresh the history grid

                    // Clear inputs
                    txtNewVersion.Text = string.Empty;
                    txtRevComment.Text = string.Empty;

                    MessageBox.Show("New revision logged successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error logging revision: {ex.Message}", "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // --- Search Logic ---

        private void TxtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtSearch.Text == "Search documents...")
            {
                txtSearch.Text = "";
                txtSearch.Foreground = new SolidColorBrush(Color.FromRgb(45, 55, 72));
            }
        }

        private void TxtSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtSearch.Foreground = new SolidColorBrush(Color.FromRgb(160, 174, 192));
                txtSearch.Text = "Search documents...";
                LoadDocuments();
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtSearch.Text != "Search documents...")
            {
                LoadDocuments(txtSearch.Text);
            }
        }
    }
}