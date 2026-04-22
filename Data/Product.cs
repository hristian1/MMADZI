using System.ComponentModel.DataAnnotations.Schema;

namespace ASPMMA.Data
{
    public class Product
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Size { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }

        public string ImageUrl { get; set; }


        public int CategoryId { get; set; }
        public Category Categorys { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public ICollection<Order> Orders { get; set; }
        public ICollection<Cart> Carts { get; set; }


    }
}
