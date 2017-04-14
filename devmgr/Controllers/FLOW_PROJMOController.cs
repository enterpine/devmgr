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
    public class FLOW_PROJMOController : Controller
    {
        private Model1 db = new Model1();

        // GET: FLOW_PROJMO
        public ActionResult Index()
        {
            return View(db.FLOW_PROJMO.ToList());
        }

        // GET: FLOW_PROJMO/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FLOW_PROJMO fLOW_PROJMO = db.FLOW_PROJMO.Find(id);
            if (fLOW_PROJMO == null)
            {
                return HttpNotFound();
            }
            return View(fLOW_PROJMO);
        }

        // GET: FLOW_PROJMO/Create
        public ActionResult Create()
        {
            List<SYS_USER> categories_user = SYS_USER.GETALL();
            ViewData["Categories_user"] = new SelectList(categories_user, "id", "cname");

            List<FLOW_PROJECT> categories_proj = FLOW_PROJECT.GETALL();
            ViewData["categories_proj"] = new SelectList(categories_proj, "id", "desc_text");

            return View();
        }

        // POST: FLOW_PROJMO/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,code,name,projectid_fx,responserid_fx,startdate,enddate,request_text,remark,whocreateid,createdate")] FLOW_PROJMO fLOW_PROJMO)
        {
            Model1 ef = new Model1();
            String username = Request.Cookies["username"].Value.ToString();
            String cuuserid = ef.SYS_USER.Where(item => item.account_id == username).First<SYS_USER>().id.ToString();
            var obj = ef.FLOW_PROJMO.Where(item => item.id >= 0);
            int nowcode = 0, maxid = 0;
            if (obj.Count<FLOW_PROJMO>() > 0)
            {
                maxid = obj.Max(item => item.id);
                nowcode = maxid + 1;
            }
            else
            {
                nowcode = 1;
            }

            fLOW_PROJMO.code = "PRMO" + nowcode.ToString().PadLeft(5, '0');
            fLOW_PROJMO.createdate = DateTime.Now;
            fLOW_PROJMO.whocreateid = int.Parse(cuuserid);

            if (ModelState.IsValid)
            {
                db.FLOW_PROJMO.Add(fLOW_PROJMO);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            List<SYS_USER> categories_user = SYS_USER.GETALL();
            ViewData["Categories_user"] = new SelectList(categories_user, "id", "cname");

            List<FLOW_PROJECT> categories_proj = FLOW_PROJECT.GETALL();
            ViewData["categories_proj"] = new SelectList(categories_proj, "id", "desc_text");
            return View(fLOW_PROJMO);
        }

        // GET: FLOW_PROJMO/Edit/5
        public ActionResult Edit(int? id)
        {
            List<SYS_USER> categories_user = SYS_USER.GETALL();
            ViewData["Categories_user"] = new SelectList(categories_user, "id", "cname");

            List<FLOW_PROJECT> categories_proj = FLOW_PROJECT.GETALL();
            ViewData["categories_proj"] = new SelectList(categories_proj, "id", "desc_text");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FLOW_PROJMO fLOW_PROJMO = db.FLOW_PROJMO.Find(id);
            if (fLOW_PROJMO == null)
            {
                return HttpNotFound();
            }
            return View(fLOW_PROJMO);
        }

        // POST: FLOW_PROJMO/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,code,name,projectid_fx,responserid_fx,startdate,enddate,request_text,remark,whocreateid,createdate")] FLOW_PROJMO fLOW_PROJMO)
        {
            if (ModelState.IsValid)
            {
                db.Entry(fLOW_PROJMO).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            List<SYS_USER> categories_user = SYS_USER.GETALL();
            ViewData["Categories_user"] = new SelectList(categories_user, "id", "cname");

            List<FLOW_PROJECT> categories_proj = FLOW_PROJECT.GETALL();
            ViewData["categories_proj"] = new SelectList(categories_proj, "id", "desc_text");
            return View(fLOW_PROJMO);
        }

        // GET: FLOW_PROJMO/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FLOW_PROJMO fLOW_PROJMO = db.FLOW_PROJMO.Find(id);
            if (fLOW_PROJMO == null)
            {
                return HttpNotFound();
            }
            return View(fLOW_PROJMO);
        }

        // POST: FLOW_PROJMO/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            FLOW_PROJMO fLOW_PROJMO = db.FLOW_PROJMO.Find(id);
            db.FLOW_PROJMO.Remove(fLOW_PROJMO);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
