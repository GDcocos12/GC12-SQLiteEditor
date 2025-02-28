namespace GC12_SQLiteGUI.Models
{
    public class ColumnDefinition
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsNotNull { get; set; }

    }
}