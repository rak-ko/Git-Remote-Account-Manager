namespace GAM
{
    class GAMConsole
    {
        const string helpString = "-c [username] [email (connected to your remote git account)] Create new account\n" +
        "-u Current use account\n" +
        "-s Set current account\n" +
        "-l List all accounts\n" +
        // "-i [file path] Import accounts from *.json file (Doesn't work [doesn't transfer ssh keys aka it's basically useless. Also this can cause duplicate ids])\n" +
        "-e Edit user\n" +
        "-r Remove user\n" +
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
                    SetAccountConsole();
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
        public void SetAccountConsole()
        {
            if(Program.accounts.Count == 0) { Console.WriteLine("No accounts registered yet"); return; }

            AccountSelector((ulong id) => { 
                bool result = commands.SetCurrentAccount(id);
                Console.Clear();
                Console.ForegroundColor = (result) ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine((result) ? "Account changed" : "Account wasn't changed");
                Console.ResetColor(); 
            }, (string toPrint) => {
                Console.ForegroundColor = ConsoleColor.Yellow;
                toPrint = "Selected: " + toPrint;
                return toPrint;
            });
        }
        public void CreateAccountConsole(string[] args)
        {
            if(args.Length < 3) { Console.WriteLine("Not enough arguments"); return; }

            //generate file name
            string fileName = "";
            int i = 0;
            Random rnd = new Random();
            while (true)
            {
                int extension = rnd.Next(int.MinValue, int.MaxValue);
                fileName = commands.sshPath + args[1] + "_" + args[2] + "_" + extension;
                if(!File.Exists(fileName)) { break; }

                i++;
                if(i > 50) { throw new Exception("Couldn't find suitable key name. Try running the command again"); }
            }

            commands.CreateAccount(args[1], args[2], fileName);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Take the public key at '"+ fileName +".pub' and add it to your git remote account");
            Console.ResetColor();
        }
        public void EditAccountConsole(string[] args)
        {
            AccountSelector((ulong id) => {
                Account? account = Program.accounts.FirstOrDefault(x => x._ID == id);
                if(account == null) { throw new NullReferenceException(); }
                Console.Clear();

                //get new values
                Console.Write("Enter new username (leave empty to keep current) >>> ");
                string? username = Console.ReadLine();
                Console.Write("Enter new email (leave empty to keep current) >>> ");
                string? email = Console.ReadLine();
                Console.Write("Enter new private key path (leave empty to keep current) >>> ");
                string? privateKeyPath = Console.ReadLine();

                commands.EditAccount(account, username, email, privateKeyPath);
                Console.ResetColor();
            });
        }
        public void RemoveAccountConsole(string[] args)
        {
            AccountSelector((ulong id) => {
                Console.Clear();
                Console.CursorVisible = true;

                //check if the user is sure
                string code = new Random().Next(0, 6000).ToString();
                Console.Write("To confirm removal please input this confirmation code '" + code + "' >>> ");
                string? checkString = Console.ReadLine();
                if(checkString == null || checkString != code) { Console.WriteLine("Entered incorrect confirmation code"); return; }

                bool result = commands.RemoveAccount(id);
                Console.ForegroundColor = (result) ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine((result) ? "Account removed" : "Account doesn't exist");
                Console.ResetColor(); 
            }, (string toPrint) => {
                Console.ForegroundColor = ConsoleColor.Yellow;
                toPrint = "Selected: " + toPrint;
                return toPrint;
            });
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
    
        /// <param name="onEnter">OnEnter(User id)</param>
        /// <param name="onAccountIsCurrent">OnAccountIsCurrent(account.ToString()) | returns edited string</param>
        public void AccountSelector(Action<ulong> onEnter, Func<string, string>? onAccountIsCurrent = null)
        {
            //selection
            int curIndex = 0;
            Console.CursorVisible = false;
            while(true)
            {
                bool selected = false;

                //print accounts
                Console.Clear();
                for (int i = 0; i < Program.accounts.Count; i++) 
                { 
                    string toPrint = Program.accounts[i].ToString(true);
                    if(Program.currentAccount == Program.accounts[i] && onAccountIsCurrent != null) { toPrint = onAccountIsCurrent(toPrint); }
                    if(i == curIndex) 
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        toPrint = toPrint + "  <";
                    }
                    Console.WriteLine(toPrint);
                    Console.ResetColor();
                }

                //change selected
                ConsoleKeyInfo pressed = Console.ReadKey(true);
                switch (pressed.Key)
                {
                    case ConsoleKey.UpArrow:
                        curIndex--;
                        if(curIndex < 0) { curIndex = Program.accounts.Count - 1; }
                        break;
                    case ConsoleKey.DownArrow:
                        curIndex++;
                        if(curIndex > Program.accounts.Count - 1) { curIndex = 0; }
                        break;
                    case ConsoleKey.Enter:
                        onEnter(Program.accounts[curIndex]._ID);
                        selected = true;
                        break;
                }
                
                if(selected) { break; }
            }
            Console.CursorVisible = true;
        }
    }
}