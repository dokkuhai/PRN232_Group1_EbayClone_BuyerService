using System;
using System.Collections.Generic;

namespace EbayCloneBuyerService_CoreAPI.Models;

public partial class Address1
{
    public int AddressId { get; set; }

    public int UserId { get; set; }

    public string AddressType { get; set; } = null!;

    public string AddressLine { get; set; } = null!;

    public string? City { get; set; }

    public string? State { get; set; }

    public string? PostalCode { get; set; }

    public string? Country { get; set; }

    public DateTime? LastUpdated { get; set; }

    public virtual User User { get; set; } = null!;
}
