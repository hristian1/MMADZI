namespace ASPMMA.Models
{
    public class AdminUserRowViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Roles { get; set; } = string.Empty;
        public int OrdersCount { get; set; }
        public int CartItemsCount { get; set; }
    }
}
