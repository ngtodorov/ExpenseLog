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
using Microsoft.AspNet.Identity;

namespace ContosoUniversity.Controllers
{
    public class ExpenseEntityController : Controller
    {
        private ExpenseLogContext db = new ExpenseLogContext();

        // GET: ExpenseEntity
        [RequireHttps]
        [Authorize]
        public ActionResult Index(string sortOrder)
        {
            string userId = User.Identity.GetUserId();
            var items = db.ExpenseEntities.Where(x => x.UserId == userId).Include(a=>a.ExpenseType);

            return View(items.ToList());
        }

        // GET: ExpenseEntity/Create
        [RequireHttps]
        [Authorize]
        public ActionResult Create()
        {
            string userId = User.Identity.GetUserId();
            //--- prepare the "select list" of expense types for the "Type" combobox
            ViewBag.ExpenseTypeID = new SelectList(db.ExpenseTypes.Where(x => x.UserId == userId), "ID", "Title");
            return View();
        }

        // POST: ExpenseEntity/Create
        [RequireHttps]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create([Bind(Include = "ID,ExpenseTypeID,ExpenseEntityName,ExpenseEntityDescription")] ExpenseEntity expenseEntity)
        {
            string userId = User.Identity.GetUserId();
            if (ModelState.IsValid)
            {
                expenseEntity.UserId = userId;
                db.ExpenseEntities.Add(expenseEntity);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            //--- if the model is not valid, show the same create new form
            ViewBag.ExpenseTypeID = new SelectList(db.ExpenseTypes.Where(x => x.UserId == userId), "ID", "Title", expenseEntity.ExpenseTypeID);
            return View(expenseEntity);
        }

        // GET: ExpenseEntity/Edit/5
        [RequireHttps]
        [Authorize]
        public ActionResult Edit(int? id)
        {
            string userId = User.Identity.GetUserId();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExpenseEntity expenseEntity = db.ExpenseEntities.Find(id);
            if (expenseEntity == null)
            {
                return HttpNotFound();
            }
            //--- prepare the "select list" of expense types for the "Type" combobox and select the current type
            ViewBag.ExpenseTypeID = new SelectList(db.ExpenseTypes.Where(x => x.UserId == userId), "ID", "Title", expenseEntity.ExpenseTypeID);
            return View(expenseEntity);
        }

        // POST: ExpenseEntity/Edit/5
        [RequireHttps]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Edit([Bind(Include = "ID,ExpenseTypeID,ExpenseEntityName,ExpenseEntityDescription")] ExpenseEntity expenseEntity)
        {
            string userId = User.Identity.GetUserId();
            if (ModelState.IsValid)
            {
                expenseEntity.UserId = userId;
                db.Entry(expenseEntity).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            //--- if the model is not valid, show the same edit form
            ViewBag.ExpenseTypeID = new SelectList(db.ExpenseTypes.Where(x => x.UserId == userId), "ID", "Title", expenseEntity.ExpenseTypeID);
            return View(expenseEntity);
        }

        // GET: ExpenseEntity/Delete/5
        [RequireHttps]
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExpenseEntity expenseEntity = db.ExpenseEntities.Find(id);
            if (expenseEntity == null)
            {
                return HttpNotFound();
            }
            string expenseEntityName = expenseEntity.ExpenseEntityName;
            try
            {
                db.ExpenseEntities.Remove(expenseEntity);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                string errorMessage = String.Empty;
                try
                {
                    string userId = User.Identity.GetUserId();
                    ViewBag.ExpenseTypeID = new SelectList(db.ExpenseTypes.Where(x => x.UserId == userId), "ID", "Title", expenseEntity.ExpenseTypeID);
                }
                catch(Exception ex2)
                {
                    ViewBag.Title = $"Delete Expense Entity:  (#{id}) : {expenseEntityName}";
                    ViewData["message"] = ex2.GetBaseException().Message;
                    ViewData["trace"] = ex.StackTrace;
                    return View("ErrorDescr");
                }
                ExpenseLog.Utils.ExceptionHandler exceptionHandler = new ExpenseLog.Utils.ExceptionHandler();
                this.ModelState.AddModelError("ExpenseEntityDescription", exceptionHandler.GetExceptionMessage(ex));
                return View("Edit", expenseEntity);
            }
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
