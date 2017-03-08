namespace devmgr.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class FLOW_MISSION
    {
        public int id { get; set; }

        [StringLength(30)]
        [Display(Name = "任务编码")]
        public string code { get; set; }

        [Display(Name = "所属项目")]
        public int? projectid_fx { get; set; }

        [Display(Name = "谁指派的")]
        public int? fromwhoid_fx { get; set; }

        [Display(Name = "指派给谁")]
        public int? towhoid_fx { get; set; }

        [Column(TypeName = "date")]
        [Display(Name = "开始日期")]
        public DateTime? fromdate { get; set; }

        [Column(TypeName = "date")]
        [Display(Name = "截止日期")]
        public DateTime? todate { get; set; }

        [Display(Name = "父级任务")]
        public int? dad_mission { get; set; }

        [Display(Name = "等级")]
        public int? dad_level { get; set; }

        [Display(Name = "是否底层")]
        public int? isbottom { get; set; }

        [StringLength(500)]
        [Display(Name = "百度编辑器内容")]
        public string request_text { get; set; }

        [StringLength(1000)]
        [Display(Name = "上传文件路径")]
        public string request_file { get; set; }

        [Display(Name = "是否完成")]
        public int? iscomplete { get; set; }

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
    }
}
