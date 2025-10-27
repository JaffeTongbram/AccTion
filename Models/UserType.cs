using System;
using System.Collections.Generic;

namespace AccTion.Models;

public partial class UserType
{
    public int Id { get; set; }

    public string TypeName { get; set; } = null!;

    //public virtual ICollection<UserTable> UserTables { get; set; } = new List<UserTable>();
}
