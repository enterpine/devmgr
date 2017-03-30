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
        public JsonResult CheckDepName(string name)
        {
            Model1 ef = new Model1();
            var obj = ef.SYS_DEPART.Where(item => item.name == name);
            int a = obj.Count<SYS_DEPART>();
            if (a == 0)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            return Json("抱歉，该名称已存在！", JsonRequestBehavior.AllowGet);
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
    }
}