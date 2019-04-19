using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ExpenseLog.DAL;
using ExpenseLog.Models;

namespace ExpenseLog.Controllers
{
    public class EntityController : Controller
    {
        private ExpenseLogContext db = new ExpenseLogContext();

        // GET: ExpenseEntities
        [Authorize]
        public ActionResult Index()
        {
            return View(db.ExpenseEntities.ToList());
        }

        // GET: ExpenseEntities/Details/5
        [Authorize]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExpenseEntity ExpenseEntity = db.ExpenseEntities.Find(id);
            if (ExpenseEntity == null)
            {
                return HttpNotFound();
            }
            return View(ExpenseEntity);
        }

        // GET: ExpenseEntities/Create
        [Authorize]
        public ActionResult Create()
        {
            return View();
        }

        // POST: ExpenseEntities/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create([Bind(Include = "ID,ExpenseEntityName,ExpenseEntityDescription")] ExpenseEntity ExpenseEntity)
        {
            if (ModelState.IsValid)
            {
                db.ExpenseEntities.Add(ExpenseEntity);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(ExpenseEntity);
        }

        // GET: ExpenseEntities/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExpenseEntity ExpenseEntity = db.ExpenseEntities.Find(id);
            if (ExpenseEntity == null)
            {
                return HttpNotFound();
            }
            return View(ExpenseEntity);
        }

        // POST: ExpenseEntities/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Edit([Bind(Include = "ID,ExpenseEntityName,ExpenseEntityDescription")] ExpenseEntity ExpenseEntity)
        {
            if (ModelState.IsValid)
            {
                db.Entry(ExpenseEntity).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(ExpenseEntity);
        }

        // GET: ExpenseEntities/Delete/5
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExpenseEntity ExpenseEntity = db.ExpenseEntities.Find(id);
            if (ExpenseEntity == null)
            {
                return HttpNotFound();
            }
            return View(ExpenseEntity);
        }

        // POST: ExpenseEntities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult DeleteConfirmed(int id)
        {
            ExpenseEntity ExpenseEntity = db.ExpenseEntities.Find(id);
            db.ExpenseEntities.Remove(ExpenseEntity);
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
