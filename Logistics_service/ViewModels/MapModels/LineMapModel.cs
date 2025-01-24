using System.ComponentModel.DataAnnotations;

namespace Logistics_service.ViewModels.MapModels
{
    public class LineMapModel
    {
        [Required(ErrorMessage = "Start is required")]
        public int Start { get; set; }

        [Required(ErrorMessage = "End is required")]
        public int End { get; set; }
    }
}
