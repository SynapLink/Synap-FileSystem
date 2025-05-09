using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Security.Cryptography;
using System.Text;

namespace SynapEditor
{
    public partial class SettingsWindow : Window
    {
        private readonly string _keyFilePath;
        private byte[] _key;
        private readonly string _settingsFilePath;
        private readonly string _passwordHashFilePath;
        private bool _passwordSet = false;
        
        public bool SettingsChanged { get; private set; } = false;
        
        public SettingsWindow()
        {
            InitializeComponent();
            
            // Set up paths
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            _keyFilePath = Path.Combine(appDir, "synap_editor.key");
            _settingsFilePath = Path.Combine(appDir, "synap_settings.xml");
            _passwordHashFilePath = Path.Combine(appDir, "synap_security.dat");
            
            // Load the encryption key
            LoadKey();
            
            // Check if password is set
            _passwordSet = File.Exists(_passwordHashFilePath);
            
            // Load saved settings if they exist
            LoadSettings();
        }
        
        private void LoadKey()
        {
            if (File.Exists(_keyFilePath))
            {
                _key = File.ReadAllBytes(_keyFilePath);
            }
            else
            {
                // If the key doesn't exist, we'll let MainWindow create it
            }
        }
        
        private void LoadSettings()
        {
            if (File.Exists(_settingsFilePath))
            {
                try
                {
                    // Simple settings file format
                    string[] settings = File.ReadAllLines(_settingsFilePath);
                    
                    foreach (string line in settings)
                    {
                        string[] parts = line.Split('=');
                        if (parts.Length == 2)
                        {
                            string key = parts[0].Trim();
                            string value = parts[1].Trim();
                            
                            switch (key)
                            {
                                case "Theme":
                                    if (value == "Dark")
                                    {
                                        DarkThemeRadio.IsChecked = true;
                                    }
                                    else
                                    {
                                        LightThemeRadio.IsChecked = true;
                                    }
                                    break;
                                
                                case "FontSize":
                                    foreach (ComboBoxItem item in FontSizeCombo.Items)
                                    {
                                        if (item.Tag.ToString() == value)
                                        {
                                            FontSizeCombo.SelectedItem = item;
                                            break;
                                        }
                                    }
                                    break;
                                
                                case "FontFamily":
                                    foreach (ComboBoxItem item in FontFamilyCombo.Items)
                                    {
                                        if (item.Tag.ToString() == value)
                                        {
                                            FontFamilyCombo.SelectedItem = item;
                                            break;
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        private void SaveSettings()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(_settingsFilePath))
                {
                    // Theme
                    string theme = DarkThemeRadio.IsChecked == true ? "Dark" : "Light";
                    writer.WriteLine($"Theme={theme}");
                    
                    // Font Size
                    ComboBoxItem fontSizeItem = FontSizeCombo.SelectedItem as ComboBoxItem;
                    string fontSize = fontSizeItem?.Tag.ToString() ?? "12";
                    writer.WriteLine($"FontSize={fontSize}");
                    
                    // Font Family
                    ComboBoxItem fontFamilyItem = FontFamilyCombo.SelectedItem as ComboBoxItem;
                    string fontFamily = fontFamilyItem?.Tag.ToString() ?? "Consolas";
                    writer.WriteLine($"FontFamily={fontFamily}");
                }
                
                SettingsChanged = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            MessageBox.Show("Settings have been saved and will be applied the next time you open the editor.", 
                "Settings Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        
        private void GenerateKeyButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "WARNING: Generating a new key will make all previously encrypted files unreadable unless you've backed up the current key.\n\n" +
                "Are you sure you want to continue?", 
                "Generate New Key", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Warning);
                
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Backup the old key first if it exists
                    if (File.Exists(_keyFilePath))
                    {
                        string backupPath = _keyFilePath + ".backup";
                        File.Copy(_keyFilePath, backupPath, true);
                    }
                    
                    // We'll let the MainWindow generate a new key on next startup
                    if (File.Exists(_keyFilePath))
                    {
                        File.Delete(_keyFilePath);
                    }
                    
                    MessageBox.Show("New encryption key will be generated the next time you start the application.",
                        "Key Generation", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error generating new key: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        private void ViewKeyButton_Click(object sender, RoutedEventArgs e)
        {
            // First, make sure we have a key to view
            if (_key == null || _key.Length == 0)
            {
                MessageBox.Show("No encryption key is currently available. Please restart the application to generate a key.",
                    "No Key Available", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            // Check if the password is set
            if (!_passwordSet)
            {
                // First time - ask to set a password
                PasswordWindow pwdWindow = new PasswordWindow(PasswordWindow.PasswordMode.Set);
                pwdWindow.Owner = this;
                
                if (pwdWindow.ShowDialog() == true)
                {
                    string password = pwdWindow.Password;
                    
                    // Hash and save the password
                    if (SavePasswordHash(password))
                    {
                        _passwordSet = true;
                        
                        // Now show the key
                        ShowKey();
                    }
                }
            }
            else
            {
                // Verify the password
                PasswordWindow pwdWindow = new PasswordWindow(PasswordWindow.PasswordMode.Verify);
                pwdWindow.Owner = this;
                
                if (pwdWindow.ShowDialog() == true)
                {
                    string password = pwdWindow.Password;
                    
                    // Verify the password
                    if (VerifyPassword(password))
                    {
                        // Password is correct, show the key
                        ShowKey();
                    }
                    else
                    {
                        MessageBox.Show("Incorrect password. Access denied.", 
                            "Authentication Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        
        private bool SavePasswordHash(string password)
        {
            try
            {
                // Generate a random salt
                byte[] salt = new byte[16];
                using (var rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(salt);
                }
                
                // Hash the password with the salt
                byte[] hash = HashPassword(password, salt);
                
                // Combine salt and hash
                byte[] hashWithSalt = new byte[salt.Length + hash.Length];
                Array.Copy(salt, 0, hashWithSalt, 0, salt.Length);
                Array.Copy(hash, 0, hashWithSalt, salt.Length, hash.Length);
                
                // Save to file
                File.WriteAllBytes(_passwordHashFilePath, hashWithSalt);
                
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving password: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        
        private bool VerifyPassword(string password)
        {
            try
            {
                // Read the stored hash with salt
                byte[] hashWithSalt = File.ReadAllBytes(_passwordHashFilePath);
                
                // Extract the salt (first 16 bytes)
                byte[] salt = new byte[16];
                Array.Copy(hashWithSalt, 0, salt, 0, 16);
                
                // Extract the original hash
                byte[] originalHash = new byte[hashWithSalt.Length - 16];
                Array.Copy(hashWithSalt, 16, originalHash, 0, originalHash.Length);
                
                // Hash the provided password with the same salt
                byte[] newHash = HashPassword(password, salt);
                
                // Compare the hashes
                if (originalHash.Length != newHash.Length)
                {
                    return false;
                }
                
                for (int i = 0; i < originalHash.Length; i++)
                {
                    if (originalHash[i] != newHash[i])
                    {
                        return false;
                    }
                }
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        private byte[] HashPassword(string password, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000))
            {
                return pbkdf2.GetBytes(32); // 256-bit hash
            }
        }
        
        private void ShowKey()
        {
            // Display the key in a secure window
            KeyViewWindow keyWindow = new KeyViewWindow(_key);
            keyWindow.Owner = this;
            keyWindow.ShowDialog();
        }
        
        private void ExportKeyButton_Click(object sender, RoutedEventArgs e)
        {
            // First, make sure we have a key to export
            if (_key == null || _key.Length == 0)
            {
                MessageBox.Show("No encryption key is currently available. Please restart the application to generate a key.",
                    "No Key Available", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            // Check if the password is set
            if (_passwordSet)
            {
                // Verify the password before exporting
                PasswordWindow pwdWindow = new PasswordWindow(PasswordWindow.PasswordMode.Verify);
                pwdWindow.Owner = this;
                
                if (pwdWindow.ShowDialog() != true || !VerifyPassword(pwdWindow.Password))
                {
                    MessageBox.Show("Incorrect password. Export canceled.", 
                        "Authentication Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            
            // If no password set or password verified, proceed with export
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Export Encryption Key",
                Filter = "Key Files (*.key)|*.key|All files (*.*)|*.*",
                DefaultExt = ".key",
                FileName = "synap_export.key"
            };
            
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Export the key directly from memory
                    File.WriteAllBytes(saveFileDialog.FileName, _key);
                    MessageBox.Show("Encryption key exported successfully.\n\nWARNING: Keep this key secure! Anyone with this key can decrypt your files.", 
                        "Key Exported", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting key: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        private void ImportKeyButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Import Encryption Key",
                Filter = "Key Files (*.key)|*.key|All files (*.*)|*.*",
                DefaultExt = ".key"
            };
            
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Verify key is valid (32 bytes for AES-256)
                    byte[] importedKey = File.ReadAllBytes(openFileDialog.FileName);
                    
                    if (importedKey.Length != 32)
                    {
                        MessageBox.Show("The selected file is not a valid encryption key. It must be exactly 32 bytes in length.",
                            "Invalid Key", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    
                    MessageBoxResult result = MessageBox.Show(
                        "WARNING: Importing a new key will make all previously encrypted files unreadable with the current key.\n\n" +
                        "Are you sure you want to import this key?", 
                        "Import Key", 
                        MessageBoxButton.YesNo, 
                        MessageBoxImage.Warning);
                        
                    if (result == MessageBoxResult.Yes)
                    {
                        // Backup the old key first if it exists
                        if (File.Exists(_keyFilePath))
                        {
                            string backupPath = _keyFilePath + ".backup";
                            File.Copy(_keyFilePath, backupPath, true);
                        }
                        
                        // Save the imported key
                        File.WriteAllBytes(_keyFilePath, importedKey);
                        _key = importedKey;
                        
                        MessageBox.Show("Encryption key imported successfully.\n\nNOTE: Files encrypted with the previous key will no longer be readable unless you restore the backup key.", 
                            "Key Imported", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error importing key: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
} 