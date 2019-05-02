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
using System.Web.UI.WebControls;
using System.Web.UI;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Collections.ObjectModel;

namespace ExpenseLog.Controllers
{
    [Authorize]
    public class ExpenseRecordController : Controller
    {
        private ExpenseLogContext db = new ExpenseLogContext();

        // GET: ExpenseRecord
        [RequireHttps]
        public ActionResult Index(string expenseTypeIDFilter, string expenseEntityIDFilter, string fromDateFilter, string toDateFilter,  string sortOrder, string descriptionSearchFilter)
        {
            string userId = User.Identity.GetUserId();

            #region Read the filter values

            if (!DateTime.TryParse(fromDateFilter, out DateTime filterDateFrom))
                filterDateFrom = DateTime.Today.AddMonths(-1);

            if (!DateTime.TryParse(toDateFilter, out DateTime filterDateTo))
                filterDateTo = DateTime.Today;

            int filterExpenseTypeID = string.IsNullOrEmpty(expenseTypeIDFilter) ? 0 : int.Parse(expenseTypeIDFilter);
            int filterExpenseEntityID = string.IsNullOrEmpty(expenseEntityIDFilter) ? 0 : int.Parse(expenseEntityIDFilter);

            //--- Set current filter selections into ViewBag
            SetFilter2ViewBag(filterExpenseTypeID.ToString(), filterExpenseEntityID.ToString(), filterDateFrom.ToString("MM/dd/yyyy"), filterDateTo.ToString("MM/dd/yyyy"), descriptionSearchFilter);

            #endregion

            #region Filter expense records
            var expenseRecords = db.ExpenseRecords
                .Where(x => x.UserId == userId
                        && x.ExpenseDate >= filterDateFrom
                        && x.ExpenseDate <= filterDateTo
                        && (filterExpenseTypeID == 0 || x.ExpenseTypeID == filterExpenseTypeID)
                        && (filterExpenseEntityID == 0 || x.ExpenseEntityID == filterExpenseEntityID)
                        && (String.IsNullOrEmpty(descriptionSearchFilter) || x.ExpenseDescription.IndexOf(descriptionSearchFilter) >= 0))
                .Include(e => e.ExpenseEntity)
                .Include(e => e.ExpenseType);
            #endregion

            #region Calculate total expenses
            if (expenseRecords != null && expenseRecords.ToList().Count > 0)
                ViewBag.Total = expenseRecords.Sum(x => x.ExpensePrice);
            else
                ViewBag.Total = 0;
            #endregion

            expenseRecords = ApplyOrderBy(sortOrder, expenseRecords);
            SetExpenseTypeList2ViewBag(userId);
            SetExpenseEntityList2ViewBag(userId, filterExpenseTypeID);

            return View(expenseRecords.ToList());
        }

        // GET: ExpenseRecord/Create
        [RequireHttps]
        [Authorize]
        public ActionResult Create(string expenseTypeIDFilter, string expenseEntityIDFilter, string fromDateFilter, string toDateFilter, string descriptionSearchFilter)
        {
            try
            {
                //--- Set current variables in ViewBag
                SetVariables2ViewBag();
                
                //--- Set current filter selections into ViewBag
                SetFilter2ViewBag(expenseTypeIDFilter, expenseEntityIDFilter, fromDateFilter, toDateFilter, descriptionSearchFilter);

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
        [RequireHttps]
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
                    });

                    if (!String.IsNullOrEmpty(uploadAttachmentTaskResult))
                        throw new Exception($"Some of the attachments were not uploaded. {uploadAttachmentTaskResult}");


                    //--- redirect to Index page and pass the filter
                    return RedirectToAction("Index", IndexViewRouteValues());
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
        [RequireHttps]
        [Authorize]
        public ActionResult Edit(int? id, string expenseTypeIDFilter, string expenseEntityIDFilter, string fromDateFilter, string toDateFilter, string descriptionSearchFilter)
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

            //--- Set current filter selections into ViewBag
            SetFilter2ViewBag(expenseTypeIDFilter, expenseEntityIDFilter, fromDateFilter, toDateFilter, descriptionSearchFilter);

            SetVariables2ViewBag(expenseRecord);

            return View(expenseRecord);
            
        }
        
        // POST: ExpenseRecord/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [RequireHttps]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> Edit([Bind(Include = "ExpenseRecordID,ExpenseTypeID,ExpenseType,ExpenseEntityID,ExpenseEntity,ExpenseDate,ExpensePrice,ExpenseDescription,selectFiles,FilesToDelete")] ExpenseRecord expenseRecord)
        {
            string userId = User.Identity.GetUserId();
            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(expenseRecord).State = EntityState.Modified;
                    await DeleteSelectedAttachmentFilesAsync(expenseRecord.ExpenseRecordID);
                    string uploadAttachmentTaskResult = await InsertAttachmentRecordsAsync(expenseRecord.ExpenseRecordID);
                    await Task.Factory.StartNew(() =>
                    {
                        expenseRecord.ExpenseLogDate = DateTime.Now;
                        expenseRecord.UserId = userId;
                        db.SaveChanges();
                    });

                    if (uploadAttachmentTaskResult != String.Empty)
                        throw new Exception($"Some of the attachments were not uploaded. {uploadAttachmentTaskResult}");

                    //---redirect to Index page and pass the filter
                    return RedirectToAction("Index", IndexViewRouteValues());

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

        // POST: ExpenseRecord/Delete/5
        [RequireHttps]
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
                    //--- Delete all attachments for the current expense record
                    if (await DeleteAttachmentFilesAsync(expenseRecord.ExpenseAttachments.Select(x => x.ExpenseAttachmentName)))
                    {
                        //--- Delete current expense record and it will delete also the related attachment records
                        db.ExpenseRecords.Remove(expenseRecord);
                        db.SaveChanges();
                    }
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    SetVariables2ViewBag(expenseRecord);
                    this.ModelState.AddModelError("ExpenseDescription", ex.GetBaseException().Message);
                    return View("Edit", expenseRecord);
                }
            }
        }

        // GET: ExpenseRecord/GetEntityListByType/5
        [RequireHttps]
        [Authorize]
        public JsonResult GetEntityListByType(int expenseTypeID, string page)
        {
            string userId = User.Identity.GetUserId();

            Dictionary<string, string> expenseEntities = db.ExpenseEntities
                        .Where(x => x.UserId == userId && x.ExpenseTypeID == expenseTypeID)
                        .ToDictionary(x => x.ID.ToString(), x => x.ExpenseEntityName);

            if (expenseEntities.Count == 0 || page == "ExpenseRecord")
                expenseEntities.Add("0", "ALL ENTITIES");

            JsonResult result = new JsonResult { Data = expenseEntities.ToList().OrderBy(x => x.Key), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            return result;
        }

        // GET: ExportData
        [Authorize]
        [RequireHttps]
        public ActionResult ExportToExcel()
        {
            //--- Set the data source
            GridView gridview = new GridView
            {
                DataSource = db.ExpenseRecords.ToList()
            };
            gridview.DataBind();

            //--- Clear all the content from the current response
            Response.ClearContent();
            Response.Buffer = true;
            //--- Set the header
            Response.AddHeader("content-disposition", "attachment;filename = itfunda.xls");
            Response.ContentType = "application/ms-excel";
            Response.Charset = "";

            //--- Create HtmlTextWriter object with StringWriter
            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter htw = new HtmlTextWriter(sw))
                {
                    //--- Render the GridView to the HtmlTextWriter
                    gridview.RenderControl(htw);
                    //--- Output the GridView content saved into StringWriter
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

        private void SetViewBagSelectLists(string userId, ExpenseRecord expenseRecord = null)
        {
            IEnumerable<ExpenseType> expenseTypes = db.ExpenseTypes.Where(x => x.UserId == userId).OrderBy(x => x.Title);

            if (expenseRecord == null)
            {
                ViewBag.ExpenseTypeID = new SelectList(expenseTypes, "ID", "Title");
                ViewBag.ExpenseEntityID = new SelectList(db.ExpenseEntities.Where(x => x.UserId == userId && x.ExpenseTypeID == expenseTypes.FirstOrDefault().ID).OrderByDescending(x => x.ID), "ID", "ExpenseEntityName");
            }
            else
            {
                ViewBag.ExpenseTypeID = new SelectList(expenseTypes, "ID", "Title", expenseRecord.ExpenseTypeID);
                ViewBag.ExpenseEntityID = new SelectList(db.ExpenseEntities.Where(x => x.UserId == userId && x.ExpenseTypeID == expenseRecord.ExpenseTypeID).OrderByDescending(x => x.ID), "ID", "ExpenseEntityName", expenseRecord.ExpenseEntityID);
            }
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

        private async Task<bool> DeleteAttachmentFilesAsync(IEnumerable<string> attachments)
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

        private void SetVariables2ViewBag(ExpenseRecord expenseRecord = null)
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

        private void SetFilter2ViewBag(string filterExpenseTypeID, string filterExpenseEntityID, string filterDateFrom, string filterDateTo, string filterDescriptionSearch)
        {
            //--- Save Filter to ViewBag
            ViewBag.FilterExpenseTypeID = filterExpenseTypeID;
            ViewBag.FilterExpenseEntityID = filterExpenseEntityID;
            ViewBag.FilterDateFrom = filterDateFrom;
            ViewBag.FilterDateTo = filterDateTo;
            ViewBag.FilterDescriptionSearch = filterDescriptionSearch;
        }

        private async Task DeleteSelectedAttachmentFilesAsync(int expenseRecordID)
        {
            if (Request["FilesToDelete"] != null)
            {
                string filesToDelete = Request["FilesToDelete"];
                if (filesToDelete != String.Empty)
                {
                    //--- select those attachments that are listed for deletion
                    List<ExpenseAttachment> attachmentsToDelete =
                        db.ExpenseRecords
                        .Where(x => x.ExpenseRecordID == expenseRecordID)
                        .Include("ExpenseAttachments")
                        .FirstOrDefault()
                        .ExpenseAttachments
                        .Where(x => filesToDelete.Contains($"[{x.ID}]"))
                        .ToList();

                    //--- delete selected attachment files
                    await DeleteAttachmentFilesAsync(attachmentsToDelete.Select(x => x.ExpenseAttachmentName));

                    //--- delete selected attachment records
                    foreach (ExpenseAttachment attachment in attachmentsToDelete)
                    {
                        db.ExpenseAttachments.Remove(attachment);
                    }
                }
            }
        }

        private IQueryable<ExpenseRecord> ApplyOrderBy(string sortOrder, IQueryable<ExpenseRecord> expenseRecords)
        {
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

            return expenseRecords;
        }

        
        private void SetExpenseEntityList2ViewBag(string userId, int filterExpenseTypeID)
        {
            List<ExpenseEntity> entities = new List<ExpenseEntity>
            {
                new ExpenseEntity { ID = 0, ExpenseEntityName = "ALL ENTITIES" }
            };
            entities.AddRange(db.ExpenseEntities.Where(x => x.UserId == userId && (filterExpenseTypeID == 0 || x.ExpenseTypeID == filterExpenseTypeID)));

            ViewBag.ExpenseEntities = entities.Select(item => new SelectListItem
            {
                Value = item.ID.ToString(),
                Text = item.ExpenseEntityName.ToString(),
                Selected = "select" == item.ID.ToString()
            });
        }

        private void SetExpenseTypeList2ViewBag(string userId)
        {
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
        }

        private object IndexViewRouteValues()
        {
            return new
            {
                expenseTypeIDFilter = Request["FilterExpenseTypeID"],
                expenseEntityIDFilter = Request["FilterExpenseEntityID"],
                fromDateFilter = Request["FilterDateFrom"],
                toDateFilter = Request["FilterDateTo"],
                descriptionSearchFilter = Request["FilterDescriptionSearch"]
            };
        }

        #endregion
    }

}
