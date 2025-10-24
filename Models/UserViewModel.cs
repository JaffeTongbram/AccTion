using System;

namespace AccTion.Models
{
    public class UserViewModel
    {
        public int UserID { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public int UserType { get; set; }
        public int? SubscriptionTypeId { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PhotoPath { get; set; }
        public string? PhotoBase64 { get; set; }  // For displaying in view
    }
}