using System.Web;

namespace VDXApp.WebSocketService
{
    public class VDXSocketFile
    {
        public HttpPostedFileBase File { get; set; }
        public string Title { get; set; }
        public string User { get; set; }
        public string Date { get; set; }

        public string TargetFileName { get; set; }
        public string OrigPath { get; set; }
        public string DestPath { get; set; }

        public VDXSocketFile(HttpPostedFileBase file, string title, string user, string date)
        {
            this.File = file;
            this.Title = title;
            this.User = user;
            this.Date = date;


        }

        public void PrepareForConversion(string path, string target_type)
        {
            this.TargetFileName = FormatTitle(this.Title) + "." + target_type;

            this.OrigPath = path + this.File.FileName;
            this.DestPath = path + this.TargetFileName;

        }

        private string FormatTitle(string to_format)
        {
            string ret = to_format;
            if (ret.Contains(" "))
            {
                ret = ret.Replace(" ", "_");
            }
            return ret;
        }
    }
}