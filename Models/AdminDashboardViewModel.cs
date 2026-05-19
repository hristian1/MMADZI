using ASPMMA.Data;

namespace ASPMMA.Models
{
    public class AdminDashboardViewModel
    {
        public int ProductsCount { get; set; }
        public int CategoriesCount { get; set; }
        public int UsersCount { get; set; }
        public int OrdersCount { get; set; }
        public decimal Revenue { get; set; }
        public int PendingOrdersCount { get; set; }
        public IReadOnlyList<Product> LowStockProducts { get; set; } = [];
        public IReadOnlyList<Order> RecentOrders { get; set; } = [];
    }
}
