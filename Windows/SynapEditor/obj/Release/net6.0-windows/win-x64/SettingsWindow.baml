   M S B A M L   `   `   `  ����   SynapEditor     SynapEditor.SettingsWindow   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml       � Title$   Settings and Customization��$	��400��$	��500�� � WindowStartupLocation$ CenterScreen=� � 
ResizeMode$ NoResize=�.��� $��20q� � RowDefinitions �� $
��Auto ��� $��* ��� $
��Auto �.+��� $ ��Settings and Customization��$��18(�$
��Bold"�$��0��$��0,0,0,15q��� $��1��.@��� $��
Appearance��.���� $��15q�.+��� $��Theme��$��0,0,0,10q�.���� $��10q�.+�	� -    	�Name$ LightThemeRadio��$��Light Theme�� d� 	IsChecked$
 TrueY�$	��0,5q�	� -   $ DarkThemeRadio��$��
Dark Theme��$	��0,5q��� $
��Font��$��0,0,0,10q�.��� $��10q� � ColumnDefinitions �� $
��Auto ��� $��* � �� $
��Auto ��� $
��Auto �.+��� $��
Font Size:��$��0��$��0��$��Center=�$��0,0,10,0q��� -    ��Name$ FontSizeCombo��$��0��$��1��$	��0,5q�.@��� $��Small (10pt)��
 � Tag$ 10���� $��Medium (12pt)��$ 12��	 �� 
IsSelected$
	 True���� $��Large (14pt)��$ 14���� $��Extra Large (16pt)��$ 16���� $��Font Family:��$��1��$��0��$��Center=�$��	0,10,10,0q��� -   $ FontFamilyCombo��$��1��$��1��$��0,10,0,5q�.@��� $��Consolas (Monospace)��$ Consolas��$
	 True���� $��Segoe UI (Sans-serif)��$ Segoe UI���� $��Times New Roman (Serif)��$ Times New Roman���� $��Courier New (Monospace)��$ Courier New���� $��
Encryption��.���� $��15q�.+��� $��Encryption Key Management��$
��Bold"�$��0,0,0,10q��� $
��Wrap=�$��0,0,0,15q�[YYour files are secured with AES-256 encryption. You can manage your encryption keys here.�� -   
 ��Name$
 GenerateKeyButton��$!��Generate New Encryption Key��$
��10,5q�$	��0,5q��� -   $
 ViewKeyButton��$+��%View Current Key (Password Protected)��$
��10,5q�$	��0,5q��� -   $
 ExportKeyButton��$��Export Current Key��$
��10,5q�$	��0,5q��� -   $
 ImportKeyButton��$��Import Key File��$
��10,5q�$	��0,5q��� ���	#FFAAAAAA$��1q�$��0,15,0,0q� �� Padding$ 10q����	#FFEEEEEE._��� $
��Wrap=�$��Italic$���Warning: Generating a new key or importing a different key will make previously encrypted files unreadable unless you keep a backup of the original key.�� $��About��.���� $��15q�.+��� $��Synap Editor��$��20(�$
��Bold"��� $0��*Secure Text Editor with AES-256 Encryption��$��0,5,0,15q��� $
��Wrap=�$��0,0,0,10q���Synap Editor is a secure text editor that encrypts your files using AES-256 encryption. All files are stored with strong encryption to protect your sensitive information.�� $
��Wrap=�$��0,0,0,15q���Features: • AES-256 encryption for all files • Text formatting options (bold, italic) • Customizable appearance • Portable encryption key management�� $��© Synap Editor��$
��Bold"��� $��2�� �� Orientation$ 
Horizontal=�$��Right=�$��0,15,0,0q�.+��� -	   $
 ApplyButton��$��Apply��$��80��$��0,0,10,0q��� -
   $
 CloseButton��$��Close��$��80��