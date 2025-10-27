using System;
using System.Collections.Generic;

namespace AccTion.Models;

public partial class Interaction
{
    public int InteractionId { get; set; }

    public int PostId { get; set; }

    public int InteractionUserId { get; set; }

    public int? Likenum { get; set; }

    public int? Commentnum { get; set; }
    public string? CommentText { get; set; }
    public DateTime? CreatedAt { get; set; }

    // public virtual UserTable InteractionUser { get; set; } = null!;

    // public virtual Post Post { get; set; } = null!;
}
