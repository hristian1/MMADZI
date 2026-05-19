using System.ComponentModel.DataAnnotations;

namespace ASPMMA.Data
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Категория")]
        public string CategoryName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Описание")]
        public string Description { get; set; } = string.Empty;

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
