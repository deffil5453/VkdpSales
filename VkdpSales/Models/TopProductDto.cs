using System;
using System.Collections.Generic;
using System.Text;

namespace VkdpSales.Models
{
    public class TopProductDto
    {
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public int QuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
