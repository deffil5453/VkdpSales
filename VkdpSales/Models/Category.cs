using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VkdpSales.Models
{
    public class Category
    {
        [Key] public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = "";

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
