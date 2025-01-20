namespace Logistics_service.Models.Users
{
    public class Administrator : User
    {
        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        public Administrator() { }

        /// <summary>
        /// Конструктор для инициализации администратора на основе объекта пользователя.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public Administrator(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "Пользователь не может быть null.");
            }

            if (user.Role != UserRole.Administrator)
            {
                throw new InvalidOperationException("Пользователь не администратор!");
            }

            Name = user.Name;
            Role = user.Role;
            Email = user.Email;
            PasswordHash = user.PasswordHash;
        }
    }
}