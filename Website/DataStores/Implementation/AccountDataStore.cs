using System;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;
using Microsoft.Extensions.Configuration;
using Website.DataStores.Interface;
using Website.Models;

namespace Website.DataStores.Implementation
{
    public class AccountDataStore : IAccountDataStore
    {
        private readonly string _connectionString;

        public AccountDataStore(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public Response<Wallet> GetAccountBalance(long accountNumber)
        {
            var wallet = new Wallet()
            {
                AccountNumber = accountNumber,
                Balance = 0,
            };


            var response = new Response<Wallet>()
            {
                ErrorMessage = string.Empty,
                Data = wallet
            };

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand("SELECT Balance, RowVersion FROM Wallet WHERE AccountNumber = @AccountNumber", connection))
                    {
                        command.Parameters.AddWithValue("@AccountNumber", accountNumber);

                        using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    wallet.Balance = (decimal)reader["Balance"];
                                    wallet.RowVersion = (byte[])reader["RowVersion"];
                                }

                            }
                        }
                    }
                }

                response.Data = wallet;
            }
            catch (Exception e)
            {
                response.ErrorMessage = e.Message;
            }
            return response;
        }

        public Response<AccountDetails> GetAccountDetails(string loginName)
        {
            var response = new Response<AccountDetails>()
            {
                ErrorMessage = string.Empty,
                Data = new AccountDetails()
            };

            try
            {

                const string sql = @"SELECT AccountId, AccountNumber, FirstName, LastName, Address, EmailAddress 
                                FROM [AccountDetails] ad 
                                INNER JOIN [Account] a ON a.Id = ad.AccountId WHERE a.LoginName = @LoginName";
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@LoginName", loginName);

                        using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    response.Data.AccountId = (int) reader["AccountId"];
                                    response.Data.AccountNumber = (long) reader["AccountNumber"];
                                    response.Data.FirstName = reader["FirstName"].ToString();
                                    response.Data.LastName = reader["LastName"].ToString();
                                    response.Data.Address = reader["Address"].ToString();
                                    response.Data.EmailAddress = reader["EmailAddress"].ToString();
                                }
                            }
                            else
                            {
                                response.ErrorMessage = "Account does not exist.";
                            }
                        }
                    }

                }
            }
            catch (Exception e)
            {
                response.ErrorMessage = e.Message;
            }

            return response;
        }

        public Response<bool> IsValidCredential(string loginName, string password)
        {
            var response = new Response<bool>()
            {
                ErrorMessage = string.Empty,
                Data = false
            };

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    using (var command = new SqlCommand("SELECT 1 FROM Account WHERE LoginName = @LoginName AND Password = @Password", connection))
                    {
                        command.Parameters.AddWithValue("@LoginName", loginName);
                        command.Parameters.AddWithValue("@Password", password);

                        using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                        {
                            response.Data = reader.HasRows;
                        }
                    }

                }
            }
            catch (Exception e)
            {
                response.ErrorMessage = e.Message;
            }

            return response;
        }

        public Response<bool> Register(AccountDetails newAccount)
        {

            var response = new Response<bool>()
            {
                ErrorMessage = string.Empty,
                Data = false
            };

            using (var ts = new TransactionScope())
            {
                //Create Account
                try
                {
                    int accountId;
                    using (var connection = new SqlConnection(_connectionString))
                    {
                        connection.Open();
                        using (var command = new SqlCommand("INSERT INTO [Account] (LoginName, Password) VALUES (@Username, @Password); SELECT SCOPE_IDENTITY();", connection))
                        {
                            command.Parameters.AddWithValue("@Username", newAccount.LoginName);
                            command.Parameters.AddWithValue("@Password", newAccount.Password);
                            accountId = Convert.ToInt32(command.ExecuteScalar());

                        }
                    }

                    //Create Account Details
                    long accountDetails;
                    using (var connection = new SqlConnection(_connectionString))
                    {
                        connection.Open();
                        using (var command = new SqlCommand("INSERT INTO [AccountDetails] (AccountId, FirstName, LastName, Address, EmailAddress) VALUES (@AccountId, @FirstName, @LastName, @Address, @EmailAddress); SELECT SCOPE_IDENTITY();", connection))
                        {
                            command.Parameters.AddWithValue("@AccountId", accountId);
                            command.Parameters.AddWithValue("@FirstName", newAccount.FirstName);
                            command.Parameters.AddWithValue("@LastName", newAccount.LastName);
                            command.Parameters.AddWithValue("@Address", newAccount.Address);
                            command.Parameters.AddWithValue("@EmailAddress", newAccount.EmailAddress);
                            accountDetails = Convert.ToInt64(command.ExecuteScalar());

                        }
                    }

                    //Create Wallet
                    int wallet;
                    using (var connection = new SqlConnection(_connectionString))
                    {
                        connection.Open();
                        using (var command = new SqlCommand("INSERT INTO [Wallet] (AccountNumber) VALUES (@AccountNumber)", connection))
                        {
                            command.Parameters.AddWithValue("@AccountNumber", accountDetails);
                            wallet = command.ExecuteNonQuery();

                        }
                    }

                    if (accountId > 0 && accountDetails > 0 && wallet > 0)
                    {
                        ts.Complete();
                        response.Data = true;
                    }
                    else
                    {
                        response.Data = false;
                        ts.Dispose();
                    }

                }
                catch (Exception e)
                {
                    response.ErrorMessage = "Oops. Something went wrong.";
                }

                return response;
            }
        }

        public Response<bool> CheckLoginNameExist(string loginName)
        {
            var response = new Response<bool>()
            {
                ErrorMessage = string.Empty,
                Data = false
            };

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    using (var command = new SqlCommand("SELECT 1 FROM Account WHERE LoginName = @LoginName", connection))
                    {
                        command.Parameters.AddWithValue("@LoginName", loginName);

                        using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                        {
                            response.Data = reader.HasRows;
                        }
                    }

                }
            }
            catch (Exception e)
            {
                response.ErrorMessage = "Oops. Something went wrong.";
            }

            return response;
        }
    }
}
