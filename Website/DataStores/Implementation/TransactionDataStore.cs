using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Transactions;
using Website.DataStores.Interface;
using Website.Enum;
using Website.Models;

namespace Website.DataStores.Implementation
{
    public class TransactionDataStore : ITransactionDataStore
    {
        private readonly string _connectionString;

        private const string SqlDepositWallet = "UPDATE [Wallet] SET Balance = Balance + @Balance WHERE AccountNumber = @AccountNumber AND RowVersion = @RowVersion";
        private const string SqlWithdrawWallet = @"DECLARE @Balance DECIMAL(18,2)
                        SELECT @Balance = Balance FROM [Wallet] WHERE AccountNumber = @AccountNumber
                        IF (@Balance < @WithdrawAmount) BEGIN RAISERROR('Insufficient Funds',16,1); END
                        ELSE BEGIN UPDATE [Wallet] SET Balance = Balance - @WithdrawAmount WHERE AccountNumber = @AccountNumber AND RowVersion = @RowVersion END";

        private const string SqlDepositWalletNoRowVersion = "UPDATE [Wallet] SET Balance = Balance + @Balance WHERE AccountNumber = @AccountNumber";

        private const string SqlDepositTransaction = @"DECLARE @Balance DECIMAL(18,2) 
                                        SELECT @Balance = Balance FROM [Wallet] WHERE AccountNumber = @AccountNumber
                            INSERT INTO [Transaction] (TransactionType, Credit, Balance, AccountNumber) VALUES (@TransactionType, @Credit, @Balance, @AccountNumber)";

        private const string SqlWithdrawTransaction = @"DECLARE @Balance DECIMAL(18,2) 
                                        SELECT @Balance = Balance FROM [Wallet] WHERE AccountNumber = @AccountNumber
                            INSERT INTO [Transaction] (TransactionType, Debit, Balance, AccountNumber) VALUES (@TransactionType, @Debit, @Balance, @AccountNumber)";


        private const string SqlWithdrawTransactionWithIdentityScope = @"DECLARE @Balance DECIMAL(18,2) 
                                        SELECT @Balance = Balance FROM [Wallet] WHERE AccountNumber = @AccountNumber
                            INSERT INTO [Transaction] (TransactionType, Debit, Balance, AccountNumber) VALUES (@TransactionType, @Debit, @Balance, @AccountNumber); SELECT SCOPE_IDENTITY();";

        private const string SqlDepositTransactionWithTransactionReference = @"DECLARE @Balance DECIMAL(18,2) 
                                        SELECT @Balance = Balance FROM [Wallet] WHERE AccountNumber = @AccountNumber
                            INSERT INTO [Transaction] (TransactionType, Credit, Balance, AccountNumber, TransactionReference) VALUES (@TransactionType, @Credit, @Balance, @AccountNumber, @TransactionReference)";

        public TransactionDataStore(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public Response<List<WalletTransaction>> GetAccountTransactionsHistoryList(long accountNumber)
        {
            var response = new Response<List<WalletTransaction>>()
            {
                ErrorMessage = string.Empty,
                Data = new List<WalletTransaction>()
            };    

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    using (var command =
                        new SqlCommand(
                            "SELECT TransactionDate, Id, AccountNumber, TransactionType, Credit, Debit, TransactionReference, Balance FROM [Transaction] WHERE AccountNumber = @AccountNumber",
                            connection))
                    {
                        command.Parameters.AddWithValue("@AccountNumber", accountNumber);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    var transaction = new WalletTransaction()
                                    {
                                        TransactionDate = (DateTime) reader["TransactionDate"],
                                        Id = (int) reader["Id"],
                                        AccountNumber = (long) reader["AccountNumber"],
                                        Credit = reader["Credit"] as decimal?,
                                        Debit = reader["Debit"] as decimal?,
                                        Balance = (decimal) reader["Balance"],
                                        TransactionType = (TransactionType) reader["TransactionType"],
                                        TransactionReference = reader["TransactionReference"] as int?
                                    };

                                    response.Data.Add(transaction);
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception)
            {
                response.ErrorMessage = "Oops. Something went wrong.";
            }

            return response;
        }

        #region Deposit

        public Response<bool> Deposit(long accountNumber, decimal amount, byte[] rowVersion)
        {
            var response = new Response<bool>()
            {
                ErrorMessage = string.Empty,
                Data = false
            };

            using (var ts = new TransactionScope())
            {
                try
                {
                    SqlParameter[] depositSqlParameters =
                    {
                        new SqlParameter("@Balance", amount),
                        new SqlParameter("@AccountNumber", accountNumber),
                        new SqlParameter("@RowVersion", rowVersion),
                    };

                    var rowsAffectedWallet = ExecuteTransaction(SqlDepositWallet, depositSqlParameters);

                    SqlParameter[] transactionSqlParameters =
                    {
                        new SqlParameter("@TransactionType", TransactionType.Deposit),
                        new SqlParameter("@Credit", amount),
                        new SqlParameter("@AccountNumber", accountNumber)
                    };

                    var rowsAffectedTransaction = ExecuteTransaction(SqlDepositTransaction, transactionSqlParameters);

                    if (rowsAffectedTransaction > 0 && rowsAffectedWallet > 0)
                    {
                        response.Data = true;
                        ts.Complete();
                    }
                    else
                    {
                        response.ErrorMessage =
                            "The account you are working on has been modified by another user. Changes you have made have not been committed, please resubmit.";
                        ts.Dispose();
                    }

                }
                catch (Exception)
                {
                    response.ErrorMessage = "Oops. Something went wrong.";
                    response.Data = false;
                }
            }

            return response;
        }

        #endregion

        #region Withdraw

        public Response<bool> Withdraw(long accountNumber, decimal amount, byte[] rowVersion)
        {
            var response = new Response<bool>()
            {
                ErrorMessage = string.Empty,
                Data = false
            };

            using (var ts = new TransactionScope())
            {

                try
                {
                    SqlParameter[] withdrawSqlParameters =
                    {
                        new SqlParameter("@WithdrawAmount", amount),
                        new SqlParameter("@AccountNumber", accountNumber),
                        new SqlParameter("@RowVersion", rowVersion),
                    };

                    var rowsAffectedWallet = ExecuteTransaction(SqlWithdrawWallet, withdrawSqlParameters);

                    SqlParameter[] transactionSqlParameters =
                    {
                        new SqlParameter("@TransactionType", TransactionType.Withdraw),
                        new SqlParameter("@Debit", amount),
                        new SqlParameter("@AccountNumber", accountNumber),
                    };

                    var rowsAffectedTransaction = ExecuteTransaction(SqlWithdrawTransaction, transactionSqlParameters);

                    if (rowsAffectedWallet > 0 && rowsAffectedTransaction > 0)
                    {
                        response.Data = true;
                        ts.Complete();
                    }
                    else
                    {
                        response.ErrorMessage =
                            "The account you are working on has been modified by another user. Changes you have made have not been committed, please resubmit.";
                        ts.Dispose();
                    }

                }
                catch (Exception e)
                {
                    response.ErrorMessage = e.Message;
                    response.Data = false;
                }
            }

            return response;
        }

        #endregion

        public Response<bool> FundTransfer(long accountNumber, long accountNumberReceiver, decimal amount, byte[] rowVersion)
        {
            var response = new Response<bool>()
            {
                ErrorMessage = string.Empty,
                Data = false
            };

            using (var ts = new TransactionScope())
            {
                try
                {
                    #region WithdrawFundTransfer
                    SqlParameter[] walletSqlParametersWithdraw =
                    {
                        new SqlParameter("@WithdrawAmount", amount),
                        new SqlParameter("@AccountNumber", accountNumber),
                        new SqlParameter("@RowVersion", rowVersion),
                    };

                    var rowsAffectedWalletWithdraw = ExecuteTransaction(SqlWithdrawWallet, walletSqlParametersWithdraw);

                    SqlParameter[] transactionSqlParametersWithdraw =
                    {
                        new SqlParameter("@TransactionType", TransactionType.FundTransfer),
                        new SqlParameter("@Debit", amount),
                        new SqlParameter("@AccountNumber", accountNumber),
                    };

                    var transactionScopeIdentity = ExecuteTransactionScopeIdentity(SqlWithdrawTransactionWithIdentityScope, transactionSqlParametersWithdraw);
                    #endregion

                    #region DepositFundTransfer
                    SqlParameter[] depositSqlParameters =
                    {
                        new SqlParameter("@Balance", amount),
                        new SqlParameter("@AccountNumber", accountNumberReceiver),                        
                    };

                    var rowsAffectedWalletDeposit = ExecuteTransaction(SqlDepositWalletNoRowVersion, depositSqlParameters);

                    SqlParameter[] transactionSqlParameters =
                    {
                        new SqlParameter("@TransactionType", TransactionType.FundReceived),
                        new SqlParameter("@Credit", amount),
                        new SqlParameter("@AccountNumber", accountNumberReceiver),
                        new SqlParameter("@TransactionReference", transactionScopeIdentity),
                        
                    };

                    var rowsAffectedTransactionDeposit = ExecuteTransaction(SqlDepositTransactionWithTransactionReference, transactionSqlParameters);

                    #endregion

                    #region TransactionCompleteOrDispose

                    if (transactionScopeIdentity > 0 && rowsAffectedWalletWithdraw > 0 
                                                            && rowsAffectedWalletDeposit > 0 
                                                            && rowsAffectedTransactionDeposit > 0)
                    {
                        response.Data = true;
                        ts.Complete();
                    }
                    else
                    {
                        response.ErrorMessage =
                            "The account you are working on has been modified by another user. Changes you have made have not been committed, please resubmit.";
                        ts.Dispose();
                    }

                    #endregion

                }
                catch (Exception e)
                {
                    response.ErrorMessage = e.Message.Contains("Insufficient Funds") ? "Insufficient Funds" : "Oops. Something went wrong.";
                    response.Data = false;
                }
            }

            return response;
        }

        #region PrivateMethod

        private int ExecuteTransaction(string sql, SqlParameter[] parameters)
        {
            int rowsAffected;
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(sql,connection))
                {
                    command.Parameters.AddRange(parameters);

                    rowsAffected = command.ExecuteNonQuery();
                }
            }

            return rowsAffected;
        }

        private int ExecuteTransactionScopeIdentity(string sql, SqlParameter[] parameters)
        {

            int identity;
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddRange(parameters);

                    identity = Convert.ToInt32(command.ExecuteScalar());
                }
            }

            return identity;
        }

        #endregion
    }
}
