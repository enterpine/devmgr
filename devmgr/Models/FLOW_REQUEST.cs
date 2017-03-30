namespace devmgr.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class FLOW_REQUEST
    {
        public int id { get; set; }

        [StringLength(30)]
        [Display(Name = "需求编号")]
        public string code { get; set; }
        [Display(Name = "所属产品")]
        public int? productid_fx { get; set; }

        [Column(TypeName = "date")]
        [Display(Name = "起始日期")]
        public DateTime? startdate { get; set; }

        [Column(TypeName = "date")]
        [Display(Name = "截止日期")]
        public DateTime? deadline { get; set; }

        [StringLength(500)]
        [Display(Name = "需求内容")]
        public string request_text { get; set; }

        [StringLength(1000)]
        [Display(Name = "相关文档")]
        public string request_file { get; set; }

        [StringLength(100)]
        [Display(Name = "描 述")]
        public string desc_text { get; set; }

        [StringLength(200)]
        [Display(Name = "备 注")]
        public string remark { get; set; }

        [Display(Name = "创建者")]
        public int? whocreateid_fx { get; set; }

        [Column(TypeName = "date")]
        [Display(Name = "创建日期")]
        public DateTime? createdate { get; set; }
    }
}
