using Microsoft.EntityFrameworkCore;

namespace Document_Manager.Data
{
    public class AppDbContextMongo : DbContext
    {
        public AppDbContextMongo(DbContextOptions options) : base(options)
        {
        }
    }
}
