﻿namespace Logistics_service.Models.Users
{
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public UserRole Role { get; set; }

        public string Email { get; set; }

        public string PasswordHash { get; set; }
    }
}