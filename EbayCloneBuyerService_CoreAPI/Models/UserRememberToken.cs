using System;
using System.Collections.Generic;

namespace EbayCloneBuyerService_CoreAPI.Models;

public partial class UserRememberToken
{
    public int TokenId { get; set; }

    public int UserId { get; set; }

    public string TokenHash { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
