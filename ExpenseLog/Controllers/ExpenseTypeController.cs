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
    public class ExpenseTypeController : Controller
    {
        private ExpenseLogContext db = new ExpenseLogContext();
        

        public async Task<string> GetStringAsync(string path)
        {
            string result = String.Empty;

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://expenselog2gowebapi.azurewebsites.net");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using (HttpResponseMessage response = await client.GetAsync(path))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        result = await response.Content.ReadAsStringAsync();
                    }
                }
            }
            return result;
        }


        // GET: Value
        [Authorize]
        public async Task<string> Value(string path)
        {
            string result = String.Empty;
            result = await GetStringAsync(path);
            return result;
        }

        // GET: ExpenseType
        [RequireHttps]
        [Authorize]
        public ActionResult Index()
        {
            string userId = User.Identity.GetUserId();
            return View(db.ExpenseTypes.Where(x => x.UserId == userId).ToList());
        }

        // GET: ExpenseType/Details/5
        [RequireHttps]
        [Authorize]
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
        [Authorize]
        public ActionResult Create()
        {
            return View();
        }

        // POST: ExpenseType/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [RequireHttps]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create([Bind(Include = "ID,Title")] ExpenseType ExpenseType)
        {
            if (ModelState.IsValid)
            {
                ExpenseType.UserId = User.Identity.GetUserId();
                db.ExpenseTypes.Add(ExpenseType);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(ExpenseType);
        }

        // GET: ExpenseType/Edit/5
        [RequireHttps]
        [Authorize]
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
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [RequireHttps]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Edit([Bind(Include = "ID,Title")] ExpenseType ExpenseType)
        {
            if (ModelState.IsValid)
            {
                ExpenseType.UserId = User.Identity.GetUserId();
                db.Entry(ExpenseType).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(ExpenseType);
        }

        // GET: ExpenseType/Delete/5
        [RequireHttps]
        [Authorize]
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
            string expenseTypeTitle = expenseType.Title;
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
