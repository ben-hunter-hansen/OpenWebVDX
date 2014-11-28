using MySql.Data.MySqlClient;
using OpenWebVDX.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
namespace Utils
{
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
        public static VideoInfo AllVideoInfo()
        {
            VideoInfo allVideoInfo = new VideoInfo();

            List<string> videoTitles = new List<string>();
            List<string> videoDates = new List<string>();
            List<string> videoUsers = new List<string>();

            MySqlConnection connection = new MySqlConnection();
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString;
            try
            {
                connection.Open();
                System.Diagnostics.Debug.WriteLine("Connected to database..");

                MySqlCommand cmd = new MySqlCommand("SELECT title, date, user FROM videos", connection);

                MySqlDataReader rdr = cmd.ExecuteReader();

                while(rdr.Read())
                {
                    videoTitles.Add(rdr.GetString(0));
                    videoDates.Add(GetDate(rdr.GetString(1)));
                    videoUsers.Add(rdr.GetString(2));
                }

                allVideoInfo.Titles = videoTitles;
                allVideoInfo.Dates = videoDates;
                allVideoInfo.Users = videoUsers;

                return allVideoInfo;
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
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