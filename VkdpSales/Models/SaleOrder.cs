using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VkdpSales.Models
{
    public class SaleOrder
    {
        [Key] public int Id { get; set; }

        [Required, MaxLength(50)]
        public string OrderNumber { get; set; } = "";

        public int ClientId { get; set; }
        public Client Client { get; set; }

        public int SellerId { get; set; }
        public User Seller { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required, MaxLength(20)]
        public string Status { get; set; } = "New"; // New, Paid, Shipped, Completed, Cancelled

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [MaxLength(500)]
        public string? Comment { get; set; }

        public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
    }
}
