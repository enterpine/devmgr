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
    public class FLOW_CLIENTController : Controller
    {
        private Model1 db = new Model1();

        // GET: FLOW_CLIENT
        public ActionResult Index()
        {
            return View(db.FLOW_CLIENT.ToList());
        }

        // GET: FLOW_CLIENT/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FLOW_CLIENT fLOW_CLIENT = db.FLOW_CLIENT.Find(id);
            if (fLOW_CLIENT == null)
            {
                return HttpNotFound();
            }
            return View(fLOW_CLIENT);
        }

        // GET: FLOW_CLIENT/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: FLOW_CLIENT/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,code,company_name,uname,tel,email,desc_text,remark,whocreateid_fx,createdate")] FLOW_CLIENT fLOW_CLIENT)
        {
            if (ModelState.IsValid)
            {
                db.FLOW_CLIENT.Add(fLOW_CLIENT);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(fLOW_CLIENT);
        }

        // GET: FLOW_CLIENT/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FLOW_CLIENT fLOW_CLIENT = db.FLOW_CLIENT.Find(id);
            if (fLOW_CLIENT == null)
            {
                return HttpNotFound();
            }
            return View(fLOW_CLIENT);
        }

        // POST: FLOW_CLIENT/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,code,company_name,uname,tel,email,desc_text,remark,whocreateid_fx,createdate")] FLOW_CLIENT fLOW_CLIENT)
        {
            if (ModelState.IsValid)
            {
                db.Entry(fLOW_CLIENT).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(fLOW_CLIENT);
        }

        // GET: FLOW_CLIENT/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FLOW_CLIENT fLOW_CLIENT = db.FLOW_CLIENT.Find(id);
            if (fLOW_CLIENT == null)
            {
                return HttpNotFound();
            }
            return View(fLOW_CLIENT);
        }

        // POST: FLOW_CLIENT/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            FLOW_CLIENT fLOW_CLIENT = db.FLOW_CLIENT.Find(id);
            db.FLOW_CLIENT.Remove(fLOW_CLIENT);
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
