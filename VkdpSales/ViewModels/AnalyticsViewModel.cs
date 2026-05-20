using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using VkdpSales.Data;
using VkdpSales.Models;
using VkdpSales.ViewModels.Commands;

namespace VkdpSales.ViewModels
{
    public class AnalyticsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private readonly VkdpdbContext _context;
        private DateTime _startDate;
        private DateTime _endDate;
        private decimal _totalRevenue;
        private ObservableCollection<ProductSalesDto> _topProducts;
        private ObservableCollection<ManagerSalesDto> _managerStats;
        private string _statusText;

        public DateTime StartDate { get => _startDate; set { _startDate = value; OnPropertyChanged("StartDate"); } }
        public DateTime EndDate { get => _endDate; set { _endDate = value; OnPropertyChanged("EndDate"); } }
        public decimal TotalRevenue { get => _totalRevenue; set { _totalRevenue = value; OnPropertyChanged("TotalRevenue"); } }
        public ObservableCollection<ProductSalesDto> TopProducts { get => _topProducts; set { _topProducts = value; OnPropertyChanged("TopProducts"); } }
        public ObservableCollection<ManagerSalesDto> ManagerStats { get => _managerStats; set { _managerStats = value; OnPropertyChanged("ManagerStats"); } }
        public string StatusText { get => _statusText; set { _statusText = value; OnPropertyChanged("StatusText"); } }

        public ICommand GenerateCommand { get; set; }
        public ICommand ExportCommand { get; set; }

        public AnalyticsViewModel()
        {
            _context = new VkdpdbContext();
            _topProducts = new ObservableCollection<ProductSalesDto>();
            _managerStats = new ObservableCollection<ManagerSalesDto>();
            _startDate = DateTime.Now.AddMonths(-1);
            _endDate = DateTime.Now;
            _statusText = "Выберите период и нажмите 'Сформировать'";

            GenerateCommand = new VKDPCommand(OnGenerate);
            ExportCommand = new VKDPCommand(OnExport);
        }

        private void OnGenerate(object parameter)
        {
            TopProducts.Clear();
            ManagerStats.Clear();
            TotalRevenue = 0;
            StatusText = "Загрузка данных...";

            // 1. Загружаем заказы за период
            var orders = new List<SaleOrder>();
            foreach (var o in _context.SaleOrders)
            {
                if (o.OrderDate >= StartDate.Date && o.OrderDate <= EndDate.Date.AddDays(1))
                {
                    // Подгружаем связанные данные
                    o.Client = _context.Clients.Find(o.ClientId);
                    o.Seller = _context.Users.Find(o.SellerId);
                    orders.Add(o);
                }
            }

            // 2. Загружаем позиции заказов
            var items = new System.Collections.Generic.List<SaleItem>();
            foreach (var o in orders)
            {
                foreach (var i in _context.SaleItems)
                {
                    if (i.OrderId == o.Id)
                    {
                        i.Product = _context.Products.Find(i.ProductId);
                        items.Add(i);
                    }
                }
            }

            // 3. Считаем общую выручку
            foreach (var item in items)
            {
                TotalRevenue += item.LineTotal;
            }

            // 4. Топ-5 товаров по количеству
            var productCounts = new System.Collections.Generic.Dictionary<int, ProductSalesDto>();
            foreach (var item in items)
            {
                if (item.Product != null)
                {
                    if (productCounts.ContainsKey(item.ProductId) == false)
                    {
                        productCounts.Add(item.ProductId, new ProductSalesDto { Name = item.Product.Name, Quantity = 0, Total = 0 });
                    }
                    productCounts[item.ProductId].Quantity += item.Quantity;
                    productCounts[item.ProductId].Total += item.LineTotal;
                }
            }

            // ✅ Копируем значения словаря во временный список для сортировки
            var tempList = new System.Collections.Generic.List<ProductSalesDto>();
            foreach (var dto in productCounts.Values)
            {
                tempList.Add(dto);
            }

            // ✅ Сортировка пузырьком по убыванию количества (без лямбд)
            for (int i = 0; i < tempList.Count - 1; i++)
            {
                for (int j = 0; j < tempList.Count - 1 - i; j++)
                {
                    if (tempList[j].Quantity < tempList[j + 1].Quantity)
                    {
                        // Меняем местами
                        ProductSalesDto temp = tempList[j];
                        tempList[j] = tempList[j + 1];
                        tempList[j + 1] = temp;
                    }
                }
            }

            // ✅ Заполняем ObservableCollection уже отсортированными данными (топ-5)
            TopProducts.Clear();
            int limit = tempList.Count < 5 ? tempList.Count : 5;
            for (int i = 0; i < limit; i++)
            {
                TopProducts.Add(tempList[i]);
            }

            // Оставляем только топ-5
            while (TopProducts.Count > 5)
            {
                TopProducts.RemoveAt(5);
            }

            // 5. Статистика по менеджерам
            var managerCounts = new System.Collections.Generic.Dictionary<int, ManagerSalesDto>();
            foreach (var order in orders)
            {
                if (order.Seller != null)
                {
                    if (managerCounts.ContainsKey(order.SellerId) == false)
                    {
                        managerCounts.Add(order.SellerId, new ManagerSalesDto { Name = order.Seller.FullName, OrdersCount = 0, Total = 0 });
                    }
                    managerCounts[order.SellerId].OrdersCount++;
                    managerCounts[order.SellerId].Total += order.TotalAmount;
                }
            }
            foreach (var dto in managerCounts.Values)
            {
                ManagerStats.Add(dto);
            }

            StatusText = "Отчёт сформирован за " + StartDate.ToString("dd.MM") + " — " + EndDate.ToString("dd.MM.yyyy");
        }

        private void OnExport(object parameter)
        {
            if (TopProducts.Count == 0 && ManagerStats.Count == 0)
            {
                MessageBox.Show("Сначала сформируйте отчёт");
                return;
            }

            var dialog = new SaveFileDialog();
            dialog.Filter = "CSV файлы|*.csv";
            dialog.FileName = "VKDP_Analytics_" + DateTime.Now.ToString("yyyyMMdd") + ".csv";
            dialog.DefaultExt = ".csv";

            if (dialog.ShowDialog() == true)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Отчёт по продажам ООО ВКДП");
                sb.AppendLine("Период: " + StartDate.ToString("dd.MM.yyyy") + " - " + EndDate.ToString("dd.MM.yyyy"));
                sb.AppendLine("Общая выручка: " + TotalRevenue.ToString("F2") + " ₽");
                sb.AppendLine();

                sb.AppendLine("Топ товаров:");
                sb.AppendLine("Название;Количество;Сумма");
                foreach (var p in TopProducts)
                {
                    sb.AppendLine(p.Name + ";" + p.Quantity + ";" + p.Total.ToString("F2"));
                }
                sb.AppendLine();

                sb.AppendLine("Эффективность менеджеров:");
                sb.AppendLine("ФИО;Заказов;Сумма");
                foreach (var m in ManagerStats)
                {
                    sb.AppendLine(m.Name + ";" + m.OrdersCount + ";" + m.Total.ToString("F2"));
                }

                // Запись в файл в кодировке UTF-8 с BOM (чтобы Excel открывал кириллицу корректно)
                File.WriteAllText(dialog.FileName, sb.ToString(), new System.Text.UTF8Encoding(true));
                MessageBox.Show("Отчёт сохранён: " + dialog.FileName);
            }
        }
    }

    // Вспомогательные классы для отображения в таблице
    public class ProductSalesDto
    {
        public string Name { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Total { get; set; }
    }

    public class ManagerSalesDto
    {
        public string Name { get; set; } = "";
        public int OrdersCount { get; set; }
        public decimal Total { get; set; }
    }
}
