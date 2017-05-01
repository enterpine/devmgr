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
        [Required(AllowEmptyStrings = false, ErrorMessage = "所属项目不能为空")]
        public int? projectid_fx { get; set; }

        [Display(Name = "所属模块")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "所属模块不能为空")]
        public int? projmotid_fx { get; set; }

        [Display(Name = "谁指派的")]
        //[Required(AllowEmptyStrings = false, ErrorMessage = "不能为空")]
        public int? fromwhoid_fx { get; set; }

        [Display(Name = "指派给谁")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "不能为空")]
        public int? towhoid_fx { get; set; }

        [Column(TypeName = "date")]
        [Display(Name = "开始日期")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "开始日期不能为空")]
        public DateTime? fromdate { get; set; }

        [Column(TypeName = "date")]
        [Display(Name = "截止日期")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "截止日期不能为空")]
        public DateTime? todate { get; set; }

        [Display(Name = "父级任务")]
        public int? dad_mission { get; set; }

        [Display(Name = "等级")]
        public int? dad_level { get; set; }

        [Display(Name = "是否底层")]
        public int? isbottom { get; set; }

        [StringLength(500)]
        [Display(Name = "任务内容")]
        [DataType(DataType.MultilineText)]
        public string request_text { get; set; }

        [StringLength(500)]
        [Display(Name = "完成情况说明")]
        [DataType(DataType.MultilineText)]
        public string finish_text { get; set; }

        [StringLength(1000)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "名称不能为空")]
        [Display(Name = "名称")]
        public string request_file { get; set; }

        [Display(Name = "任务状态")]
        public int? iscomplete { get; set; }

        [StringLength(10)]
        [Display(Name = "任务类型")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "任务类型不能为空")]
        public string missiontype { get; set; }

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

        [Column(TypeName = "date")]
        [Display(Name = "完成日期")]
        public DateTime? finishdate { get; set; }
    }
}
