using System;
using System.Collections.Generic;

namespace AccTion.Models;

public partial class UserTable
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? PNo { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? SubscriptionTypeId { get; set; }

    public int? UserTypeId { get; set; }

    public string Password { get; set; } = null!;

    public string? PhotoPath { get; set; }

    public byte[]? PhotoData { get; set; }

    // public virtual ICollection<Interaction> Interactions { get; set; } = new List<Interaction>();

    // public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    // public virtual ICollection<Subscriber> SubscriberSubscribeByNavigations { get; set; } = new List<Subscriber>();

    // public virtual ICollection<Subscriber> SubscriberSubscribeToNavigations { get; set; } = new List<Subscriber>();

    // public virtual SubscriptionType? SubscriptionType { get; set; }

    // public virtual UserType? UserType { get; set; }
}
