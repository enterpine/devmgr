﻿using System;
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
    public class FLOW_REQUESTController : Controller
    {
        private Model1 db = new Model1();

        // GET: FLOW_REQUEST
        public ActionResult Index(int? pageNum, int? searchProd, string sortOrder,int? mistatus)
        {
            var requests = from s in db.FLOW_REQUEST
                           select s;

            List<FLOW_PRODUCT> categories_prod = FLOW_PRODUCT.GETALL();
            ViewData["categories_prod"] = new SelectList(categories_prod, "id", "name");

            ViewBag.CurrentSort = sortOrder;
            ViewBag.CodeSortParm = sortOrder == "Code_asc" ? "Code_desc" : "Code_asc";
            ViewBag.CreatedateSortParm = sortOrder == "date_asc" ? "date_desc" : "date_asc";

            //#########################################################
            if (Request.Cookies["islogin"] == null)
            {
                Response.Redirect("/Account/Login");
            }
            Model1 ef = new Model1();
            string caid = Request.Cookies["username"].Value.ToString();
            int cuid = ef.SYS_USER.Where(item => item.account_id == caid).First<SYS_USER>().id;
            int? ugid = int.Parse(ef.SYS_USER.Where(item => item.account_id == caid).First<SYS_USER>().usertypeid_fx.ToString());
            int mod1id = ef.SYS_MODULE.Where(item => item.code == "MOD00011").First<SYS_MODULE>().id;
            var obj = ef.SYS_UTYPE_MODULE.Where(item => item.usertypeid_fx == ugid && item.moduleid_fx == mod1id);
            if (obj.First<SYS_UTYPE_MODULE>().isenable == 0)
            {
                //START:与我相关的项目
                var products = from s in db.FLOW_PRODUCT
                               select s;
                products = products.Where(s => s.whocreateid_fx == cuid || s.Responserid_fx == cuid);
                //END:与我相关的项目
                var productids = from s in products
                                 select s.id;

                int[] pdids = productids.ToArray();
                requests = from s in requests
                           where s.whocreateid_fx == ugid || productids.Contains((int)s.productid_fx)
                           select s;
            }

            if (mistatus == 1)
            {//我创建的
                requests = requests.Where(s => s.whocreateid_fx == cuid);
            }

            //###########################################################

            if (searchProd != null)
            {//项目筛选
                requests = requests.Where(s => s.productid_fx == searchProd);
            }
            switch (sortOrder)
            {
                case "Code_desc":
                    requests = requests.OrderByDescending(s => s.code);
                    break;
                case "Code_asc":
                    requests = requests.OrderBy(s => s.code);
                    break;
                case "date_asc":
                    requests = requests.OrderBy(s => s.createdate);
                    break;
                case "date_desc":
                    requests = requests.OrderByDescending(s => s.createdate);
                    break;
                default:
                    requests = requests.OrderBy(s => s.createdate);
                    break;
            }

            return View(requests.ToPagedList(pageNum ?? 1, 5));
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
            List<FLOW_PRODUCT> categories = FLOW_PRODUCT.GETALL();
            ViewData["Categories_prod"] = new SelectList(categories, "id", "name");

            return View();
        }

        // POST: FLOW_REQUEST/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateInput(false)]
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
            List<FLOW_PRODUCT> categories = FLOW_PRODUCT.GETALL();
            ViewData["Categories_prod"] = new SelectList(categories, "id", "name");
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
            List<FLOW_PRODUCT> categories = FLOW_PRODUCT.GETALL();
            ViewData["Categories_prod"] = new SelectList(categories, "id", "name");
            return View(fLOW_REQUEST);
        }

        // POST: FLOW_REQUEST/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,code,productid_fx,startdate,deadline,request_text,request_file,desc_text,remark,whocreateid_fx,createdate")] FLOW_REQUEST fLOW_REQUEST)
        {
            if (ModelState.IsValid)
            {
                db.Entry(fLOW_REQUEST).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            List<FLOW_PRODUCT> categories = FLOW_PRODUCT.GETALL();
            ViewData["Categories_prod"] = new SelectList(categories, "id", "name");
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
