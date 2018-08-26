using System.Collections.Generic;
using System.ComponentModel;
using Website.Enum;

namespace Website.ViewModels
{
    public class WalletViewModel
    {
        [DisplayName("Transaction Type")]
        public TransactionType TransactionType { get; set; }

        public decimal Amount { get; set; }

        public long? AccountNumber { get; set; }

        [ReadOnly(true)]
        public decimal Balance { get; set; }

        public List<TransactionViewModel> Transactions { get; set; } = new List<TransactionViewModel>();
    }
}
