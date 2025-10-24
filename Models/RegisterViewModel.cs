using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccTion.Models
{
    public class RegisterViewModel
   {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? PNo { get; set; } = string.Empty;
        public int UserTypeId { get; set; }  // Required
        public int? SubscriptionTypeId { get; set; }

    }
}