using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ExpenseLog.DAL;
using ExpenseLog.Models;
using Microsoft.AspNet.Identity;

namespace ExpenseLog.Controllers
{
    [Authorize]
    public class ExpenseTypeController : Controller
    {
        private ExpenseLogContext db = new ExpenseLogContext();
        
        // GET: ExpenseType
        [RequireHttps]
        public ActionResult Index(string sortOrder)
        {
            string userId = User.Identity.GetUserId();

            var items = db.ExpenseTypes.Where(x => x.UserId == userId);

            #region Column Ordering
            ViewBag.TitleSortParm = (String.IsNullOrEmpty(sortOrder) || sortOrder == "Title") ? "Title_desc" : "Title";
            switch (sortOrder)
            {
                case "Title_desc":
                    items = items.OrderByDescending(s => s.Title);
                    break;
                default:
                    items = items.OrderBy(s => s.Title);
                    break;
            }
            #endregion

            return View(items.ToList());
        }

        // GET: ExpenseType/Details/5
        [RequireHttps]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExpenseType ExpenseType = db.ExpenseTypes.Find(id);
            if (ExpenseType == null)
            {
                return HttpNotFound();
            }
            return View(ExpenseType);
        }

        // GET: ExpenseType/Create
        [RequireHttps]
        public ActionResult Create()
        {
            return View();
        }

        // POST: ExpenseType/Create
        [RequireHttps]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Title")] ExpenseType expenseType)
        {
            if (ModelState.IsValid)
            {
                expenseType.UserId = User.Identity.GetUserId();
                db.ExpenseTypes.Add(expenseType);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(expenseType);
        }

        // GET: ExpenseType/Edit/5
        [RequireHttps]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExpenseType ExpenseType = db.ExpenseTypes.Find(id);
            if (ExpenseType == null)
            {
                return HttpNotFound();
            }
            return View(ExpenseType);
        }

        // POST: ExpenseType/Edit/5
        [RequireHttps]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Title")] ExpenseType expenseType)
        {
            if (ModelState.IsValid)
            {
                expenseType.UserId = User.Identity.GetUserId();
                db.Entry(expenseType).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(expenseType);
        }

        // GET: ExpenseType/Delete/5
        [RequireHttps]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExpenseType expenseType = db.ExpenseTypes.Find(id);
            
            if (expenseType == null)
            {
                return HttpNotFound();
            }
            
            try
            {
                db.ExpenseTypes.Remove(expenseType);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ExpenseLog.Utils.ExceptionHandler exceptionHandler = new ExpenseLog.Utils.ExceptionHandler();
                this.ModelState.AddModelError("Title", exceptionHandler.GetExceptionMessage(ex));
                return View("Edit", expenseType);
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
