namespace devmgr.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class FLOW_PRODUCT
    {
        public int id { get; set; }

        [StringLength(30)]
        public string code { get; set; }

        public int? Responserid_fx { get; set; }

        [StringLength(20)]
        public string name { get; set; }

        public int? clientid_fx { get; set; }

        [Column(TypeName = "date")]
        public DateTime? stratdate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? deadlinedate { get; set; }

        [StringLength(500)]
        public string request_text { get; set; }

        [StringLength(1000)]
        public string request_file { get; set; }

        [StringLength(20)]
        public string statuss { get; set; }

        [StringLength(100)]
        public string desc_text { get; set; }

        [StringLength(200)]
        public string remark { get; set; }

        public int? whocreateid_fx { get; set; }

        [Column(TypeName = "date")]
        public DateTime? createdate { get; set; }
    }
}
