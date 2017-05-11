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
    public class FLOW_PROJECTController : Controller
    {
        private Model1 db = new Model1();

        // GET: FLOW_PROJECT
        public ActionResult Index(int? pageNum,string searchName, int? searchProd,string sortOrder, int? mistatus)
        {
            List<FLOW_PRODUCT> categories_prod = FLOW_PRODUCT.GETALL();
            ViewData["categories_prod"] = new SelectList(categories_prod, "id", "name");

            ViewBag.CurrentSort = sortOrder;
            ViewBag.CodeSortParm = sortOrder == "Code_asc" ? "Code_desc" : "Code_asc";
            ViewBag.CreatedateSortParm = sortOrder == "date_asc" ? "date_desc" : "date_asc";

            var projects = from s in db.FLOW_PROJECT
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
            int mod1id = ef.SYS_MODULE.Where(item => item.code == "MOD00002").First<SYS_MODULE>().id;
            var obj = ef.SYS_UTYPE_MODULE.Where(item => item.usertypeid_fx == ugid && item.moduleid_fx == mod1id);
            if (obj.First<SYS_UTYPE_MODULE>().isenable == 0)
            {
                //START:与我相关的项目
                var products = from s in db.FLOW_PRODUCT
                               select s;
                products = products.Where(s => s.whocreateid_fx == cuid|| s.Responserid_fx == cuid);
                //END:与我相关的项目
                var productids = from s in products
                               select s.id;

                int[] pdids = productids.ToArray();
                projects = from s in projects
                           where s.whocreateid_fx == ugid || s.responserid_fx ==ugid || productids.Contains((int)s.productid_fx)
                           select s;
            }

            if (mistatus == 1)
            {//我创建的
                projects = projects.Where(s => s.whocreateid_fx == cuid);
            }
            if (mistatus == 0)
            {//我负责的
                projects = projects.Where(s => s.responserid_fx == cuid);
            }
            //###########################################################

            if (searchProd != null)
            {//项目筛选
                projects = projects.Where(s => s.productid_fx == searchProd);
            }
            if (!String.IsNullOrEmpty(searchName))
            {//搜索名称
                projects = projects.Where(s => s.desc_text.Contains(searchName));
            }

            switch (sortOrder)
            {
                case "Code_desc":
                    projects = projects.OrderByDescending(s => s.code);
                    break;
                case "Code_asc":
                    projects = projects.OrderBy(s => s.code);
                    break;
                case "date_asc":
                    projects = projects.OrderBy(s => s.createdate);
                    break;
                case "date_desc":
                    projects = projects.OrderByDescending(s => s.createdate);
                    break;
                default:
                    projects = projects.OrderBy(s => s.createdate);
                    break;
            }

            return View(projects.ToPagedList(pageNum ?? 1, 5));
        }

        // GET: FLOW_PROJECT/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FLOW_PROJECT fLOW_PROJECT = db.FLOW_PROJECT.Find(id);
            if (fLOW_PROJECT == null)
            {
                return HttpNotFound();
            }
            return View(fLOW_PROJECT);
        }

        // GET: FLOW_PROJECT/Create
        public ActionResult Create()
        {
            List<SYS_USER> categories_user = SYS_USER.GETALL();
            ViewData["Categories_user"] = new SelectList(categories_user, "id", "cname");

            List<FLOW_PRODUCT> categories = FLOW_PRODUCT.GETALL();
            ViewData["Categories_prod"] = new SelectList(categories, "id", "name");
            return View();
        }

        // POST: FLOW_PROJECT/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,code,productid_fx,responserid_fx,startdate,deadline,request_text,request_file,dad_projectid_fx,dad_level,is_bottom,desc_text,remark,whocreateid_fx,createdate")] FLOW_PROJECT fLOW_PROJECT)
        {
            Model1 ef = new Model1();
            String username = Request.Cookies["username"].Value.ToString();
            String cuuserid = ef.SYS_USER.Where(item => item.account_id == username).First<SYS_USER>().id.ToString();
            var obj = ef.FLOW_PROJECT.Where(item => item.id >= 0);
            int nowcode = 0, maxid = 0;
            if (obj.Count<FLOW_PROJECT>() > 0)
            {
                maxid = obj.Max(item => item.id);
                nowcode = maxid + 1;
            }
            else
            {
                nowcode = 1;
            }

            fLOW_PROJECT.code = "PRJ" + nowcode.ToString().PadLeft(5, '0');
            fLOW_PROJECT.createdate = DateTime.Now;
            fLOW_PROJECT.whocreateid_fx = int.Parse(cuuserid);
            if (ModelState.IsValid)
            {
                db.FLOW_PROJECT.Add(fLOW_PROJECT);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            List<SYS_USER> categories_user = SYS_USER.GETALL();
            ViewData["Categories_user"] = new SelectList(categories_user, "id", "cname");

            List<FLOW_PRODUCT> categories = FLOW_PRODUCT.GETALL();
            ViewData["Categories_prod"] = new SelectList(categories, "id", "name");
            return View(fLOW_PROJECT);
        }

        // GET: FLOW_PROJECT/Edit/5
        public ActionResult Edit(int? id)
        {
            List<SYS_USER> categories_user = SYS_USER.GETALL();
            ViewData["Categories_user"] = new SelectList(categories_user, "id", "cname");

            List<FLOW_PRODUCT> categories = FLOW_PRODUCT.GETALL();
            ViewData["Categories_prod"] = new SelectList(categories, "id", "name");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FLOW_PROJECT fLOW_PROJECT = db.FLOW_PROJECT.Find(id);
            if (fLOW_PROJECT == null)
            {
                return HttpNotFound();
            }
            return View(fLOW_PROJECT);
        }

        // POST: FLOW_PROJECT/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,code,productid_fx,responserid_fx,startdate,deadline,request_text,request_file,dad_projectid_fx,dad_level,is_bottom,desc_text,remark,whocreateid_fx,createdate")] FLOW_PROJECT fLOW_PROJECT)
        {
            if (ModelState.IsValid)
            {
                db.Entry(fLOW_PROJECT).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            List<SYS_USER> categories_user = SYS_USER.GETALL();
            ViewData["Categories_user"] = new SelectList(categories_user, "id", "cname");

            List<FLOW_PRODUCT> categories = FLOW_PRODUCT.GETALL();
            ViewData["Categories_prod"] = new SelectList(categories, "id", "name");
            return View(fLOW_PROJECT);
        }

        // GET: FLOW_PROJECT/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FLOW_PROJECT fLOW_PROJECT = db.FLOW_PROJECT.Find(id);
            if (fLOW_PROJECT == null)
            {
                return HttpNotFound();
            }
            return View(fLOW_PROJECT);
        }

        // POST: FLOW_PROJECT/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            FLOW_PROJECT fLOW_PROJECT = db.FLOW_PROJECT.Find(id);
            db.FLOW_PROJECT.Remove(fLOW_PROJECT);
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
