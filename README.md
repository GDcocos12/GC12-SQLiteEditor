# GC12-SQLiteEditor

GC12-SQLiteEditor is a simple, user-friendly SQLite database editor and creator built with C# and WPF.  It allows you to create, open, and manage multiple SQLite databases, create and edit tables, execute SQL scripts, and import data from text files. This project is ideal for learning purposes, small projects, or anyone who needs a lightweight tool to interact with SQLite databases.

## Features

*   **Create and Open Databases:** Easily create new SQLite databases or open existing ones.
*   **Multiple Database Support:**  Work with multiple SQLite databases simultaneously. Each database is displayed as a separate root node in a tree view.
*   **Table Management:**
    *   Create tables with a graphical interface, specifying column names, data types, primary keys, and NOT NULL constraints.
    *   Delete tables.
    *   Edit table data (add, delete, and modify rows) using a data grid.
    *  Import table data from delimited text files (e.g., CSV).  Specify the delimiter (default is ';').  Handles different numbers of columns and values, empty values, and quoted strings. The first column is automatically set as the primary key.
*   **SQL Script Execution:**
    *   Execute arbitrary SQL scripts against the selected database.
    *   View results in either a data grid or a text box.
    *   Save and load SQL scripts from files.
    *   Syntax highlighting (planned, not yet implemented).
*   **User Interface:**
    *   Tree view to display the database structure (databases and tables).
    *   Context menu for common actions (create table, delete table, edit table, open script, import text, close database).
    *   Toolbar with buttons for common actions.
    *   Status bar to display messages.
*  **Error Handling:** Includes robust error handling for common database operations, SQL syntax errors, and file operations.
*  **Close Database Functionality:** Close individual databases, which automatically hides associated Edit and Script windows.

## Prerequisites

*   Visual Studio (2019 or later recommended).
*   .NET Framework (4.7.2 or later) or .NET (6.0 or later) - the project is currently configured for .NET Framework, but can be easily migrated.
*  System.Data.SQLite NuGet package.
* Basic understanding of SQL and SQLite.

## Getting Started

1.  **Clone the repository:**

    ```bash
    git clone https://github.com/GDcocos12/GC12-SQLiteEditor.git
    ```

2.  **Open the solution (`GC12_SQLiteGUI.sln`) in Visual Studio.**

3.  **Build the solution (Build -> Build Solution).**  This will restore the necessary NuGet packages (primarily `System.Data.SQLite`).

4.  **Run the application (Debug -> Start Debugging or press F5).**

## Usage

1.  **Create a new database:** Click `File` -> `New Database`.  Choose a file name and location.
2.  **Open an existing database:** Click `File` -> `Open Database`.  Select the `.sqlite` or `.db` file.
3.  **The database structure will be displayed in the tree view on the left.**  The root node represents the database, and child nodes represent tables.
4.  **Create Table:** Select database, click "Create Table" button or right-click and select this option in context menu.
5.  **Delete Table:** Select table in TreeView, click "Delete Table" button or right-click and select this option in context menu.
6.  **Edit Table:** Select table in TreeView and click "Edit Table" button or double-click on it.
7.  **To execute SQL scripts:**  Select a database in the tree view (or click the "Open Script" button/menu item after selecting a database), enter your SQL script in the script editor, and click "Execute".
8.  **To import data from a text file:** Select database in the tree view, click the "Import Text" button (or right-click and choose "Import Text"), enter the table name, separator, and the text data, then click "Import".

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Future Enhancements

*   **Syntax highlighting for the SQL script editor.**
*   **Query history.** (Medium priority)
*   **Table renaming.**
*   **Column editing (add, delete, modify).**
*   **Data export (CSV, Excel, etc.).**
