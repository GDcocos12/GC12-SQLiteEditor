using Microsoft.Win32;
using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GC12_SQLiteGUI
{
    public partial class SqlScriptWindow : UserControl
    {
        public readonly string _databasePath;
        private bool _scriptTextChanged = false;

        public SqlScriptWindow(string databasePath)
        {
            InitializeComponent();
            _databasePath = databasePath;

            resultTypeComboBox.DataContext = this;
        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_databasePath))
            {
                MessageBox.Show("Please open or create a database first.", "No Database", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(scriptTextBox.Text))
            {
                MessageBox.Show("Please enter a SQL script.", "Empty Script", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var connection = new SQLiteConnection($"Data Source={_databasePath}"))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(scriptTextBox.Text, connection))
                    {
                        if (resultTypeComboBox.Text == "Grid")
                        {
                            try
                            {
                                using (var adapter = new SQLiteDataAdapter(command))
                                {
                                    var dataTable = new DataTable();
                                    adapter.Fill(dataTable);

                                    var dataGrid = new DataGrid();
                                    dataGrid.ItemsSource = dataTable.DefaultView;
                                    dataGrid.AutoGenerateColumns = true;
                                    dataGrid.IsReadOnly = true;
                                    resultContentControl.Content = dataGrid;
                                }
                            }
                            catch (Exception ex)
                            {
                                ShowErrorInTextBox(ex);
                            }
                        }
                        else
                        {
                            try
                            {
                                using (var reader = command.ExecuteReader())
                                {
                                    var stringBuilder = new System.Text.StringBuilder();
                                    while (reader.Read())
                                    {
                                        for (int i = 0; i < reader.FieldCount; i++)
                                        {
                                            stringBuilder.Append(reader[i].ToString());
                                            if (i < reader.FieldCount - 1)
                                            {
                                                stringBuilder.Append(", ");
                                            }
                                        }
                                        stringBuilder.AppendLine();
                                    }
                                    var textBox = new TextBox();
                                    textBox.Text = stringBuilder.ToString();
                                    textBox.IsReadOnly = true;
                                    textBox.FontFamily = new FontFamily("Consolas");
                                    textBox.FontSize = 12;
                                    textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

                                    resultContentControl.Content = textBox;
                                }
                            }
                            catch (Exception ex)
                            {
                                ShowErrorInTextBox(ex);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorInTextBox(ex);
            }
        }
        private void ShowErrorInTextBox(Exception ex)
        {
            var errorTextBox = new TextBox();
            errorTextBox.Text = $"Error: {ex.Message}\n\nDetails:\n{ex}";
            errorTextBox.IsReadOnly = true;
            errorTextBox.Foreground = Brushes.Red;
            errorTextBox.FontFamily = new FontFamily("Consolas");
            errorTextBox.FontSize = 12;
            errorTextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            resultContentControl.Content = errorTextBox;
        }

        private void SaveScriptButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "SQL Script Files (*.sql)|*.sql|All Files (*.*)|*.*",
                Title = "Save SQL Script"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, scriptTextBox.Text);
                    _scriptTextChanged = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving script: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadScriptButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "SQL Script Files (*.sql)|*.sql|All Files (*.*)|*.*",
                Title = "Load SQL Script"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    if (_scriptTextChanged)
                    {
                        MessageBoxResult result = MessageBox.Show("Current script has unsaved changes.  Do you want to save it before loading a new script?",
                            "Unsaved Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                        if (result == MessageBoxResult.Yes)
                        {
                            SaveScriptButton_Click(this, new RoutedEventArgs());
                            if (_scriptTextChanged) return;
                        }
                        else if (result == MessageBoxResult.Cancel)
                        {
                            return;
                        }
                    }

                    scriptTextBox.Text = File.ReadAllText(openFileDialog.FileName);
                    _scriptTextChanged = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading script: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ScriptTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _scriptTextChanged = true;
        }
    }
}