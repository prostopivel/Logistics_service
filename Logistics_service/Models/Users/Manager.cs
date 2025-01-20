using System;

namespace Logistics_service.Models.Users
{
    public class Manager : User
    {
        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        public Manager() { }

        /// <summary>
        /// Конструктор для инициализации менеджера на основе объекта пользователя.
        /// </summary>
        /// <exception cref="ArgumentNullException">Выбрасывается, если user равен null.</exception>
        /// <exception cref="InvalidOperationException">Выбрасывается, если роль пользователя не является менеджером.</exception>
        public Manager(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "Пользователь не может быть null.");
            }

            if (user.Role != UserRole.Manager)
            {
                throw new InvalidOperationException("Пользователь не менеджер!");
            }

            Name = user.Name;
            Role = user.Role;
            Email = user.Email;
            PasswordHash = user.PasswordHash;
        }
    }
}