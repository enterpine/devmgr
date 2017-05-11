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
    public class FLOW_PRODUCTController : Controller
    {
        private Model1 db = new Model1();

        // GET: FLOW_PRODUCT
        public ActionResult Index(string searchName, string sortOrder,int? pageNum,int? mistatus,int? sst)
        {
            var products = from s in db.FLOW_PRODUCT
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
            int mod1id = ef.SYS_MODULE.Where(item => item.code == "MOD00001").First<SYS_MODULE>().id;
            var obj = ef.SYS_UTYPE_MODULE.Where(item => item.usertypeid_fx == ugid && item.moduleid_fx == mod1id);
            if (obj.First<SYS_UTYPE_MODULE>().isenable == 0) {
                products = products.Where(s => s.whocreateid_fx==ugid || s.Responserid_fx == ugid);
            }
              
            if (mistatus == 1)
            {//我创建的
                products = products.Where(s => s.whocreateid_fx == cuid);
            }
            if (mistatus == 0)
            {//我负责的
                products = products.Where(s => s.Responserid_fx == cuid);
            }
            if (sst == 1) {
                products = products.Where(s => s.statuss == "进行中");
            }
            if (sst == 0)
            {
                products = products.Where(s => s.statuss == "已完成");
            }
            //###########################################################

            if (!String.IsNullOrEmpty(searchName))
            {//搜索名称
                products = products.Where(s => s.name.Contains(searchName));
            }
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CodeSortParm = sortOrder == "Code_asc" ? "Code_desc" : "Code_asc";
            ViewBag.CreatedateSortParm = sortOrder == "date_asc" ? "date_desc" : "date_asc";
            switch (sortOrder)
            {
                case "Code_desc":
                    products = products.OrderByDescending(s => s.code);
                    break;
                case "Code_asc":
                    products = products.OrderBy(s => s.code);
                    break;
                case "date_asc":
                    products = products.OrderBy(s => s.createdate);
                    break;
                case "date_desc":
                    products = products.OrderByDescending(s => s.createdate);
                    break;

                default:
                    products = products.OrderBy(s => s.createdate);
                    break;
            }

            return View(products.ToPagedList(pageNum ?? 1, 5));
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
            List<SYS_USER> categories = SYS_USER.GETALL();
            ViewData["Categories"] = new SelectList(categories, "id", "cname");

            List<FLOW_CLIENT> categoriesclient = FLOW_CLIENT.GETALL();
            ViewData["Categoriesclient"] = new SelectList(categoriesclient, "id", "company_name");
            return View();
        }

        // POST: FLOW_PRODUCT/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateInput(false)]
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
            fLOW_PRODUCT.statuss = "进行中";
            if (ModelState.IsValid)
            {
                db.FLOW_PRODUCT.Add(fLOW_PRODUCT);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            List<SYS_USER> categories = SYS_USER.GETALL();
            ViewData["Categories"] = new SelectList(categories, "id", "cname");

            List<FLOW_CLIENT> categoriesclient = FLOW_CLIENT.GETALL();
            ViewData["Categoriesclient"] = new SelectList(categoriesclient, "id", "company_name");
            return View(fLOW_PRODUCT);
        }

        // GET: FLOW_PRODUCT/Edit/5
        public ActionResult Edit(int? id)
        {
            List<SYS_USER> categories = SYS_USER.GETALL();
            ViewData["Categories"] = new SelectList(categories, "id", "cname");

            List<FLOW_CLIENT> categoriesclient = FLOW_CLIENT.GETALL();
            ViewData["Categoriesclient"] = new SelectList(categoriesclient, "id", "company_name");

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
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Include = "id,code,Responserid_fx,name,clientid_fx,stratdate,deadlinedate,request_text,statuss,request_file,statuss,desc_text,remark,whocreateid_fx,createdate")] FLOW_PRODUCT fLOW_PRODUCT)
        {
            if (ModelState.IsValid)
            {
                db.Entry(fLOW_PRODUCT).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            List<SYS_USER> categories = SYS_USER.GETALL();
            ViewData["Categories"] = new SelectList(categories, "id", "cname");

            List<FLOW_CLIENT> categoriesclient = FLOW_CLIENT.GETALL();
            ViewData["Categoriesclient"] = new SelectList(categoriesclient, "id", "company_name");
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
        public ActionResult endproduct(int? id)
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
        [HttpPost, ActionName("endproduct")]
        [ValidateAntiForgeryToken]
        public ActionResult endproduct(int id)
        {
            string strSql = "update FLOW_PRODUCT set statuss = '已完成' where id = "+id;
            SqlHelper.ExecuteNonQuery(strSql);
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
