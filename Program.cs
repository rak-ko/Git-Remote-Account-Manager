using Newtonsoft.Json;

namespace GAM
{
    class Program
    {
        const string helpString = @"-c [username] [email (connected to your remote git account)] Create new account\n
        -u Current use account\n
        -s [account id] Set current account\n
        -l List all accounts\n
        -i [file path] Import accounts from *.json file\n
        -e [account id] Edit user\n
        -saveLoc Get .json account file location\n
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
                        SetAccountConsole(args);
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
                if(ulong.TryParse(args[1], out ulong id))
                { 
                    bool result = SetCurrentAccount(id);
                    Console.WriteLine((result) ? "Account changed" : "Account doesn't exist");
                }
                else { Console.WriteLine("ID is not an integer"); return; }
            }
        }

        //commands
        static bool SetCurrentAccount(ulong id)
        {
            int index = accounts.FindIndex(x => x._ID == id);
            if(index != -1) { return false;  }
            currentAccount = accounts[index];

            //change git
            System.Diagnostics.Process.Start("CMD.exe", "git config --global user.name" + currentAccount._username);
            System.Diagnostics.Process.Start("CMD.exe", "git config --global user.email" + currentAccount._email);

            return true;
        }
        /// <returns>returns public key</returns>
        static string CreateAccount(string username, string email)
        {
            ulong curHighestID = accounts.Max(x => x._ID);
            Account newAccount = new Account(curHighestID+1, username, email);
            accounts.Add(newAccount);

            //generate ssh keys
            string publicKey = "";

            SaveAccount(newAccount);
            return publicKey;
        }
        static void SaveAccount(Account account)
        {
            string fileContents = "";
            for (int i = 0; i < accounts.Count; i++)
            {
                // fileContents += 
            }
        }
    }
}