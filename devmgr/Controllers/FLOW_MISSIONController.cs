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
    public class FLOW_MISSIONController : Controller
    {
        private Model1 db = new Model1();

        // GET: FLOW_MISSION
        public ActionResult Index()
        {
            return View(db.FLOW_MISSION.ToList());
        }

        // GET: FLOW_MISSION/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FLOW_MISSION fLOW_MISSION = db.FLOW_MISSION.Find(id);
            if (fLOW_MISSION == null)
            {
                return HttpNotFound();
            }
            return View(fLOW_MISSION);
        }

        // GET: FLOW_MISSION/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: FLOW_MISSION/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,code,projectid_fx,fromwhoid_fx,towhoid_fx,fromdate,todate,dad_mission,dad_level,isbottom,request_text,request_file,iscomplete,desc_text,remark,whocreateid_fx,createdate")] FLOW_MISSION fLOW_MISSION)
        {
            if (ModelState.IsValid)
            {
                db.FLOW_MISSION.Add(fLOW_MISSION);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(fLOW_MISSION);
        }

        // GET: FLOW_MISSION/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FLOW_MISSION fLOW_MISSION = db.FLOW_MISSION.Find(id);
            if (fLOW_MISSION == null)
            {
                return HttpNotFound();
            }
            return View(fLOW_MISSION);
        }

        // POST: FLOW_MISSION/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,code,projectid_fx,fromwhoid_fx,towhoid_fx,fromdate,todate,dad_mission,dad_level,isbottom,request_text,request_file,iscomplete,desc_text,remark,whocreateid_fx,createdate")] FLOW_MISSION fLOW_MISSION)
        {
            if (ModelState.IsValid)
            {
                db.Entry(fLOW_MISSION).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(fLOW_MISSION);
        }

        // GET: FLOW_MISSION/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FLOW_MISSION fLOW_MISSION = db.FLOW_MISSION.Find(id);
            if (fLOW_MISSION == null)
            {
                return HttpNotFound();
            }
            return View(fLOW_MISSION);
        }

        // POST: FLOW_MISSION/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            FLOW_MISSION fLOW_MISSION = db.FLOW_MISSION.Find(id);
            db.FLOW_MISSION.Remove(fLOW_MISSION);
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
