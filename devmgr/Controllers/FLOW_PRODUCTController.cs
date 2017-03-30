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
    public class FLOW_PRODUCTController : Controller
    {
        private Model1 db = new Model1();

        // GET: FLOW_PRODUCT
        public ActionResult Index()
        {
            return View(db.FLOW_PRODUCT.ToList());
        }

        // GET: FLOW_PRODUCT/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FLOW_PRODUCT fLOW_PRODUCT = db.FLOW_PRODUCT.Find(id);
            if (fLOW_PRODUCT == null)
            {
                return HttpNotFound();
            }
            return View(fLOW_PRODUCT);
        }

        // GET: FLOW_PRODUCT/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: FLOW_PRODUCT/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,code,Responserid_fx,name,clientid_fx,stratdate,deadlinedate,request_text,request_file,statuss,desc_text,remark,whocreateid_fx,createdate")] FLOW_PRODUCT fLOW_PRODUCT)
        {
            Model1 ef = new Model1();
            String username = Request.Cookies["username"].Value.ToString();
            String cuuserid = ef.SYS_USER.Where(item => item.account_id == username).First<SYS_USER>().id.ToString();
            var obj = ef.FLOW_PRODUCT.Where(item => item.id >= 0);
            int nowcode = 0, maxid = 0;
            if (obj.Count<FLOW_PRODUCT>() > 0)
            {
                maxid = obj.Max(item => item.id);
                nowcode = maxid + 1;
            }
            else
            {
                nowcode = 1;
            }

            fLOW_PRODUCT.code = "PRD" + nowcode.ToString().PadLeft(5, '0');
            fLOW_PRODUCT.createdate = DateTime.Now;
            fLOW_PRODUCT.whocreateid_fx = int.Parse(cuuserid);
            if (ModelState.IsValid)
            {
                db.FLOW_PRODUCT.Add(fLOW_PRODUCT);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(fLOW_PRODUCT);
        }

        // GET: FLOW_PRODUCT/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FLOW_PRODUCT fLOW_PRODUCT = db.FLOW_PRODUCT.Find(id);
            if (fLOW_PRODUCT == null)
            {
                return HttpNotFound();
            }
            return View(fLOW_PRODUCT);
        }

        // POST: FLOW_PRODUCT/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,code,Responserid_fx,name,clientid_fx,stratdate,deadlinedate,request_text,request_file,statuss,desc_text,remark,whocreateid_fx,createdate")] FLOW_PRODUCT fLOW_PRODUCT)
        {
            if (ModelState.IsValid)
            {
                db.Entry(fLOW_PRODUCT).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(fLOW_PRODUCT);
        }

        // GET: FLOW_PRODUCT/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FLOW_PRODUCT fLOW_PRODUCT = db.FLOW_PRODUCT.Find(id);
            if (fLOW_PRODUCT == null)
            {
                return HttpNotFound();
            }
            return View(fLOW_PRODUCT);
        }

        // POST: FLOW_PRODUCT/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            FLOW_PRODUCT fLOW_PRODUCT = db.FLOW_PRODUCT.Find(id);
            db.FLOW_PRODUCT.Remove(fLOW_PRODUCT);
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
