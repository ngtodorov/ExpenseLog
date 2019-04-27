using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExpenseLogWebAPI.Models
{
    public class Attachment
    {
        //--- Constructor 1
        public Attachment()
        {

        }

        //--- Constructor 2
        public Attachment(string name, string type, string originalName, long lenght, string uri)
        {
            this.ID = Guid.NewGuid().ToString();
            this.Name = name;
            this.Type = type;
            this.OriginalName = originalName;
            this.Length = lenght;
            this.UploadDate = DateTime.Now;
            this.Uri = uri;
        }

        public string ID { get; set; }              //--- Unique attachment ID (GUID) assigned at the timie of upload /model creation/

        public string Name { get; set; }            //--- Attachment name as stored in Azure blob storage

        public string Type { get; set; }            //--- Attachment file type (Media Type)

        public string Uri { get; set; }             //--- Full path to the file on Azure blob storage

        public string OriginalName { get; set; }    //--- Original file name as uploaded from the user

        public long Length { get; set; }            //--- Attachment file bytes

        public DateTime UploadDate { get; set; }    //--- Date/Time of the initial file upload
    }
}