using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmptClient
{
    public class DataInput
    {
        public ServerConfiguration server { get; set; }
        public DataItem common { get; set; }
        public DataItem[] items { get; set; }
    }

    public class ServerConfiguration
    {
        public string host { get; set; }
        public int port { get; set; }
        public string username { get; set; }
        public string apikey { get; set; }
    }

    public class DataItem
    {
        public string fromEmail { get; set; }
        public string fromName { get; set; }
        public string subject { get; set; }
        public string htmlFile { get; set; }
        public string textFile { get; set; }
        public string toEmail { get; set; }
        public string toName { get; set; }
        public string[] attachmentFiles { get; set; } = new string[0];
    }
}
