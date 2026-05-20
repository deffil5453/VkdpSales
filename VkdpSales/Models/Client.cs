using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VkdpSales.Models
{
    public class Client
    {
        [Key] public int Id { get; set; }

        [Required, MaxLength(20)]
        public string Type { get; set; } = "B2C"; // B2B или B2C

        [Required, MaxLength(150)]
        public string Name { get; set; } = "";

        [MaxLength(12)]
        public string? INN { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; } = "";

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal DiscountPercent { get; set; } = 0;

        public ICollection<SaleOrder> Orders { get; set; } = new List<SaleOrder>();
    }
}
