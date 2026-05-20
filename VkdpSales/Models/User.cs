using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VkdpSales.Models
{
    public class User
    {
        [Key] public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Login { get; set; } = "";

        [Required, MaxLength(255)]
        public string Password { get; set; } = "";

        [Required, MaxLength(100)]
        public string FullName { get; set; } = "";

        public int RoleId { get; set; }
        public Role Role { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<SaleOrder> ManagedOrders { get; set; } = new List<SaleOrder>();
    }
}
