using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication12.ViewModels
{
    public class ExpenseViewModel
    {
        public int id { get;set; }
        public string Note { get; set; }
        public float FriendUserValue { get; set; }
        public string FriendName { get; set; }
        public int IsOwe { get; set; }
    }
}
