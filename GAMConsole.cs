namespace GAM
{
    class GAMConsole
    {
        const string helpString = "-c [username] [email (connected to your remote git account)] Create new account\n" +
        "-u Current use account\n" +
        "-s [account id] Set current account\n" +
        "-l List all accounts\n" +
        // "-i [file path] Import accounts from *.json file (Doesn't work [doesn't transfer ssh keys aka it's basically useless. Also this can cause duplicate ids])\n" +
        "-e [account id] Edit user\n" +
        "-r [account id] Remove user\n" +
        "-saveLoc Get .json account file location\n" +
        "-rConf Restores ssh config \n" +
        "-h Help";
        Commands commands;
        
        public GAMConsole(Commands commands)
        {
            this.commands = commands;
        }
        public void RunCommand(string[] args)
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
                        if(Program.accounts.Count == 0) { Console.WriteLine("No accounts registered yet"); }
                        else { for (int i = 0; i < Program.accounts.Count; i++) { Console.WriteLine(Program.accounts[i].ToString()); }}
                        break;
                    case "-e":
                        EditAccountConsole(args);
                        break;
                    case "-r":
                        RemoveAccountConsole(args);
                        break;
                    case "-saveLoc":
                        Console.WriteLine(commands.GetSaveFileLocation());
                        break;
                    // case "-i":
                    //     ImportAccountConsole(args);
                    //     break;
                    case "-rConf":
                        Console.WriteLine((commands.RestoreSSHConfig()) ? "SSH config restored" : "Can't restore SSH config because no account has been selected");
                        break;
                    default:
                        Console.WriteLine("Unknown command");
                        break;
                }
        }

        public void PrintCurrentAccount()
        {
            if(Program.currentAccount == null) { Console.WriteLine("No account selected"); }
            else { Console.WriteLine(Program.currentAccount.ToString()); }
        }
        public void SetAccountConsole(string[] args)
        {
            if(args.Length < 2) { Console.WriteLine("Not enough arguments"); return; }
            else
            {
                if(ulong.TryParse(args[1], out ulong id))
                { 
                    bool result = commands.SetCurrentAccount(id);
                    Console.WriteLine((result) ? "Account changed" : "Account doesn't exist");
                }
                else { Console.WriteLine("ID is not an integer"); return; }
            }
        }
        public void CreateAccountConsole(string[] args)
        {
            if(args.Length < 3) { Console.WriteLine("Not enough arguments"); return; }
            commands.CreateAccount(args[1], args[2]);
            Console.WriteLine("Take the public key and add it to your git remote account");
        }
        public void EditAccountConsole(string[] args)
        {
            if(args.Length < 2) { Console.WriteLine("Not enough arguments"); return; }
            if(ulong.TryParse(args[1], out ulong id))
            {
                int index = Program.accounts.FindIndex(x => x._ID == id);
                if(index == -1) { Console.WriteLine("Account doesn't exits"); return; }

                //get new values
                Account account = Program.accounts[index];
                Console.Write("Enter new username (leave empty to keep current) >>> ");
                string? username = Console.ReadLine();
                Console.Write("Enter new email (leave empty to keep current) >>> ");
                string? email = Console.ReadLine();
                Console.Write("Enter new private key path (leave empty to keep current) >>> ");
                string? privateKeyPath = Console.ReadLine();

                commands.EditAccount(account, username, email, privateKeyPath);
            }
            else { Console.WriteLine("ID not an integer"); return; }
        }
        public void RemoveAccountConsole(string[] args)
        {
            //check args
            if(args.Length < 2) { Console.WriteLine("Not enough arguments"); return; }
            else
            {
                //check if the user is sure
                string code = new Random().Next(0, 6000).ToString();
                Console.Write("To confirm removal please input this confirmation code '" + code + "' >>> ");
                string? checkString = Console.ReadLine();
                if(checkString == null || checkString != code) { Console.WriteLine("Entered incorrect confirmation code"); return; }

                if(ulong.TryParse(args[1], out ulong id))
                { 
                    bool result = commands.RemoveAccount(id);
                    Console.WriteLine((result) ? "Account removed" : "Account doesn't exist");
                }
                else { Console.WriteLine("ID is not an integer"); return; }
            }
        }
        public void ImportAccountConsole(string[] args)
        {
            if(args.Length < 2) { Console.WriteLine("Not enough arguments"); return; }
            else
            {
                (bool, int) result = commands.ImportAccount(args[1]);
                Console.WriteLine((result.Item1) ? result.Item2 + " Account(s) imported" : "No accounts imported");
            }
        }
    }
}