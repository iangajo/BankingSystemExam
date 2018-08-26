using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Website.ViewModels
{
    public class TransactionViewModel
    {
        public DateTime TransactionDateTime { get; set; }

        public string Description { get; set; }

        public long? Reference { get; set; }

        public decimal? Credit { get; set; }

        public decimal? Debit { get; set; }

        public decimal? Balance { get; set; }
    }
}
