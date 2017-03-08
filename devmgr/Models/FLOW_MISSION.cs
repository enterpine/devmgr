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
        [Display(Name = "�������")]
        public string code { get; set; }

        [Display(Name = "������Ŀ")]
        public int? projectid_fx { get; set; }

        [Display(Name = "˭ָ�ɵ�")]
        public int? fromwhoid_fx { get; set; }

        [Display(Name = "ָ�ɸ�˭")]
        public int? towhoid_fx { get; set; }

        [Column(TypeName = "date")]
        [Display(Name = "��ʼ����")]
        public DateTime? fromdate { get; set; }

        [Column(TypeName = "date")]
        [Display(Name = "��ֹ����")]
        public DateTime? todate { get; set; }

        [Display(Name = "��������")]
        public int? dad_mission { get; set; }

        [Display(Name = "�ȼ�")]
        public int? dad_level { get; set; }

        [Display(Name = "�Ƿ�ײ�")]
        public int? isbottom { get; set; }

        [StringLength(500)]
        [Display(Name = "�ٶȱ༭������")]
        public string request_text { get; set; }

        [StringLength(1000)]
        [Display(Name = "�ϴ��ļ�·��")]
        public string request_file { get; set; }

        [Display(Name = "�Ƿ����")]
        public int? iscomplete { get; set; }

        [StringLength(100)]
        [Display(Name = "����")]
        public string desc_text { get; set; }

        [StringLength(200)]
        [Display(Name = "��ע")]
        public string remark { get; set; }

        [Display(Name = "������")]
        public int? whocreateid_fx { get; set; }

        [Column(TypeName = "date")]
        [Display(Name = "��������")]
        public DateTime? createdate { get; set; }
    }
}
