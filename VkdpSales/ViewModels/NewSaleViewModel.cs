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
    public class NewSaleViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private readonly VkdpdbContext _context;
        private ObservableCollection<Client> _clients;
        private Client _selectedClient;
        private ObservableCollection<Product> _products;
        private Product _selectedProduct;
        private ObservableCollection<SaleItem> _cartItems;
        private int _quantity = 1;
        private decimal _unitPrice;
        private decimal _discount;
        private decimal _lineTotal;
        private decimal _orderTotal;
        private string _comment;

        public ObservableCollection<Client> Clients { get => _clients; set { _clients = value; OnPropertyChanged("Clients"); } }
        public Client SelectedClient { get => _selectedClient; set { _selectedClient = value; OnPropertyChanged("SelectedClient"); } }
        public ObservableCollection<Product> Products { get => _products; set { _products = value; OnPropertyChanged("Products"); } }
        public Product SelectedProduct { get => _selectedProduct; set { _selectedProduct = value; OnPropertyChanged("SelectedProduct"); OnProductSelected(); } }
        public ObservableCollection<SaleItem> CartItems { get => _cartItems; set { _cartItems = value; OnPropertyChanged("CartItems"); } }
        public int Quantity { get => _quantity; set { _quantity = value; OnPropertyChanged("Quantity"); CalculateLineTotal(); } }
        public decimal UnitPrice { get => _unitPrice; set { _unitPrice = value; OnPropertyChanged("UnitPrice"); CalculateLineTotal(); } }
        public decimal Discount { get => _discount; set { _discount = value; OnPropertyChanged("Discount"); CalculateLineTotal(); } }
        public decimal LineTotal { get => _lineTotal; set { _lineTotal = value; OnPropertyChanged("LineTotal"); } }
        public decimal OrderTotal { get => _orderTotal; set { _orderTotal = value; OnPropertyChanged("OrderTotal"); } }
        public string Comment { get => _comment; set { _comment = value; OnPropertyChanged("Comment"); } }

        public ICommand AddToCartCommand { get; set; }
        public ICommand RemoveFromCartCommand { get; set; }
        public ICommand SaveOrderCommand { get; set; }
        public ICommand ClearCartCommand { get; set; }

        public NewSaleViewModel()
        {
            _context = new VkdpdbContext();
            _cartItems = new ObservableCollection<SaleItem>();
            _orderTotal = 0;

            LoadData();

            AddToCartCommand = new VKDPCommand(OnAddToCart);
            RemoveFromCartCommand = new VKDPCommand(OnRemoveFromCart);
            SaveOrderCommand = new VKDPCommand(OnSaveOrder);
            ClearCartCommand = new VKDPCommand(OnClearCart);
        }

        private void LoadData()
        {
            _clients = new ObservableCollection<Client>();
            int clientCount = 0;
            foreach (var c in _context.Clients)
            {
                _clients.Add(c);
                clientCount++;
            }
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Загружено клиентов: {clientCount}");

            _products = new ObservableCollection<Product>();
            int productCount = 0;
            foreach (var p in _context.Products)
            {
                if (p.IsActive && p.CurrentStock > 0)
                {
                    _products.Add(p);
                    productCount++;
                }
            }
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Загружено товаров: {productCount}");
        }

        private void OnProductSelected()
        {
            if (_selectedProduct != null)
            {
                _unitPrice = _selectedProduct.BasePrice;
                _quantity = 1;
                _discount = 0;
                CalculateLineTotal();
            }
        }

        private void CalculateLineTotal()
        {
            if (_quantity > 0 && _unitPrice > 0)
            {
                decimal subtotal = _quantity * _unitPrice;
                if (_discount > 0 && _discount <= 100)
                {
                    _lineTotal = subtotal * (1 - _discount / 100);
                }
                else
                {
                    _lineTotal = subtotal;
                }
            }
            else
            {
                _lineTotal = 0;
            }
            OnPropertyChanged("LineTotal");
        }

        private void OnAddToCart(object parameter)
        {
            if (_selectedProduct == null) { MessageBox.Show("Выберите товар"); return; }
            if (_quantity <= 0) { MessageBox.Show("Количество должно быть больше 0"); return; }
            if (_quantity > _selectedProduct.CurrentStock) { MessageBox.Show("Недостаточно товара на складе. Доступно: " + _selectedProduct.CurrentStock); return; }

            var newItem = new SaleItem
            {
                ProductId = _selectedProduct.Id,
                Product = _selectedProduct, // Для отображения в UI
                Quantity = _quantity,
                UnitPrice = _unitPrice,
                Discount = _discount,
                LineTotal = _lineTotal
            };

            CartItems.Add(newItem);
            RecalculateOrderTotal();

            // Сброс полей ввода
            _selectedProduct = null;
            _quantity = 1;
            _unitPrice = 0;
            _discount = 0;
            _lineTotal = 0;
            OnPropertyChanged("SelectedProduct");
            OnPropertyChanged("Quantity");
            OnPropertyChanged("UnitPrice");
            OnPropertyChanged("Discount");
            OnPropertyChanged("LineTotal");
        }

        private void OnRemoveFromCart(object parameter)
        {
            if (parameter is SaleItem item)
            {
                CartItems.Remove(item);
                RecalculateOrderTotal();
            }
        }

        private void RecalculateOrderTotal()
        {
            _orderTotal = 0;
            foreach (var item in CartItems)
            {
                _orderTotal += item.LineTotal;
            }
            OnPropertyChanged("OrderTotal");
        }

        private void OnClearCart(object parameter)
        {
            CartItems.Clear();
            _orderTotal = 0;
            OnPropertyChanged("CartItems");
            OnPropertyChanged("OrderTotal");
        }

        private void OnSaveOrder(object parameter)
        {
            if (_selectedClient == null) { MessageBox.Show("Выберите клиента"); return; }
            if (CartItems.Count == 0) { MessageBox.Show("Корзина пуста"); return; }

            string orderNumber = GenerateOrderNumber();

            var newOrder = new SaleOrder
            {
                OrderNumber = orderNumber,
                ClientId = _selectedClient.Id,
                SellerId = 2, // TODO: Подставьте ID текущего пользователя
                OrderDate = DateTime.Now,
                Status = "New",
                TotalAmount = _orderTotal,
                Comment = _comment ?? ""
            };

            foreach (var cartItem in CartItems)
            {
                var orderItem = new SaleItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.UnitPrice,
                    Discount = cartItem.Discount,
                    LineTotal = cartItem.LineTotal
                };
                newOrder.Items.Add(orderItem);

                // Обновляем остаток на складе
                var product = _context.Products.Find(cartItem.ProductId);
                if (product != null)
                {
                    product.CurrentStock -= cartItem.Quantity;
                }
            }

            _context.SaleOrders.Add(newOrder);
            _context.SaveChanges();

            MessageBox.Show("Заказ " + orderNumber + " успешно создан!");
            OnClearCart(null);
            _comment = "";
            OnPropertyChanged("Comment");
        }

        private string GenerateOrderNumber()
        {
            int year = DateTime.Now.Year;
            int count = 0;
            foreach (var o in _context.SaleOrders)
            {
                if (o.OrderDate.Year == year) count++;
            }
            return "VKDP-" + year + "-" + (count + 1).ToString("D5");
        }
    }
}
