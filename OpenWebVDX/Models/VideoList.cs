using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenWebVDX.Models
{
    public class VideoList
    {
        public List<string> Ids { get; set; }
        public List<string> Titles { get; set; }
        public List<string> Dates { get; set; }
        public List<string> Users { get; set; }
    }
}