using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExpenseLog.Models
{
    public class Attachment
    {
        public Attachment()
        {

        }

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

        public string ID { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public string Uri { get; set; }

        public string OriginalName { get; set; }

        public long Length { get; set; }

        public DateTime UploadDate { get; set; }
    }
}