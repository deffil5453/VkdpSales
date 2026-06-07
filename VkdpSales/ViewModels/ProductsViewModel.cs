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
    public class ProductsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private readonly VkdpdbContext _context;
        private ObservableCollection<Product> _products;
        private Product _selectedProduct;
        private string _sku;
        private string _name;
        private int _categoryId;
        private string _unit;
        private decimal _price;
        private int _stock;
        private bool _isActive;
        private string _formTitle;
        private bool _isEditMode;

        public ObservableCollection<Product> Products { get => _products; set { _products = value; OnPropertyChanged("Products"); } }
        public Product SelectedProduct { get => _selectedProduct; set { _selectedProduct = value; OnPropertyChanged("SelectedProduct"); } }
        public string SKU { get => _sku; set { _sku = value; OnPropertyChanged("SKU"); } }
        public string Name { get => _name; set { _name = value; OnPropertyChanged("Name"); } }
        public int CategoryId { get => _categoryId; set { _categoryId = value; OnPropertyChanged("CategoryId"); } }
        public string Unit { get => _unit; set { _unit = value; OnPropertyChanged("Unit"); } }
        public decimal Price { get => _price; set { _price = value; OnPropertyChanged("Price"); } }
        public int Stock { get => _stock; set { _stock = value; OnPropertyChanged("Stock"); } }
        public bool IsActive { get => _isActive; set { _isActive = value; OnPropertyChanged("IsActive"); } }
        public string FormTitle { get => _formTitle; set { _formTitle = value; OnPropertyChanged("FormTitle"); } }
        public bool IsEditMode { get => _isEditMode; set { _isEditMode = value; OnPropertyChanged("IsEditMode"); } }
        // Фильтр по категориям
        private ObservableCollection<Category> _categories;
        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set { _categories = value; OnPropertyChanged("Categories"); }
        }

        private Category _selectedCategory;
        public Category SelectedCategory
        {
            get => _selectedCategory;
            set { _selectedCategory = value; OnPropertyChanged("SelectedCategory"); ApplyCategoryFilter(); }
        }
        public ICommand LoadCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        public ProductsViewModel()
        {
            _context = new VkdpdbContext();
            _products = new ObservableCollection<Product>();
            _isActive = true;
            _unit = "шт";
            _formTitle = "Добавление товара";
            _isEditMode = false;

            LoadCommand = new VKDPCommand(OnLoad);
            AddCommand = new VKDPCommand(OnAdd);
            EditCommand = new VKDPCommand(OnEdit);
            SaveCommand = new VKDPCommand(OnSave);
            DeleteCommand = new VKDPCommand(OnDelete);
            CancelCommand = new VKDPCommand(OnCancel);
            // Загружаем категории для фильтра
            _categories = new ObservableCollection<Category>();
            _categories.Add(new Category { Id = 0, Name = "Все категории" }); // Пункт "Все"
            foreach (var c in _context.Categories)
            {
                _categories.Add(c);
            }
            SelectedCategory = _categories[0]; // По умолчанию "Все"
            OnLoad(null);
        }
        private void ApplyCategoryFilter()
        {
            var filtered = new ObservableCollection<Product>();
            foreach (var p in _context.Products)
            {
                // Показываем, если выбрано "Все" (Id=0) или категория совпадает
                if (SelectedCategory == null || SelectedCategory.Id == 0 || p.CategoryId == SelectedCategory.Id)
                {
                    filtered.Add(p);
                }
            }
            Products = filtered;
            OnPropertyChanged("Products");
        }
        private void OnLoad(object parameter)
        {
            _products.Clear();
            foreach (var p in _context.Products)
            {
                _products.Add(p);
            }
            // Применяем текущий фильтр после загрузки
            ApplyCategoryFilter();
        }

        private void OnAdd(object parameter)
        {
            _selectedProduct = null;
            SKU = "";
            Name = "";
            CategoryId = 1; // По умолчанию
            Unit = "шт";
            Price = 0;
            Stock = 0;
            IsActive = true;
            FormTitle = "Добавление товара";

            // ✅ ИЗМЕНЕНО: true вместо false
            // Это покажет кнопки "Сохранить" и "Отмена"
            IsEditMode = true;

            // ✅ Уведомляем об изменении свойств
            OnPropertyChanged("SelectedProduct");
            OnPropertyChanged("IsEditMode");
            OnPropertyChanged("FormTitle");
        }

        private void OnEdit(object parameter)
        {
            if (_selectedProduct == null)
            {
                MessageBox.Show("Выберите товар для редактирования");
                return;
            }
            SKU = _selectedProduct.SKU;
            Name = _selectedProduct.Name;
            CategoryId = _selectedProduct.CategoryId;
            Unit = _selectedProduct.Unit;
            Price = _selectedProduct.BasePrice;
            Stock = _selectedProduct.CurrentStock;
            IsActive = _selectedProduct.IsActive;
            FormTitle = "Редактирование: " + _selectedProduct.Name;
            IsEditMode = true;
        }

        private void OnSave(object parameter)
        {
            if (string.IsNullOrEmpty(SKU) || string.IsNullOrEmpty(Name))
            {
                MessageBox.Show("Заполните артикул и название");
                return;
            }

            // ✅ ПРОВЕРКА: если _selectedProduct == null → добавляем новый товар
            if (_selectedProduct == null)
            {
                // Проверка на дубль артикула
                bool exists = false;
                foreach (var p in _context.Products)
                {
                    if (p.SKU == SKU)
                    {
                        exists = true;
                        break;
                    }
                }
                if (exists)
                {
                    MessageBox.Show("Товар с таким артикулом уже существует");
                    return;
                }

                var newProduct = new Product
                {
                    SKU = SKU,
                    Name = Name,
                    CategoryId = CategoryId,
                    Unit = Unit,
                    BasePrice = Price,
                    CurrentStock = Stock,
                    IsActive = IsActive
                };
                _context.Products.Add(newProduct);
            }
            else
            {
                // ✅ Редактирование существующего товара
                _selectedProduct.SKU = SKU;
                _selectedProduct.Name = Name;
                _selectedProduct.CategoryId = CategoryId;
                _selectedProduct.Unit = Unit;
                _selectedProduct.BasePrice = Price;
                _selectedProduct.CurrentStock = Stock;
                _selectedProduct.IsActive = IsActive;
            }

            _context.SaveChanges();
            OnLoad(null);
            OnCancel(null);
            MessageBox.Show("Товар сохранён");
        }

        private void OnDelete(object parameter)
        {
            if (_selectedProduct == null)
            {
                MessageBox.Show("Выберите товар для удаления");
                return;
            }

            MessageBoxResult result = MessageBox.Show("Удалить товар \"" + _selectedProduct.Name + "\"?", "Подтверждение", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                _context.Products.Remove(_selectedProduct);
                _context.SaveChanges();
                OnLoad(null);
                OnCancel(null);
                MessageBox.Show("Товар удалён");
            }
        }

        private void OnCancel(object parameter)
        {
            _selectedProduct = null;
            FormTitle = "Справочник товаров";
            IsEditMode = false;
            OnPropertyChanged("SelectedProduct");
            OnPropertyChanged("FormTitle");
        }
    }
}
