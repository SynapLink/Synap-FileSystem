   M S B A M L   `   `   `  ����   SynapEditor    SynapEditor.MainWindow   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml       � Title$  Synap Editor��$	��600��$	��800�� � WindowStartupLocation$ CenterScreen=�.���  � RowDefinitions �� $
��Auto ��� $��* ��� $
��Auto �.+�c� $��0��.@��� -    ��Name$ 
MenuButton��$��☰ Menu��$��80��$��2q� � ToolTip$ Show start menu���� �� -   $ 
BoldButton��$
��Bold��$��60��$��2q�$ Wrap text with **bold**���� -   $ ItalicButton��$��Italic��$��60��$��2q�$! Wrap text with ***italic***���� -   $ BoldItalicButton��$��Bold+Italic��$��80��$��2q�$( "Wrap text with ****bold italic****���� �� $��Format: ��$��Center=�$	��5,0q��� -    ��Name$ 
FormatHelp��$	��120�� �� 
IsReadOnly$
 True�� �� SelectedIndex$ 0��.@��� $��**Bold**���� $��***Italic***���� $��****Bold Italic****���� �� -   $ SettingsButton��$��⚙️ Settings��$��80��$��2q�$ Open settings���� -    ��Name$ EditorTextBox��$��1��	 �� AcceptsReturn$
	 True��
 �� 
AcceptsTab$

 True�� �� TextWrapping$
 Wrap=�" �� VerticalScrollBarVisibility$
 Auto=�$ �� HorizontalScrollBarVisibility$
 Auto=�$��Consolas)�$��10q�� $��2��$��10q� � ColumnDefinitions �� $��* ��� $
��Auto ��� $��* �.+��� $��0�� �� Orientation$ 
Horizontal=�$
��Left=�.+��� -   $ 	NewButton��$��New File��$	��100��$��30��$��5q��� $��1��$ 
Horizontal=�$��Center=�.+��� -	   $ 
OpenButton��$
��Open��$	��100��$��30��$��5q��� -
   $ 
SaveButton��$
��Save��$	��100��$��30��$��5q��� $��2��$ 
Horizontal=�$��Right=�.+��� -   $ ImportKeyButton��$��
Import Key��$	��100��$��30��$��5q��� -   $ GenerateKeyButton��$��Generate New Key��$	��120��$��30��$��5q�