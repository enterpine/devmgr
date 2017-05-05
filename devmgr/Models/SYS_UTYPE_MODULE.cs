namespace devmgr.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SYS_UTYPE_MODULE
    {
        public int id { get; set; }

        [StringLength(30)]
        [Display(Name = "编 号")]
        public string code { get; set; }
        [Display(Name = "用户组")]
        public int? usertypeid_fx { get; set; }
        [Display(Name = "模 块")]
        public int? moduleid_fx { get; set; }

        [Display(Name = "默认启用")]
        public int? isdefault { get; set; }

        [Display(Name = "是否能全部查看")]
        public int? isenable { get; set; }
        [Display(Name = "读权限")]
        public int? isread { get; set; }
        [Display(Name = "写权限")]
        public int? iswrite { get; set; }

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
