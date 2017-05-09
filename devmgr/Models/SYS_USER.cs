namespace devmgr.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Linq;

    public partial class SYS_USER
    {
        public int id { get; set; }

        [StringLength(30)]
        [Display(Name = "工 号")]
        public string code { get; set; }

        [StringLength(10)]
        [Display(Name = "中文姓名")]
        public string cname { get; set; }

        [StringLength(20)]
        [Display(Name = "账户名")]
        public string account_id { get; set; }

        [StringLength(20)]
        [DataType(DataType.Password)]
        [Display(Name = "密 码")]
        public string pwd { get; set; }

        [Column(TypeName = "date")]
        [Display(Name = "生 日")]
        public DateTime? birthdate { get; set; }

        [StringLength(20)]
        [Display(Name = "手机号码")]
        public string tel { get; set; }

        [StringLength(20)]
        [Display(Name = "电子邮箱")]
        public string email { get; set; }

        [Display(Name = "所属部门")]
        public int? departid_fx { get; set; }
        [Display(Name = "用户组")]
        public int? usertypeid_fx { get; set; }

        [StringLength(100)]
        [Display(Name = "描 述")]
        public string desc_text { get; set; }

        [StringLength(200)]
        [Display(Name = "备 注")]
        public string remark { get; set; }

        [Display(Name = "创建人")]
        public int? whocreateid_fx { get; set; }

        [Column(TypeName = "date")]
        [Display(Name = "创建日期")]
        public DateTime? createdate { get; set; }

        static public List<SYS_USER> GETALL()
        {
            Model1 db = new Model1();
            return db.SYS_USER.ToList();
        }
    }
}
