using System;
using System.ComponentModel.DataAnnotations;

namespace GameContextAPI.Entities
{
    public class User
    {
        [Key]
        public Guid user_id { get; set; }
        public string display_name { get; set; }
        public double points { get; set; }
        public string country { get; set; }
        public string timestamp { get; set; }
    }
}
