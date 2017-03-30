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
    public class FLOW_REQUESTController : Controller
    {
        private Model1 db = new Model1();

        // GET: FLOW_REQUEST
        public ActionResult Index()
        {
            return View(db.FLOW_REQUEST.ToList());
        }

        // GET: FLOW_REQUEST/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FLOW_REQUEST fLOW_REQUEST = db.FLOW_REQUEST.Find(id);
            if (fLOW_REQUEST == null)
            {
                return HttpNotFound();
            }
            return View(fLOW_REQUEST);
        }

        // GET: FLOW_REQUEST/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: FLOW_REQUEST/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,code,productid_fx,startdate,deadline,request_text,request_file,desc_text,remark,whocreateid_fx,createdate")] FLOW_REQUEST fLOW_REQUEST)
        {
            Model1 ef = new Model1();
            String username = Request.Cookies["username"].Value.ToString();
            String cuuserid = ef.SYS_USER.Where(item => item.account_id == username).First<SYS_USER>().id.ToString();
            var obj = ef.FLOW_REQUEST.Where(item => item.id >= 0);
            int nowcode = 0, maxid = 0;
            if (obj.Count<FLOW_REQUEST>() > 0)
            {
                maxid = obj.Max(item => item.id);
                nowcode = maxid + 1;
            }
            else
            {
                nowcode = 1;
            }

            fLOW_REQUEST.code = "REQ" + nowcode.ToString().PadLeft(5, '0');
            fLOW_REQUEST.createdate = DateTime.Now;
            fLOW_REQUEST.whocreateid_fx = int.Parse(cuuserid);
            if (ModelState.IsValid)
            {
                db.FLOW_REQUEST.Add(fLOW_REQUEST);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(fLOW_REQUEST);
        }

        // GET: FLOW_REQUEST/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FLOW_REQUEST fLOW_REQUEST = db.FLOW_REQUEST.Find(id);
            if (fLOW_REQUEST == null)
            {
                return HttpNotFound();
            }
            return View(fLOW_REQUEST);
        }

        // POST: FLOW_REQUEST/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,code,productid_fx,startdate,deadline,request_text,request_file,desc_text,remark,whocreateid_fx,createdate")] FLOW_REQUEST fLOW_REQUEST)
        {
            if (ModelState.IsValid)
            {
                db.Entry(fLOW_REQUEST).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(fLOW_REQUEST);
        }

        // GET: FLOW_REQUEST/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FLOW_REQUEST fLOW_REQUEST = db.FLOW_REQUEST.Find(id);
            if (fLOW_REQUEST == null)
            {
                return HttpNotFound();
            }
            return View(fLOW_REQUEST);
        }

        // POST: FLOW_REQUEST/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            FLOW_REQUEST fLOW_REQUEST = db.FLOW_REQUEST.Find(id);
            db.FLOW_REQUEST.Remove(fLOW_REQUEST);
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
