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
    public class FLOW_MISSIONController : Controller
    {
        private Model1 db = new Model1();

        public ActionResult Index(string searchName, int? searchProj,int? searchProjmo,string searchType, string sortOrder,int? mistatus,int? pageNum)
        {
            List<FLOW_PRODUCT> categories_prod = FLOW_PRODUCT.GETALL();
            ViewData["categories_prod"] = new SelectList(categories_prod, "id", "name");
            List<FLOW_PROJECT> categories_proj = FLOW_PROJECT.GETALL();
            ViewData["categories_proj"] = new SelectList(categories_proj, "id", "desc_text");
            List<FLOW_PROJMO> categories_projmo = FLOW_PROJMO.GETALL();
            ViewData["categories_projmo"] = new SelectList(categories_projmo, "id", "name");
            List<DD_MISSIONTYPE> missiontype = DD_MISSIONTYPE.GETALL();
            ViewData["missiontype"] = new SelectList(missiontype, "code", "cvalue");

            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = sortOrder == "name_asc" ? "name_desc" : "name_asc";
            ViewBag.CodeSortParm = sortOrder == "Code_asc" ? "Code_desc" : "Code_asc";
            ViewBag.CreatedateSortParm = sortOrder=="date_asc" ? "date_desc" : "date_asc";


            var missions = from s in db.FLOW_MISSION
                           select s;
            if (searchProj != null)
            {//项目筛选
                missions = missions.Where(s => s.projectid_fx == searchProj);
            }
            if (searchProjmo != null)
            {//模块筛选
                missions = missions.Where(s => s.projmotid_fx == searchProjmo);
            }
            if (!String.IsNullOrEmpty(searchName))
            {//搜索名称
                missions = missions.Where(s => s.request_file.Contains(searchName));
            }
            if (!String.IsNullOrEmpty(searchType))
            {//搜索类型
               missions = missions.Where(s => s.missiontype.Contains(searchType));
            }
            if (mistatus == 1) {
                missions = missions.Where(s => s.iscomplete==1);
            }
            if (mistatus == 0)
            {
                missions = missions.Where(s => s.iscomplete == 0);
            }
            switch (sortOrder)
            {
                case "name_desc":
                    missions = missions.OrderByDescending(s => s.request_file);
                    break;
                case "name_asc":
                    missions = missions.OrderBy(s => s.request_file);
                    break;
                case "Code_desc":
                    missions = missions.OrderByDescending(s => s.code);
                    break;
                case "Code_asc":
                    missions = missions.OrderBy(s => s.code);
                    break;
                case "date_asc":
                    missions = missions.OrderBy(s => s.createdate);
                    break;
                case "date_desc":
                    missions = missions.OrderByDescending(s => s.createdate);
                    break; 

                default:
                    missions = missions.OrderBy(s => s.createdate);
                    break;
            }
            ViewBag.model2 = missions.ToList();
            return View(missions.ToPagedList(pageNum ?? 1, 10));
            //return View(missions.ToList());
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
            List<FLOW_PRODUCT> categories_prod = FLOW_PRODUCT.GETALL();
            ViewData["categories_prod"] = new SelectList(categories_prod, "id", "name");

            List<FLOW_PROJECT> categories_proj = FLOW_PROJECT.GETALL();
            ViewData["categories_proj"] = new SelectList(categories_proj, "id", "desc_text");

            List<SYS_USER> category_user = SYS_USER.GETALL();
            ViewData["category_user"] = new SelectList(category_user, "id", "cname");

            List<FLOW_PROJMO> categories_projmo = FLOW_PROJMO.GETALL();
            ViewData["categories_projmo"] = new SelectList(categories_projmo, "id", "name");

            List<DD_MISSIONTYPE> missiontype = DD_MISSIONTYPE.GETALL();
            ViewData["missiontype"] = new SelectList(missiontype, "code", "cvalue");

            return View();
        }

        // POST: FLOW_MISSION/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "productid_fx,missiontype,projmotid_fx,id,code,projectid_fx,fromwhoid_fx,towhoid_fx,fromdate,todate,dad_mission,dad_level,isbottom,request_text,request_file,iscomplete,desc_text,remark,whocreateid_fx,createdate")] FLOW_MISSION fLOW_MISSION)
        {
            Model1 ef = new Model1();
            String username = Request.Cookies["username"].Value.ToString();
            String cuuserid = ef.SYS_USER.Where(item => item.account_id == username).First<SYS_USER>().id.ToString();
            var obj = ef.FLOW_MISSION.Where(item => item.id >= 0);
            int nowcode = 0, maxid = 0;
            if (obj.Count<FLOW_MISSION>() > 0)
            {
                maxid = obj.Max(item => item.id);
                nowcode = maxid + 1;
            }
            else
            {
                nowcode = 1;
            }

            fLOW_MISSION.code = "MIS" + nowcode.ToString().PadLeft(5, '0');
            fLOW_MISSION.createdate = DateTime.Now;
            fLOW_MISSION.whocreateid_fx = int.Parse(cuuserid);
            fLOW_MISSION.fromwhoid_fx = int.Parse(cuuserid);
            fLOW_MISSION.iscomplete = int.Parse("0");
            if (ModelState.IsValid)
            {
                db.FLOW_MISSION.Add(fLOW_MISSION);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            List<FLOW_PRODUCT> categories_prod = FLOW_PRODUCT.GETALL();
            ViewData["categories_prod"] = new SelectList(categories_prod, "id", "name");

            List<FLOW_PROJECT> categories_proj = FLOW_PROJECT.GETALL();
            ViewData["categories_proj"] = new SelectList(categories_proj, "id", "desc_text");

            List<SYS_USER> category_user = SYS_USER.GETALL();
            ViewData["category_user"] = new SelectList(category_user, "id", "cname");

            List<FLOW_PROJMO> categories_projmo = FLOW_PROJMO.GETALL();
            ViewData["categories_projmo"] = new SelectList(categories_projmo, "id", "name");

            List<DD_MISSIONTYPE> missiontype = DD_MISSIONTYPE.GETALL();
            ViewData["missiontype"] = new SelectList(missiontype, "code", "cvalue");

            return View(fLOW_MISSION);
        }

        // GET: FLOW_MISSION/Edit/5
        public ActionResult Edit(int? id)
        {
            List<FLOW_PRODUCT> categories_prod = FLOW_PRODUCT.GETALL();
            ViewData["categories_prod"] = new SelectList(categories_prod, "id", "name");

            List<FLOW_PROJECT> categories_proj = FLOW_PROJECT.GETALL();
            ViewData["categories_proj"] = new SelectList(categories_proj, "id", "desc_text");

            List<SYS_USER> category_user = SYS_USER.GETALL();
            ViewData["category_user"] = new SelectList(category_user, "id", "cname");

            List<FLOW_PROJMO> categories_projmo = FLOW_PROJMO.GETALL();
            ViewData["categories_projmo"] = new SelectList(categories_projmo, "id", "name");

            List<DD_MISSIONTYPE> missiontype = DD_MISSIONTYPE.GETALL();
            ViewData["missiontype"] = new SelectList(missiontype, "code", "cvalue");

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
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "productid_fx,missiontype,projmotid_fx,id,code,projectid_fx,fromwhoid_fx,towhoid_fx,fromdate,todate,dad_mission,dad_level,isbottom,request_text,request_file,iscomplete,desc_text,remark,whocreateid_fx,createdate")] FLOW_MISSION fLOW_MISSION)
        {

            if (ModelState.IsValid)
            {
                db.Entry(fLOW_MISSION).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            List<FLOW_PRODUCT> categories_prod = FLOW_PRODUCT.GETALL();
            ViewData["categories_prod"] = new SelectList(categories_prod, "id", "name");

            List<FLOW_PROJECT> categories_proj = FLOW_PROJECT.GETALL();
            ViewData["categories_proj"] = new SelectList(categories_proj, "id", "desc_text");

            List<SYS_USER> category_user = SYS_USER.GETALL();
            ViewData["category_user"] = new SelectList(category_user, "id", "cname");

            List<FLOW_PROJMO> categories_projmo = FLOW_PROJMO.GETALL();
            ViewData["categories_projmo"] = new SelectList(categories_projmo, "id", "name");

            List<DD_MISSIONTYPE> missiontype = DD_MISSIONTYPE.GETALL();
            ViewData["missiontype"] = new SelectList(missiontype, "code", "cvalue");

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

        /// <summary>
        /// 获取项目
        /// </summary>
        public JsonResult getproj(int productid_fx)
        {
            List<FLOW_PROJECT> categories_project = FLOW_PROJECT.GETALL();
            var proj = categories_project.Where(m => m.productid_fx == productid_fx).ToList();
            List<SelectListItem> item = new List<SelectListItem>();
            foreach (var i in proj)
            {
                item.Add(new SelectListItem { Text = i.desc_text, Value = i.id.ToString() });
            }
            return Json(item, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取模块
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public JsonResult getprojmo(int projectid_fx)
        {
            List<FLOW_PROJMO> categories_projmo = FLOW_PROJMO.GETALL();
            var projmo = categories_projmo.Where(m => m.projectid_fx == projectid_fx).ToList();
            List<SelectListItem> item = new List<SelectListItem>();
            foreach (var i in projmo)
            {
                item.Add(new SelectListItem { Text = i.name, Value = i.id.ToString() });
            }
            return Json(item, JsonRequestBehavior.AllowGet);
        }
        //submission:GET
        public ActionResult submission(int? id)
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
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult submission([Bind(Include = "finish_text,id,code,projectid_fx,fromwhoid_fx,towhoid_fx,fromdate,todate,dad_mission,dad_level,isbottom,request_text,request_file,iscomplete,desc_text,remark,whocreateid_fx,createdate")] FLOW_MISSION fLOW_MISSION)
        {
            if (ModelState.IsValid)
            {
                fLOW_MISSION.iscomplete = 1;
                fLOW_MISSION.finishdate = DateTime.Now;
                db.Entry(fLOW_MISSION).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(fLOW_MISSION);
        }
        public ActionResult subdetails(int? id)
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
    }
}
