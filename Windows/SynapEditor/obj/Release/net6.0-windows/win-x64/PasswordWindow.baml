   M S B A M L   `   `   `  ����   SynapEditor     SynapEditor.PasswordWindow   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml       � Title$  Password Required��$	��220��$	��400�� � WindowStartupLocation$ CenterOwner=� � 
ResizeMode$ NoResize=�.��� $��20q� � RowDefinitions �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $��* ��� $
��Auto �.+��� -    ��Name$ 
HeaderText��$��0��$��Enter Password��$��16(�$
��Bold"�$��0,0,0,15q��� -   $ DescriptionText��$��1��$P��JThis action is password protected. Please enter your password to continue.��$
��Wrap=�$��0,0,0,15q�� $��2�� � ColumnDefinitions �� $
��Auto ��� $��* �.+��� -   $ Password1Label��$��0��$��	Password:��$��Center=�$��0,0,10,0q�G� -    G�Name$ PasswordBox1��$��1��$	��0,5q�$	��5,3q�� -    �Name$ ConfirmPasswordGrid��$��3��$y�	Collapsed=� �� $
��Auto ��� $��* �.+��� $��0��$��Confirm:��$��Center=�$��0,0,10,0q�G� -   $ PasswordBox2��$��1��$	��0,5q�$	��5,3q��� $��5�� �� Orientation$ 
Horizontal=�$��Right=�$��0,15,0,0q�.+��� -   	 ��Name$	 OkButton��$��OK��$��80��$��0,0,10,0q�
 �� 	IsDefault$

 True���� -   $	 CancelButton��$��Cancel��$��80�� �� IsCancel$
 True��