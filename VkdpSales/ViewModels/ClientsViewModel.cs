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
    public class ClientsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private readonly VkdpdbContext _context;
        private ObservableCollection<Client> _clients;
        private Client _selectedClient;
        private string _type;
        private string _name;
        private string _inn;
        private string _phone;
        private string _email;
        private string _address;
        private decimal _discount;
        private string _formTitle;
        private bool _isEditMode;

        public ObservableCollection<Client> Clients { get => _clients; set { _clients = value; OnPropertyChanged("Clients"); } }
        public Client SelectedClient { get => _selectedClient; set { _selectedClient = value; OnPropertyChanged("SelectedClient"); } }
        public string Type { get => _type; set { _type = value; OnPropertyChanged("Type"); } }
        public string Name { get => _name; set { _name = value; OnPropertyChanged("Name"); } }
        public string INN { get => _inn; set { _inn = value; OnPropertyChanged("INN"); } }
        public string Phone { get => _phone; set { _phone = value; OnPropertyChanged("Phone"); } }
        public string Email { get => _email; set { _email = value; OnPropertyChanged("Email"); } }
        public string Address { get => _address; set { _address = value; OnPropertyChanged("Address"); } }
        public decimal Discount { get => _discount; set { _discount = value; OnPropertyChanged("Discount"); } }
        public string FormTitle { get => _formTitle; set { _formTitle = value; OnPropertyChanged("FormTitle"); } }
        public bool IsEditMode { get => _isEditMode; set { _isEditMode = value; OnPropertyChanged("IsEditMode"); } }

        public ObservableCollection<string> ClientTypes { get; set; }
        public ICommand LoadCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        public ClientsViewModel()
        {
            _context = new VkdpdbContext();
            _clients = new ObservableCollection<Client>();
            _type = "B2C";
            _discount = 0;
            _formTitle = "Справочник клиентов";
            _isEditMode = false;

            ClientTypes = new ObservableCollection<string>();
            ClientTypes.Add("B2C");
            ClientTypes.Add("B2B");

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
            _clients.Clear();
            foreach (var c in _context.Clients)
            {
                _clients.Add(c);
            }
        }

        private void OnAdd(object parameter)
        {
            _selectedClient = null;
            Type = "B2C";
            Name = "";
            INN = "";
            Phone = "";
            Email = "";
            Address = "";
            Discount = 0;
            FormTitle = "Добавление клиента";
            IsEditMode = false;
            OnPropertyChanged("SelectedClient");
        }

        private void OnEdit(object parameter)
        {
            if (_selectedClient == null)
            {
                MessageBox.Show("Выберите клиента для редактирования");
                return;
            }
            Type = _selectedClient.Type;
            Name = _selectedClient.Name;
            INN = _selectedClient.INN ?? "";
            Phone = _selectedClient.Phone;
            Email = _selectedClient.Email ?? "";
            Address = _selectedClient.Address ?? "";
            Discount = _selectedClient.DiscountPercent;
            FormTitle = "Редактирование: " + _selectedClient.Name;
            IsEditMode = true;
        }

        private void OnSave(object parameter)
        {
            if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Phone))
            {
                MessageBox.Show("Заполните имя и телефон");
                return;
            }
            if (Type == "B2B" && string.IsNullOrEmpty(INN))
            {
                MessageBox.Show("Для юрлица обязательно указать ИНН");
                return;
            }

            if (IsEditMode == false)
            {
                // Проверка на дубль (по имени + телефону)
                bool exists = false;
                foreach (var c in _context.Clients)
                {
                    if (c.Name == Name && c.Phone == Phone)
                    {
                        exists = true;
                        break;
                    }
                }
                if (exists)
                {
                    MessageBox.Show("Клиент с таким именем и телефоном уже существует");
                    return;
                }

                var newClient = new Client
                {
                    Type = Type,
                    Name = Name,
                    INN = string.IsNullOrEmpty(INN) ? null : INN,
                    Phone = Phone,
                    Email = string.IsNullOrEmpty(Email) ? null : Email,
                    Address = string.IsNullOrEmpty(Address) ? null : Address,
                    DiscountPercent = Discount
                };
                _context.Clients.Add(newClient);
            }
            else
            {
                _selectedClient.Type = Type;
                _selectedClient.Name = Name;
                _selectedClient.INN = string.IsNullOrEmpty(INN) ? null : INN;
                _selectedClient.Phone = Phone;
                _selectedClient.Email = string.IsNullOrEmpty(Email) ? null : Email;
                _selectedClient.Address = string.IsNullOrEmpty(Address) ? null : Address;
                _selectedClient.DiscountPercent = Discount;
            }

            _context.SaveChanges();
            OnLoad(null);
            OnCancel(null);
            MessageBox.Show("Клиент сохранён");
        }

        private void OnDelete(object parameter)
        {
            if (_selectedClient == null)
            {
                MessageBox.Show("Выберите клиента для удаления");
                return;
            }

            // Проверка: есть ли заказы у этого клиента
            bool hasOrders = false;
            foreach (var o in _context.SaleOrders)
            {
                if (o.ClientId == _selectedClient.Id)
                {
                    hasOrders = true;
                    break;
                }
            }
            if (hasOrders)
            {
                MessageBox.Show("Нельзя удалить: у клиента есть история заказов");
                return;
            }

            MessageBoxResult result = MessageBox.Show("Удалить клиента \"" + _selectedClient.Name + "\"?", "Подтверждение", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                _context.Clients.Remove(_selectedClient);
                _context.SaveChanges();
                OnLoad(null);
                OnCancel(null);
                MessageBox.Show("Клиент удалён");
            }
        }

        private void OnCancel(object parameter)
        {
            _selectedClient = null;
            FormTitle = "Справочник клиентов";
            IsEditMode = false;
            OnPropertyChanged("SelectedClient");
            OnPropertyChanged("FormTitle");
        }
    }
}
