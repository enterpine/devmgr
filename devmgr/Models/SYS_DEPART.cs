namespace devmgr.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Linq;
    using System.Web.Mvc;

    public partial class SYS_DEPART
    {
        public int id { get; set; }

        [StringLength(30)]
        [Display(Name = "部门编号")]
        public string code { get; set; }

        [StringLength(20)]
        [Display(Name = "部门名称")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "名称不能为空")]
        [Remote("CheckDepName", "Validate", ErrorMessage = "名称已存在")]
        public string name { get; set; }

        [Display(Name = "部门主管")]
        public int? managerid_fx { get; set; }

        [StringLength(100)]
        [Display(Name = "描述")]
        public string desc_text { get; set; }

        [StringLength(200)]
        [Display(Name = "备注")]
        public string remark { get; set; }

        [Display(Name = "由谁创建")]
        public int? whocreateid_fx { get; set; }

        [Column(TypeName = "date")]
        public DateTime? createdate { get; set; }

        static public List<SYS_DEPART> GETALL() {
            Model1 db = new Model1();
            return db.SYS_DEPART.ToList();
        }
    }
}
