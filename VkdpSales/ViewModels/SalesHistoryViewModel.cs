using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using VkdpSales.Data;
using VkdpSales.Models;
using VkdpSales.ViewModels.Commands;

namespace VkdpSales.ViewModels
{
    public class SalesHistoryViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private readonly VkdpdbContext _context;
        private ObservableCollection<SaleOrder> _allOrders;
        private ObservableCollection<SaleOrder> _filteredOrders;
        private ObservableCollection<Client> _clients;
        private Client _selectedClient;
        private DateTime _startDate;
        private DateTime _endDate;
        private string _selectedStatus;
        private string _searchText;

        public ObservableCollection<SaleOrder> FilteredOrders
        {
            get => _filteredOrders;
            set { _filteredOrders = value; OnPropertyChanged("FilteredOrders"); }
        }
        public ObservableCollection<Client> Clients { get => _clients; set { _clients = value; OnPropertyChanged("Clients"); } }
        public Client SelectedClient { get => _selectedClient; set { _selectedClient = value; OnPropertyChanged("SelectedClient"); } }
        public DateTime StartDate { get => _startDate; set { _startDate = value; OnPropertyChanged("StartDate"); } }
        public DateTime EndDate { get => _endDate; set { _endDate = value; OnPropertyChanged("EndDate"); } }
        public string SelectedStatus { get => _selectedStatus; set { _selectedStatus = value; OnPropertyChanged("SelectedStatus"); } }
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged("SearchText"); } }

        public ObservableCollection<string> Statuses { get; set; }
        public ICommand FilterCommand { get; set; }
        public ICommand RefreshCommand { get; set; }

        public SalesHistoryViewModel()
        {
            _context = new VkdpdbContext();
            _filteredOrders = new ObservableCollection<SaleOrder>();
            _allOrders = new ObservableCollection<SaleOrder>();

            Statuses = new ObservableCollection<string> { "Все", "New", "Paid", "Shipped", "Completed", "Cancelled" };
            _selectedStatus = "Все";

            LoadData();

            FilterCommand = new VKDPCommand(OnFilter);
            RefreshCommand = new VKDPCommand(OnRefresh);
        }

        private void LoadData()
        {
            _allOrders.Clear();
            // Загружаем заказы вместе с клиентами для отображения
            foreach (var order in _context.SaleOrders)
            {
                // Явная подгрузка связанного клиента
                order.Client = _context.Clients.Find(order.ClientId);
                _allOrders.Add(order);
            }

            _clients = new ObservableCollection<Client>(_context.Clients);

            // Даты по умолчанию: последний месяц
            _startDate = DateTime.Now.AddMonths(-1);
            _endDate = DateTime.Now;

            ApplyFilters();
        }

        private void OnFilter(object parameter) { ApplyFilters(); }
        private void OnRefresh(object parameter) { LoadData(); }

        private void ApplyFilters()
        {
            _filteredOrders.Clear();

            foreach (var order in _allOrders)
            {
                bool dateMatch = order.OrderDate.Date >= _startDate.Date && order.OrderDate.Date <= _endDate.Date;
                bool clientMatch = _selectedClient == null || order.ClientId == _selectedClient.Id;
                bool statusMatch = _selectedStatus == "Все" || order.Status == _selectedStatus;
                bool searchMatch = string.IsNullOrEmpty(_searchText) ||
                                   order.OrderNumber.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                                   (order.Comment != null && order.Comment.Contains(_searchText, StringComparison.OrdinalIgnoreCase));

                if (dateMatch && clientMatch && statusMatch && searchMatch)
                {
                    _filteredOrders.Add(order);
                }
            }
            OnPropertyChanged("FilteredOrders");
        }
    }
}
