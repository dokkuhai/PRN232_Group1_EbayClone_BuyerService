namespace EbayCloneBuyerService_CoreAPI.Repositories.Impl
{
    using EbayCloneBuyerService_CoreAPI.Models; // Đảm bảo namespace này đúng
    using EbayCloneBuyerService_CoreAPI.Models.Responses;
    using EbayCloneBuyerService_CoreAPI.Repositories.Interface;
    using Microsoft.EntityFrameworkCore;

    public class OrderRepository : IOrderRepository
    {
        private readonly CloneEbayDbContext _context;

        public OrderRepository(CloneEbayDbContext context)
        {
            _context = context;
        }

        public async Task<int> CreateAddressAsync(Address address)
        {
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();
            return address.Id;
        }

        public async Task<int> CreateOrderAsync(OrderTable order, IEnumerable<OrderItemDto> items)
        {
            // Sử dụng Transaction để đảm bảo tính toàn vẹn (atomicity)
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Gán Order Items vào Order Entity
                order.OrderItems = items.Select(item => new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    // OrderId sẽ tự động được gán khi Order được lưu
                }).ToList();

                // 2. Thêm Order. EF Core tự động thêm OrderItems nhờ tracking
                _context.OrderTables.Add(order);
                await _context.SaveChangesAsync();

                // 3. Giảm Inventory (Business logic quan trọng, thường nằm trong Service hoặc riêng biệt)
                foreach (var item in items)
                {
                    var inventory = await _context.Inventories
                                        .FirstOrDefaultAsync(i => i.ProductId == item.ProductId);

                    if (inventory != null)
                    {
                        inventory.Quantity -= item.Quantity;
                        inventory.LastUpdated = DateTime.Now;
                        // Không cần Add/Update rõ ràng, EF Core đã theo dõi (tracking) sự thay đổi này.
                    }
                    // TODO: Xử lý lỗi nếu inventory không đủ hoặc không tìm thấy
                }
                await _context.SaveChangesAsync();

                // 4. Commit Transaction
                await transaction.CommitAsync();
                return order.Id;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Lỗi khi tạo đơn hàng và cập nhật kho hàng.", ex);
            }
        }

        public async Task ClearCartItemsByUserIdAsync(int userId)
        {
            // Lấy Cart ID của User
            var cartId = await _context.Carts
                                       .Where(c => c.UserId == userId)
                                       .Select(c => c.Id)
                                       .FirstOrDefaultAsync();

            if (cartId != 0)
            {
                // Lấy tất cả CartItems thuộc Cart đó
                var cartItems = _context.CartItems
                                        .Where(ci => ci.CartId == cartId);

                // Xóa CartItems
                _context.CartItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();
            }
            // Có thể không cần xóa Cart Entity nếu nó là unique_user_cart
        }
    }
}
