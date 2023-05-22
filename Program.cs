using Newtonsoft.Json;
using System.Diagnostics;

namespace GAM
{
    class Program
    {
        //TODO: rsa keys are useless (maybe just use the ssh-keygen command to generate them)

        const string helpString = "-c [username] [email (connected to your remote git account)] Create new account\n" +
        "-u Current use account\n" +
        "-s [account id] Set current account\n" +
        "-l List all accounts\n" +
        "-i [file path] Import accounts from *.json file (Doesn't work [doesn't transfer ssh keys aka it's basically useless. Also this can cause duplicate ids])\n" +
        "-e [account id] Edit user\n" +
        "-r [account id] Remove user\n" +
        "-saveLoc Get .json account file location\n" +
        "-h Help";
        const string accountsFilePath = "gamAccounts.json";

        static Account? currentAccount;
        static List<Account> accounts = new List<Account>();

        static void Main(string[] args)
        {
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
            string publicKey = CreateAccount(args[1], args[2]);
            Console.WriteLine("\nPublic key:\n" + publicKey + "\nAdd this to your git remote account.");
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
                    FileName = "git",
                    WorkingDirectory = @"C:\",
                    Arguments = "config --global user.name " + currentAccount._username
                }
            }.Start();
            var p2 = new Process
            {
                StartInfo = 
                {
                    FileName = "git",
                    WorkingDirectory = @"C:\",
                    Arguments = "config --global user.email " + currentAccount._email
                }
            }.Start();
            
            return true;
        }
        static bool RemoveAccount(ulong id)
        {
            int index = accounts.FindIndex(x => x._ID == id);
            if(index == -1) { return false; }
            Account account = accounts[index];
            if(File.Exists(account._privateKeyPath)) { File.Delete(account._privateKeyPath); }
            if(File.Exists(account._publicKeyPath)) { File.Delete(account._publicKeyPath); }
            accounts.Remove(account);
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
        static string CreateAccount(string username, string email)
        {
            ulong curHighestID = (accounts.Count > 0) ? accounts.Max(x => x._ID) : 0;

            //generate ssh keys
            // string? password = Console.ReadLine();
            SshKeyGenerator.SshKeyGenerator keyGenerator = new SshKeyGenerator.SshKeyGenerator(4096);
            string privateKey = keyGenerator.ToPrivateKey();
            string publicKey = keyGenerator.ToRfcPublicKey(email);

            //save keys
            string sshPath = "C:\\Users\\sebik/.ssh/";
            int counter = 0;
            Random rnd = new Random();
            while(true)
            {
                string numbersAfter = rnd.Next(int.MinValue, int.MaxValue).ToString();
                string prvKeyPath = sshPath + "id_rsa" + numbersAfter;
                string pubKeyPath = sshPath + "id_rsa" + numbersAfter + ".pub";
                if(!File.Exists(prvKeyPath) && !File.Exists(pubKeyPath))
                {
                    //save to file
                    Directory.CreateDirectory(sshPath);
                    using(StreamWriter streamWriter = new StreamWriter(prvKeyPath)) {}
                    using(StreamWriter streamWriter = new StreamWriter(pubKeyPath)) {}
                    File.WriteAllText(prvKeyPath, privateKey);
                    File.WriteAllText(pubKeyPath, publicKey);
                    
                    Account newAccount = new Account(curHighestID+1, username, email, pubKeyPath, prvKeyPath);
                    accounts.Add(newAccount);

                    // //register private key
                    // Process p1 = new Process
                    // {
                    //     StartInfo = 
                    //     {
                    //         FileName = "cmd.exe",
                    //         Arguments = "/c start \"\" \"%PROGRAMFILES%\\Git\\bin\\sh.exe\" --login eval $(ssh-agent -s)"
                    //     }
                    // };
                    // p1.Start();
                    // p1.WaitForExit();
                    // Process p2 = new Process
                    // {
                    //     StartInfo = 
                    //     {
                    //         FileName = "cmd.exe",
                    //         Arguments = "/c start \"\" \"%PROGRAMFILES%\\Git\\bin\\sh.exe\" --login ssh-add" + prvKeyPath
                    //     }
                    // };
                    // p2.Start();
                    // p2.WaitForExit();

                    break;
                }

                counter++;
                if(counter > 50) { throw new Exception("Could not find suitable name for key pair files. Try again"); }
            }

            SaveAccounts();
            return publicKey;
        }
        static void SaveAccounts()
        {
            string json = JsonConvert.SerializeObject(accounts, Formatting.Indented);
            using(StreamWriter streamWriter = new StreamWriter(accountsFilePath)) {}
            File.WriteAllText(accountsFilePath, json);
        }
    }
}