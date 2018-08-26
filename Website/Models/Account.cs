using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Website.Models
{
    public class Account
    {
        public int Id { get; set; }
        
        public string LoginName { get; set; }

        public string Password { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
