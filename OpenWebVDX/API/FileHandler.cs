using System.Web;
using System.Web.Mvc;

namespace OpenWebVDX.API.FileHandler
{
   public class VDXFile
   {
       private HttpPostedFileBase file;

       public VDXFile(HttpPostedFileBase file)
       {
           this.file = file;
       }

       public void writeToAppData(HttpContextBase context)
       {
           int fileSize = file.ContentLength;
           string fileName = file.FileName;
           string mimeType = file.ContentType;
           System.IO.Stream fileContent = file.InputStream;
           file.SaveAs(context.Server.MapPath("~/App_Data/uploads/") + fileName); //File will be saved in application root
       }
   }
}