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
    public class SYS_USERTYPEController : Controller
    {
        private Model1 db = new Model1();

        // GET: SYS_USERTYPE
        public ActionResult Index()
        {
            return View(db.SYS_USERTYPE.ToList());
        }

        // GET: SYS_USERTYPE/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SYS_USERTYPE sYS_USERTYPE = db.SYS_USERTYPE.Find(id);
            if (sYS_USERTYPE == null)
            {
                return HttpNotFound();
            }
            return View(sYS_USERTYPE);
        }

        // GET: SYS_USERTYPE/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: SYS_USERTYPE/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,code,typename,desc_text,remark,whocreateid_fx,createdate")] SYS_USERTYPE sYS_USERTYPE)
        {
            if (ModelState.IsValid)
            {
                db.SYS_USERTYPE.Add(sYS_USERTYPE);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(sYS_USERTYPE);
        }

        // GET: SYS_USERTYPE/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SYS_USERTYPE sYS_USERTYPE = db.SYS_USERTYPE.Find(id);
            if (sYS_USERTYPE == null)
            {
                return HttpNotFound();
            }
            return View(sYS_USERTYPE);
        }

        // POST: SYS_USERTYPE/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,code,typename,desc_text,remark,whocreateid_fx,createdate")] SYS_USERTYPE sYS_USERTYPE)
        {
            if (ModelState.IsValid)
            {
                db.Entry(sYS_USERTYPE).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(sYS_USERTYPE);
        }

        // GET: SYS_USERTYPE/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SYS_USERTYPE sYS_USERTYPE = db.SYS_USERTYPE.Find(id);
            if (sYS_USERTYPE == null)
            {
                return HttpNotFound();
            }
            return View(sYS_USERTYPE);
        }

        // POST: SYS_USERTYPE/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SYS_USERTYPE sYS_USERTYPE = db.SYS_USERTYPE.Find(id);
            db.SYS_USERTYPE.Remove(sYS_USERTYPE);
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
