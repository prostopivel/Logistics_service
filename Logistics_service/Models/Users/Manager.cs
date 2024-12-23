using System.Drawing;

namespace Logistics_service.Models.Users
{
    public class Manager : User
    {
        public Manager() { }

        public Manager(User user)
        {
            if (user.Role != UserRole.Manager)
            {
                throw new Exception("Пользователь не менеджер!");
            }

            Name = user.Name;
            Role = user.Role;
            Email = user.Email;
            PasswordHash = user.PasswordHash;
        }
    }
}
