using System;
using System.Windows;

namespace SynapEditor
{
    public partial class KeyViewWindow : Window
    {
        public KeyViewWindow(byte[] key)
        {
            InitializeComponent();
            
            // Convert the key to different formats for display
            DisplayKey(key);
        }
        
        private void DisplayKey(byte[] key)
        {
            // Display in hexadecimal format
            HexKeyTextBox.Text = BitConverter.ToString(key).Replace("-", " ");
            
            // Display in Base64 format
            Base64KeyTextBox.Text = Convert.ToBase64String(key);
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Copy the Base64 representation to clipboard
                Clipboard.SetText(Base64KeyTextBox.Text);
                MessageBox.Show("Key copied to clipboard. Please be careful with this sensitive information.", 
                    "Key Copied", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error copying to clipboard: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
} 