using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Cache;
using System.Threading.Tasks;

namespace Website.Models
{
    public class AccountDetails : Account
    {
        public long AccountNumber { get; set; }

        public int AccountId { get; set; }

        public string FirstName { get; set; }
        
        public string LastName { get; set; }

        public string Address { get; set; }

        public string RowVersion { get; set; }
    }
}
