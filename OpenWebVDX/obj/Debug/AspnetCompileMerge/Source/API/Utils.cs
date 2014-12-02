using MySql.Data.MySqlClient;
using OpenWebVDX.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
namespace Utils
{
    public static class RequestValidation
    {
        public static bool HasNullParams(string parameter)
        {
            if (parameter == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool HasNullParams(string[] parameters)
        {
            if(parameters == null)
            {
                return true;
            }
            else
            {
                for(int i = 0; i < parameters.Length; i++)
                {
                    if(parameters[i] == null)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
    public static class StringOps
    {
        public static string ExtractJsonKey(string json_str)
        {
            for (int i = 0; i < json_str.Length; i++)
            {
                if (json_str[i] == '=')
                {
                    json_str = json_str.Substring(0, i);
                }
            }
            json_str = json_str.Replace("+", " ");
            return json_str;
        }

        public static string ExtractJsonValue(string json_str)
        {
            for (int i = 0; i < json_str.Length; i++)
            {
                if (json_str[i] == '=')
                {
                    json_str = json_str.Substring(i + 1);
                }
            }
            json_str = json_str.Replace("+", " ");
            return json_str;
        }
    }

    public static class DatabaseOps
    {
        public static string GetVideoPath(string id)
        {
            MySqlConnection connection = new MySqlConnection();
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString;
            try
            {
                connection.Open();

                MySqlCommand cmd = new MySqlCommand("SELECT path FROM videos WHERE id=@id", connection);
                cmd.Parameters.AddWithValue("@id", id);
                MySqlDataReader rdr = cmd.ExecuteReader();
                rdr.Read();
                return rdr.GetString(0);
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("DatabaseOps - > GetVideoPath() Error:" +ex.Message);
                return "";
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                }
            }
        }
        public static void UploadNotification(string title, string user, string date, string path)
        {
            MySqlConnection connection = new MySqlConnection();
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString;
            try
            {
                connection.Open();

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
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("DatabaseOps - > UploadNotification() Error:" + ex.Message);
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                }
            }
        }

        public static Video GetVideoRecord(string id)
        {
            Video requestedVideo = new Video();

            MySqlConnection connection = new MySqlConnection();
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString;
            try
            {
                connection.Open();

                MySqlCommand cmd = new MySqlCommand("SELECT id, title, date, user FROM videos WHERE id=@id", connection);
                cmd.Parameters.AddWithValue("@id", id);
                MySqlDataReader rdr = cmd.ExecuteReader();

                rdr.Read();

                requestedVideo.Id = rdr.GetString(0);
                requestedVideo.Title = rdr.GetString(1);
                requestedVideo.Date = rdr.GetString(2);
                requestedVideo.User = rdr.GetString(3);


                return requestedVideo;
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("DatabaseOps - > GetVideoRecord() Error:" + ex.Message);
                return requestedVideo;
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                }
            }
        }

        public static VideoList AllVideoInfo()
        {
            VideoList allVideoInfo = new VideoList();

            List<string> videoIds = new List<string>();
            List<string> videoTitles = new List<string>();
            List<string> videoDates = new List<string>();
            List<string> videoUsers = new List<string>();

            MySqlConnection connection = new MySqlConnection();
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString;
            try
            {
                connection.Open();

                MySqlCommand cmd = new MySqlCommand("SELECT id, title, date, user FROM videos", connection);

                MySqlDataReader rdr = cmd.ExecuteReader();

                while(rdr.Read())
                {
                    videoIds.Add(rdr.GetString(0));
                    videoTitles.Add(rdr.GetString(1));
                    videoDates.Add(GetDate(rdr.GetString(2)));
                    videoUsers.Add(rdr.GetString(3));
                }

                allVideoInfo.Ids = videoIds;
                allVideoInfo.Titles = videoTitles;
                allVideoInfo.Dates = videoDates;
                allVideoInfo.Users = videoUsers;

                return allVideoInfo;
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("DatabaseOps - > VideoList() Error:" + ex.Message);
                return allVideoInfo;
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                }
            }
        }

        private static string GetDate(string date_time_str)
        {
            string str = date_time_str;
            string ret = "";

            for(int i = 0; i < str.Length; i++)
            {
                if(str[i] == ' ')
                {
                    ret = str.Substring(0, i);
                    break;
                }
            }

            if(ret.Equals(""))
            {
                System.Diagnostics.Debug.WriteLine("Utils: GetDate() failed to identify a date string.");
            }

            return ret;
        }
    }
}