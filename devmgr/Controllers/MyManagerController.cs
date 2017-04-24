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

        // Index:GET
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

        //Edit:GET
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

        //Edit:POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,code,cname,account_id,pwd,birthdate,tel,email,departid_fx,usertypeid_fx,desc_text,remark,whocreateid_fx,createdate")] SYS_USER sYS_USER)
        {
            if (ModelState.IsValid)
            {
                db.Entry(sYS_USER).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index",new { id = sYS_USER.id });
            }
            List<SYS_DEPART> categories = SYS_DEPART.GETALL();
            ViewData["Categories"] = new SelectList(categories, "id", "name");
            return View(sYS_USER);
        }

        //changepwd:GET
        public ActionResult changepasswd(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MyIdentify Iden = new MyIdentify();
            Iden.id =(int)id;
            return View(Iden);
        }

        //changepwd:POST
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult changepasswd([Bind(Include = "id,oldpwd,newpwd,newpwd2")] MyIdentify myIdentify)
        {

            if (ModelState.IsValid)
            {
                int idx = myIdentify.id;
                SYS_USER sYS_USER = db.SYS_USER.Find(idx);
                sYS_USER.pwd = myIdentify.newpwd2;

                db.Entry(sYS_USER).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index",new { id = idx } );
            }

            return View(myIdentify);
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