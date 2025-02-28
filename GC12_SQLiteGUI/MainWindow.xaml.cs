using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Diagnostics;

namespace GC12_SQLiteGUI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void NewDatabase_Click(object sender, RoutedEventArgs e)
        {
            var createDbWindow = new CreateDatabaseWindow();
            if (createDbWindow.ShowDialog() == true)
            {
                LoadDatabaseStructure(createDbWindow.DatabasePath);
            }
        }

        private void OpenDatabase_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "SQLite Database Files (*.sqlite, *.db)|*.sqlite;*.db|All Files (*.*)|*.*",
                Title = "Open SQLite Database"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                LoadDatabaseStructure(openFileDialog.FileName);
            }
        }
        public void LoadDatabaseStructure(string databasePath = null)
        {
            if (string.IsNullOrEmpty(databasePath))
            {
                Dispatcher.Invoke(() =>
                {
                    List<string> loadedDBs = new List<string>();

                    foreach (TreeViewItem item in databaseTreeView.Items.OfType<TreeViewItem>())
                    {
                        loadedDBs.Add(item.Tag.ToString());
                    }
                    databaseTreeView.Items.Clear();

                    foreach (var db in loadedDBs)
                    {
                        LoadDatabaseStructure(db);
                    }
                });
                return;
            }

            foreach (TreeViewItem item in databaseTreeView.Items.OfType<TreeViewItem>())
            {
                if (item.Tag?.ToString() == databasePath)
                {
                    if (new StackTrace().GetFrames().Any(x => x.GetMethod().Name == "OpenDatabase_Click" || x.GetMethod().Name == "NewDatabase_Click"))
                    {
                        MessageBox.Show("This database is already open.", "Database Open", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                }
            }

            try
            {
                using (var connection = new SQLiteConnection($"Data Source={databasePath}"))
                {
                    connection.Open();

                    var tableNames = new List<string>();
                    using (var command = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table';", connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                tableNames.Add(reader.GetString(0));
                            }
                        }
                    }

                    var databaseNode = new TreeViewItemData
                    {
                        Name = Path.GetFileName(databasePath),
                        ImagePath = "Images/db.png",
                        Tag = databasePath
                    };

                    foreach (var tableName in tableNames)
                    {
                        var tableNode = new TreeViewItemData
                        {
                            Name = tableName,
                            ImagePath = "Images/table.png",
                            Tag = tableName
                        };
                        databaseNode.Children.Add(tableNode);
                    }
                    Dispatcher.Invoke(() =>
                    {
                        TreeViewItem existingDatabaseNode = null;
                        foreach (TreeViewItem item in databaseTreeView.Items.OfType<TreeViewItem>())
                        {
                            if (item.Tag?.ToString() == databasePath)
                            {
                                existingDatabaseNode = item;
                                break;
                            }
                        }
                        if (existingDatabaseNode == null)
                        {
                            existingDatabaseNode = new TreeViewItem();
                            existingDatabaseNode.Header = new StackPanel()
                            {
                                Orientation = Orientation.Horizontal,
                                Children =
                               {
                                   new Image()
                                   {
                                       Source = new System.Windows.Media.Imaging.BitmapImage(new Uri($"pack://application:,,,/{databaseNode.ImagePath}")),
                                       Width = 16, Height=16, Margin= new Thickness(0,0,5,0)
                                   },
                                   new TextBlock(){Text = databaseNode.Name}
                               }
                            };
                            existingDatabaseNode.Tag = databaseNode.Tag;

                            foreach (var child in databaseNode.Children)
                            {
                                TreeViewItem childTreeViewItem = new TreeViewItem
                                {
                                    Header = new StackPanel()
                                    {
                                        Orientation = Orientation.Horizontal,
                                        Children =
                                       {
                                            new Image()
                                            {
                                                Source = new System.Windows.Media.Imaging.BitmapImage(new Uri($"pack://application:,,,/{child.ImagePath}")),
                                                Width = 16, Height=16, Margin= new Thickness(0,0,5,0)
                                            },
                                           new TextBlock(){Text = child.Name}
                                       }
                                    },
                                    Tag = child.Tag
                                };
                                existingDatabaseNode.Items.Add(childTreeViewItem);
                            }
                            databaseTreeView.Items.Add(existingDatabaseNode);
                        }
                        else
                        {
                            existingDatabaseNode.Items.Clear();

                            foreach (var child in databaseNode.Children)
                            {
                                TreeViewItem childTreeViewItem = new TreeViewItem
                                {
                                    Header = new StackPanel()
                                    {
                                        Orientation = Orientation.Horizontal,
                                        Children =
                                       {
                                            new Image()
                                            {
                                                Source = new System.Windows.Media.Imaging.BitmapImage(new Uri($"pack://application:,,,/{child.ImagePath}")),
                                                Width = 16, Height=16, Margin= new Thickness(0,0,5,0)
                                            },
                                           new TextBlock(){Text = child.Name}
                                       }
                                    },
                                    Tag = child.Tag
                                };
                                existingDatabaseNode.Items.Add(childTreeViewItem);

                            }
                        }
                        existingDatabaseNode.IsExpanded = true;
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading database structure: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DatabaseTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

            if (databaseTreeView.SelectedItem is TreeViewItem selectedItem)
            {
                string databasePath = GetDatabasePathForTreeViewItem(selectedItem);
                if (string.IsNullOrEmpty(databasePath)) return;

                if (selectedItem.Tag is string tableName && !tableName.Contains(".db") && !tableName.Contains(".sqlite"))
                {
                    var editTableWindow = new EditTableWindow(databasePath, tableName);
                    editTableWindow.Unloaded += EditTableWindow_Unloaded;
                    contentArea.Content = editTableWindow;
                }
                else
                {
                    var sqlScriptWindow = new SqlScriptWindow(databasePath);
                    sqlScriptWindow.Unloaded += SqlScriptWindow_Unloaded;
                    contentArea.Content = sqlScriptWindow;
                }
            }
        }

        private void EditTableWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sender is EditTableWindow editTableWindow)
            {
                if (editTableWindow.IsDirty)
                {
                    MessageBoxResult result = MessageBox.Show($"Table '{editTableWindow._tableName}' has unsaved changes.  Do you want to save them?",
                        "Unsaved Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        editTableWindow.SaveChanges_Click(sender, e);
                    }
                    else if (result == MessageBoxResult.Cancel)
                    {
                        return;
                    }
                }
            }
        }

        private void SqlScriptWindow_Unloaded(object sender, RoutedEventArgs e)
        {
        }


        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void CreateTable_Click(object sender, RoutedEventArgs e)
        {
            string databasePath = GetDatabasePathForTreeViewItem(databaseTreeView.SelectedItem as TreeViewItem);
            if (string.IsNullOrEmpty(databasePath))
            {
                MessageBox.Show("Please select a database first.", "No Database", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var createTableWindow = new CreateTableWindow(databasePath);
            if (createTableWindow.ShowDialog() == true)
            {
                LoadDatabaseStructure(databasePath);
            }
        }

        private void DeleteTable_Click(object sender, RoutedEventArgs e)
        {
            string databasePath = GetDatabasePathForTreeViewItem(databaseTreeView.SelectedItem as TreeViewItem);
            if (string.IsNullOrEmpty(databasePath))
            {
                MessageBox.Show("Please select a database first.", "No Database", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (databaseTreeView.SelectedItem is TreeViewItem selectedItem && selectedItem.Tag is string tableName)
            {
                MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete the table '{tableName}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var connection = new SQLiteConnection($"Data Source={databasePath}"))
                        {
                            connection.Open();
                            using (var command = new SQLiteCommand($"DROP TABLE {tableName}", connection))
                            {
                                command.ExecuteNonQuery();
                            }
                        }
                        LoadDatabaseStructure(databasePath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting table: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a table to delete.", "No Table Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void EditTable_Click(object sender, RoutedEventArgs e)
        {
            string databasePath = GetDatabasePathForTreeViewItem(databaseTreeView.SelectedItem as TreeViewItem);
            if (string.IsNullOrEmpty(databasePath))
            {
                MessageBox.Show("Please select a database first.", "No Database", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (databaseTreeView.SelectedItem is TreeViewItem selectedItem && selectedItem.Tag is string tableName)
            {
                var editTableWindow = new EditTableWindow(databasePath, tableName);
                editTableWindow.Unloaded += EditTableWindow_Unloaded;
                contentArea.Content = editTableWindow;
            }
            else
            {
                MessageBox.Show("Please select a table to edit.", "No Table Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OpenScript_Click(object sender, RoutedEventArgs e)
        {
            string databasePath = GetDatabasePathForTreeViewItem(databaseTreeView.SelectedItem as TreeViewItem);
            if (string.IsNullOrEmpty(databasePath))
            {
                MessageBox.Show("Please select a database first.", "No Database", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sqlScriptWindow = new SqlScriptWindow(databasePath);
            sqlScriptWindow.Unloaded += SqlScriptWindow_Unloaded;
            contentArea.Content = sqlScriptWindow;
        }

        private void ImportText_Click(object sender, RoutedEventArgs e)
        {
            string databasePath = GetDatabasePathForTreeViewItem(databaseTreeView.SelectedItem as TreeViewItem);
            if (string.IsNullOrEmpty(databasePath))
            {
                MessageBox.Show("Please select a database first.", "No Database", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var importWindow = new ImportTextWindow();
            if (importWindow.ShowDialog() == true)
            {
                try
                {
                    using (var connection = new SQLiteConnection($"Data Source={databasePath}"))
                    {
                        connection.Open();
                        using (var transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                var lines = importWindow.TableText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                                if (lines.Length == 0)
                                {
                                    MessageBox.Show("No data to import.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    transaction.Rollback();
                                    return;
                                }

                                var headers = lines[0].Split(new[] { importWindow.Separator }, StringSplitOptions.None);

                                string createTableSql = $"CREATE TABLE {importWindow.TableName} (";

                                string primaryKeyColumn = "";

                                for (int i = 0; i < headers.Length; i++)
                                {
                                    string header = headers[i].Trim();
                                    if (string.IsNullOrEmpty(header))
                                    {
                                        header = $"Column{i + 1}";
                                    }

                                    int count = 0;
                                    string originalHeader = header;
                                    while (headers.Take(i).Any(h => h.Trim().Equals(header, StringComparison.OrdinalIgnoreCase)))
                                    {
                                        count++;
                                        header = $"{originalHeader}_{count}";
                                    }
                                    headers[i] = header;

                                    createTableSql += $"{header} TEXT";

                                    if (i == 0)
                                    {
                                        createTableSql += " PRIMARY KEY";
                                        primaryKeyColumn = header;
                                    }

                                    if (i < headers.Length - 1)
                                    {
                                        createTableSql += ", ";
                                    }
                                }
                                createTableSql += ");";

                                using (var command = new SQLiteCommand(createTableSql, connection, transaction))
                                {
                                    command.ExecuteNonQuery();
                                }

                                for (int i = 1; i < lines.Length; i++)
                                {
                                    var values = lines[i].Split(new[] { importWindow.Separator }, StringSplitOptions.None);
                                    string insertSql = $"INSERT INTO {importWindow.TableName} ({string.Join(", ", headers)}) VALUES (";
                                    for (int j = 0; j < values.Length; j++)
                                    {
                                        string value = values[j].Replace("'", "''");
                                        insertSql += $"'{value}'";
                                        if (j < values.Length - 1)
                                        {
                                            insertSql += ", ";
                                        }
                                    }

                                    for (int k = values.Length; k < headers.Length; k++)
                                    {
                                        insertSql += ", ''";
                                    }

                                    insertSql += ");";

                                    using (var command = new SQLiteCommand(insertSql, connection, transaction))
                                    {
                                        command.ExecuteNonQuery();
                                    }
                                }

                                transaction.Commit();
                                LoadDatabaseStructure(databasePath);
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                MessageBox.Show($"Error importing data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening database connection: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private string GetDatabasePathForTreeViewItem(TreeViewItem item)
        {
            if (item == null) return null;

            while (item.Parent is TreeViewItem)
            {
                item = (TreeViewItem)item.Parent;
            }
            return item.Tag as string;
        }

        private void CloseDatabase_Click(object sender, RoutedEventArgs e)
        {
            if (!(databaseTreeView.SelectedItem is TreeViewItem selectedItem))
            {
                return;
            }

            string databasePath = GetDatabasePathForTreeViewItem(selectedItem);
            if (string.IsNullOrEmpty(databasePath))
            {
                return;
            }

            if (contentArea.Content is EditTableWindow editTableWindow && editTableWindow._databasePath == databasePath)
            {
                contentArea.Content = null;
            }
            else if (contentArea.Content is SqlScriptWindow sqlScriptWindow && sqlScriptWindow._databasePath == databasePath)
            {
                contentArea.Content = null;
            }

            TreeViewItem databaseNode = null;
            foreach (TreeViewItem item in databaseTreeView.Items.OfType<TreeViewItem>())
            {
                if (item.Tag?.ToString() == databasePath)
                {
                    databaseNode = item;
                    break;
                }
            }

            if (databaseNode != null)
            {
                Dispatcher.Invoke(() => databaseTreeView.Items.Remove(databaseNode));
            }
            LoadDatabaseStructure();
        }
        private void databaseTreeView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (databaseTreeView.SelectedItem is TreeViewItem selectedItem)
            {
                string databasePath = GetDatabasePathForTreeViewItem(selectedItem);
                bool isDatabaseNode = !string.IsNullOrEmpty(databasePath) &&
                                        (selectedItem.Tag is string tag) &&
                                        (tag.EndsWith(".db", StringComparison.OrdinalIgnoreCase) || tag.EndsWith(".sqlite", StringComparison.OrdinalIgnoreCase));

                foreach (MenuItem menuItem in databaseTreeView.ContextMenu.Items)
                {
                    if (menuItem.Header.ToString() == "Close Database")
                    {
                        menuItem.Visibility = isDatabaseNode ? Visibility.Visible : Visibility.Collapsed;
                    }
                    else
                    {
                        menuItem.Visibility = isDatabaseNode ? Visibility.Collapsed : Visibility.Visible;
                    }
                }
            }
            else
            {
                foreach (MenuItem menuItem in databaseTreeView.ContextMenu.Items)
                {
                    menuItem.Visibility = Visibility.Collapsed;
                }
            }
        }
    }

    public class TreeViewItemData
    {
        public string Name { get; set; }
        public string ImagePath { get; set; }
        public string Tag { get; set; }
        public ObservableCollection<TreeViewItemData> Children { get; set; } = new ObservableCollection<TreeViewItemData>();
    }
}