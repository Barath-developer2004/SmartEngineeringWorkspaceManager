using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SmartEngineeringWorkspaceManager.Models;
using SmartEngineeringWorkspaceManager.Services;

namespace SmartEngineeringWorkspaceManager.Pages
{
    public partial class ProjectsPage : UserControl
    {
        private readonly ProjectService _projectService;
        private int _selectedProjectId = 0;

        public ProjectsPage()
        {
            InitializeComponent();
            _projectService = new ProjectService();
            LoadProjects();
        }

        // --- Data Binding & Loading ---

        private void LoadProjects(string searchQuery = "")
        {
            var projects = _projectService.GetAllProjects();

            // Simple search filtering
            if (!string.IsNullOrWhiteSpace(searchQuery) && searchQuery != "Search projects...")
            {
                projects = projects.Where(p => 
                    p.ProjectName.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (p.Description != null && p.Description.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
                ).ToList();
            }

            // Bind the List of Project models directly to the DataGrid!
            dgProjects.ItemsSource = projects;
        }

        // --- Form Actions (CRUD) ---

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validate Input
            if (string.IsNullOrWhiteSpace(txtProjectName.Text))
            {
                MessageBox.Show("Project Name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (dpDeadline.SelectedDate == null)
            {
                MessageBox.Show("Deadline is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Map UI Form Data to our Model
            var project = new Project
            {
                ProjectId = _selectedProjectId, // If 0, it means it's a new project
                ProjectName = txtProjectName.Text,
                Deadline = dpDeadline.SelectedDate.Value,
                Status = (cbStatus.SelectedItem as ComboBoxItem)?.Content.ToString(),
                Description = txtDescription.Text
            };

            // 3. Send Model to Database via Service
            try
            {
                if (_selectedProjectId == 0)
                {
                    _projectService.AddProject(project); // CREATE
                }
                else
                {
                    _projectService.UpdateProject(project); // UPDATE
                }

                ClearForm();
                LoadProjects(); // Refresh the grid to show changes
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving project: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            _selectedProjectId = 0;
            FormTitle.Text = "Create New Project";
            btnSave.Content = "Save Project";

            txtProjectName.Text = string.Empty;
            dpDeadline.SelectedDate = null;
            cbStatus.SelectedIndex = 0;
            txtDescription.Text = string.Empty;

            dgProjects.SelectedItem = null;
        }

        // --- DataGrid Interactions ---

        private void DgProjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // When a user clicks a row, map the selected Project Model back into the Form
            if (dgProjects.SelectedItem is Project selectedProject)
            {
                _selectedProjectId = selectedProject.ProjectId;
                FormTitle.Text = "Edit Project";
                btnSave.Content = "Update Project";

                txtProjectName.Text = selectedProject.ProjectName;
                dpDeadline.SelectedDate = selectedProject.Deadline;
                txtDescription.Text = selectedProject.Description;

                // Select proper ComboBox item
                foreach (ComboBoxItem item in cbStatus.Items)
                {
                    if (item.Content.ToString() == selectedProject.Status)
                    {
                        cbStatus.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            // Grab the specific Project model attached to the row where the button was clicked
            if ((sender as Button)?.DataContext is Project projectToDelete)
            {
                var result = MessageBox.Show($"Are you sure you want to delete '{projectToDelete.ProjectName}'?", 
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _projectService.DeleteProject(projectToDelete.ProjectId); // DELETE
                    ClearForm();
                    LoadProjects();
                }
            }
        }

        // --- Search Box Logic ---

        private void TxtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtSearch.Text == "Search projects...")
            {
                txtSearch.Text = "";
                txtSearch.Foreground = new SolidColorBrush(Color.FromRgb(45, 55, 72)); // #2D3748
            }
        }

        private void TxtSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtSearch.Foreground = new SolidColorBrush(Color.FromRgb(160, 174, 192)); // #A0AEC0
                txtSearch.Text = "Search projects...";
                LoadProjects();
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Filter live as the user types
            if (txtSearch.Text != "Search projects...")
            {
                LoadProjects(txtSearch.Text);
            }
        }
    }
}