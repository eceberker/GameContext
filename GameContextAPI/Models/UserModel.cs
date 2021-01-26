using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameContextAPI.Models
{
    public class UserModel
    {
        public string user_id { get; set; }
        public string display_name { get; set; }
        public double points { get; set; }
        public string country { get; set; }
        public string timestamp { get; set; }
    }
}
