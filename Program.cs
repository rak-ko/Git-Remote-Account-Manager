using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;

namespace GAM
{
    class Program
    {
        const string helpString = "-c [username] [email (connected to your remote git account)] Create new account\n" +
        "-u Current use account\n" +
        "-s [account id] Set current account\n" +
        "-l List all accounts\n" +
        "-i [file path] Import accounts from *.json file (Doesn't work [doesn't transfer ssh keys aka it's basically useless. Also this can cause duplicate ids])\n" +
        "-e [account id] Edit user\n" +
        "-r [account id] Remove user\n" +
        "-saveLoc Get .json account file location\n" +
        "-rConf Restores ssh config \n" +
        "-h Help";
        const string accountsFileName = "gamAccounts.json";
        static string accountsFilePath = ""; 

        static Account? currentAccount;
        static List<Account> accounts = new List<Account>();

        static void Main(string[] args)
        {
            accountsFilePath = AppDomain.CurrentDomain.BaseDirectory + "/" + accountsFileName;
            //load accounts
            if(File.Exists(accountsFilePath)) 
            { 
                string json = File.ReadAllText(accountsFilePath);
                List<Account>? tmp = JsonConvert.DeserializeObject<List<Account>>(json);
                if(tmp != null) { accounts = tmp; }
            }

            //load current account
            string username = "";
            string email = "";
            Process pUsername = new Process
            {
                StartInfo = 
                {
                    FileName = "cmd.exe",
                    Arguments = "/c git config --global user.name"
                }
            };
            pUsername.StartInfo.RedirectStandardOutput = true;
            pUsername.Start();
            while (!pUsername.StandardOutput.EndOfStream) { username = pUsername.StandardOutput.ReadToEnd(); }
            pUsername.WaitForExit();
            Process pEmail = new Process
            {
                StartInfo = 
                {
                    FileName = "cmd.exe",
                    Arguments = "/c git config --global user.email",
                    RedirectStandardOutput = true
                }
            };
            pEmail.StartInfo.RedirectStandardOutput = true;
            pEmail.Start();
            while (!pEmail.StandardOutput.EndOfStream) { email = pEmail.StandardOutput.ReadToEnd(); }
            pEmail.WaitForExit();
            username = username.Replace("\n", "");
            email = email.Replace("\n", "");
            for (int i = 0; i < accounts.Count; i++)
            {
                if(accounts[i]._email == email && accounts[i]._username == username)
                {
                    currentAccount = accounts[i];
                    break;
                }
            }

            //run commands
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
                    case "-c":
                        CreateAccountConsole(args);
                        break;
                    case "-l":
                        if(accounts.Count == 0) { Console.WriteLine("No accounts registered yet"); }
                        else { for (int i = 0; i < accounts.Count; i++) { Console.WriteLine(accounts[i].ToString()); }}
                        break;
                    case "-e":
                        EditAccountConsole(args);
                        break;
                    case "-r":
                        RemoveAccountConsole(args);
                        break;
                    case "-saveLoc":
                        Console.WriteLine(Path.GetFullPath(accountsFilePath));
                        break;
                    case "-i":
                        ImportAccountConsole(args);
                        break;
                    case "-rConf":
                        RestoreSSHConfig();
                        Console.WriteLine("SSH config restored");
                        break;
                    default:
                        Console.WriteLine("Unknown command");
                        break;
                }
            }
            else { Console.WriteLine(helpString); }
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
        static void CreateAccountConsole(string[] args)
        {
            if(args.Length < 3) { Console.WriteLine("Not enough arguments"); return; }
            CreateAccount(args[1], args[2]);
            Console.WriteLine("Take the public key and add it to your git remote account");
        }
        static void EditAccountConsole(string[] args)
        {
            if(args.Length < 2) { Console.WriteLine("Not enough arguments"); return; }
            if(ulong.TryParse(args[1], out ulong id))
            {
                int index = accounts.FindIndex(x => x._ID == id);
                if(index == -1) { Console.WriteLine("Account doesn't exits"); return; }

                //get new values
                Account account = accounts[index];
                Console.Write("Enter new username (leave empty to keep current) >>> ");
                string? username = Console.ReadLine();
                Console.Write("Enter new email (leave empty to keep current) >>> ");
                string? email = Console.ReadLine();

                EditAccount(account, username, email);
            }
            else { Console.WriteLine("ID not an integer"); return; }
        }
        static void RemoveAccountConsole(string[] args)
        {
            //check if the user is sure
            string code = new Random().Next(0, 6000).ToString();
            Console.Write("To confirm removal please input this confirmation code '" + code + "' >>> ");
            string? checkString = Console.ReadLine();
            if(checkString == null || checkString != code) { Console.WriteLine("Entered incorrect confirmation code"); return; }

            //check args
            if(args.Length < 2) { Console.WriteLine("Not enough arguments"); return; }
            else
            {
                if(ulong.TryParse(args[1], out ulong id))
                { 
                    bool result = RemoveAccount(id);
                    Console.WriteLine((result) ? "Account removed" : "Account doesn't exist");
                }
                else { Console.WriteLine("ID is not an integer"); return; }
            }
        }
        static void ImportAccountConsole(string[] args)
        {
            if(args.Length < 2) { Console.WriteLine("Not enough arguments"); return; }
            else
            {
                (bool, int) result = ImportAccount(args[1]);
                Console.WriteLine((result.Item1) ? result.Item2 + " Account(s) imported" : "No accounts imported");
            }
        }

        //commands
        static bool SetCurrentAccount(ulong id)
        {
            int index = accounts.FindIndex(x => x._ID == id);
            if(index == -1) { return false; }
            currentAccount = accounts[index];

            //change git
            var p1 = new Process
            {
                StartInfo = 
                {
                    FileName = "cmd.exe",
                    Arguments = "/c git config --global user.name " + currentAccount._username
                }
            };
            p1.Start();
            p1.WaitForExit();
            var p2 = new Process
            {
                StartInfo = 
                {
                    FileName = "cmd.exe",
                    Arguments = "/c git config --global user.email " + currentAccount._email
                }
            };
            p2.Start();
            p2.WaitForExit();
            
            RestoreSSHConfig();
            return true;
        }
        static bool RemoveAccount(ulong id)
        {
            int index = accounts.FindIndex(x => x._ID == id);
            if(index == -1) { return false; }
            Account account = accounts[index];
            //set new current account
            if(currentAccount == account && accounts.Count > 0) 
            { 
                for (int i = 0; i < accounts.Count; i++)
                {
                    if(accounts[i] != currentAccount)
                    {
                        SetCurrentAccount(accounts[i]._ID);
                        break;
                    }
                }
            }
            accounts.Remove(account);
            RestoreSSHConfig();
            SaveAccounts();
            return true;
        }
        static void EditAccount(Account account, string? username, string? email)
        {
            if(username != null && username != "") { account._username = username; }
            if(email != null && email != "") { account._email = email; }

            //update accounts
            SaveAccounts();
            if(account == currentAccount) { SetCurrentAccount(account._ID); }
        }
        static (bool, int) ImportAccount(string path)
        {
            if(!File.Exists(path) || File.GetAttributes(path).HasFlag(FileAttributes.Directory)) { return (false, 0); }
            string json = File.ReadAllText(path);
            List<Account>? newAccounts = JsonConvert.DeserializeObject<List<Account>>(json);
            if(newAccounts == null) { return (false, 0); }
            accounts.AddRange(newAccounts);
            SaveAccounts();
            return (true, newAccounts.Count);
        }
        /// <returns>returns public key</returns>
        static void CreateAccount(string username, string email)
        {
            ulong curHighestID = (accounts.Count > 0) ? accounts.Max(x => x._ID) : 0;

            //generate ssh keys
            Process p = new Process
            {
                StartInfo = 
                {
                    FileName = "cmd.exe",
                    Arguments = "/c ssh-keygen -t ed25519 -C \"" + email + "\""
                }
            };
            p.Start();
            p.WaitForExit();
            
            string prvKeyPath = "";
            while(true)
            {
                Console.Write("Please enter the full path of your new private key >>> ");
                string? path = Console.ReadLine();
                if(path == null || !File.Exists(path)) { Console.WriteLine("Not a key"); }
                else
                {
                    prvKeyPath = path;
                    break;
                }
            }

            //add account
            Account newAccount = new Account(curHighestID+1, username, email, prvKeyPath);
            accounts.Add(newAccount);
            
            SaveAccounts();
        }
        static void SaveAccounts()
        {
            string json = JsonConvert.SerializeObject(accounts, Formatting.Indented);
            using(StreamWriter streamWriter = new StreamWriter(accountsFilePath)) {}
            File.WriteAllText(accountsFilePath, json);
        }
        static void RestoreSSHConfig()
        {
            string sshPath = "C:/Users/sebik/.ssh/";
            string configName = "config";
            Directory.CreateDirectory(sshPath);
            using(StreamWriter wrt = new StreamWriter(sshPath + "/" + configName)) {}

            string newConfigText = "Host github.com \n";
            if(currentAccount == null) { Console.WriteLine("Can't restore SSH config because no account has been selected"); return; }
            newConfigText += "IdentityFile " + currentAccount._privateKeyPath;
            File.WriteAllText(sshPath + "/" + configName, newConfigText);
        }   
    }
}