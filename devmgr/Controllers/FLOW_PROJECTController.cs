﻿using System;
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
    public class FLOW_PROJECTController : Controller
    {
        private Model1 db = new Model1();

        // GET: FLOW_PROJECT
        public ActionResult Index()
        {
            return View(db.FLOW_PROJECT.ToList());
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

            return View(fLOW_PROJECT);
        }

        // GET: FLOW_PROJECT/Edit/5
        public ActionResult Edit(int? id)
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
