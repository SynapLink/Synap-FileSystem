   M S B A M L   `   `   `  ����   SynapEditor    SynapEditor.KeyViewWindow   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml       � Title$  Encryption Key��$	��300��$	��500�� � WindowStartupLocation$ CenterOwner=� � 
ResizeMode$ NoResize=�.��� $��20q� � RowDefinitions �� $
��Auto ��� $
��Auto ��� $��* ��� $
��Auto �.+��� $��0��$��Your Encryption Key��$��16(�$
��Bold"�$��0,0,0,10q��� $��1��$
��Wrap=����#C00000$��SemiBold"�$��0,0,0,15q�omWARNING: This key allows anyone to decrypt your files. Never share it with anyone you don't trust completely.�� $��2�����#CCCCCC$��1q����#F8F8F8 �� Padding$ 10q�._��  �� $
��Auto ��� $��* �.+��� $��0��$��0,0,0,10q�.+��� $��Key (Hexadecimal Format):��$
��Bold"�$��0,0,0,5q��� -    ��Name$ HexKeyTextBox�� �� 
IsReadOnly$
 True��$��Consolas)�$��5q� �� TextWrapping$
 Wrap=��� $��1��.+��� $��Key (Base64 Format):��$
��Bold"�$��0,0,0,5q��� -   $ Base64KeyTextBox��$
 True��$��Consolas)�$��5q�$
 Wrap=��� $��3�� �� Orientation$ 
Horizontal=�$��Right=�$��0,15,0,0q�.+��� -   	 ��Name$	 
CopyButton��$��Copy to Clipboard��$	��120��$��0,0,10,0q��� -   $	 CloseButton��$��Close��$��80��