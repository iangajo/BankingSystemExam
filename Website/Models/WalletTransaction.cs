using System;
using Website.Enum;

namespace Website.Models
{
    public class WalletTransaction
    {
        
        public int Id { get; set; }

        public DateTime TransactionDate { get; set; }

        public long AccountNumber { get; set; }

        public TransactionType TransactionType { get; set; }

        public decimal? Credit { get; set; }

        public decimal? Debit { get; set; }

        public decimal Balance { get; set; }

        public long? TransactionReference { get; set; }
    }
}
