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
    public class SYS_UTYPE_MODULEController : Controller
    {
        private Model1 db = new Model1();

        // GET: SYS_UTYPE_MODULE
        public ActionResult Index()
        {
            return View(db.SYS_UTYPE_MODULE.ToList());
        }

        // GET: SYS_UTYPE_MODULE/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SYS_UTYPE_MODULE sYS_UTYPE_MODULE = db.SYS_UTYPE_MODULE.Find(id);
            if (sYS_UTYPE_MODULE == null)
            {
                return HttpNotFound();
            }
            return View(sYS_UTYPE_MODULE);
        }

        // GET: SYS_UTYPE_MODULE/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: SYS_UTYPE_MODULE/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,code,usertypeid_fx,moduleid_fx,isdefault,isenable,isread,iswrite,desc_text,remark,whocreateid_fx,createdate")] SYS_UTYPE_MODULE sYS_UTYPE_MODULE)
        {
            Model1 ef = new Model1();
            String username = Request.Cookies["username"].Value.ToString();
            String cuuserid = ef.SYS_USER.Where(item => item.account_id == username).First<SYS_USER>().id.ToString();
            var obj = ef.SYS_UTYPE_MODULE.Where(item => item.id > 0);
            int nowcode = 0, maxid = 0;
            if (obj.Count<SYS_UTYPE_MODULE>() > 0)
            {
                maxid = obj.Max(item => item.id);
                nowcode = maxid + 1;
            }
            else
            {
                nowcode = 1;
            }

            sYS_UTYPE_MODULE.whocreateid_fx = int.Parse(cuuserid);
            sYS_UTYPE_MODULE.createdate = DateTime.Now;
            sYS_UTYPE_MODULE.code = "UTM" + nowcode.ToString().PadLeft(5, '0');
            if (ModelState.IsValid)
            {
                db.SYS_UTYPE_MODULE.Add(sYS_UTYPE_MODULE);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(sYS_UTYPE_MODULE);
        }

        // GET: SYS_UTYPE_MODULE/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SYS_UTYPE_MODULE sYS_UTYPE_MODULE = db.SYS_UTYPE_MODULE.Find(id);
            if (sYS_UTYPE_MODULE == null)
            {
                return HttpNotFound();
            }
            return View(sYS_UTYPE_MODULE);
        }

        // POST: SYS_UTYPE_MODULE/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,code,usertypeid_fx,moduleid_fx,isdefault,isenable,isread,iswrite,desc_text,remark,whocreateid_fx,createdate")] SYS_UTYPE_MODULE sYS_UTYPE_MODULE)
        {
            if (ModelState.IsValid)
            {
                db.Entry(sYS_UTYPE_MODULE).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(sYS_UTYPE_MODULE);
        }

        // GET: SYS_UTYPE_MODULE/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SYS_UTYPE_MODULE sYS_UTYPE_MODULE = db.SYS_UTYPE_MODULE.Find(id);
            if (sYS_UTYPE_MODULE == null)
            {
                return HttpNotFound();
            }
            return View(sYS_UTYPE_MODULE);
        }

        // POST: SYS_UTYPE_MODULE/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SYS_UTYPE_MODULE sYS_UTYPE_MODULE = db.SYS_UTYPE_MODULE.Find(id);
            db.SYS_UTYPE_MODULE.Remove(sYS_UTYPE_MODULE);
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
