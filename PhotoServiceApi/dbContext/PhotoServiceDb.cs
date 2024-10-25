using Microsoft.EntityFrameworkCore;
using PhotoServiceApi.Models;

namespace PhotoServiceApi.dbContext
{
    public class PhotoServiceDb :DbContext
    {
        public PhotoServiceDb(DbContextOptions options) : base(options)
        {

        }
       public DbSet<Photo> Photos { get; set; }
    }
}
