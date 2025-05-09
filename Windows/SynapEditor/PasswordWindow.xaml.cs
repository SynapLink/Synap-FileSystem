using System.Windows;

namespace SynapEditor
{
    public partial class PasswordWindow : Window
    {
        public enum PasswordMode
        {
            Set,
            Verify
        }

        public string Password { get; private set; }
        private PasswordMode _mode;

        public PasswordWindow(PasswordMode mode)
        {
            InitializeComponent();
            _mode = mode;
            
            // Configure UI based on mode
            if (mode == PasswordMode.Set)
            {
                Title = "Set Password";
                HeaderText.Text = "Create New Password";
                DescriptionText.Text = "Please create a password to protect your encryption key. You will need this password whenever you want to view or export your key.";
                Password1Label.Text = "New Password:";
                ConfirmPasswordGrid.Visibility = Visibility.Visible;
                Height = 250; // Make window taller to accommodate the second password field
            }
            else
            {
                Title = "Password Required";
                HeaderText.Text = "Enter Password";
                DescriptionText.Text = "This action is password protected. Please enter your password to continue.";
                Password1Label.Text = "Password:";
                ConfirmPasswordGrid.Visibility = Visibility.Collapsed;
            }

            // Focus the first password box
            Loaded += (s, e) => PasswordBox1.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (_mode == PasswordMode.Set)
            {
                // Validate passwords match
                if (PasswordBox1.Password != PasswordBox2.Password)
                {
                    MessageBox.Show("Passwords do not match. Please try again.", "Password Mismatch", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    PasswordBox1.Clear();
                    PasswordBox2.Clear();
                    PasswordBox1.Focus();
                    return;
                }
                
                // Validate minimum length
                if (PasswordBox1.Password.Length < 6)
                {
                    MessageBox.Show("Password must be at least 6 characters long.", "Password Too Short", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            
            // Store password and close with success
            Password = PasswordBox1.Password;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
} 