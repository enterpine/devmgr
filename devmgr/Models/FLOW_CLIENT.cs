namespace devmgr.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class FLOW_CLIENT
    {
        public int id { get; set; }

        [StringLength(30)]
        [Display(Name = "�ͻ����")]
        public string code { get; set; }

        [StringLength(30)]
        [Display(Name = "��˾����")]
        public string company_name { get; set; }

        [StringLength(30)]
        [Display(Name = "�ͷ�������")]
        public string uname { get; set; }

        [StringLength(20)]
        [Display(Name = "�����˵绰")]
        public string tel { get; set; }

        [StringLength(20)]
        [Display(Name = "�ͷ�������")]
        public string email { get; set; }

        [StringLength(100)]
        [Display(Name = "����")]
        public string desc_text { get; set; }

        [StringLength(200)]
        [Display(Name = "��ע")]
        public string remark { get; set; }

        public int? whocreateid_fx { get; set; }

        [Column(TypeName = "date")]
        [Display(Name = "��������")]
        public DateTime? createdate { get; set; }
    }
}
