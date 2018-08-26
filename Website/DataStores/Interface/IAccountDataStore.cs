using Website.Models;

namespace Website.DataStores.Interface
{
    public interface IAccountDataStore
    {
        Response<AccountDetails> GetAccountDetails(string loginName);

        Response<Wallet> GetAccountBalance(long accountNumber);

        Response<bool> IsValidCredential(string loginName, string password);

        Response<bool> Register(AccountDetails newAccount);

        Response<bool> CheckLoginNameExist(string loginName);
    }
}
