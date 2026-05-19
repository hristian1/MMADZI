using System.ComponentModel.DataAnnotations;

namespace ASPMMA.Data
{
    public class Order
    {
        public int Id { get; set; }

        [Display(Name = "Продукт")]
        public int ProductId { get; set; }

        public Product Products { get; set; } = null!;

        [Display(Name = "Количество")]
        public int Quantity { get; set; }

        public string ClientId { get; set; } = string.Empty;

        public Client Clients { get; set; } = null!;

        [Display(Name = "Дата на поръчка")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Статус")]
        public OrderStatus Status { get; set; } = OrderStatus.New;
    }
}
