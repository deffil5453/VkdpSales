using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using VkdpSales.Data;
using VkdpSales.Models;

namespace VkdpSales.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private readonly VkdpdbContext _context;

        // --- Свойства для интерфейса ---
        private decimal _todayRevenue;
        public decimal TodayRevenue
        {
            get => _todayRevenue;
            set { _todayRevenue = value; OnPropertyChanged(nameof(TodayRevenue)); }
        }

        private int _todayOrders;
        public int TodayOrders
        {
            get => _todayOrders;
            set { _todayOrders = value; OnPropertyChanged(nameof(TodayOrders)); }
        }

        private ObservableCollection<TopProductDto> _topProducts;
        public ObservableCollection<TopProductDto> TopProducts
        {
            get => _topProducts;
            set { _topProducts = value; OnPropertyChanged(nameof(TopProducts)); }
        }

        public DashboardViewModel()
        {
            _context = new VkdpdbContext();
            TopProducts = new ObservableCollection<TopProductDto>();
            LoadAnalytics();
        }

        private void LoadAnalytics()
        {
            // 1. Считаем метрики за СЕГОДНЯ
            var today = System.DateTime.Today;
            decimal revenue = 0;
            int orders = 0;

            foreach (var order in _context.SaleOrders)
            {
                if (order.OrderDate.Date == today && order.Status != "Cancelled")
                {
                    revenue += order.TotalAmount;
                    orders++;
                }
            }
            TodayRevenue = revenue;
            TodayOrders = orders;

            // 2. Собираем статистику по товарам (Топ-5 ходовых)
            var productStats = new Dictionary<int, TopProductDto>();

            foreach (var item in _context.SaleItems)
            {
                var product = _context.Products.Find(item.ProductId);
                if (product != null)
                {
                    // Если товара еще нет в словаре, добавляем
                    if (!productStats.ContainsKey(product.Id))
                    {
                        productStats.Add(product.Id, new TopProductDto
                        {
                            Name = product.Name,
                            Category = product.Category?.Name ?? "Общее",
                            QuantitySold = 0,
                            TotalRevenue = 0
                        });
                    }

                    // Прибавляем проданное количество и деньги
                    productStats[product.Id].QuantitySold += item.Quantity;
                    productStats[product.Id].TotalRevenue += item.LineTotal;
                }
            }

            // Сортируем по количеству продаж (по убыванию)
            var sortedList = new List<TopProductDto>(productStats.Values);
            sortedList.Sort((a, b) => b.QuantitySold.CompareTo(a.QuantitySold));

            // Берем только первые 5
            TopProducts.Clear();
            for (int i = 0; i < 5 && i < sortedList.Count; i++)
            {
                TopProducts.Add(sortedList[i]);
            }
        }
    }
}
