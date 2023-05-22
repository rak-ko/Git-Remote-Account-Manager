using System.Linq;

namespace GAM
{
    class Program
    {
        const string helpString = @"-c [username] [email (connected to your remote git account)] Create new account\n
        -u Current use account\n
        -s [account id] Set current account\n
        -l List all accounts\n
        -i [file path] Import accounts from *.gam file\n
        -e [account id] Edit user\n
        -gamLoc Get .gam file location\n
        -h Help";
        static Account? currentAccount;
        static List<Account> accounts = new List<Account>();

        static void Main(string[] args)
        {
            //load accounts
            

            if(args.Length > 0)
            {
                string firstArg = args[0];
                switch (firstArg)
                {
                    case "-h":
                        Console.WriteLine(helpString);
                        break;
                    case "-u":
                        PrintCurrentAccount();
                        break;
                    case "-s":
                        break;
                }
            }
        }

        //console
        static void PrintCurrentAccount()
        {
            if(currentAccount == null) { Console.WriteLine("No account selected"); }
            else { Console.WriteLine(currentAccount.ToString()); }
        }
        static void SetAccountConsole(string[] args)
        {
            if(args.Length < 2) { Console.WriteLine("Not enough arguments"); return; }
            else
            {
                if(ulong.TryParse(args[1], out ulong id)) { SetCurrentAccount(id); }
                else { Console.WriteLine("ID not an integer"); return; }
            }
        }

        //commands
        static void SetCurrentAccount(ulong id)
        {

        }
        /// <returns>returns public key</returns>
        static string CreateAccount(string username, string email)
        {
            ulong curHighestID = accounts.Max(x => x._ID);
            Account newAccount = new Account(curHighestID+1, username, email);
            accounts.Add(newAccount);

            //generate ssh keys
            

            SaveAccount(newAccount);

            return "";
        }
        static void SaveAccount(Account account)
        {
            
        }
    }
}