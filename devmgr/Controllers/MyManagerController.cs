using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using devmgr.Models;

namespace devmgr.Controllers
{
    public class MyManagerController : Controller
    {
        private Model1 db = new Model1();

        // GET: MyManager
        public ActionResult Index(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SYS_USER sYS_USER = db.SYS_USER.Find(id);
            if (sYS_USER == null)
            {
                return HttpNotFound();
            }
            return View(sYS_USER);
        }

        //GET
        public ActionResult Edit(int? id)
        {
            List<SYS_DEPART> categories = SYS_DEPART.GETALL();
            ViewData["Categories"] = new SelectList(categories, "id", "name");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SYS_USER sYS_USER = db.SYS_USER.Find(id);
            if (sYS_USER == null)
            {
                return HttpNotFound();
            }

            return View(sYS_USER);
        }
        public ActionResult changepasswd(int? id)
        {
            List<SYS_DEPART> categories = SYS_DEPART.GETALL();
            ViewData["Categories"] = new SelectList(categories, "id", "name");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SYS_USER sYS_USER = db.SYS_USER.Find(id);
            if (sYS_USER == null)
            {
                return HttpNotFound();
            }

            return View(sYS_USER);
            return View();
        }
        /*
         * public ActionResult Edit(int? id)
            {
                List<SYS_DEPART> categories = SYS_DEPART.GETALL();
                ViewData["Categories"] = new SelectList(categories, "id", "name");
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                SYS_USER sYS_USER = db.SYS_USER.Find(id);
                if (sYS_USER == null)
                {
                    return HttpNotFound();
                }

                return View(sYS_USER);
            }
          public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SYS_USER sYS_USER = db.SYS_USER.Find(id);
            if (sYS_USER == null)
            {
                return HttpNotFound();
            }
            return View(sYS_USER);
        }
             */
        //GET

    }
}