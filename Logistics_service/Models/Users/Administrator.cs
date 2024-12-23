namespace Logistics_service.Models.Users
{
    public class Administrator : User
    {
        public Administrator() { }

        public Administrator(User user)
        {
            if (user.Role != UserRole.Administrator)
            {
                throw new Exception("Пользователь не администратор!");
            }

            Name = user.Name;
            Role = user.Role;
            Email = user.Email;
            PasswordHash = user.PasswordHash;
        }
    }
}
