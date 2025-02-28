using System;
using System.Windows;

namespace GC12_SQLiteGUI
{
    public partial class ImportTextWindow : Window
    {
        public string TableText { get; private set; }
        public string Separator { get; private set; }
        public string TableName { get; private set; }

        public ImportTextWindow()
        {
            InitializeComponent();
            separatorTextBox.Text = ";";
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tableNameTextBox.Text))
            {
                MessageBox.Show("Please enter a table name.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(textTextBox.Text))
            {
                MessageBox.Show("Please enter the text to import.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(separatorTextBox.Text))
            {
                MessageBox.Show("Please enter the separator.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TableText = textTextBox.Text;
            Separator = separatorTextBox.Text;
            TableName = tableNameTextBox.Text;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}