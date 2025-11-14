using System;
using System.Collections.Generic;

namespace EbayCloneBuyerService_CoreAPI.Models;

public partial class OrderTable
{
    public int Id { get; set; }

    public int? BuyerId { get; set; }

    public int? AddressId { get; set; }

    public DateTime? OrderDate { get; set; }

    public decimal? TotalPrice { get; set; }

    public string? Status { get; set; }

    public virtual Address? Address { get; set; }

    public virtual User? Buyer { get; set; }

    public virtual ICollection<CouponUsage> CouponUsages { get; set; } = new List<CouponUsage>();

    public virtual ICollection<Dispute> Disputes { get; set; } = new List<Dispute>();

    public virtual ICollection<Orderitem> OrderItems { get; set; } = new List<Orderitem>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<ReturnRequest> ReturnRequests { get; set; } = new List<ReturnRequest>();

    public virtual ICollection<Shippinginfo> ShippingInfos { get; set; } = new List<Shippinginfo>();
}
