namespace ASPMMA.Data
{
    public class Category
    {
        public int Id { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}
