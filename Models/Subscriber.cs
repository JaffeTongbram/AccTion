using System;
using System.Collections.Generic;

namespace AccTion.Models;

public partial class Subscriber
{
    public int SubId { get; set; }

    public int SubscribeBy { get; set; }

    public int SubscribeTo { get; set; }

    // public virtual UserTable SubscribeByNavigation { get; set; } = null!;

    // public virtual UserTable SubscribeToNavigation { get; set; } = null!;
}
