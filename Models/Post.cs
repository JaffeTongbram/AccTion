using System;
using System.Collections.Generic;

namespace AccTion.Models;

public partial class Post
{
    public int PostId { get; set; }

    public int UserTableId { get; set; }

    public string? Image { get; set; }

    public string? Video { get; set; }

    public string? Caption { get; set; }

    public int? LikeCount { get; set; }

    public int? CommentCount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? SubPermission { get; set; }

    // public virtual ICollection<Interaction> Interactions { get; set; } = new List<Interaction>();

     public virtual UserTable UserTable { get; set; } = null!;
}
