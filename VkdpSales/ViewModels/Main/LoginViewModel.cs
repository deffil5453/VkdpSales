using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VkdpSales.Services;
using VkdpSales.ViewModels.Commands;
using VkdpSales.Views.Main;

namespace VkdpSales.ViewModels.Main
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private AuthService authService;
        private string login;
        private string errorMessage;

        public string Login
        {
            get { return login; }
            set { login = value; OnPropertyChanged("Login"); }
        }

        public string ErrorMessage
        {
            get { return errorMessage; }
            set { errorMessage = value; OnPropertyChanged("ErrorMessage"); }
        }

        public ICommand LoginCommand { get; set; }

        public LoginViewModel()
        {
            authService = new AuthService();
            LoginCommand = new VKDPCommand(OnLoginClicked);
        }

        private void OnLoginClicked(object parameter)
        {
            PasswordBox passwordBox = parameter as PasswordBox;

            if (string.IsNullOrEmpty(Login) || passwordBox == null || string.IsNullOrEmpty(passwordBox.Password))
            {
                ErrorMessage = "Заполните логин и пароль";
                return;
            }

            string password = passwordBox.Password;
            var user = authService.Authenticate(Login, password);

            if (user != null)
            {
                // ✅ Создаём главное окно только после успешного входа
                MainWindow mainWin = new MainWindow();
                mainWin.DataContext = new MainViewModel(user);
                mainWin.Show();

                // ✅ Закрываем окно входа через явный перебор (без лямбд)
                Window windowToClose = null;
                foreach (Window w in Application.Current.Windows)
                {
                    if (w is LoginWindow)
                    {
                        windowToClose = w;
                        break;
                    }
                }
                if (windowToClose != null)
                {
                    windowToClose.Close();
                }
            }
            else
            {
                ErrorMessage = "Неверный логин или пароль";
            }
        }
    }
}
