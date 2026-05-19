namespace ASPMMA.Models
{
    public class AdminUsersViewModel
    {
        public string DefaultPassword { get; set; } = string.Empty;
        public IReadOnlyList<AdminUserRowViewModel> Users { get; set; } = [];
    }
}
