using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Web;
using System.Web.Mvc;

namespace OpenWebVDX.API.FileHandler
{
   public class VDXFile
   {
       private HttpPostedFileBase file;
       private string title;

       public HttpPostedFileBase File { get { return file; } }

       public VDXFile(HttpPostedFileBase file, string title)
       {
           this.file = file;
           this.title = title;
       }

       public void writeUpload(HttpContextBase context, string user, string date)
       {
           int fileSize = file.ContentLength;
           string fileName = file.FileName;
           string mimeType = file.ContentType;

           System.IO.Stream fileContent = file.InputStream;

           string pathToUpload = context.Server.MapPath("~/App_Data/uploads/") + fileName;

           notifyDatabase(this.title, user, date, pathToUpload);  //Db info
           file.SaveAs(pathToUpload); //Binary to disk
       }
       
       private void notifyDatabase(string title, string user, string date, string path)
       {
           MySqlConnection connection = new MySqlConnection();
           connection.ConnectionString = ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString;
           try
           {
               connection.Open();
               System.Diagnostics.Debug.WriteLine("Connected to database..");

               MySqlCommand cmd = new MySqlCommand();
               cmd.Connection = connection;

               cmd.CommandText = "INSERT INTO videos(title,date,user,path) VALUES (@title, @date, @user, @path)";
               cmd.Prepare();

               cmd.Parameters.AddWithValue("@title", title);
               cmd.Parameters.AddWithValue("@date", date);
               cmd.Parameters.AddWithValue("@user", user);
               cmd.Parameters.AddWithValue("@path", path);

               cmd.ExecuteNonQuery();
           } 
           catch(MySqlException ex)
           {
               System.Diagnostics.Debug.WriteLine(ex.Message);
           }
           finally
           {
               if(connection != null)
               {
                   connection.Close();
               }
           }

       }
   }

   public static class VDXFileValidator
   {
       public static bool isVideoFormat(VDXFile toValidate)
       {
           if(toValidate.File.ContentType.Contains("video"))
           {
               return true;
           } 
           else
           {
               return false;
           }
       }
   }
}