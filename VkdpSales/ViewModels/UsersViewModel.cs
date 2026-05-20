using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Input;
using VkdpSales.Data;
using VkdpSales.Models;
using VkdpSales.ViewModels.Commands;

namespace VkdpSales.ViewModels
{
   public class UsersViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private readonly VkdpdbContext _context;
        private ObservableCollection<User> _users;
        private User _selectedUser;
        private string _login;
        private string _password;
        private string _fullName;
        private string _selectedRole;
        private bool _isActive;
        private string _formTitle;
        private bool _isEditMode;

        public ObservableCollection<User> Users { get => _users; set { _users = value; OnPropertyChanged("Users"); } }
        public User SelectedUser { get => _selectedUser; set { _selectedUser = value; OnPropertyChanged("SelectedUser"); } }
        public string Login { get => _login; set { _login = value; OnPropertyChanged("Login"); } }
        public string Password { get => _password; set { _password = value; OnPropertyChanged("Password"); } }
        public string FullName { get => _fullName; set { _fullName = value; OnPropertyChanged("FullName"); } }
        public string SelectedRole { get => _selectedRole; set { _selectedRole = value; OnPropertyChanged("SelectedRole"); } }
        public bool IsActive { get => _isActive; set { _isActive = value; OnPropertyChanged("IsActive"); } }
        public string FormTitle { get => _formTitle; set { _formTitle = value; OnPropertyChanged("FormTitle"); } }
        public bool IsEditMode { get => _isEditMode; set { _isEditMode = value; OnPropertyChanged("IsEditMode"); } }

        public ObservableCollection<string> Roles { get; set; }
        public ICommand LoadCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        public UsersViewModel()
        {
            _context = new VkdpdbContext();
            _users = new ObservableCollection<User>();
            _isActive = true;
            _formTitle = "Управление пользователями";
            _isEditMode = false;

            Roles = new ObservableCollection<string>();
            Roles.Add("Admin");
            Roles.Add("Manager");
            Roles.Add("Analyst");

            LoadCommand = new VKDPCommand(OnLoad);
            AddCommand = new VKDPCommand(OnAdd);
            EditCommand = new VKDPCommand(OnEdit);
            SaveCommand = new VKDPCommand(OnSave);
            DeleteCommand = new VKDPCommand(OnDelete);
            CancelCommand = new VKDPCommand(OnCancel);

            OnLoad(null);
        }

        private void OnLoad(object parameter)
        {
            _users.Clear();
            foreach (var u in _context.Users)
            {
                u.Role = _context.Roles.Find(u.RoleId);
                _users.Add(u);
            }
        }

        private void OnAdd(object parameter)
        {
            _selectedUser = null;
            Login = "";
            Password = "";
            FullName = "";
            SelectedRole = "Manager";
            IsActive = true;
            FormTitle = "Добавление пользователя";
            IsEditMode = false;
            OnPropertyChanged("SelectedUser");
        }

        private void OnEdit(object parameter)
        {
            if (_selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя для редактирования");
                return;
            }
            Login = _selectedUser.Login;
            FullName = _selectedUser.FullName;
            SelectedRole = _selectedUser.Role?.Name ?? "Manager";
            IsActive = _selectedUser.IsActive;
            FormTitle = "Редактирование: " + _selectedUser.Login;
            IsEditMode = true;
        }

        private void OnSave(object parameter)
        {
            if (string.IsNullOrEmpty(Login) || string.IsNullOrEmpty(FullName))
            {
                MessageBox.Show("Заполните логин и ФИО");
                return;
            }
            if (IsEditMode == false && string.IsNullOrEmpty(Password))
            {
                MessageBox.Show("Введите пароль для нового пользователя");
                return;
            }

            if (IsEditMode == false)
            {
                // Проверка на дубль логина
                bool exists = false;
                foreach (var u in _context.Users)
                {
                    if (u.Login == Login)
                    {
                        exists = true;
                        break;
                    }
                }
                if (exists)
                {
                    MessageBox.Show("Пользователь с таким логином уже существует");
                    return;
                }

                var newUser = new User
                {
                    Login = Login,
                    Password = Password, // TODO: Добавить хэширование при сдаче
                    FullName = FullName,
                    RoleId = GetRoleIdByName(SelectedRole),
                    IsActive = IsActive,
                    CreatedAt = DateTime.Now
                };
                _context.Users.Add(newUser);
            }
            else
            {
                _selectedUser.Login = Login;
                _selectedUser.FullName = FullName;
                _selectedUser.RoleId = GetRoleIdByName(SelectedRole);
                _selectedUser.IsActive = IsActive;
                // Пароль не меняем, если поле пустое
                if (!string.IsNullOrEmpty(Password))
                {
                    _selectedUser.Password = Password;
                }
            }

            _context.SaveChanges();
            OnLoad(null);
            OnCancel(null);
            MessageBox.Show("Пользователь сохранён");
        }

        private void OnDelete(object parameter)
        {
            if (_selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя для удаления");
                return;
            }
            if (_selectedUser.Login == "admin")
            {
                MessageBox.Show("Нельзя удалить главного администратора");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Удалить пользователя \"" + _selectedUser.Login + "\"?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _context.Users.Remove(_selectedUser);
                _context.SaveChanges();
                OnLoad(null);
                OnCancel(null);
                MessageBox.Show("Пользователь удалён");
            }
        }

        private void OnCancel(object parameter)
        {
            _selectedUser = null;
            Password = "";
            FormTitle = "Управление пользователями";
            IsEditMode = false;
            OnPropertyChanged("SelectedUser");
            OnPropertyChanged("FormTitle");
        }

        private int GetRoleIdByName(string roleName)
        {
            foreach (var r in _context.Roles)
            {
                if (r.Name == roleName)
                {
                    return r.Id;
                }
            }
            return 2; // По умолчанию Manager
        }
    }
}
