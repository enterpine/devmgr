namespace devmgr.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("enterpine.FLOW_PROJMO")]
    public partial class FLOW_PROJMO
    {
        public int id { get; set; }

        [StringLength(30)]
        [Display(Name = "编号")]
        public string code { get; set; }

        [StringLength(30)]
        [Display(Name = "名称")]
        public string name { get; set; }

        [Display(Name = "所属项目")]
        public int? projectid_fx { get; set; }

        [Display(Name = "负责人")]
        public int? responserid_fx { get; set; }

        [Column(TypeName = "date")]
        [Display(Name = "起始日期")]
        public DateTime? startdate { get; set; }

        [Column(TypeName = "date")]
        [Display(Name = "结束日期")]
        public DateTime? enddate { get; set; }

        [StringLength(2000)]
        [Display(Name = "相关文档")]
        [DataType(DataType.MultilineText)]
        public string request_text { get; set; }

        [StringLength(200)]
        [Display(Name = "备注")]

        public string remark { get; set; }
        [Display(Name = "创建者")]
        public int? whocreateid { get; set; }

        [Column(TypeName = "date")]
        [Display(Name = "创建日期")]
        public DateTime? createdate { get; set; }
    }
}
