using System;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Windows;
using GC12_SQLiteGUI.Models;
using System.Linq;
using System.Text;

namespace GC12_SQLiteGUI
{
    public partial class CreateTableWindow : Window
    {
        private readonly string _databasePath;
        public ObservableCollection<ColumnDefinition> Columns { get; set; } = new ObservableCollection<ColumnDefinition>();

        public event EventHandler TableCreated;

        public CreateTableWindow(string databasePath)
        {
            InitializeComponent();
            _databasePath = databasePath;
            columnsListView.ItemsSource = Columns;
            Columns.Add(new ColumnDefinition { Name = "Id", DataType = "INTEGER", IsPrimaryKey = true, IsNotNull = true });
        }

        private void AddColumn_Click(object sender, RoutedEventArgs e)
        {
            Columns.Add(new ColumnDefinition { Name = "NewColumn", DataType = "TEXT" });
        }

        private void DeleteColumn_Click(object sender, RoutedEventArgs e)
        {
            if (columnsListView.SelectedItem is ColumnDefinition selectedColumn)
            {
                Columns.Remove(selectedColumn);
            }
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tableNameTextBox.Text))
            {
                MessageBox.Show("Please enter a table name.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (Columns.Any(c => string.IsNullOrWhiteSpace(c.Name)))
            {
                MessageBox.Show("Column names cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (Columns.GroupBy(c => c.Name).Any(g => g.Count() > 1))
            {
                MessageBox.Show("Column names must be unique.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (Columns.Count == 0)
            {
                MessageBox.Show("Please add at least one column.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!Columns.Any(c => c.IsPrimaryKey))
            {
                MessageBox.Show("Please select at least one primary key.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var connection = new SQLiteConnection($"Data Source={_databasePath}"))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(connection))
                    {
                        StringBuilder sqlBuilder = new StringBuilder($"CREATE TABLE {tableNameTextBox.Text} (");

                        foreach (var column in Columns)
                        {
                            sqlBuilder.Append($"{column.Name} {column.DataType.ToUpper()}");

                            if (column.IsPrimaryKey)
                            {
                                sqlBuilder.Append(" PRIMARY KEY");
                            }
                            if (column.IsNotNull)
                            {
                                sqlBuilder.Append(" NOT NULL");
                            }
                            sqlBuilder.Append(", ");
                        }

                        if (Columns.Count > 0)
                        {
                            sqlBuilder.Length -= 2;
                        }

                        sqlBuilder.Append(");");

                        command.CommandText = sqlBuilder.ToString();
                        command.ExecuteNonQuery();
                    }
                }

                DialogResult = true;
                TableCreated?.Invoke(this, EventArgs.Empty);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating table: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}