namespace devmgr.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Linq;

    public partial class FLOW_CLIENT
    {
        public int id { get; set; }

        [StringLength(30)]
        [Display(Name = "客户编号")]
        public string code { get; set; }

        [StringLength(30)]
        [Display(Name = "公司名称")]
        public string company_name { get; set; }

        [StringLength(30)]
        [Display(Name = "客服负责人")]
        public string uname { get; set; }

        [StringLength(20)]
        [Display(Name = "负责人电话")]
        public string tel { get; set; }

        [StringLength(20)]
        [Display(Name = "客服负邮箱")]
        public string email { get; set; }

        [StringLength(100)]
        [Display(Name = "描述")]
        public string desc_text { get; set; }

        [StringLength(200)]
        [Display(Name = "备注")]
        public string remark { get; set; }

        [Display(Name = "创建者")]
        public int? whocreateid_fx { get; set; }

        [Column(TypeName = "date")]
        [Display(Name = "创建日期")]
        public DateTime? createdate { get; set; }

        static public List<FLOW_CLIENT> GETALL()
        {
            Model1 db = new Model1();
            return db.FLOW_CLIENT.ToList();
        }
    }
}
