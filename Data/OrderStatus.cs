using System.ComponentModel.DataAnnotations;

namespace ASPMMA.Data
{
    public enum OrderStatus
    {
        [Display(Name = "Нова")]
        New = 0,

        [Display(Name = "Обработва се")]
        Processing = 1,

        [Display(Name = "Изпратена")]
        Shipped = 2,

        [Display(Name = "Завършена")]
        Completed = 3,

        [Display(Name = "Отказана")]
        Cancelled = 4
    }

    public static class OrderStatusExtensions
    {
        public static string GetLabel(this OrderStatus status)
        {
            return status switch
            {
                OrderStatus.New => "Нова",
                OrderStatus.Processing => "Обработва се",
                OrderStatus.Shipped => "Изпратена",
                OrderStatus.Completed => "Завършена",
                OrderStatus.Cancelled => "Отказана",
                _ => status.ToString()
            };
        }

        public static string GetBadgeClass(this OrderStatus status)
        {
            return status switch
            {
                OrderStatus.New => "status-badge status-new",
                OrderStatus.Processing => "status-badge status-processing",
                OrderStatus.Shipped => "status-badge status-shipped",
                OrderStatus.Completed => "status-badge status-completed",
                OrderStatus.Cancelled => "status-badge status-cancelled",
                _ => "status-badge"
            };
        }
    }
}
