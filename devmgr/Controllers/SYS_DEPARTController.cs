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
    public class SYS_DEPARTController : Controller
    {
        private Model1 db = new Model1();

        // GET: SYS_DEPART
        public ActionResult Index()
        {
            return View(db.SYS_DEPART.ToList());
        }

        // GET: SYS_DEPART/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SYS_DEPART sYS_DEPART = db.SYS_DEPART.Find(id);
            if (sYS_DEPART == null)
            {
                return HttpNotFound();
            }
            return View(sYS_DEPART);
        }

        // GET: SYS_DEPART/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: SYS_DEPART/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,code,name,managerid_fx,desc_text,remark,whocreateid_fx,createdate")] SYS_DEPART sYS_DEPART)
        {
            if (ModelState.IsValid)
            {
                Model1 ef = new Model1();
                String username = Request.Cookies["username"].Value.ToString();
                String cuuserid = ef.SYS_USER.Where(item => item.account_id == username).First<SYS_USER>().id.ToString();
                var obj = ef.SYS_DEPART.Where(item => item.id >= 0);
                int nowcode= 0,maxid=0;
                if (obj.Count<SYS_DEPART>()>0) {
                    maxid=obj.Max(item => item.id);
                    nowcode = maxid + 1; }
                else {
                    nowcode = 1;
                }

                sYS_DEPART.code = "DEP" + nowcode.ToString().PadLeft(5,'0');
                sYS_DEPART.createdate = DateTime.Now;
                sYS_DEPART.whocreateid_fx = int.Parse(cuuserid);

                db.SYS_DEPART.Add(sYS_DEPART);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(sYS_DEPART);
        }

        // GET: SYS_DEPART/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SYS_DEPART sYS_DEPART = db.SYS_DEPART.Find(id);
            if (sYS_DEPART == null)
            {
                return HttpNotFound();
            }
            return View(sYS_DEPART);
        }

        // POST: SYS_DEPART/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,code,name,managerid_fx,desc_text,remark,whocreateid_fx,createdate")] SYS_DEPART sYS_DEPART)
        {
            if (ModelState.IsValid)
            {
                db.Entry(sYS_DEPART).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(sYS_DEPART);
        }

        // GET: SYS_DEPART/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SYS_DEPART sYS_DEPART = db.SYS_DEPART.Find(id);
            if (sYS_DEPART == null)
            {
                return HttpNotFound();
            }
            return View(sYS_DEPART);
        }

        // POST: SYS_DEPART/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SYS_DEPART sYS_DEPART = db.SYS_DEPART.Find(id);
            db.SYS_DEPART.Remove(sYS_DEPART);
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
