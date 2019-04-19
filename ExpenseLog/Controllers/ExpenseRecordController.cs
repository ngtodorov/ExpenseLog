using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ExpenseLog.DAL;
using ExpenseLog.Models;
using Microsoft.AspNet.Identity;


using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;

using System.Web.UI.WebControls;
using System.Web.UI;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace ExpenseLog.Controllers
{
    public class ExpenseRecordController : Controller
    {
        private ExpenseLogContext db = new ExpenseLogContext();

        // GET: ExpenseRecord
        [Authorize]
        public ActionResult Index(string fromDateFilter, string toDateFilter, string expenseTypeID, string expenseEntityID, string sortOrder)
        {
            if (!DateTime.TryParse(fromDateFilter, out DateTime filterDateFrom))
                filterDateFrom = DateTime.Today.AddMonths(-1);

            if (!DateTime.TryParse(toDateFilter, out DateTime filterDateTo))
                filterDateTo = DateTime.Today;

            string userId = User.Identity.GetUserId();
            int expenseTypeID2 = String.IsNullOrEmpty(expenseTypeID) ? 0 : int.Parse(expenseTypeID);
            int expenseEntityID2 = String.IsNullOrEmpty(expenseEntityID) ? 0 : int.Parse(expenseEntityID);

            var expenseRecords = db.ExpenseRecords
                .Where(x => x.UserId == userId
                        && x.ExpenseDate >= filterDateFrom
                        && x.ExpenseDate <= filterDateTo
                        && (expenseTypeID2 == 0 || x.ExpenseTypeID == expenseTypeID2)
                        && (expenseEntityID2 == 0 || x.ExpenseEntityID == expenseEntityID2))
                .Include(e => e.ExpenseEntity)
                .Include(e => e.ExpenseType);


            #region Column Ordering
            ViewBag.NameSortParam = String.IsNullOrEmpty(sortOrder) ? "Entity_Desc" : "";
            ViewBag.DescrSortParam = sortOrder == "Description" ? "Description_Desc" : "Description";
            ViewBag.DateSortParam = sortOrder == "Date" ? "Date_Desc" : "Date";
            ViewBag.PriceSortParam = sortOrder == "Price" ? "Price_Desc" : "Price";
            switch (sortOrder)
            {
                case "Entity_Desc":
                    expenseRecords = expenseRecords.OrderByDescending(s => s.ExpenseEntity.ExpenseEntityName);
                    break;
                case "Description":
                    expenseRecords = expenseRecords.OrderBy(s => s.ExpenseDescription);
                    break;
                case "Description_Desc":
                    expenseRecords = expenseRecords.OrderByDescending(s => s.ExpenseDescription);
                    break;
                case "Date":
                    expenseRecords = expenseRecords.OrderBy(s => s.ExpenseDate);
                    break;
                case "Date_Desc":
                    expenseRecords = expenseRecords.OrderByDescending(s => s.ExpenseDate);
                    break;
                case "Price":
                    expenseRecords = expenseRecords.OrderBy(s => s.ExpensePrice);
                    break;
                case "Price_Desc":
                    expenseRecords = expenseRecords.OrderByDescending(s => s.ExpensePrice);
                    break;
                default:
                    expenseRecords = expenseRecords.OrderBy(s => s.ExpenseDate);
                    break;
            }
            #endregion


            if (expenseRecords != null && expenseRecords.ToList().Count > 0)
                ViewBag.Total = expenseRecords.Sum(x => x.ExpensePrice);
            else
                ViewBag.Total = 0;

            ViewBag.FilterDateFrom = filterDateFrom.ToString("MM/dd/yyyy");
            ViewBag.FilterDateTo = filterDateTo.ToString("MM/dd/yyyy");

            //--- Types
            List<ExpenseType> types = new List<ExpenseType>
            {
                new ExpenseType { ID = 0, Title = "ALL TYPES" }
            };
            types.AddRange(db.ExpenseTypes.Where(x => x.UserId == userId));

            ViewBag.ExpenseTypes = types.Select(item => new SelectListItem
            {
                Value = item.ID.ToString(),
                Text = item.Title.ToString(),
                Selected = "select" == item.ID.ToString()
            });


            //--- Entities
            List<ExpenseEntity> entities = new List<ExpenseEntity>
            {
                new ExpenseEntity { ID = 0, ExpenseEntityName = "ALL ENTITIES" }
            };
            entities.AddRange(db.ExpenseEntities.Where(x => x.UserId == userId && (expenseTypeID2 == 0 || x.ExpenseTypeID == expenseTypeID2)));

            ViewBag.ExpenseEntities = entities.Select(item => new SelectListItem
            {
                Value = item.ID.ToString(),
                Text = item.ExpenseEntityName.ToString(),
                Selected = "select" == item.ID.ToString()
            });

            //var items = from r in expenseRecords
            //  select new
            //      {
            //          expenseRecord = r,
            //          ExpenseAttachmentCount = r.ExpenseAttachments.Count(p => p.ExpenseRecordID == r.ExpenseRecordID)
            //      };

            return View(expenseRecords.ToList());
        }

        // GET: ExpenseRecord/Create
        [Authorize]
        public ActionResult Create()
        {
            try
            {
                SetViewBagVariables();

                return View();
            }
            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
                ViewData["trace"] = ex.StackTrace;
                return View("ErrorDescr");
            }
        }



        // POST: ExpenseRecord/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> Create([Bind(Include = "ExpenseRecordID,ExpenseTypeID,ExpenseEntityID,ExpenseDate,ExpensePrice,ExpenseDescription,selectFiles,attachmentsJson")] ExpenseRecord expenseRecord)
        {
            string userId = User.Identity.GetUserId();

            if (ModelState.IsValid)
            {
                try
                {
                    
                    string uploadAttachmentTaskResult = await InsertAttachmentRecordsAsync(expenseRecord.ExpenseRecordID);
                    
                    await Task.Factory.StartNew(() =>
                    {
                        expenseRecord.ExpenseLogDate = DateTime.Now;
                        expenseRecord.UserId = userId;
                        db.ExpenseRecords.Add(expenseRecord);
                        db.SaveChanges();
                    }
                    );

                    if (!String.IsNullOrEmpty(uploadAttachmentTaskResult))
                        throw new Exception($"Some of the attachments were not uploaded. {uploadAttachmentTaskResult}");

                    return RedirectToAction("Index");
                }
                catch (Exception ex1)
                {
                    ViewData["message"] = ex1.Message;
                    ViewData["trace"] = ex1.StackTrace;
                    return View("ErrorDescr");
                }
            }

            SetViewBagSelectLists(userId, expenseRecord);

            return View(expenseRecord);
        }

        // GET: ExpenseRecord/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExpenseRecord expenseRecord = db.ExpenseRecords.Find(id);
            if (expenseRecord == null)
            {
                return HttpNotFound();
            }

            if (EditPrep(expenseRecord))
                return View(expenseRecord);
            else
                return View("ErrorDescr");
        }

        private bool EditPrep(ExpenseRecord expenseRecord)
        {
            bool result = true;
            try
            {

                SetViewBagVariables(expenseRecord);

                #region Attachments

                List<Uri> attachmentUris = new List<Uri>();
                foreach (ExpenseAttachment attachment in expenseRecord.ExpenseAttachments.OrderBy(x => x.ID))
                    if (!String.IsNullOrEmpty(attachment.ExpenseAttachmentUri))
                        attachmentUris.Add(new Uri(attachment.ExpenseAttachmentUri));

                ViewBag.ExpenseAttachmentUris = attachmentUris;

                #endregion
            }
            catch (Exception ex)
            {
                ViewBag.Title = "Expense Record";
                ViewData["message"] = ex.GetBaseException().Message;
                ViewData["trace"] = ex.StackTrace;
                result = false;
            }
            return result;
        }

        // POST: ExpenseRecord/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> Edit([Bind(Include = "ExpenseRecordID,ExpenseTypeID,ExpenseEntityID,ExpenseDate,ExpensePrice,ExpenseDescription,selectFiles, FilesToDelete")] ExpenseRecord expenseRecord)
        {
            string userId = User.Identity.GetUserId();
            if (ModelState.IsValid)
            {
                try
                {
                    await DeleteSelectedAttachmentFiles(expenseRecord);
                    string uploadAttachmentTaskResult = await InsertAttachmentRecordsAsync(expenseRecord.ExpenseRecordID);
                    await Task.Factory.StartNew(() =>
                    {
                        expenseRecord.ExpenseLogDate = DateTime.Now;
                        expenseRecord.UserId = userId;
                        db.Entry(expenseRecord).State = EntityState.Modified;
                        db.SaveChanges();
                    });

                    if (uploadAttachmentTaskResult != String.Empty)
                        throw new Exception($"Some of the attachments were not uploaded. {uploadAttachmentTaskResult}");

                    return RedirectToAction("Index");
                }
                catch (Exception ex1)
                {
                    ViewData["message"] = ex1.Message;
                    ViewData["trace"] = ex1.StackTrace;
                    return View("ErrorDescr");
                }

            }
            SetViewBagSelectLists(userId, expenseRecord);
            return View(expenseRecord);
        }

        private async Task DeleteSelectedAttachmentFiles(ExpenseRecord expenseRecord)
        {
            if (Request["FilesToDelete"] != null)
            {
                string filesToDelete = Request["FilesToDelete"];
                if (filesToDelete != String.Empty)
                {
                    int i = 0;
                    List<ExpenseAttachment> attachmentsToDelete = new List<ExpenseAttachment>();
                    List<ExpenseAttachment> expenseAttachments = db.ExpenseAttachments.SqlQuery($"SELECT * FROM dbo.ExpenseAttachment WHERE ExpenseRecordID={expenseRecord.ExpenseRecordID} ORDER BY ID").ToList<ExpenseAttachment>();
                    foreach (ExpenseAttachment attachment in expenseAttachments)
                    {
                        i++;
                        if (filesToDelete.Contains($"[{i}]"))
                        {
                            attachmentsToDelete.Add(attachment);
                        }
                    }

                    //--- delete selected attachment files
                    await DeleteAttachmentFiles(attachmentsToDelete.Select(x => x.ExpenseAttachmentName));

                    //--- delete selected attachment records
                    foreach (ExpenseAttachment attachment in attachmentsToDelete)
                    {
                        db.ExpenseAttachments.Remove(attachment);
                    }
                }
            }
        }

        // POST: ExpenseRecord/Delete/5
        [Authorize]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ExpenseRecord expenseRecord = db.ExpenseRecords.Find(id);

            if (expenseRecord == null)
            {
                return HttpNotFound();
            }
            else
            {
                try
                {
                    //--- delete all attachments for the current expense record
                    if (await DeleteAttachmentFiles(expenseRecord.ExpenseAttachments.Select(x => x.ExpenseAttachmentName)))
                    {
                        //--- Delete current expense record and it will delete also the related attachment records
                        db.ExpenseRecords.Remove(expenseRecord);
                        db.SaveChanges();
                    }

                    return RedirectToAction("Index");

                }
                catch (Exception ex)
                {
                    if (EditPrep(expenseRecord))
                    {
                        this.ModelState.AddModelError("ExpenseDescription", ex.GetBaseException().Message);
                        return View("Edit", expenseRecord);
                    }
                    else
                    {
                        return View("ErrorDescr");
                    }
                }
            }
        }

        // GET: ExpenseRecord/GetEntityListByType/5
        [Authorize]
        public JsonResult GetEntityListByType(int expenseTypeID, string page)
        {
            string userId = User.Identity.GetUserId();

            Dictionary<string, string> expenseEntities = db.ExpenseEntities.Where(x => x.UserId == userId && x.ExpenseTypeID == expenseTypeID).ToDictionary(x => x.ID.ToString(), x => x.ExpenseEntityName);

            if (expenseEntities.Count == 0 || page == "ExpenseRecord")
                expenseEntities.Add("0", "ALL ENTITIES");

            JsonResult result = new JsonResult { Data = expenseEntities.ToList().OrderBy(x => x.Key), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            return result;
        }

        // GET: ExportData :http://techfunda.com/howto/308/export-data-into-ms-excel-from-mvc
        [Authorize]
        public ActionResult ExportToExcel()
        {
            // set the data source
            GridView gridview = new GridView
            {
                DataSource = db.ExpenseRecords.ToList()
            };
            gridview.DataBind();

            // Clear all the content from the current response
            Response.ClearContent();
            Response.Buffer = true;
            // set the header
            Response.AddHeader("content-disposition", "attachment;filename = itfunda.xls");
            Response.ContentType = "application/ms-excel";
            Response.Charset = "";
            // create HtmlTextWriter object with StringWriter
            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter htw = new HtmlTextWriter(sw))
                {
                    // render the GridView to the HtmlTextWriter
                    gridview.RenderControl(htw);
                    // Output the GridView content saved into StringWriter
                    Response.Output.Write(sw.ToString());
                    Response.Flush();
                    Response.End();
                }
            }
            return View();
        }

        #region Helpers

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary> 
        /// Generates a unique random file name to be uploaded  
        /// </summary> 
        private string GetUniqueBlobName(string userID, int recordID, string filename)
        {
            return string.Format("{0}_{1}_{2:10}_{3}{4}", userID, recordID, DateTime.Now.Ticks, Guid.NewGuid(), Path.GetExtension(filename));
        }

        private void SetViewBagSelectLists(string userId, ExpenseRecord expenseRecord = null)
        {
            IEnumerable<ExpenseType> expenseTypes = db.ExpenseTypes.Where(x => x.UserId == userId).OrderBy(x => x.Title);
            if (expenseTypes != null && expenseTypes.Count() > 0)
            {
                if (expenseRecord == null)
                {
                    IEnumerable<ExpenseEntity> expenseEntities = db.ExpenseEntities.Where(x => x.UserId == userId && x.ExpenseTypeID == expenseTypes.FirstOrDefault().ID).OrderByDescending(x => x.ID);
                    if (expenseEntities!=null && expenseEntities.Count()>0)
                    {
                        ViewBag.ExpenseTypeID = new SelectList(expenseTypes, "ID", "Title");
                        ViewBag.ExpenseEntityID = new SelectList(expenseEntities, "ID", "ExpenseEntityName");
                    }
                }
                else
                {
                    IEnumerable<ExpenseEntity> expenseEntities = db.ExpenseEntities.Where(x => x.UserId == userId && x.ExpenseTypeID == expenseRecord.ExpenseTypeID).OrderByDescending(x => x.ID);
                    if (expenseEntities != null && expenseEntities.Count() > 0)
                    {
                        ViewBag.ExpenseTypeID = new SelectList(expenseTypes, "ID", "Title", expenseRecord.ExpenseTypeID);
                        ViewBag.ExpenseEntityID = new SelectList(expenseEntities, "ID", "ExpenseEntityName", expenseRecord.ExpenseEntityID);
                    }
                }

                if (ViewBag.ExpenseEntityID==null)
                    throw new Exception("No Expense Entities are found. Please enter at least one Expense Entity.");
            }
            else
                throw new Exception("No Expense Types are found. Please enter at least one Expense Type.");
        }

        private async Task<string> InsertAttachmentRecordsAsync(int expenseRecordID)
        {
            string result = String.Empty;

            try
            {
                if (!String.IsNullOrEmpty(Request["attachmentsJson"]))
                {
                    foreach (Attachment attachment in JsonConvert.DeserializeObject<IEnumerable<Attachment>>(Request["attachmentsJson"]))
                    {
                        //--- add record to the Attachment table
                        await Task.Factory.StartNew(() =>
                                    db.ExpenseAttachments.Add(new ExpenseAttachment
                                    {
                                        ExpenseAttachmentName = attachment.Name,
                                        ExpenseAttachmentOriginalName = attachment.OriginalName.Substring(attachment.OriginalName.Length <= 200 ? 0 : attachment.OriginalName.Length - 200),
                                        ExpenseAttachmentLength = attachment.Length,
                                        ExpenseAttachmentType = attachment.Type,
                                        ExpenseAttachmentUri = attachment.Uri,
                                        ExpsenseAttachmentDate = attachment.UploadDate,
                                        ExpenseRecordID = expenseRecordID
                                    })
                                );
                    }
                }
            }
            catch(Exception ex)
            {
                result = ex.GetBaseException().Message;
            }
            return result;
        }

        private async Task<bool> DeleteAttachmentFiles(IEnumerable<string> attachments)
        {
            ExpenseLogCommon.Utils utils = new ExpenseLogCommon.Utils();

            string attachmentNameListJson = utils.Encrypt(JsonConvert.SerializeObject(attachments));

            using (HttpClient client = new HttpClient())
            {
                using (var content = new MultipartFormDataContent())
                {
                    content.Add(new StringContent(attachmentNameListJson, Encoding.UTF8, "application/json"), "attachmentNameList");
                    string requestUri = $"{utils.GetAppSetting("EL_EXPENSE_LOG_WEB_API_URI")}api/attachment/delete";

                    HttpResponseMessage result = await client.PostAsync(requestUri, content);

                    return (result.StatusCode == System.Net.HttpStatusCode.OK);
                }
            }
        }

        private void SetViewBagVariables(ExpenseRecord expenseRecord = null)
        {
            //--- Get current user
            string userId = User.Identity.GetUserId();

            //--- Set User ID
            ExpenseLogCommon.Utils utils = new ExpenseLogCommon.Utils();
            ViewBag.UserID = utils.Encrypt(userId);

            //--- Set attachmentUploadWebAPIUrl
            string webAPIUri = utils.GetAppSetting("EL_EXPENSE_LOG_WEB_API_URI");
            Uri uri = new Uri(new Uri(webAPIUri), "/api/attachment/upload");
            ViewBag.AttachmentUploadWebAPIUrl = uri.ToString();

            //--- Set Lists
            SetViewBagSelectLists(userId, expenseRecord);
        }
        #endregion
    }

}
