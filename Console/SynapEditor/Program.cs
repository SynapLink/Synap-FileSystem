using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Collections.Generic;

namespace SynapEditor.Console
{
    class Program
    {
        private static string _keyFilePath;
        private static byte[] _key;
        private static string? _currentFilePath;
        private static string _currentText = "";
        private const string FILE_EXTENSION = ".synap";
        
        // settings and preferences
        private static bool _darkMode = false;
        private static ConsoleColor _textColor = ConsoleColor.White;
        private static ConsoleColor _backgroundColor = ConsoleColor.Black;
        private static ConsoleColor _highlightColor = ConsoleColor.Cyan;
        private static int _previewLength = 200;
        
        // storing recent files max 5
        private static List<string> _recentFiles = new List<string>(5);

        [STAThread] // needed for dialogs
        static void Main(string[] args)
        {
            // init windows forms for dialogs
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // setup key location
            string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".";
            _keyFilePath = Path.Combine(appDir, "synap_editor.key");
            
            // load or create key
            _key = LoadKey();
            
            // load user prefs if any
            LoadSettings();
            
            // apply color theme
            ApplyTheme();
            
            // show welcome
            DisplayWelcomeScreen();
            
            // main loop
            bool exit = false;
            while (!exit)
            {
                System.Console.Clear();
                DisplayHeader();
                exit = MainMenu();
            }
            
            // save prefs before exit
            SaveSettings();
            
            // exit msg
            System.Console.Clear();
            SetColor(_highlightColor);
            System.Console.WriteLine(new string('=', 60));
            System.Console.WriteLine("Thank you for using Synap Editor!".PadLeft(40));
            System.Console.WriteLine(new string('=', 60));
            ResetColor();
            System.Console.WriteLine("\nYour files remain securely encrypted with AES-256.");
            System.Console.WriteLine("All settings have been saved for your next session.");
            System.Console.WriteLine("\nPress any key to exit...");
            System.Console.ReadKey();
        }
        
        private static void DisplayWelcomeScreen()
        {
            System.Console.Clear();
            SetColor(_highlightColor);
            System.Console.WriteLine(new string('=', 60));
            System.Console.WriteLine("WELCOME TO SYNAP EDITOR".PadLeft(40));
            System.Console.WriteLine(new string('=', 60));
            ResetColor();
            
            System.Console.WriteLine("\nA secure text editor with AES-256 encryption");
            System.Console.WriteLine("\nKey Features:");
            System.Console.WriteLine(" - Encrypted file storage with .synap format");
            System.Console.WriteLine(" - Text formatting (bold, italic)");
            System.Console.WriteLine(" - Customizable appearance");
            System.Console.WriteLine(" - Portable encryption key management");
            
            System.Console.WriteLine("\nPress any key to continue...");
            System.Console.ReadKey();
        }

        private static void DisplayHeader()
        {
            string title = _currentFilePath != null 
                ? $"Synap Editor - {Path.GetFileName(_currentFilePath)}" 
                : "Synap Editor - New File";
            
            SetColor(_highlightColor);
            System.Console.WriteLine(new string('=', 60));
            System.Console.WriteLine(title.PadLeft(30 + title.Length / 2));
            System.Console.WriteLine(new string('=', 60));
            ResetColor();
            System.Console.WriteLine();
            
            if (_currentText.Length > 0)
            {
                // Display preview of the text
                string preview = _currentText.Length > _previewLength 
                    ? _currentText.Substring(0, _previewLength) + "..." 
                    : _currentText;
                
                System.Console.WriteLine("Current Text Preview:");
                System.Console.WriteLine(new string('-', 60));
                System.Console.WriteLine(preview);
                System.Console.WriteLine(new string('-', 60));
            }
            else
            {
                System.Console.WriteLine("No text in the current file.");
                System.Console.WriteLine(new string('-', 60));
            }
            
            System.Console.WriteLine();
        }

        private static bool MainMenu()
        {
            SetColor(_highlightColor);
            System.Console.WriteLine("MAIN MENU");
            ResetColor();
            
            System.Console.WriteLine("1. File Operations (Open, Save, Import/Export Key)");
            System.Console.WriteLine("2. Text Editor (Edit current file with formatting)");
            System.Console.WriteLine("3. Settings & Customization (Theme, Colors, Options)");
            
            if (_recentFiles.Count > 0)
            {
                System.Console.WriteLine("\nRecent Files:");
                for (int i = 0; i < _recentFiles.Count; i++)
                {
                    System.Console.WriteLine($"R{i+1}. {Path.GetFileName(_recentFiles[i])}");
                }
            }
            
            System.Console.WriteLine("\n0. Exit");
            System.Console.WriteLine();
            System.Console.Write("Enter your choice: ");
            
            string choice = System.Console.ReadLine() ?? "";
            
            // Handle recent files options
            if (choice.StartsWith("R", StringComparison.OrdinalIgnoreCase) && 
                int.TryParse(choice.Substring(1), out int recentIndex) && 
                recentIndex > 0 && recentIndex <= _recentFiles.Count)
            {
                OpenSpecificFile(_recentFiles[recentIndex - 1]);
                return false;
            }
            
            switch (choice)
            {
                case "1":
                    return FileMenu();
                case "2":
                    EditText();
                    return false;
                case "3":
                    SettingsMenu();
                    return false;
                case "0":
                    return true; // Exit
                default:
                    System.Console.WriteLine("Invalid option. Press any key to continue...");
                    System.Console.ReadKey();
                    return false;
            }
        }

        private static bool FileMenu()
        {
            bool backToMainMenu = false;
            
            while (!backToMainMenu)
            {
                System.Console.Clear();
                DisplayHeader();
                
                SetColor(_highlightColor);
                System.Console.WriteLine("FILE OPERATIONS");
                ResetColor();
                
                System.Console.WriteLine("1. New File (Create an empty document)");
                System.Console.WriteLine("2. Open File (Open an existing .synap file)");
                System.Console.WriteLine("3. Save File (Encrypt and save current file)");
                System.Console.WriteLine("4. Export Key (Save your encryption key for backup)");
                System.Console.WriteLine("5. Import Key (Load an existing encryption key)");
                System.Console.WriteLine("0. Back to Main Menu");
                System.Console.WriteLine();
                System.Console.Write("Enter your choice: ");
                
                string choice = System.Console.ReadLine() ?? "";
                
                switch (choice)
                {
                    case "1":
                        CreateNewFile();
                        break;
                    case "2":
                        OpenFile();
                        break;
                    case "3":
                        SaveFile();
                        break;
                    case "4":
                        ExportKey();
                        break;
                    case "5":
                        ImportKey();
                        break;
                    case "0":
                        backToMainMenu = true;
                        break;
                    default:
                        System.Console.WriteLine("Invalid option. Press any key to continue...");
                        System.Console.ReadKey();
                        break;
                }
            }
            
            return false; // Don't exit the application
        }

        private static void SettingsMenu()
        {
            bool backToMainMenu = false;
            
            while (!backToMainMenu)
            {
                System.Console.Clear();
                DisplayHeader();
                
                SetColor(_highlightColor);
                System.Console.WriteLine("SETTINGS & CUSTOMIZATION");
                ResetColor();
                
                System.Console.WriteLine($"1. Toggle Dark/Light Mode (Current: {(_darkMode ? "Dark" : "Light")})");
                System.Console.WriteLine($"2. Change Text Color (Current: {_textColor})");
                System.Console.WriteLine($"3. Change Highlight Color (Current: {_highlightColor})");
                System.Console.WriteLine($"4. Change Preview Length (Current: {_previewLength} chars)");
                System.Console.WriteLine("5. Generate New Encryption Key");
                System.Console.WriteLine("6. Clear Recent Files List");
                System.Console.WriteLine("0. Back to Main Menu");
                System.Console.WriteLine();
                System.Console.Write("Enter your choice: ");
                
                string choice = System.Console.ReadLine() ?? "";
                
                switch (choice)
                {
                    case "1":
                        _darkMode = !_darkMode;
                        _backgroundColor = _darkMode ? ConsoleColor.Black : ConsoleColor.White;
                        _textColor = _darkMode ? ConsoleColor.White : ConsoleColor.Black;
                        ApplyTheme();
                        System.Console.WriteLine($"Theme switched to {(_darkMode ? "Dark" : "Light")} mode.");
                        System.Console.WriteLine("Press any key to continue...");
                        System.Console.ReadKey();
                        break;
                    case "2":
                        ChangeColor("text", ref _textColor);
                        ApplyTheme();
                        break;
                    case "3":
                        ChangeColor("highlight", ref _highlightColor);
                        ApplyTheme();
                        break;
                    case "4":
                        System.Console.Write("Enter new preview length (50-500 chars): ");
                        if (int.TryParse(System.Console.ReadLine(), out int length) && length >= 50 && length <= 500)
                        {
                            _previewLength = length;
                            System.Console.WriteLine("Preview length updated.");
                        }
                        else
                        {
                            System.Console.WriteLine("Invalid input. Preview length not changed.");
                        }
                        System.Console.WriteLine("Press any key to continue...");
                        System.Console.ReadKey();
                        break;
                    case "5":
                        GenerateNewKey();
                        break;
                    case "6":
                        _recentFiles.Clear();
                        System.Console.WriteLine("Recent files list cleared.");
                        System.Console.WriteLine("Press any key to continue...");
                        System.Console.ReadKey();
                        break;
                    case "0":
                        backToMainMenu = true;
                        break;
                    default:
                        System.Console.WriteLine("Invalid option. Press any key to continue...");
                        System.Console.ReadKey();
                        break;
                }
            }
        }

        private static void ChangeColor(string type, ref ConsoleColor color)
        {
            System.Console.WriteLine($"\nAvailable colors for {type}:");
            System.Console.WriteLine(new string('-', 30));
            
            var colors = Enum.GetValues(typeof(ConsoleColor));
            for (int i = 0; i < colors.Length; i++)
            {
                System.Console.WriteLine($"{i}. {colors.GetValue(i)}");
            }
            
            System.Console.Write($"\nSelect a color (0-{colors.Length - 1}): ");
            if (int.TryParse(System.Console.ReadLine(), out int index) && index >= 0 && index < colors.Length)
            {
                color = (ConsoleColor)colors.GetValue(index)!;
                System.Console.WriteLine("Color updated.");
            }
            else
            {
                System.Console.WriteLine("Invalid option. Color not changed.");
            }
            System.Console.WriteLine("Press any key to continue...");
            System.Console.ReadKey();
        }

        private static void ApplyTheme()
        {
            System.Console.BackgroundColor = _backgroundColor;
            System.Console.ForegroundColor = _textColor;
            System.Console.Clear();
        }

        private static void SetColor(ConsoleColor color)
        {
            System.Console.ForegroundColor = color;
        }

        private static void ResetColor()
        {
            System.Console.ForegroundColor = _textColor;
        }

        private static void LoadSettings()
        {
            string settingsPath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".",
                "synap_settings.cfg");
                
            if (File.Exists(settingsPath))
            {
                try
                {
                    string[] settings = File.ReadAllLines(settingsPath);
                    
                    foreach (string line in settings)
                    {
                        string[] parts = line.Split('=');
                        if (parts.Length == 2)
                        {
                            string key = parts[0].Trim();
                            string value = parts[1].Trim();
                            
                            switch (key)
                            {
                                case "DarkMode":
                                    _darkMode = bool.Parse(value);
                                    _backgroundColor = _darkMode ? ConsoleColor.Black : ConsoleColor.White;
                                    _textColor = _darkMode ? ConsoleColor.White : ConsoleColor.Black;
                                    break;
                                case "TextColor":
                                    _textColor = Enum.Parse<ConsoleColor>(value);
                                    break;
                                case "HighlightColor":
                                    _highlightColor = Enum.Parse<ConsoleColor>(value);
                                    break;
                                case "PreviewLength":
                                    _previewLength = int.Parse(value);
                                    break;
                                case "RecentFiles":
                                    if (!string.IsNullOrEmpty(value))
                                    {
                                        string[] files = value.Split('|');
                                        _recentFiles = new List<string>(files);
                                        // Remove non-existent files from the recent files list
                                        _recentFiles.RemoveAll(f => !File.Exists(f));
                                    }
                                    break;
                            }
                        }
                    }
                }
                catch
                {
                    // If there's an error reading settings, just use defaults
                }
            }
        }

        private static void SaveSettings()
        {
            string settingsPath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".",
                "synap_settings.cfg");
                
            try
            {
                using (StreamWriter writer = new StreamWriter(settingsPath))
                {
                    writer.WriteLine($"DarkMode={_darkMode}");
                    writer.WriteLine($"TextColor={_textColor}");
                    writer.WriteLine($"HighlightColor={_highlightColor}");
                    writer.WriteLine($"PreviewLength={_previewLength}");
                    writer.WriteLine($"RecentFiles={string.Join("|", _recentFiles)}");
                }
            }
            catch
            {
                // If there's an error saving settings, just continue
            }
        }

        private static void CreateNewFile()
        {
            if (!string.IsNullOrEmpty(_currentText))
            {
                System.Console.WriteLine("Do you want to save the current file? (Y/N)");
                string response = System.Console.ReadLine() ?? "";
                
                if (response.ToLower() == "y" || response.ToLower() == "yes")
                {
                    SaveFile();
                }
            }
            
            _currentText = "";
            _currentFilePath = null;
            System.Console.WriteLine("New file created. Press any key to continue...");
            System.Console.ReadKey();
        }

        private static void OpenFile()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = $"Synap Files (*{FILE_EXTENSION})|*{FILE_EXTENSION}|All files (*.*)|*.*";
                openFileDialog.DefaultExt = FILE_EXTENSION;
                openFileDialog.Title = "Open Encrypted File";
                
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string encryptedContent = File.ReadAllText(openFileDialog.FileName);
                        string decryptedContent = Decrypt(encryptedContent);
                        _currentText = decryptedContent;
                        _currentFilePath = openFileDialog.FileName;
                        System.Console.WriteLine("File opened successfully!");
                        
                        // Add to recent files
                        AddToRecentFiles(_currentFilePath);
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine($"Error opening file: {ex.Message}");
                    }
                    
                    System.Console.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                }
            }
        }

        private static void SaveFile()
        {
            if (_currentFilePath == null)
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = $"Synap Files (*{FILE_EXTENSION})|*{FILE_EXTENSION}|All files (*.*)|*.*";
                    saveFileDialog.DefaultExt = FILE_EXTENSION;
                    saveFileDialog.Title = "Save Encrypted File";
                    
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        _currentFilePath = saveFileDialog.FileName;
                    }
                    else
                    {
                        return;
                    }
                }
            }
            
            try
            {
                string encryptedContent = Encrypt(_currentText);
                File.WriteAllText(_currentFilePath, encryptedContent);
                System.Console.WriteLine("File saved and encrypted successfully!");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error saving file: {ex.Message}");
            }
            
            System.Console.WriteLine("Press any key to continue...");
            System.Console.ReadKey();
        }

        private static void EditText()
        {
            System.Console.Clear();
            SetColor(_highlightColor);
            System.Console.WriteLine("TEXT EDITOR");
            System.Console.WriteLine(new string('-', 60));
            ResetColor();
            
            System.Console.WriteLine("Available commands:");
            System.Console.WriteLine(" :save  - Save and exit");
            System.Console.WriteLine(" :exit  - Discard changes and exit");
            System.Console.WriteLine(" :bold  - Format selected text as bold (**text**)");
            System.Console.WriteLine(" :italic - Format selected text as italic (***text***)");
            System.Console.WriteLine(" :bi    - Format text as bold+italic (****text****)");
            System.Console.WriteLine(" :clear - Clear all text");
            System.Console.WriteLine(" :help  - Show these commands again");
            System.Console.WriteLine(new string('-', 60));
            
            if (!string.IsNullOrEmpty(_currentText))
            {
                System.Console.WriteLine(_currentText);
            }
            
            StringBuilder newText = new StringBuilder(_currentText);
            
            while (true)
            {
                System.Console.Write("> ");
                string line = System.Console.ReadLine() ?? "";
                
                if (line == ":save")
                {
                    _currentText = newText.ToString();
                    System.Console.WriteLine("Changes saved to memory. (File not saved to disk yet)");
                    break;
                }
                else if (line == ":exit")
                {
                    System.Console.WriteLine("Changes discarded.");
                    break;
                }
                else if (line == ":help")
                {
                    System.Console.Clear();
                    SetColor(_highlightColor);
                    System.Console.WriteLine("TEXT EDITOR COMMANDS");
                    System.Console.WriteLine(new string('-', 60));
                    ResetColor();
                    
                    System.Console.WriteLine(" :save  - Save and exit");
                    System.Console.WriteLine(" :exit  - Discard changes and exit");
                    System.Console.WriteLine(" :bold  - Format selected text as bold (**text**)");
                    System.Console.WriteLine(" :italic - Format selected text as italic (***text***)");
                    System.Console.WriteLine(" :bi    - Format text as bold+italic (****text****)");
                    System.Console.WriteLine(" :clear - Clear all text");
                    System.Console.WriteLine(" :help  - Show these commands again");
                    System.Console.WriteLine(new string('-', 60));
                    
                    if (newText.Length > 0)
                    {
                        System.Console.WriteLine(newText.ToString());
                    }
                }
                else if (line == ":clear")
                {
                    System.Console.WriteLine("Are you sure you want to clear all text? (Y/N)");
                    string response = System.Console.ReadLine() ?? "";
                    
                    if (response.ToLower() == "y" || response.ToLower() == "yes")
                    {
                        newText.Clear();
                        System.Console.WriteLine("Text cleared.");
                    }
                }
                else if (line == ":bold" || line == ":italic" || line == ":bi")
                {
                    System.Console.Write("Enter text to format: ");
                    string textToFormat = System.Console.ReadLine() ?? "";
                    
                    if (!string.IsNullOrEmpty(textToFormat))
                    {
                        string formattedText = line switch
                        {
                            ":bold" => $"**{textToFormat}**",
                            ":italic" => $"***{textToFormat}***",
                            ":bi" => $"****{textToFormat}****",
                            _ => textToFormat
                        };
                        
                        if (newText.Length > 0)
                        {
                            newText.AppendLine();
                        }
                        newText.Append(formattedText);
                        System.Console.WriteLine($"Added: {formattedText}");
                    }
                }
                else
                {
                    if (newText.Length > 0)
                    {
                        newText.AppendLine();
                    }
                    newText.Append(line);
                }
            }
            
            System.Console.WriteLine("Press any key to continue...");
            System.Console.ReadKey();
        }

        private static void ImportKey()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Key Files (*.key)|*.key|All Files (*.*)|*.*";
                openFileDialog.DefaultExt = ".key";
                openFileDialog.Title = "Import Encryption Key";
                
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        byte[] importedKey = File.ReadAllBytes(openFileDialog.FileName);
                        
                        if (importedKey.Length != 32)
                        {
                            System.Console.WriteLine("The selected file is not a valid encryption key. It must be exactly 32 bytes in length.");
                            System.Console.WriteLine("Press any key to continue...");
                            System.Console.ReadKey();
                            return;
                        }
                        
                        System.Console.WriteLine("WARNING: Importing a new key will make all previously encrypted files unreadable with the current key.");
                        System.Console.WriteLine("Are you sure you want to import this key? (Y/N)");
                        
                        string response = System.Console.ReadLine() ?? "";
                        
                        if (response.ToLower() == "y" || response.ToLower() == "yes")
                        {
                            // Backup the old key
                            string backupPath = _keyFilePath + ".backup";
                            File.Copy(_keyFilePath, backupPath, true);
                            
                            // Save the imported key
                            _key = importedKey;
                            SaveKey(_key);
                            
                            System.Console.WriteLine("Encryption key imported successfully!");
                            System.Console.WriteLine($"Your old key has been backed up to: {backupPath}");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine($"Error importing key: {ex.Message}");
                    }
                    
                    System.Console.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                }
            }
        }

        private static void ExportKey()
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Key Files (*.key)|*.key|All Files (*.*)|*.*";
                saveFileDialog.DefaultExt = ".key";
                saveFileDialog.FileName = "synap_export.key";
                saveFileDialog.Title = "Export Encryption Key";
                
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        File.Copy(_keyFilePath, saveFileDialog.FileName, true);
                        System.Console.WriteLine("Encryption key exported successfully!");
                        System.Console.WriteLine("WARNING: Keep this key secure! Anyone with this key can decrypt your files.");
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine($"Error exporting key: {ex.Message}");
                    }
                    
                    System.Console.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                }
            }
        }

        private static void GenerateNewKey()
        {
            System.Console.WriteLine("WARNING: Generating a new key will make all previously encrypted files unreadable unless you've backed up the current key.");
            System.Console.WriteLine("Are you sure you want to continue? (Y/N)");
            
            string response = System.Console.ReadLine() ?? "";
            
            if (response.ToLower() == "y" || response.ToLower() == "yes")
            {
                try
                {
                    // Backup the old key
                    string backupPath = _keyFilePath + ".backup";
                    File.Copy(_keyFilePath, backupPath, true);
                    
                    // Generate and save new key
                    _key = GenerateNewKeyBytes();
                    SaveKey(_key);
                    
                    System.Console.WriteLine("New encryption key generated successfully!");
                    System.Console.WriteLine($"Your old key has been backed up to: {backupPath}");
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"Error generating new key: {ex.Message}");
                }
                
                System.Console.WriteLine("Press any key to continue...");
                System.Console.ReadKey();
            }
        }

        private static byte[] LoadKey()
        {
            if (!File.Exists(_keyFilePath))
            {
                // Generate a new 32-byte (256-bit) key
                byte[] key = GenerateNewKeyBytes();
                SaveKey(key);
                return key;
            }
            
            return File.ReadAllBytes(_keyFilePath);
        }

        private static byte[] GenerateNewKeyBytes()
        {
            byte[] key = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }
            return key;
        }

        private static void SaveKey(byte[] key)
        {
            File.WriteAllBytes(_keyFilePath, key);
        }

        private static string Encrypt(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = _key;
                aes.GenerateIV(); // Generate a random IV for each encryption
                
                using (MemoryStream ms = new MemoryStream())
                {
                    // First write the IV to the output
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

        private static string Decrypt(string cipherText)
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            
            using (Aes aes = Aes.Create())
            {
                aes.Key = _key;
                
                // Get the IV from the cipher bytes (first 16 bytes)
                byte[] iv = new byte[16];
                Array.Copy(cipherBytes, 0, iv, 0, iv.Length);
                aes.IV = iv;
                
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        // Skip IV in the input
                        cs.Write(cipherBytes, iv.Length, cipherBytes.Length - iv.Length);
                    }
                    
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }

        private static void AddToRecentFiles(string filePath)
        {
            // Remove the file if it already exists in the list
            _recentFiles.Remove(filePath);
            
            // Add to the beginning of the list
            _recentFiles.Insert(0, filePath);
            
            // Ensure we only keep the 5 most recent files
            while (_recentFiles.Count > 5)
            {
                _recentFiles.RemoveAt(_recentFiles.Count - 1);
            }
        }
        
        private static void OpenSpecificFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                System.Console.WriteLine($"File not found: {filePath}");
                System.Console.WriteLine("Press any key to continue...");
                System.Console.ReadKey();
                return;
            }
            
            try
            {
                string encryptedContent = File.ReadAllText(filePath);
                string decryptedContent = Decrypt(encryptedContent);
                _currentText = decryptedContent;
                _currentFilePath = filePath;
                System.Console.WriteLine("File opened successfully!");
                
                // Add to recent files
                AddToRecentFiles(filePath);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error opening file: {ex.Message}");
            }
            
            System.Console.WriteLine("Press any key to continue...");
            System.Console.ReadKey();
        }
    }
} 