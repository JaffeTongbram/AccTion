using System;
using System.Collections.Generic;

namespace AccTion.Models;

public partial class SubscriptionType
{
    public int Id { get; set; }

    public string Level { get; set; } = null!;

    public decimal Price { get; set; }

    public TimeSpan Validity { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    // public virtual ICollection<UserTable> UserTables { get; set; } = new List<UserTable>();
}
