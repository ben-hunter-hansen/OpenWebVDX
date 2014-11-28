using System;
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
}