using System;
using System.Collections.Generic;
using System.Text;
using VkdpSales.Data;
using VkdpSales.Models;

namespace VkdpSales.Services
{
    public class AuthService
    {
        private VkdpdbContext context;

        public AuthService()
        {
            context = new VkdpdbContext();
        }

        public User Authenticate(string login, string password)
        {
            User foundUser = null;

            // ✅ Поиск без LINQ-лямбд, через явный перебор
            foreach (User u in context.Users)
            {
                if (u.Login == login && u.Password == password && u.IsActive == true)
                {
                    foundUser = u;
                    break;
                }
            }

            // ✅ Загрузка роли вручную
            if (foundUser != null)
            {
                foreach (Role r in context.Roles)
                {
                    if (r.Id == foundUser.RoleId)
                    {
                        foundUser.Role = r;
                        break;
                    }
                }
            }

            return foundUser;
        }
    }
}
