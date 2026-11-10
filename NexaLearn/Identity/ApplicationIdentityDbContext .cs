using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace NexaLearn.Identity
{
    public class ApplicationIdentityDbContext:IdentityDbContext
    {
        public ApplicationIdentityDbContext(
            DbContextOptions<ApplicationIdentityDbContext> options)
            : base(options)
        {
        }
    }
}
