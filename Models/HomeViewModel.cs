using ASPMMA.Data;

namespace ASPMMA.Models
{
    public class HomeViewModel
    {
        public IReadOnlyList<Product> FeaturedProducts { get; set; } = [];
        public IReadOnlyList<Category> Categories { get; set; } = [];
    }
}
