using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using VkdpSales.Models;

namespace VkdpSales.Data
{
    public class VkdpdbContext : DbContext
    {
        public VkdpdbContext() 
        {
            Database.EnsureCreated();
        }
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<SaleOrder> SaleOrders { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // 🔹 Вариант 1: SQL Server LocalDB (рекомендуется для Windows)
            optionsBuilder.UseSqlServer(
                @"Server=(localdb)\mssqllocaldb;Database=VkdpSalesDb;Trusted_Connection=True;MultipleActiveResultSets=true");

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. Точность денежных полей и скидок
            modelBuilder.Entity<Product>().Property(p => p.BasePrice).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Client>().Property(c => c.DiscountPercent).HasColumnType("decimal(5,2)");
            modelBuilder.Entity<SaleOrder>().Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<SaleItem>().Property(s => s.UnitPrice).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<SaleItem>().Property(s => s.LineTotal).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<SaleItem>().Property(s => s.Discount).HasColumnType("decimal(5,2)");

            // 2. Запрет каскадного удаления для справочников (чтобы не удалялись товары при удалении категории)
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Products)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Client>()
                .HasMany(c => c.Orders)
                .WithOne(o => o.Client)
                .HasForeignKey(o => o.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.ManagedOrders)
                .WithOne(o => o.Seller)
                .HasForeignKey(o => o.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. Заполнение базовыми данными (Seed)
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin", Description = "Полный доступ к системе" },
                new Role { Id = 2, Name = "Manager", Description = "Оформление продаж, справочники" },
                new Role { Id = 3, Name = "Analyst", Description = "Только просмотр и отчёты" }
            );

            modelBuilder.Entity<User>().HasData(
                // PasswordHash: пока заглушка. Позже заменим на BCrypt хэш "admin123" и "manager123"
                new User { Id = 1, Login = "admin", Password = "admin123", FullName = "Администратор Системы", RoleId = 1, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
                new User { Id = 2, Login = "manager", Password = "manager123", FullName = "Иванов И.И.", RoleId = 2, IsActive = true, CreatedAt = new DateTime(2024, 2, 10) }
            );

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Плиты" },
                new Category { Id = 2, Name = "Мебель" },
                new Category { Id = 3, Name = "Офисная мебель" },
                new Category { Id = 4, Name = "Древесно-волокнистые плиты (ДВП)" },
                new Category { Id = 5, Name = "Цементно-стружечные плиты (ЦСП)" }
            );

            modelBuilder.Entity<Client>().HasData(
                new Client { Id = 1, Type = "B2B", Name = "ООО «СтройИнвест»", INN = "6165012345", Phone = "+7(863)111-11-11", Email = "zakaz@stroy.ru", Address = "г. Ростов-на-Дону, ул. Ленина, 10", DiscountPercent = 5.00m },
                new Client { Id = 2, Type = "B2C", Name = "Петров П.П.", INN = null, Phone = "+7(900)222-22-22", Email = "petrov@mail.ru", Address = "г. Волгодонск, пр. Курчатова, 5", DiscountPercent = 0.00m }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, SKU = "DVP-120", Name = "ДВП шлифованная 1200x2400 мм", CategoryId = 4, Unit = "шт", BasePrice = 450.00m, CurrentStock = 320, IsActive = true },
                new Product { Id = 2, SKU = "CSP-200", Name = "ЦСП 20 мм строительная", CategoryId = 5, Unit = "м²", BasePrice = 890.00m, CurrentStock = 150, IsActive = true },
                new Product { Id = 3, SKU = "MEB-101", Name = "Стол офисный «ВКДП-Стандарт»", CategoryId = 3, Unit = "шт", BasePrice = 8500.00m, CurrentStock = 15, IsActive = true },
                new Product { Id = 4, SKU = "MEB-205", Name = "Шкаф архивный металлический", CategoryId = 3, Unit = "шт", BasePrice = 12400.00m, CurrentStock = 8, IsActive = true }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}
