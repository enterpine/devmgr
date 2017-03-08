namespace devmgr.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class FLOW_PROJECT
    {
        public int id { get; set; }

        [StringLength(30)]
        public string code { get; set; }

        public int? productid_fx { get; set; }

        public int? responserid_fx { get; set; }

        [Column(TypeName = "date")]
        public DateTime? startdate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? deadline { get; set; }

        [StringLength(500)]
        public string request_text { get; set; }

        [StringLength(1000)]
        public string request_file { get; set; }

        public int? dad_projectid_fx { get; set; }

        public int? dad_level { get; set; }

        public int? is_bottom { get; set; }

        [StringLength(100)]
        public string desc_text { get; set; }

        [StringLength(200)]
        public string remark { get; set; }

        public int? whocreateid_fx { get; set; }

        [Column(TypeName = "date")]
        public DateTime? createdate { get; set; }
    }
}
