using System;

namespace GameContextAPI.Models
{
    public class LeaderBoardModel
    {
        public long rank { get; set; }
        public double points { get; set; }
        public string display_name { get; set; }
        public string country { get; set; }
        public Guid? user_id { get; set; }
    }
}
