using System.ComponentModel.DataAnnotations;

namespace ASPMMA.Data
{
    public class Cart
    {
        public int Id { get; set; }

        [Display(Name = "Продукт")]
        public int ProductId { get; set; }

        public Product Products { get; set; } = null!;

        [Display(Name = "Количество")]
        public int Quantity { get; set; }

        public string ClientId { get; set; } = string.Empty;

        public Client Clients { get; set; } = null!;

        [Display(Name = "Добавено на")]
        public DateTime CreatedAt { get; set; }
    }
}
