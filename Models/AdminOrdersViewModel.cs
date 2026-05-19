using ASPMMA.Data;

namespace ASPMMA.Models
{
    public class AdminOrdersViewModel
    {
        public IReadOnlyList<Order> Orders { get; set; } = [];
        public IReadOnlyList<OrderStatus> AvailableStatuses { get; set; } = [];
    }
}
