using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ExpenseLogWebAPI.Helpers;

using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

using System.Web.Http.Cors;

using ExpenseLogWebAPI.Models;
using Newtonsoft.Json;

namespace ExpenseLogWebAPI.Controllers
{
    public class AttachmentController : ApiController
    {
        private readonly object _lockObject = new object();
        private static CloudBlobContainer _CloudBlobContainer;


        [SwaggerOperation("Get")]
        public string Get()
        {
            return "ExpenseLog WebAPI, Attachment Controler";
        }

        [SwaggerOperation("Get")]
        public string Get(string id, string name)
        {
            string result = "Invalid parameters";

            ExpenseLogCommon.Utils utils = new ExpenseLogCommon.Utils();

            if (!String.IsNullOrEmpty(id) && !String.IsNullOrEmpty(name))
            {
                if (id.StartsWith("encrypt", StringComparison.CurrentCultureIgnoreCase))
                    result = utils.Encrypt(name);
                else
                    if (id.StartsWith("decrypt", StringComparison.CurrentCultureIgnoreCase))
                        result = utils.Decrypt(name);
            }

            return result;
        }

        // POST: UPLOAD ATTACHMENT(s)
        [HttpPost, Route("api/attachment/upload")]
        public async Task<HttpResponseMessage> Upload()
        {
            string error = String.Empty;
            List<Attachment> result = new List<Attachment>();

            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

                //--- read encrypted userID from the request
                string userID = GetRequestParameter("userID");

                //--- initialize cloud container
                InitializeCloudBlobContainer();

                //--- loop all files in the request and process(convert & upload) one by one 
                foreach (HttpContent content in (await Request.Content.ReadAsMultipartAsync()).Contents)
                {
                    Attachment attachment = await UploadSingleFile(content, userID);
                    if (attachment != null)
                        lock (_lockObject) { result.Add(attachment); }
                }

            }
            catch (Exception ex)
            {
                error = ex.GetBaseException().Message;
            }

            //--- return the collection of Attachments in Jsaon format
            if (String.IsNullOrEmpty(error))
                return Request.CreateResponse(HttpStatusCode.OK, result, Configuration.Formatters.JsonFormatter);
            else
                return Request.CreateResponse(HttpStatusCode.InternalServerError, $"api/attachment/upload action failed: {error}");
        }

        // POST: DELETE ATTACHMENT(s)
        // Provide FormData (in the body) parameter named "attachmentNameList" with the comma separated list of all attachment names to be deleted
        [HttpPost, Route("api/attachment/delete")]
        public async Task<HttpResponseMessage> Delete()
        {
            string error = String.Empty;

            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

                List<string> attachmentNameList = GetAttachmentNameList(ref error);

                if (error == String.Empty)
                {
                    //--- initialize cloud container
                    InitializeCloudBlobContainer();

                    //--- loop thru all attachments and delete them one by one
                    foreach (string attachmentName in attachmentNameList)
                    {
                        CloudBlockBlob cloudBlockBlob = _CloudBlobContainer.GetBlockBlobReference(attachmentName);
                        if (cloudBlockBlob != null)
                        {
                            await cloudBlockBlob.DeleteIfExistsAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                error = ex.GetBaseException().Message;
            }

            if (String.IsNullOrEmpty(error))
            {
                return Request.CreateResponse(HttpStatusCode.OK, "Success", Configuration.Formatters.JsonFormatter);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, $"api/attachment/delete action failed: {error}");
            }
        }


        #region Helpers

        private List<string> GetAttachmentNameList(ref string error)
        {
            //--- Example of attachmentNameList provided by the client:
            //--- ["1234567890_1636909635301279788_8ceced0a-cedd-4147-b32d-e56b0bca4546.jpg","1234567890_1636909635304653845_c2f8af61-8ae8-4041-a5fd-73550e6055a0.JPG"]

            //--- get attachment list (comma separated list) from the encrypted paramater par of the request 
            string attachmentNameListString = GetRequestParameter("attachmentNameList");
            List<string> attachmentNameList = null;
            try
            {
                //--- deserialize to list
                attachmentNameList = JsonConvert.DeserializeObject<IEnumerable<string>>(attachmentNameListString).ToList<string>();
            }
            catch (Exception ex1)
            {
                error = $"JsonConvert.DeserializeObject failed. {ex1.GetBaseException().Message}";
            }

            return attachmentNameList;
        }

        private string GetRequestParameter(string paramName)
        {
            string result = String.Empty;

            if (System.Web.HttpContext.Current.Request.Params[paramName] != null)
            {
                ExpenseLogCommon.Utils utils = new ExpenseLogCommon.Utils();
                result = utils.Decrypt(System.Web.HttpContext.Current.Request.Params[paramName].Trim());
            }

            if (result == String.Empty)
            {
                throw new Exception($"HttpContext.Current.Request Parameter '{paramName}' is null or empty.");
            }

            return result;
        }

        /// <summary> 
        /// Generates a unique random file name to be uploaded  
        /// </summary> 
        private string GetUniqueBlobName(string userID, string filename)
        {
            return string.Format("{0}_{1:10}_{2}{3}", userID, DateTime.Now.Ticks, Guid.NewGuid(), Path.GetExtension(filename));
        }


        private void InitializeCloudBlobContainer()
        {
            if (_CloudBlobContainer == null)
            {
                ExpenseLogCommon.Utils utils = new ExpenseLogCommon.Utils();
                //--- Retrieve storage account information from connection string
                //--- How to create a storage connection string - http://msdn.microsoft.com/en-us/library/azure/ee758697.aspx
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(utils.GetAppSetting("EL_STORAGE_CONNECTION_STRING"));

                //--- Create a blob client for interacting with the blob service.
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                _CloudBlobContainer = blobClient.GetContainerReference(utils.GetAppSetting("EL_STORAGE_BLOB_CONTAINER_NAME"));
                _CloudBlobContainer.CreateIfNotExists();

                //--- To view the uploaded blob in a browser, you have two options. The first option is to use a Shared Access Signature (SAS) token to delegate  
                //--- access to the resource. See the documentation links at the top for more information on SAS. The second approach is to set permissions  
                //--- to allow public access to blobs in this container. Comment the line below to not use this approach and to use SAS. Then you can view the image  
                //--- using: https://[InsertYourStorageAccountNameHere].blob.core.windows.net/webappstoragedotnet-imagecontainer/FileName 
                _CloudBlobContainer.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            }
        }


        private async Task<Attachment> UploadSingleFile(HttpContent httpContent, string userID)
        {
            Attachment result = null;

            if (IsValidMediaType(httpContent))
            {
                string fileName = httpContent.Headers.ContentDisposition.FileName.Trim('\"');   //--- original file name
                string blobFileName = GetUniqueBlobName(userID, fileName);                      //--- new (blob storage) file name

                //--- create blob container on Azure
                CloudBlockBlob cloudBlockBlob = _CloudBlobContainer.GetBlockBlobReference(blobFileName);
                string mediaType = httpContent.Headers.ContentType.MediaType;

                if (mediaType.ToLower().Contains("image") && !mediaType.ToLower().Contains("tif"))
                {
                    //--- convert image to grayscale Jpeg & upload to the bloab
                    ImageUtils imageUtils = new ImageUtils();
                    byte[] imageFileBytes = imageUtils.ConvertToGrayscaleJpeg(await httpContent.ReadAsStreamAsync());
                    if (imageFileBytes != null && imageFileBytes.Length > 0)
                    {
                        await cloudBlockBlob.UploadFromByteArrayAsync(imageFileBytes, 0, imageFileBytes.Length);
                    }
                }
                else
                {
                    await cloudBlockBlob.UploadFromStreamAsync(await httpContent.ReadAsStreamAsync());
                }

                //--- return Attachment file object
                result = new Attachment(blobFileName, mediaType, fileName, httpContent.Headers.ContentLength.Value, cloudBlockBlob.Uri.ToString());
            }

            return result;
        }

        private bool IsValidMediaType(HttpContent httpContent)
        {
            return (httpContent != null
                && httpContent.Headers != null
                && httpContent.Headers.ContentType != null
                && httpContent.Headers.ContentType.MediaType != null
                && (httpContent.Headers.ContentType.MediaType.ToLower().Contains("image") || httpContent.Headers.ContentType.MediaType.Contains("pdf")));
        }

        #endregion
    }
}
