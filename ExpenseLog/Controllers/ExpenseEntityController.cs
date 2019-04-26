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

            #region Column Ordering
            ViewBag.TypeSortParm = (String.IsNullOrEmpty(sortOrder)|| sortOrder == "Type") ? "Type_desc" : "Type";
            ViewBag.NameSortParm = sortOrder == "Entity"? "Entity_desc" : "Entity";
            ViewBag.DescrSortParm = sortOrder == "Description" ? "descr_desc" : "Description";
            switch (sortOrder)
            {
                case "Type_desc":
                    items = items.OrderByDescending(s => s.ExpenseType.Title);
                    break;
                case "Entity":
                    items = items.OrderBy(s => s.ExpenseEntityName);
                    break;
                case "Entity_desc":
                    items = items.OrderByDescending(s => s.ExpenseEntityName);
                    break;
                case "Description":
                    items = items.OrderBy(s => s.ExpenseEntityDescription);
                    break;
                case "descr_desc":
                    items = items.OrderByDescending(s => s.ExpenseEntityDescription);
                    break;
                default:
                    items = items.OrderBy(s => s.ExpenseType.Title);
                    break;
            }
            #endregion

            return View(items.ToList());
        }

        // GET: ExpenseEntity/Details/5
        [RequireHttps]
        public ActionResult Details(int? id)
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
            return View(expenseEntity);
        }

        // GET: ExpenseEntity/Create
        [RequireHttps]
        [Authorize]
        public ActionResult Create()
        {
            string userId = User.Identity.GetUserId();
            ViewBag.ExpenseTypeID = new SelectList(db.ExpenseTypes.Where(x => x.UserId == userId), "ID", "Title");
            return View();
        }

        // POST: ExpenseEntity/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [RequireHttps]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create([Bind(Include = "ID,UserId,ExpenseTypeID,ExpenseEntityName,ExpenseEntityDescription")] ExpenseEntity expenseEntity)
        {
            string userId = User.Identity.GetUserId();
            if (ModelState.IsValid)
            {
                expenseEntity.UserId = userId;
                db.ExpenseEntities.Add(expenseEntity);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
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
            ViewBag.ExpenseTypeID = new SelectList(db.ExpenseTypes.Where(x => x.UserId == userId), "ID", "Title", expenseEntity.ExpenseTypeID);
            return View(expenseEntity);
        }

        // POST: ExpenseEntity/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [RequireHttps]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Edit([Bind(Include = "ID,UserId,ExpenseTypeID,ExpenseEntityName,ExpenseEntityDescription")] ExpenseEntity expenseEntity)
        {
            string userId = User.Identity.GetUserId();
            if (ModelState.IsValid)
            {
                expenseEntity.UserId = userId;
                db.Entry(expenseEntity).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
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
