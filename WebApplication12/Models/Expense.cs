using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication12.Models
{
    public class Expense
    {
        public int id { get; set; }
        public string CurrentUser { get; set; }
        public int IsOwe { get; set; }
        public string Note { get; set; }
        public string FriendUser { get; set; }
        public float FriendUserValue { get; set; }
    }
}
