using System.Collections.Generic;
using Website.Models;

namespace Website.DataStores.Interface
{
    public interface ITransactionDataStore
    {
        /// <summary>
        /// List of transactions for specified account number.
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        Response<List<WalletTransaction>> GetAccountTransactionsHistoryList(long accountNumber);

        /// <summary>
        /// Credit Money
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <param name="amount"></param>
        /// <param name="rowVersion"></param>
        /// <returns></returns>
        Response<bool> Deposit(long accountNumber, decimal amount, byte[] rowVersion);

        /// <summary>
        /// Debit Money
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <param name="amount"></param>
        /// <param name="rowVersion"></param>
        /// <returns></returns>
        Response<bool> Withdraw(long accountNumber, decimal amount, byte[] rowVersion);

        /// <summary>
        /// Fund Transfer to anyone
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <param name="accountNumberReceiver"></param>
        /// <param name="amount"></param>
        /// <param name="rowVersion"></param>
        /// <returns></returns>
        Response<bool> FundTransfer(long accountNumber, long accountNumberReceiver, decimal amount, byte[] rowVersion);

    }
}
;