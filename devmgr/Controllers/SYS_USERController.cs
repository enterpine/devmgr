using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using devmgr.Models;
using PagedList;

namespace devmgr.Controllers
{
    public class SYS_USERController : Controller
    {
        private Model1 db = new Model1();

        // GET: SYS_USER
        public ActionResult Index(string searchName, int? searchDep, int? searchUtype, int? pageNum)
        {
            var users = from s in db.SYS_USER
                           select s;

            List<SYS_DEPART> categories_Dep = SYS_DEPART.GETALL();
            ViewData["categories_Dep"] = new SelectList(categories_Dep, "id", "name");
            List<SYS_USERTYPE> categories_Utype = SYS_USERTYPE.GETALL();
            ViewData["categories_Utype"] = new SelectList(categories_Utype, "id", "typename");

            if (searchDep != null)
            {//部门筛选
                users = users.Where(s => s.departid_fx == searchDep);
            }
            if (searchUtype != null)
            {//用户组筛选
                users = users.Where(s => s.usertypeid_fx == searchUtype);
            }
            if (!String.IsNullOrEmpty(searchName))
            {//搜索名称
                users = users.Where(s => s.cname.Contains(searchName));
            }
            users = users.OrderBy(s => s.departid_fx);
            return View(users.ToPagedList(pageNum ?? 1, 5));
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
            List<SYS_USERTYPE> categories_ug = SYS_USERTYPE.GETALL();
            ViewData["categories_ug"] = new SelectList(categories_ug, "id", "typename");
            List<SYS_DEPART> categories = SYS_DEPART.GETALL();
            ViewData["Categories"] = new SelectList(categories, "id", "name");
            return View();
        }

        // POST: SYS_USER/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,code,cname,account_id,birthdate,tel,email,departid_fx,usertypeid_fx,desc_text,remark,whocreateid_fx,createdate")] SYS_USER sYS_USER)
        {
            Model1 ef = new Model1();
            String username = Request.Cookies["username"].Value.ToString();
            String cuuserid = ef.SYS_USER.Where(item => item.account_id == username).First<SYS_USER>().id.ToString();

            var obj = ef.SYS_USER.Where(item => item.id >0);
            int nowcode = 0, maxid = 0;
            if (obj.Count<SYS_USER>() > 0)
            {
                maxid = obj.Max(item => item.id);
                nowcode = maxid + 1;
            }
            else
            {
                nowcode = 1;
            }

            sYS_USER.pwd = "123456";
            sYS_USER.whocreateid_fx = int.Parse(cuuserid);
            sYS_USER.createdate = DateTime.Now;
            sYS_USER.code = "USR" + nowcode.ToString().PadLeft(5,'0');
            if (ModelState.IsValid)
            {
                db.SYS_USER.Add(sYS_USER);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            List<SYS_DEPART> categories = SYS_DEPART.GETALL();
            ViewData["Categories"] = new SelectList(categories, "id", "name");
            List<SYS_USERTYPE> categories_ug = SYS_USERTYPE.GETALL();
            ViewData["categories_ug"] = new SelectList(categories_ug, "id", "typename");
            return View(sYS_USER);
        }

        // GET: SYS_USER/Edit/5
        public ActionResult Edit(int? id)
        {
            List<SYS_DEPART> categories = SYS_DEPART.GETALL();
            ViewData["Categories"] = new SelectList(categories, "id", "name");
            List<SYS_USERTYPE> categories_ug = SYS_USERTYPE.GETALL();
            ViewData["categories_ug"] = new SelectList(categories_ug, "id", "typename");
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
            List<SYS_DEPART> categories = SYS_DEPART.GETALL();
            ViewData["Categories"] = new SelectList(categories, "id", "name");
            List<SYS_USERTYPE> categories_ug = SYS_USERTYPE.GETALL();
            ViewData["categories_ug"] = new SelectList(categories_ug, "id", "typename");
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
