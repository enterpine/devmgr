namespace devmgr.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Linq;

    public partial class SYS_USERTYPE
    {
        public int id { get; set; }

        [StringLength(30)]
        [Display(Name = "用户组编号")]
        public string code { get; set; }

        [StringLength(20)]
        [Display(Name = "用户组名")]
        public string typename { get; set; }

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

        static public List<SYS_USERTYPE> GETALL()
        {
            Model1 db = new Model1();
            return db.SYS_USERTYPE.ToList();
        }
    }
}
