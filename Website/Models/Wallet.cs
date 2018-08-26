using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Website.Models
{
    public class Wallet
    {
        public long AccountNumber { get; set; }

        public decimal Balance { get; set; }

        public Byte[] RowVersion { get; set; }
    }
}
