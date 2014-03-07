using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ozu_EMS
{
    public class Info
    {
        public string requestPath { get; set; }
        public int requestTime { get; set; }
        public Log log { get; set; }
        public int responseStatus { get; set; }
    }

    public class Log
    {
        public Error[] error { get; set; }
    }

    public class Error
    {
        public string[] validation { get; set; }
    }

}