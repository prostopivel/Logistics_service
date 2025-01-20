namespace Logistics_service.Models.Users
{
    public class Customer : User
    {
        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        public Customer() { }

        /// <summary>
        /// Конструктор для инициализации заказчика на основе объекта пользователя.
        /// </summary>
        /// <exception cref="ArgumentNullException">Выбрасывается, если user равен null.</exception>
        /// <exception cref="InvalidOperationException">Выбрасывается, если роль пользователя не является заказчиком.</exception>
        public Customer(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "Пользователь не может быть null.");
            }

            if (user.Role != UserRole.Customer)
            {
                throw new InvalidOperationException("Пользователь не заказчик!");
            }

            Name = user.Name;
            Role = user.Role;
            Email = user.Email;
            PasswordHash = user.PasswordHash;
        }
    }
}