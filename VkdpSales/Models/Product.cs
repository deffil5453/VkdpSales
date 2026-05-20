using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VkdpSales.Models
{
    public class Product
    {
        [Key] public int Id { get; set; }

        [Required, MaxLength(50)]
        public string SKU { get; set; } = "";

        [Required, MaxLength(150)]
        public string Name { get; set; } = "";

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        [Required, MaxLength(20)]
        public string Unit { get; set; } = "шт";

        [Column(TypeName = "decimal(18,2)")]
        public decimal BasePrice { get; set; }

        public int CurrentStock { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    }
}
