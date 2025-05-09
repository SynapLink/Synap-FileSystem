using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Linq;

namespace SynapEditor
{
    public partial class StartMenu : Window
    {
        private readonly List<string> _recentFiles = new List<string>();
        private const int MaxRecentFiles = 5;
        private readonly string _recentFilesPath;
        
        public StartMenu()
        {
            InitializeComponent();
            
            // Set up recent files path
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            _recentFilesPath = Path.Combine(appDir, "recent_files.txt");
            
            // Load recent files
            LoadRecentFiles();
            DisplayRecentFiles();
        }
        
        private void LoadRecentFiles()
        {
            if (File.Exists(_recentFilesPath))
            {
                try
                {
                    string[] files = File.ReadAllLines(_recentFilesPath);
                    _recentFiles.Clear();
                    _recentFiles.AddRange(files.Where(File.Exists).Take(MaxRecentFiles));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading recent files: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        private void SaveRecentFiles()
        {
            try
            {
                File.WriteAllLines(_recentFilesPath, _recentFiles);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving recent files: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void DisplayRecentFiles()
        {
            if (_recentFiles.Count == 0)
            {
                RecentFilesText.Text = "No recent files";
                RecentFilesText.FontStyle = FontStyles.Italic;
                RecentFilesText.Foreground = System.Windows.Media.Brushes.Gray;
                return;
            }
            
            RecentFilesText.Text = string.Empty;
            RecentFilesText.FontStyle = FontStyles.Normal;
            RecentFilesText.Foreground = System.Windows.Media.Brushes.Black;
            
            foreach (string file in _recentFiles)
            {
                RecentFilesText.Text += Path.GetFileName(file) + Environment.NewLine;
            }
        }
        
        public void AddRecentFile(string filePath)
        {
            // Remove if exists already (to move to top)
            _recentFiles.Remove(filePath);
            
            // Add to beginning
            _recentFiles.Insert(0, filePath);
            
            // Trim list if too long
            while (_recentFiles.Count > MaxRecentFiles)
            {
                _recentFiles.RemoveAt(_recentFiles.Count - 1);
            }
            
            SaveRecentFiles();
            DisplayRecentFiles();
        }
        
        private void NewFileButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
        
        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.OpenFileFromStartMenu();
            mainWindow.Show();
            this.Close();
        }
        
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // Create settings window - we'll create this next
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
        }
        
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
} 