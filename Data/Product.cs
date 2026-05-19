using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASPMMA.Data
{
    public class Product
    {
        public int Id { get; set; }

        [Display(Name = "Каталожен номер")]
        [NotMapped]
        public string CatalogNumber => $"MMA-{Id:D5}";

        [Required]
        [Display(Name = "Наименование")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Описание")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Размер")]
        public string Size { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Цена")]
        public decimal Price { get; set; }

        [Display(Name = "Наличност")]
        public int StockQuantity { get; set; }

        [Required]
        [Display(Name = "Снимка")]
        public string ImageUrl { get; set; } = string.Empty;

        [Display(Name = "Категория")]
        public int CategoryId { get; set; }

        public Category Categorys { get; set; } = null!;

        [Display(Name = "Дата на добавяне")]
        public DateTime CreatedAt { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Cart> Carts { get; set; } = new List<Cart>();
    }
}
