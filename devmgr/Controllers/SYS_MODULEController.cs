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
    public class SYS_MODULEController : Controller
    {
        private Model1 db = new Model1();

        // GET: SYS_MODULE
        public ActionResult Index()
        {
            return View(db.SYS_MODULE.ToList());
        }

        // GET: SYS_MODULE/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SYS_MODULE sYS_MODULE = db.SYS_MODULE.Find(id);
            if (sYS_MODULE == null)
            {
                return HttpNotFound();
            }
            return View(sYS_MODULE);
        }

        // GET: SYS_MODULE/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: SYS_MODULE/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,code,name,desc_text,remark,whocreateid_fx,createdate")] SYS_MODULE sYS_MODULE)
        {
            if (ModelState.IsValid)
            {
                db.SYS_MODULE.Add(sYS_MODULE);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(sYS_MODULE);
        }

        // GET: SYS_MODULE/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SYS_MODULE sYS_MODULE = db.SYS_MODULE.Find(id);
            if (sYS_MODULE == null)
            {
                return HttpNotFound();
            }
            return View(sYS_MODULE);
        }

        // POST: SYS_MODULE/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,code,name,desc_text,remark,whocreateid_fx,createdate")] SYS_MODULE sYS_MODULE)
        {
            if (ModelState.IsValid)
            {
                db.Entry(sYS_MODULE).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(sYS_MODULE);
        }

        // GET: SYS_MODULE/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SYS_MODULE sYS_MODULE = db.SYS_MODULE.Find(id);
            if (sYS_MODULE == null)
            {
                return HttpNotFound();
            }
            return View(sYS_MODULE);
        }

        // POST: SYS_MODULE/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SYS_MODULE sYS_MODULE = db.SYS_MODULE.Find(id);
            db.SYS_MODULE.Remove(sYS_MODULE);
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
