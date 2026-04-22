using Microsoft.AspNetCore.Identity;

namespace ASPMMA.Data
{
    public class Client : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
      
 
        public ICollection<Order> Orders { get; set; }
        public ICollection<Cart> Carts { get; set; }
    }
}
