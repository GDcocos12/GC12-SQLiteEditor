   M S B A M L   `   `   `  ����   GC12_SQLiteGUI&     GC12_SQLiteGUI.CreateTableWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml       � Title$  Create New Table��$	��400��$	��400��.��]� .+��� $��Bottom=� �� Orientation$ 
Horizontal=�$��Right=�$��0,10,0,0q�.+��� -   $��Create��$��5q�$
��10,2q� �� 	IsDefault . �� -   $��Cancel��$��5q�$
��10,2q� �� IsCancel . � $��10q� � RowDefinitions �� $
��Auto ��� $��* �.+��� $��0��$��0,0,0,10q�.+��� $��Table Name:���� -   $��tableNameTextBox��$��0,2,0,10q�� $��1�� �� $
��Auto ��� $��* �.+��� $��0��$��Columns:��$
��Bold"��� -   $��columnsListView��$��1��$��0,5,0,0q� �� View �� .P��� $
��Name�� �� Width$	 120�� �� CellTemplate �� .R��� ���� *Name+�� $��	Data Type��$	 100�� �� .R���  �� SelectedIndex$ 0��	 �� SelectedValuePath$	 Content��.@��� $��INTEGER���� $
��TEXT���� $
��REAL���� $
��BLOB���� $��NUMERIC��
 �� SelectedValue
 ��  �� Path$ DataType� �� Mode$ TwoWay=� �� UpdateSourceTrigger$ PropertyChanged=��� $��Primary Key��$ 80�� �� .R���  d� 	IsChecked �� *IsPrimaryKey+�� $��Not Null��$ 70�� �� .R���  �� *	IsNotNull+ � ContextMenu �� .@�m� -   $��
Add Column��m� -   $��Delete Column��