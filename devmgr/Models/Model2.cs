namespace devmgr.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class Model2 : DbContext
    {
        public Model2()
            : base("name=Model2")
        {
        }

        public virtual DbSet<article> article { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<article>()
                .Property(e => e.Title)
                .IsUnicode(false);

            modelBuilder.Entity<article>()
                .Property(e => e.Content)
                .IsUnicode(false);

            modelBuilder.Entity<article>()
                .Property(e => e.AuthorName)
                .IsUnicode(false);
        }
    }
}
