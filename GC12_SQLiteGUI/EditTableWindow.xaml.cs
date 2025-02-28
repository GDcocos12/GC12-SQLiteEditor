using System;
using System.Data;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;

namespace GC12_SQLiteGUI
{
    public partial class EditTableWindow : UserControl
    {
        public readonly string _databasePath;
        public readonly string _tableName;
        private DataTable _dataTable;
        public event EventHandler TableDataChanged;
        public bool IsDirty { get; private set; } = false;

        public EditTableWindow(string databasePath, string tableName)
        {
            InitializeComponent();
            _databasePath = databasePath;
            _tableName = tableName;
            LoadTableData();
        }

        private void LoadTableData()
        {
            try
            {
                using (var connection = new SQLiteConnection($"Data Source={_databasePath}"))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand($"SELECT * FROM {_tableName}", connection))
                    {
                        _dataTable = new DataTable();
                        using (var adapter = new SQLiteDataAdapter(command))
                        {
                            adapter.Fill(_dataTable);
                            new SQLiteCommandBuilder(adapter);
                            adapter.RowUpdating += Adapter_RowUpdating;
                            adapter.RowUpdated += Adapter_RowUpdated;
                            tableDataGrid.ItemsSource = _dataTable.DefaultView;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading table data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void Adapter_RowUpdating(object sender, System.Data.Common.RowUpdatingEventArgs e)
        {
            if (e.Status == UpdateStatus.Continue)
            {
                foreach (IDataParameter parameter in e.Command.Parameters)
                {
                    if (parameter.Value == DBNull.Value)
                    {
                        DataColumn column = _dataTable.Columns[parameter.SourceColumn];
                        if (column != null && !column.AllowDBNull)
                        {
                            if (column.DefaultValue != DBNull.Value && column.DefaultValue != null)
                            {
                                parameter.Value = column.DefaultValue;
                            }
                            else
                            {
                                MessageBox.Show($"Column '{column.ColumnName}' does not allow NULL values and has no default value.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                e.Status = UpdateStatus.ErrorsOccurred;
                                return;
                            }
                        }
                    }
                }
            }
        }

        private void AddRow_Click(object sender, RoutedEventArgs e)
        {
            DataRow newRow = _dataTable.NewRow();
            _dataTable.Rows.Add(newRow);
            TableDataChanged?.Invoke(this, EventArgs.Empty);
            IsDirty = true;
        }

        private void DeleteRow_Click(object sender, RoutedEventArgs e)
        {
            if (tableDataGrid.SelectedItem != null && tableDataGrid.SelectedItem is DataRowView rowView)
            {
                DataRow row = rowView.Row;
                row.Delete();
                UpdateDatabase();
                TableDataChanged?.Invoke(this, EventArgs.Empty);
                IsDirty = true;
            }
        }

        public void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            UpdateDatabase();
            LoadTableData();
        }

        private void CreateTable_Click(object sender, RoutedEventArgs e)
        {
            var createTableWindow = new CreateTableWindow(_databasePath);
            createTableWindow.TableCreated += CreateTableWindow_TableCreated;
            if (createTableWindow.ShowDialog() == true) { }
        }

        private void CreateTableWindow_TableCreated(object sender, EventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.LoadDatabaseStructure(_databasePath);

            }
        }

        private void UpdateDatabase()
        {
            try
            {
                using (var connection = new SQLiteConnection($"Data Source={_databasePath}"))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand($"SELECT * FROM {_tableName}", connection))
                    {
                        using (var adapter = new SQLiteDataAdapter(command))
                        {
                            using (new SQLiteCommandBuilder(adapter))
                            {
                                adapter.Update(_dataTable);

                            }
                        }
                    }
                }
                IsDirty = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating database: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Adapter_RowUpdated(object sender, System.Data.Common.RowUpdatedEventArgs e)
        {
            if (e.Status == UpdateStatus.Continue && e.RecordsAffected > 0)
            {
                IsDirty = true;
            }
        }
        private void tableDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                if (e.EditingElement is TextBox textBox)
                {
                    DataRowView rowView = e.Row.Item as DataRowView;
                    if (rowView != null)
                    {
                        DataColumn column = _dataTable.Columns[e.Column.DisplayIndex];
                        object oldValue = rowView[column.ColumnName];
                        object newValue = textBox.Text;
                        if (!object.Equals(oldValue, newValue))
                        {
                            IsDirty = true;
                            TableDataChanged?.Invoke(this, EventArgs.Empty);
                        }
                    }
                }
            }
        }
    }
}