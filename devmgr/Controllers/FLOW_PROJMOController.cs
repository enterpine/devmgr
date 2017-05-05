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
    public class FLOW_PROJMOController : Controller
    {
        private Model1 db = new Model1();

        // GET: FLOW_PROJMO
        public ActionResult Index(string searchName, int? searchProj,string sortOrder,int? pageNum, int? mistatus)
        {
            List<FLOW_PROJECT> categories_proj = FLOW_PROJECT.GETALL();
            ViewData["categories_proj"] = new SelectList(categories_proj, "id", "desc_text");

            ViewBag.CurrentSort = sortOrder;
            ViewBag.CodeSortParm = sortOrder == "Code_asc" ? "Code_desc" : "Code_asc";
            ViewBag.CreatedateSortParm = sortOrder == "date_asc" ? "date_desc" : "date_asc";

            var projmos = from s in db.FLOW_PROJMO
                           select s;
            //#########################################################
            if (Request.Cookies["islogin"] == null)
            {
                Response.Redirect("/Account/Login");
            }
            Model1 ef = new Model1();
            string caid = Request.Cookies["username"].Value.ToString();
            int cuid = ef.SYS_USER.Where(item => item.account_id == caid).First<SYS_USER>().id;
            int? ugid = int.Parse(ef.SYS_USER.Where(item => item.account_id == caid).First<SYS_USER>().usertypeid_fx.ToString());
            int mod1id = ef.SYS_MODULE.Where(item => item.code == "MOD00003").First<SYS_MODULE>().id;
            var obj = ef.SYS_UTYPE_MODULE.Where(item => item.usertypeid_fx == ugid && item.moduleid_fx == mod1id);
            if (obj.First<SYS_UTYPE_MODULE>().isenable == 0)
            {
                //START:与我相关的产品
                var products = from s in db.FLOW_PRODUCT
                               select s;
                products = products.Where(s => s.whocreateid_fx == cuid || s.Responserid_fx == cuid);
                var productids = from s in products
                                 select s.id;
                //END:与我相关的产品
                //START:与我相关的项目
                var projects = from s in db.FLOW_PROJECT
                               select s;
                projects = projects.Where(s => s.whocreateid_fx == cuid || s.responserid_fx == cuid || productids.Contains((int)s.productid_fx));
                var projectsids = from s in projects
                               select s.id;
                //END:与我相关的项目

                int[] pdids = productids.ToArray();
                projmos = from s in projmos
                          where s.whocreateid == ugid || s.responserid_fx == ugid || projectsids.Contains((int)s.projectid_fx)
                           select s;
            }

            if (mistatus == 1)
            {//我创建的
                projmos = projmos.Where(s => s.whocreateid == cuid);
            }
            if (mistatus == 0)
            {//我负责的
                projmos = projmos.Where(s => s.responserid_fx == cuid);
            }
            //###########################################################
            if (searchProj != null)
            {//项目筛选
                projmos = projmos.Where(s => s.projectid_fx == searchProj);
            }
            if (!String.IsNullOrEmpty(searchName))
            {//搜索名称
                projmos = projmos.Where(s => s.name.Contains(searchName));
            }

            switch (sortOrder)
            {
                case "Code_desc":
                    projmos = projmos.OrderByDescending(s => s.code);
                    break;
                case "Code_asc":
                    projmos = projmos.OrderBy(s => s.code);
                    break;
                case "date_asc":
                    projmos = projmos.OrderBy(s => s.createdate);
                    break;
                case "date_desc":
                    projmos = projmos.OrderByDescending(s => s.createdate);
                    break;
                default:
                    projmos = projmos.OrderBy(s => s.createdate);
                    break;
            }

            return View(projmos.ToPagedList(pageNum ?? 1, 10));
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
