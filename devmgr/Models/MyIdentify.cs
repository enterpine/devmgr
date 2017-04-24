using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Web.Mvc;
namespace devmgr.Models
{
    public class MyIdentify
    {

        public int id { get; set; }

        [StringLength(20)]
        [DataType(DataType.Password)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "旧密码不能为空")]
        [Remote("CheckOldPwd", "Validate",AdditionalFields = "id")]
        [Display(Name = "旧密码")]
        public string oldpwd{ get; set;}

        [StringLength(20)]
        [DataType(DataType.Password)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "新密码不能为空")]
        [Display(Name = "新密码")]
        public string newpwd{get; set;}

        [StringLength(20)]
        [DataType(DataType.Password)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "不能为空")]
        [Remote("CheckNewPwd", "Validate", AdditionalFields = "newpwd")]
        [Display(Name = "新密码")]
        public string newpwd2 { get; set; }
    }
}