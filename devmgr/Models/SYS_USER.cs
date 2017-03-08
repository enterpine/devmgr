namespace devmgr.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SYS_USER
    {
        public int id { get; set; }

        [StringLength(30)]
        public string code { get; set; }

        [StringLength(10)]
        public string cname { get; set; }

        [StringLength(20)]
        public string account_id { get; set; }

        [StringLength(20)]
        public string pwd { get; set; }

        [Column(TypeName = "date")]
        public DateTime? birthdate { get; set; }

        [StringLength(20)]
        public string tel { get; set; }

        [StringLength(20)]
        public string email { get; set; }

        public int? departid_fx { get; set; }

        public int? usertypeid_fx { get; set; }

        [StringLength(100)]
        public string desc_text { get; set; }

        [StringLength(200)]
        public string remark { get; set; }

        public int? whocreateid_fx { get; set; }

        [Column(TypeName = "date")]
        public DateTime? createdate { get; set; }
    }
}
