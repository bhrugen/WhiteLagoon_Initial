using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhiteLagoon_Models.ViewModels
{
    public class EmailData
    {
        public string From { get; set; } = "test@gmail.com";
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public StreamReader Template { get; set; }
        public string Password { get; set; } = "1234";
    }
}
