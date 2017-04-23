using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace devmgr.Controllers
{
    public class MyManagerController : Controller
    {
        // GET: MyManager
        public ActionResult Index()
        {
            return View();
        }

        //GET
        public ActionResult Edit()
        {
            return View();
        }

        //GET
        public ActionResult changepasswd()
        {
            return View();
        }
    }
}
}