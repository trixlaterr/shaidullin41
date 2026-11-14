using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _41
{
    /// <summary>
    /// Логика взаимодействия для AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {
        private bool _captchaRequired;
        private string _captchaCode = string.Empty;

        private static readonly Random _random = new Random();

        public AuthPage()
        {
            InitializeComponent();
        }

        public string GenerateRandomString(int length)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            var result = new StringBuilder(length);

            lock (_random)
            {
                for (int i = 0; i < length; i++)
                    result.Append(validChars[_random.Next(validChars.Length)]);
            }

            return result.ToString();
        }

        private void LoginGuest_Btn_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new ProductPage(null));
            HideCaptcha();
            LoginTB.Text = "";
            PasswordTB.Text = "";
        }

        private async void Login_Btn_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTB.Text.Trim();
            string password = PasswordTB.Text;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Есть пустые поля");
                return;
            }

            if (_captchaRequired)
            {
                if (!string.Equals(CaptchacheckTB.Text.Trim(), _captchaCode, StringComparison.Ordinal))
                {
                    MessageBox.Show("Капча введена неверно!");
                    GenerateCaptcha();
                    await LockLoginButtonAsync();
                    return;
                }

                _captchaRequired = false;
            }

            User user = Шайдуллин41Entities.GetContext().User.FirstOrDefault(p => p.UserLogin == login && p.UserPassword == password);

            if (user == null)
            {
                MessageBox.Show("Введены некорректные данные");
                ShowCaptcha();
                return;
            }

            HideCaptcha();
            LoginTB.Text = "";
            PasswordTB.Text = "";
            Manager.MainFrame.Navigate(new ProductPage(user));
        }

        private void ShowCaptcha()
        {
            _captchaRequired = true;
            CaptchacheckTB.Visibility = Visibility.Visible;
            CaptchaSP.Visibility = Visibility.Visible;
            GenerateCaptcha();
        }

        private void HideCaptcha()
        {
            _captchaRequired = false;
            CaptchacheckTB.Visibility = Visibility.Collapsed;
            CaptchaSP.Visibility = Visibility.Collapsed;
        }

        private void GenerateCaptcha()
        {
            _captchaCode = GenerateRandomString(4);

            captchaOneWord.Text = _captchaCode[0].ToString();
            captchaTwoWord.Text = _captchaCode[1].ToString();
            captchaThreeWord.Text = _captchaCode[2].ToString();
            captchaFourWord.Text = _captchaCode[3].ToString();

            CaptchacheckTB.Text = "";
        }

        private async Task LockLoginButtonAsync()
        {
            Login_Btn.IsEnabled = false;
            try
            {
                await Task.Delay(10000);
            }
            finally
            {
                Login_Btn.IsEnabled = true;
            }
        }
    }
}