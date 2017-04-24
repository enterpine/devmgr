using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using devmgr.Models;
namespace devmgr.Controllers
{
    public class ValidateController : Controller
    {
        // GET: Validate
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult CheckDepName(string name,int? id)
        {
            Model1 ef = new Model1();
            var obj = ef.SYS_DEPART.Where(item => item.name == name);
            int a = obj.Count<SYS_DEPART>();
            if (a == 0)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            if (id == null)
            {
                return Json("抱歉，该名称已存在！", JsonRequestBehavior.AllowGet);
            }
            else {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult CheckProductName(string name)
        {
            Model1 ef = new Model1();
            var obj = ef.FLOW_PRODUCT.Where(item => item.name == name);
            int a = obj.Count<FLOW_PRODUCT>();
            if (a == 0)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            return Json("抱歉，该名称已存在！", JsonRequestBehavior.AllowGet);
        }

        public JsonResult CheckOldPwd(string oldpwd,int ? id)
        {//检查旧密码
            Model1 ef = new Model1();
            var obj = ef.SYS_USER.Where(item => item.id == id);
            string opwd = obj.First<SYS_USER>().pwd.ToString();
            if (oldpwd == opwd)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            return Json("密码错误！", JsonRequestBehavior.AllowGet);
        }

        public JsonResult CheckNewPwd(string newpwd2,string newpwd)
        {//检查两此密码是否一致
            if (newpwd2 == newpwd)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            return Json("两次密码不一致！", JsonRequestBehavior.AllowGet);
        }

    }
}