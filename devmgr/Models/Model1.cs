namespace devmgr.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class Model1 : DbContext
    {
        public Model1()
            : base("name=ModelPine")
        {
        }

        public virtual DbSet<FLOW_CLIENT> FLOW_CLIENT { get; set; }
        public virtual DbSet<FLOW_MISSION> FLOW_MISSION { get; set; }
        public virtual DbSet<FLOW_PRODUCT> FLOW_PRODUCT { get; set; }
        public virtual DbSet<FLOW_PROJECT> FLOW_PROJECT { get; set; }
        public virtual DbSet<FLOW_REQUEST> FLOW_REQUEST { get; set; }
        public virtual DbSet<SYS_DEPART> SYS_DEPART { get; set; }
        public virtual DbSet<SYS_MODULE> SYS_MODULE { get; set; }
        public virtual DbSet<SYS_USER> SYS_USER { get; set; }
        public virtual DbSet<SYS_USERTYPE> SYS_USERTYPE { get; set; }
        public virtual DbSet<SYS_UTYPE_MODULE> SYS_UTYPE_MODULE { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
