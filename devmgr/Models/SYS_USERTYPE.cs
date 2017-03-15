namespace devmgr.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SYS_USERTYPE
    {
        public int id { get; set; }

        [StringLength(30)]
        public string code { get; set; }

        [StringLength(20)]
        public string typename { get; set; }

        [StringLength(100)]
        public string desc_text { get; set; }

        [StringLength(200)]
        public string remark { get; set; }

        public int? whocreateid_fx { get; set; }

        [Column(TypeName = "date")]
        public DateTime? createdate { get; set; }
    }
}