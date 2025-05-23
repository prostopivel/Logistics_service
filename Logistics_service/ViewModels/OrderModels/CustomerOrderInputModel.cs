﻿using System.ComponentModel.DataAnnotations;

namespace Logistics_service.ViewModels.OrderModels
{
    public class CustomerOrderInputModel
    {
        [Required(ErrorMessage = "BeginningAddress is required")]
        [StringLength(255, ErrorMessage = "BeginningAddress cannot be longer than 255 characters.")]
        public string BeginningAddress { get; set; }

        [Required(ErrorMessage = "DestinationAddress is required")]
        [StringLength(255, ErrorMessage = "DestinationAddress cannot be longer than 255 characters.")]
        public string DestinationAddress { get; set; }

        [Required(ErrorMessage = "ArrivalTime is required")]
        public DateTime ArrivalTime { get; set; }

        [StringLength(500, ErrorMessage = "Reason cannot be longer than 500 characters.")]
        public string? Reason { get; set; }
    }
}