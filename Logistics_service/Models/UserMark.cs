using Logistics_service.Models.Users;

namespace Logistics_service.Models
{
    public class UserMark : Marker
    {
        public UserMark() : base()
        {

        }

        public UserMark(User? user, Point? point) : base(user, point)
        {

        }
    }
}
