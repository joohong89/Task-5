using Cart.Models;
using Microsoft.EntityFrameworkCore;

namespace Order.Models
{
    public class OrderContext: DbContext
    {
        public OrderContext(DbContextOptions<OrderContext> options)
          : base(options)
        {
        }

        public DbSet<OrderMaster> Orders { get; set; } = null!;
    }
}
