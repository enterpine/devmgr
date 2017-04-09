namespace devmgr.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Linq;
    using System.Web.Mvc;

    public partial class FLOW_PRODUCT
    {
        public int id { get; set; }

        [StringLength(30)]
        [Display(Name = "产品编号")]
        public string code { get; set; }
        [Display(Name = "产品负责人")]
        public int? Responserid_fx { get; set; }

        [StringLength(20)]
        [Display(Name = "产品名称")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "名称不能为空")]
        [Remote("CheckProductName", "Validate")]
        public string name { get; set; }


        [Display(Name = "客户名称")]
        public int? clientid_fx { get; set; }

        [Column(TypeName = "date")]
        [Display(Name = "起始日期")]
        public DateTime? stratdate { get; set; }

        [Column(TypeName = "date")]
        [Display(Name = "截止日期")]
        public DateTime? deadlinedate { get; set; }

        [StringLength(500)]
        [Display(Name = "需求内容")]
        public string request_text { get; set; }

        [Display(Name = "相关文档")]
        [DataType(DataType.MultilineText)]
        public string request_file { get; set; }

        [StringLength(20)]
        [Display(Name = "产品进度")]
        public string statuss { get; set; }

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
        static public List<FLOW_PRODUCT> GETALL()
        {
            Model1 db = new Model1();
            return db.FLOW_PRODUCT.ToList();
        }


    }
}
