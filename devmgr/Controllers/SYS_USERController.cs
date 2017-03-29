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
    public class SYS_USERController : Controller
    {
        private Model1 db = new Model1();

        // GET: SYS_USER
        public ActionResult Index()
        {
            return View(db.SYS_USER.ToList());
        }

        // GET: SYS_USER/Details/5
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

        // GET: SYS_USER/Create
        public ActionResult Create()
        {
            List<SYS_DEPART> categories = SYS_DEPART.GETALL();
            ViewData["Categories"] = new SelectList(categories, "id", "name");
            return View();
        }

        // POST: SYS_USER/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,code,cname,account_id,pwd,birthdate,tel,email,departid_fx,usertypeid_fx,desc_text,remark,whocreateid_fx,createdate")] SYS_USER sYS_USER)
        {
            Model1 ef = new Model1();
            String username = Request.Cookies["username"].Value.ToString();
            String cuuserid = ef.SYS_USER.Where(item => item.account_id == username).First<SYS_USER>().id.ToString();
            String maxid = ef.SYS_USER.Where(item => item.id >0).Max(item=>item.id).ToString();
            
            sYS_USER.whocreateid_fx = int.Parse(cuuserid);
            sYS_USER.createdate = DateTime.Now;
            sYS_USER.code = "user" + maxid;
            if (ModelState.IsValid)
            {
                db.SYS_USER.Add(sYS_USER);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(sYS_USER);
        }

        // GET: SYS_USER/Edit/5
        public ActionResult Edit(int? id)
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

        // POST: SYS_USER/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,code,cname,account_id,pwd,birthdate,tel,email,departid_fx,usertypeid_fx,desc_text,remark,whocreateid_fx,createdate")] SYS_USER sYS_USER)
        {
            if (ModelState.IsValid)
            {
                db.Entry(sYS_USER).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(sYS_USER);
        }

        // GET: SYS_USER/Delete/5
        public ActionResult Delete(int? id)
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

        // POST: SYS_USER/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SYS_USER sYS_USER = db.SYS_USER.Find(id);
            db.SYS_USER.Remove(sYS_USER);
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

        public ActionResult logout()
        {
            Response.Cookies["islogin"].Value = "false";
            Response.Cookies["islogin"].Expires = DateTime.Now.AddDays(7);

            Response.Cookies["username"].Value = "null";
            Response.Cookies["username"].Expires = DateTime.Now.AddDays(7);
            Response.Redirect("/home/index");
            return View();
        }
    }
}
