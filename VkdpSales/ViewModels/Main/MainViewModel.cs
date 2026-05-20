using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Input;
using VkdpSales.Models;
using VkdpSales.ViewModels.Commands;
using VkdpSales.Views.Main;

namespace VkdpSales.ViewModels.Main
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private User currentUser;
        private string userName;
        private string userRole;
        private object currentView;

        public string UserName
        {
            get { return userName; }
            set { userName = value; OnPropertyChanged("UserName"); }
        }

        public string UserRole
        {
            get { return userRole; }
            set { userRole = value; OnPropertyChanged("UserRole"); }
        }

        public object CurrentView
        {
            get { return currentView; }
            set { currentView = value; OnPropertyChanged("CurrentView"); }
        }

        // ✅ Флаги ролей для привязки в XAML
        public bool IsAdmin { get; private set; }
        public bool IsManager { get; private set; }
        public bool IsAnalyst { get; private set; }
        public bool CanManageSales => IsAdmin || IsManager;      // Новая продажа, История продаж
        public bool CanManageProducts => IsAdmin || IsManager;   // Товары
        public bool CanManageClients => IsAdmin || IsManager;    // Клиенты
        public bool CanViewAnalytics => IsAdmin || IsAnalyst;    // Статистика
        public bool CanManageUsers => IsAdmin;
        public ICommand NewSaleCommand { get; set; }
        public ICommand SalesHistoryCommand { get; set; }
        public ICommand ProductsCommand { get; set; }
        public ICommand ClientsCommand { get; set; }
        public ICommand AnalyticsCommand { get; set; }
        public ICommand UsersCommand { get; set; }
        public ICommand LogoutCommand { get; set; }

        public MainViewModel(User loggedInUser)
        {
            currentUser = loggedInUser;
            userName = loggedInUser.FullName;
            userRole = loggedInUser.Role?.Name ?? "";

            // ✅ Определяем права доступа
            IsAdmin = (userRole == "Admin");
            IsManager = (userRole == "Manager") || IsAdmin;
            IsAnalyst = (userRole == "Analyst") || IsAdmin;

            NewSaleCommand = new VKDPCommand(OnNewSale);
            SalesHistoryCommand = new VKDPCommand(OnSalesHistory);
            ProductsCommand = new VKDPCommand(OnProducts);
            ClientsCommand = new VKDPCommand(OnClients);
            AnalyticsCommand = new VKDPCommand(OnAnalytics);
            UsersCommand = new VKDPCommand(OnUsers);
            LogoutCommand = new VKDPCommand(OnLogout);

            // ✅ По умолчанию: менеджер → Новая продажа, аналитик → Аналитика
            if (IsManager)
            {
                OnNewSale(null);
            }
            else if (IsAnalyst)
            {
                OnAnalytics(null);
            }
            OnPropertyChanged(nameof(CanManageSales));
            OnPropertyChanged(nameof(CanManageProducts));
            OnPropertyChanged(nameof(CanManageClients));
            OnPropertyChanged(nameof(CanViewAnalytics));
            OnPropertyChanged(nameof(CanManageUsers));
        }

        private void OnNewSale(object parameter) { CurrentView = new NewSaleViewModel(); }
        private void OnSalesHistory(object parameter) { CurrentView = new SalesHistoryViewModel(); }
        private void OnProducts(object parameter) { CurrentView = new ProductsViewModel(); }
        private void OnClients(object parameter) { CurrentView = new ClientsViewModel(); }
        private void OnAnalytics(object parameter) { CurrentView = new AnalyticsViewModel(); }
        private void OnUsers(object parameter) { CurrentView = new UsersViewModel(); }

        private void OnLogout(object parameter)
        {
            var loginWin = new LoginWindow();
            loginWin.DataContext = new LoginViewModel();
            loginWin.Show();

            Window mainWinToClose = null;
            foreach (Window w in Application.Current.Windows)
            {
                if (w is MainWindow)
                {
                    mainWinToClose = w;
                    break;
                }
            }
            if (mainWinToClose != null)
            {
                mainWinToClose.Close();
            }
        }
    }
}
