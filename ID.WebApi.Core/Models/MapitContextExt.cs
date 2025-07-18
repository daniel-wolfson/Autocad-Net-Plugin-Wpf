using ID.Api.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ID.Api.Models
{
    public partial class MapitContext : DbContext
    {
        private readonly string _mapitConnectionString;

        public MapitContext(DbContextOptions<MapitContext> options, IConfiguration config)
            : base(options)
        {
            _mapitConnectionString = config.GetConnectionString(GeneralConsts.DbConn);
        }

        public void CustomConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql(_mapitConnectionString);

    }
}
