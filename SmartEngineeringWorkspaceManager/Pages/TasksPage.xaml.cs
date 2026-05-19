using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SmartEngineeringWorkspaceManager.Models;
using SmartEngineeringWorkspaceManager.Services;

namespace SmartEngineeringWorkspaceManager.Pages
{
    public partial class TasksPage : UserControl
    {
        private readonly TaskService _taskService;
        private readonly ProjectService _projectService;
        private int _selectedTaskId = 0;

        public TasksPage()
        {
            InitializeComponent();
            _taskService = new TaskService();
            _projectService = new ProjectService();

            LoadProjectsToComboBox();
            LoadTasks();
        }

        private void LoadProjectsToComboBox()
        {
            var projects = _projectService.GetAllProjects();
            cbProjects.ItemsSource = projects;
        }

        private void LoadTasks(string searchQuery = "")
        {
            var tasks = _taskService.GetAllTasks();

            if (!string.IsNullOrWhiteSpace(searchQuery) && searchQuery != "Search tasks...")
            {
                tasks = tasks.Where(t => 
                    t.TaskName.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (t.ProjectName != null && t.ProjectName.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
                ).ToList();
            }

            dgTasks.ItemsSource = tasks;
        }

        // --- Form Actions (CRUD) ---

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(txtTaskName.Text))
            {
                MessageBox.Show("Task Name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (cbProjects.SelectedValue == null)
            {
                MessageBox.Show("You must assign this task to a specific Project.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (dpDeadline.SelectedDate == null)
            {
                MessageBox.Show("A deadline must be set.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var task = new TaskItem
            {
                TaskId = _selectedTaskId,
                TaskName = txtTaskName.Text,
                ProjectId = (int)cbProjects.SelectedValue,
                Deadline = dpDeadline.SelectedDate.Value,
                Status = (cbStatus.SelectedItem as ComboBoxItem)?.Content.ToString(),
                Description = txtDescription.Text,
                AssignedTo = 1 // Hardcode assigned to user 1 for now
            };

            try
            {
                if (_selectedTaskId == 0)
                    _taskService.AddTask(task);
                else
                    _taskService.UpdateTask(task);

                ClearForm();
                LoadTasks();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving task: {ex.Message}", "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            _selectedTaskId = 0;
            FormTitle.Text = "Create New Task";
            btnSave.Content = "Save Task";

            txtTaskName.Text = string.Empty;
            cbProjects.SelectedIndex = -1;
            dpDeadline.SelectedDate = null;
            cbStatus.SelectedIndex = 0;
            txtDescription.Text = string.Empty;

            dgTasks.SelectedItem = null;
        }

        // --- DataGrid Interactions ---

        private void DgTasks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgTasks.SelectedItem is TaskItem selectedTask)
            {
                _selectedTaskId = selectedTask.TaskId;
                FormTitle.Text = "Edit Task";
                btnSave.Content = "Update Task";

                txtTaskName.Text = selectedTask.TaskName;
                cbProjects.SelectedValue = selectedTask.ProjectId;
                dpDeadline.SelectedDate = selectedTask.Deadline;
                txtDescription.Text = selectedTask.Description;

                foreach (ComboBoxItem item in cbStatus.Items)
                {
                    if (item.Content.ToString() == selectedTask.Status)
                    {
                        cbStatus.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void BtnComplete_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is TaskItem completedTask)
            {
                _taskService.UpdateTaskStatus(completedTask.TaskId, "Completed");
                LoadTasks(); // Refresh grid immediately to apply grey strike-out styling
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is TaskItem taskToDelete)
            {
                var result = MessageBox.Show($"Are you sure you want to permanently delete task '{taskToDelete.TaskName}'?", 
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _taskService.DeleteTask(taskToDelete.TaskId);
                    ClearForm();
                    LoadTasks();
                }
            }
        }

        // --- Search ---

        private void TxtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtSearch.Text == "Search tasks...")
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
                txtSearch.Text = "Search tasks...";
                LoadTasks();
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtSearch.Text != "Search tasks...")
            {
                LoadTasks(txtSearch.Text);
            }
        }
    }
}