using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace SynapEditor
{
    public partial class MainWindow : Window
    {
        private readonly string _keyFilePath;
        private byte[] _key;
        private string? _currentFilePath;
        private const string FILE_EXTENSION = ".winsynap"; // windows specific ext
        private readonly string _settingsFilePath;
        private static bool _startMenuShown = false;
        
        public MainWindow()
        {
            InitializeComponent();
            
            // key stored with exe
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            _keyFilePath = Path.Combine(appDir, "synap_editor.key");
            _settingsFilePath = Path.Combine(appDir, "synap_settings.xml");
            
            // get the key
            _key = LoadKey();
            
            // create new empty file
            ClearEditor();
            
            // load settings if exist
            ApplySettings();
            
            // show start menu if first run and not specific purpose
            if (!_startMenuShown)
            {
                _startMenuShown = true; // prevent multiple menus
                ShowStartMenu();
            }
        }
        
        private void ApplySettings()
        {
            if (File.Exists(_settingsFilePath))
            {
                try
                {
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
                                    ApplyTheme(value);
                                    break;
                                case "FontSize":
                                    if (double.TryParse(value, out double fontSize))
                                    {
                                        EditorTextBox.FontSize = fontSize;
                                    }
                                    break;
                                case "FontFamily":
                                    EditorTextBox.FontFamily = new System.Windows.Media.FontFamily(value);
                                    break;
                            }
                        }
                    }
                }
                catch
                {
                    // If settings can't be loaded, just use defaults
                }
            }
        }
        
        private void ApplyTheme(string theme)
        {
            if (theme == "Dark")
            {
                // Apply dark theme
                Background = System.Windows.Media.Brushes.DarkGray;
                EditorTextBox.Background = System.Windows.Media.Brushes.Black;
                EditorTextBox.Foreground = System.Windows.Media.Brushes.White;
            }
            else
            {
                // Apply light theme (default)
                Background = System.Windows.Media.Brushes.White;
                EditorTextBox.Background = System.Windows.Media.Brushes.White;
                EditorTextBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }
        
        private void ShowStartMenu()
        {
            StartMenu startMenu = new StartMenu();
            this.Hide(); // Hide main window
            
            startMenu.ShowDialog(); // Show start menu as dialog
            
            // If the main window is closed when the start menu is shown
            if (!startMenu.IsVisible)
            {
                this.Close();
            }
        }
        
        public void OpenFileFromStartMenu()
        {
            this.Loaded += (s, e) => 
            {
                OpenButton_Click(this, new RoutedEventArgs());
            };
        }

        private void ClearEditor()
        {
            EditorTextBox.Text = "";
            _currentFilePath = null;
            Title = "Synap Editor - New File";
        }

        private byte[] LoadKey()
        {
            if (!File.Exists(_keyFilePath))
            {
                // generate a new 32-byte (256-bit) key
                byte[] key = GenerateNewKey();
                SaveKey(key);
                return key;
            }
            
            return File.ReadAllBytes(_keyFilePath);
        }
        
        private byte[] GenerateNewKey()
        {
            byte[] key = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }
            return key;
        }
        
        private void SaveKey(byte[] key)
        {
            File.WriteAllBytes(_keyFilePath, key);
        }

        private string Encrypt(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = _key;
                aes.GenerateIV(); // generate a random IV for each encryption
                
                using (MemoryStream ms = new MemoryStream())
                {
                    // write the IV to the output
                    ms.Write(aes.IV, 0, aes.IV.Length);
                    
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                    }
                    
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        private string Decrypt(string cipherText)
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            
            using (Aes aes = Aes.Create())
            {
                aes.Key = _key;
                
                // get the IV from the cipher bytes (first 16 bytes)
                byte[] iv = new byte[16];
                Array.Copy(cipherBytes, 0, iv, 0, iv.Length);
                aes.IV = iv;
                
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        // skip the IV in the input
                        cs.Write(cipherBytes, iv.Length, cipherBytes.Length - iv.Length);
                    }
                    
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }
        
        // new file button handler
        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(EditorTextBox.Text))
            {
                MessageBoxResult result = MessageBox.Show("Do you want to save the current file?", 
                    "Save File", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    SaveFile();
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }
            
            ClearEditor();
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = $"Synap Windows Files (*{FILE_EXTENSION})|*{FILE_EXTENSION}|All files (*.*)|*.*",
                DefaultExt = FILE_EXTENSION
            };
            
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string encryptedContent = File.ReadAllText(openFileDialog.FileName);
                    string decryptedContent = Decrypt(encryptedContent);
                    EditorTextBox.Text = decryptedContent;
                    _currentFilePath = openFileDialog.FileName;
                    Title = $"Synap Editor - {Path.GetFileName(openFileDialog.FileName)}";
                    
                    // Add to recent files list in start menu
                    UpdateRecentFiles(_currentFilePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        private void UpdateRecentFiles(string filePath)
        {
            // We don't need to do anything if filePath is null
            if (string.IsNullOrEmpty(filePath))
                return;
                
            // Read the current recent files list
            string recentFilesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "recent_files.txt");
            List<string> recentFiles = new List<string>();
            
            if (File.Exists(recentFilesPath))
            {
                try
                {
                    recentFiles = new List<string>(File.ReadAllLines(recentFilesPath));
                    
                    // Remove the current file if it's already in the list
                    recentFiles.Remove(filePath);
                }
                catch
                {
                    // If there's an error reading, start with an empty list
                }
            }
            
            // Add the current file to the top of the list
            recentFiles.Insert(0, filePath);
            
            // Keep only the 5 most recent files
            if (recentFiles.Count > 5)
            {
                recentFiles = recentFiles.GetRange(0, 5);
            }
            
            // Save the updated list
            try
            {
                File.WriteAllLines(recentFilesPath, recentFiles);
            }
            catch
            {
                // If we can't save the recent files list, just continue
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }
        
        private void SaveFile()
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = $"Synap Windows Files (*{FILE_EXTENSION})|*{FILE_EXTENSION}|All files (*.*)|*.*",
                    DefaultExt = FILE_EXTENSION
                };
                
                if (saveFileDialog.ShowDialog() == true)
                {
                    _currentFilePath = saveFileDialog.FileName;
                }
                else
                {
                    return;
                }
            }
            
            try
            {
                string content = EditorTextBox.Text;
                string encryptedContent = Encrypt(content);
                File.WriteAllText(_currentFilePath, encryptedContent);
                Title = $"Synap Editor - {Path.GetFileName(_currentFilePath)}";
                MessageBox.Show("File saved and encrypted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Update recent files
                UpdateRecentFiles(_currentFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        // Menu button handler
        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            ShowStartMenu();
        }
        
        // generate new key button handler
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
                    // backup the old key first
                    string backupPath = _keyFilePath + ".backup";
                    File.Copy(_keyFilePath, backupPath, true);
                    
                    // generate and save new key
                    _key = GenerateNewKey();
                    SaveKey(_key);
                    
                    MessageBox.Show($"New encryption key generated successfully!\nYour old key has been backed up to: {backupPath}", 
                        "Key Generated", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error generating new key: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        // import key button handler
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
                    // verify key is valid (32 bytes for aes-256)
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
                        // backup the old key first
                        string backupPath = _keyFilePath + ".backup";
                        File.Copy(_keyFilePath, backupPath, true);
                        
                        // save the imported key
                        _key = importedKey;
                        SaveKey(_key);
                        
                        MessageBox.Show($"Encryption key imported successfully!\nYour old key has been backed up to: {backupPath}", 
                            "Key Imported", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error importing key: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        // Open settings
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
            
            // If settings were changed, apply them
            if (settingsWindow.SettingsChanged)
            {
                ApplySettings();
            }
        }
        
        // text formatting methods
        private void BoldButton_Click(object sender, RoutedEventArgs e)
        {
            WrapSelectedTextWithMarkers("**");
        }
        
        private void ItalicButton_Click(object sender, RoutedEventArgs e)
        {
            WrapSelectedTextWithMarkers("***");
        }
        
        private void BoldItalicButton_Click(object sender, RoutedEventArgs e)
        {
            WrapSelectedTextWithMarkers("****");
        }
        
        private void WrapSelectedTextWithMarkers(string marker)
        {
            int selectionStart = EditorTextBox.SelectionStart;
            int selectionLength = EditorTextBox.SelectionLength;
            
            if (selectionLength > 0)
            {
                string selectedText = EditorTextBox.SelectedText;
                string wrappedText = marker + selectedText + marker;
                
                EditorTextBox.SelectedText = wrappedText;
                EditorTextBox.SelectionStart = selectionStart + marker.Length;
                EditorTextBox.SelectionLength = selectionLength;
            }
            else
            {
                // if no text is selected, insert markers and place cursor between them
                string markerPair = marker + marker;
                EditorTextBox.SelectedText = markerPair;
                EditorTextBox.SelectionStart = selectionStart + marker.Length;
                EditorTextBox.SelectionLength = 0;
            }
            
            EditorTextBox.Focus();
        }
    }
} 