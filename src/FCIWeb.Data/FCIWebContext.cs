using FCIWeb.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FCIWeb.Data
{
    /// <summary>Contesto EF Core per l'app cross-platform.</summary>
    public sealed class FCIWebContext : DbContext
    {
        public FCIWebContext(DbContextOptions<FCIWebContext> options) : base(options) { }

        public DbSet<Richiesta> Richieste => Set<Richiesta>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var richiesta = modelBuilder.Entity<Richiesta>();
            richiesta.ToTable("Richieste");
            richiesta.HasKey(x => x.Id);
            richiesta.Property(x => x.Id).ValueGeneratedNever();
            richiesta.Property(x => x.NDG).HasMaxLength(32).IsRequired();
            richiesta.Property(x => x.Intestazione).HasMaxLength(200).IsRequired();
            richiesta.Property(x => x.UOCode).HasMaxLength(32);
            richiesta.Property(x => x.Matricola).HasMaxLength(32);
            richiesta.Property(x => x.Note).HasMaxLength(4000);
            richiesta.Property(x => x.Stato).HasConversion<int>().IsRequired();
            richiesta.Property(x => x.DataCreazione).IsRequired();
            richiesta.HasIndex(x => x.NDG);
            richiesta.HasIndex(x => x.DataCreazione);
            richiesta.HasIndex(x => x.Stato);
        }
    }
}
