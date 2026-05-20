using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VkdpSales.Models
{
    public class Role
    {
        [Key] public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; } = "";

        [MaxLength(255)]
        public string Description { get; set; } = "";

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
