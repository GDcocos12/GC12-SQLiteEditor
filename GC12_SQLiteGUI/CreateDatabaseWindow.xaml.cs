using Microsoft.Win32;
using System;
using System.Data.SQLite;
using System.IO;
using System.Windows;

namespace GC12_SQLiteGUI
{
    public partial class CreateDatabaseWindow : Window
    {
        public string DatabasePath { get; private set; }

        public CreateDatabaseWindow()
        {
            InitializeComponent();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "SQLite Database Files (*.sqlite)|*.sqlite|All Files (*.*)|*.*",
                Title = "Create New SQLite Database"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                filePathTextBox.Text = saveFileDialog.FileName;
            }
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(filePathTextBox.Text))
            {
                MessageBox.Show("Please enter a valid file path.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string directoryPath = Path.GetDirectoryName(filePathTextBox.Text);
            if (!Directory.Exists(directoryPath))
            {
                MessageBox.Show("The specified directory does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                SQLiteConnection.CreateFile(filePathTextBox.Text);
                DatabasePath = filePathTextBox.Text;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating database: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}