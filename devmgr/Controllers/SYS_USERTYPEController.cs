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
    public class SYS_USERTYPEController : Controller
    {
        private Model1 db = new Model1();

        // GET: SYS_USERTYPE
        public ActionResult Index()
        {
            return View(db.SYS_USERTYPE.ToList());
        }

        // GET: SYS_USERTYPE/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SYS_USERTYPE sYS_USERTYPE = db.SYS_USERTYPE.Find(id);
            if (sYS_USERTYPE == null)
            {
                return HttpNotFound();
            }
            return View(sYS_USERTYPE);
        }

        // GET: SYS_USERTYPE/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: SYS_USERTYPE/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,code,typename,desc_text,remark,whocreateid_fx,createdate")] SYS_USERTYPE sYS_USERTYPE)
        {
            if (ModelState.IsValid)
            {
                Model1 ef = new Model1();
                String username = Request.Cookies["username"].Value.ToString();
                String cuuserid = ef.SYS_USER.Where(item => item.account_id == username).First<SYS_USER>().id.ToString();
                var obj = ef.SYS_USERTYPE.Where(item => item.id > 0);
                int nowcode = 0, maxid = 0;
                if (obj.Count<SYS_USERTYPE>() > 0)
                {
                    maxid = obj.Max(item => item.id);
                    nowcode = maxid + 1;
                }
                else
                {
                    nowcode = 1;
                }

                sYS_USERTYPE.whocreateid_fx = int.Parse(cuuserid);
                sYS_USERTYPE.createdate = DateTime.Now;
                sYS_USERTYPE.code = "UGR" + nowcode.ToString().PadLeft(5, '0');

                db.SYS_USERTYPE.Add(sYS_USERTYPE);
                db.SaveChanges();

                //START:添加默认权限
                for (int i = 1; i <= 12; i++)
                {
                    if (i != 6)
                    {
                        var obj2 = ef.SYS_UTYPE_MODULE.Where(item => item.id > 0);
                        int nowcode2 = 0, maxid2 = 0;
                        if (obj2.Count<SYS_UTYPE_MODULE>() > 0)
                        {
                            maxid2 = obj2.Max(item => item.id);
                            nowcode2 = maxid2 + 1;
                        }
                        else
                        {
                            nowcode2 = 1;
                        }

                        int cutid = sYS_USERTYPE.id;
                        string code = "UTM" + nowcode2.ToString().PadLeft(5, '0');
                        string strSql = "insert into sys_utype_module(code,usertypeid_fx,moduleid_fx,isenable,isread,iswrite,whocreateid_fx,createdate) values('" + code + "'," + cutid.ToString() + "," + i.ToString() + ",0,0,0,1,'"+DateTime.Now.ToString()+"')";
                        DataSet ds = SqlHelper.ExecuteDataset(strSql);
                    }
                }
                //END:添加默认权限
                return RedirectToAction("Index");
            }

            return View(sYS_USERTYPE);
        }

        // GET: SYS_USERTYPE/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SYS_USERTYPE sYS_USERTYPE = db.SYS_USERTYPE.Find(id);
            if (sYS_USERTYPE == null)
            {
                return HttpNotFound();
            }
            return View(sYS_USERTYPE);
        }

        // POST: SYS_USERTYPE/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,code,typename,desc_text,remark,whocreateid_fx,createdate")] SYS_USERTYPE sYS_USERTYPE)
        {
            if (ModelState.IsValid)
            {
                db.Entry(sYS_USERTYPE).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(sYS_USERTYPE);
        }

        // GET: SYS_USERTYPE/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SYS_USERTYPE sYS_USERTYPE = db.SYS_USERTYPE.Find(id);
            if (sYS_USERTYPE == null)
            {
                return HttpNotFound();
            }
            return View(sYS_USERTYPE);
        }

        // POST: SYS_USERTYPE/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SYS_USERTYPE sYS_USERTYPE = db.SYS_USERTYPE.Find(id);
            db.SYS_USERTYPE.Remove(sYS_USERTYPE);
            db.SaveChanges();
            string strSql = "delete from sys_utype_module where usertypeid_fx = "+id.ToString();
            DataSet ds = SqlHelper.ExecuteDataset(strSql);
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
