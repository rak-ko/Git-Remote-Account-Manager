namespace GAM
{
    class GAMConsole
    {
        const string helpString =
        "create [username] [email (connected to your remote git account)] Create new account\n" +
        "current Current use account\n" +
        "select Set current account\n" +
        "list List all accounts\n" +
        "edit Edit user\n" +
        "remove Remove user\n" +

        "\n" +
        "host add [new host (url e.g. 'Github.com')]\n" +
        "host remove\n" +
        "host edit\n" +
        "host list\n" +

        "\n" +
        "saveLoc Get .json account file location\n" +
        "configLoc Get .json config file location\n" +

        "sshRestore Restores ssh config \n" +
        "configRestore Resets config to default settings \n" +

        "\n" +
        "help Help";
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
                case "help":
                    Console.WriteLine(helpString);
                    break;
                
                case "current":
                    PrintCurrentAccount();
                    break;
                case "select":
                    SetAccountConsole();
                    break;
                case "create":
                    CreateAccount(args);
                    break;
                case "list":
                    if (Program.accounts.Count == 0) { Console.WriteLine("No accounts registered yet"); }
                    else { for (int i = 0; i < Program.accounts.Count; i++) { Console.WriteLine(Program.accounts[i].ToString(i)); } }
                    break;
                case "edit":
                    EditAccount(args);
                    break;
                case "remove":
                    RemoveAccount(args);
                    break;
                
                case "host":
                    HostCommandNest(args);
                    break;
                
                case "configLoc":
                    Console.WriteLine(commands.GetConfigFileLocation());
                    break;
                case "saveLoc":
                    Console.WriteLine(commands.GetSaveFileLocation());
                    break;
                
                case "sshRestore":
                    if (commands.RestoreSSHConfig())
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("SSH config restored");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Can't restore SSH config because no account has been selected");
                    }
                    Console.ResetColor();
                    break;
                case "configRestore":
                    commands.ResetConfigToDefault();
                    Console.WriteLine("Config successfully reset");
                    Console.ResetColor();
                    break;

                default:
                    Console.WriteLine("Unknown command");
                    break;
            }
        }
        public void HostCommandNest(string[] args)
        {
            if(args.Length < 2) { Console.WriteLine("Not enough arguments"); }
            else
            {
                switch (args[1])
                {
                    case "list":
                        for (int i = 0; i < Program.hosts.Count; i++) { Console.WriteLine(Program.hosts[i].ToString(i, false)); }
                        break;
                    case "add":
                        AddHost(args);
                        break;
                    case "remove":
                        RemoveHost();
                        break;
                    case "edit":
                        EditHost();
                        break;
                    default:
                        Console.WriteLine("Unknown command");
                        break;
                }
            }
        }

        public void PrintCurrentAccount()
        {
            if (Program.currentAccount == null) { Console.WriteLine("No account selected"); }
            else
            {
                Console.WriteLine(Program.currentAccount.ToString(Program.accounts.FindIndex(x => x == Program.currentAccount)));
            }
        }
        public void SetAccountConsole()
        {
            if (Program.accounts.Count == 0) { Console.WriteLine("No accounts registered yet"); return; }

            ListSelector<Account>(Program.accounts, (int index) =>
            {
                int result = commands.SetCurrentAccount(Program.accounts[index]);
                Console.Clear();
                if(result != 2)
                {
                    Console.ForegroundColor = (result == 1) ? ConsoleColor.Green : ConsoleColor.Red;
                    Console.WriteLine((result == 1) ? "Account changed" : "Account wasn't changed");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Account changed. Warning: This account doesn't have a host assigned, therefore the default (Github.com) was used");
                }
                Console.ResetColor();
            }, "Select an account", (string toPrint) =>
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                toPrint = "Selected: " + toPrint;
                return toPrint;
            });
        }
        
        public void CreateAccount(string[] args)
        {
            if (args.Length < 3) { Console.WriteLine("Not enough arguments"); return; }

            //generate file name
            string fileName = "";
            int i = 0;
            Random rnd = new Random();
            while (true)
            {
                int extension = rnd.Next(int.MinValue, int.MaxValue);
                fileName = commands.sshPath + args[1] + "_" + args[2] + "_" + extension;
                if (!File.Exists(fileName)) { break; }

                i++;
                if (i > 50) { throw new Exception("Couldn't find suitable key name. Try running the command again"); }
            }

            //get passphrase
            bool doingConfirmation = false;
            string passphrase = "";
            while (true)
            {
                bool confirmPassword = false;
                if (!doingConfirmation)
                {
                    doingConfirmation = true;
                    Console.Write("Input a passphrase (Leave empty for no passphrase) >>> ");
                }
                else
                {
                    Console.Write("Repeat your passphrase >>> ");
                    doingConfirmation = false;
                    confirmPassword = true;
                }

                string tmpPassphrase = GetPassphrase();
                Console.WriteLine();
                if (confirmPassword)
                {
                    if (tmpPassphrase == passphrase) { break; }
                    else
                    {
                        Console.Write("Passphrase doesn't match... (Enter)");
                        Console.ReadKey();
                        Console.Clear();
                    }
                }
                else { passphrase = tmpPassphrase; }
            }
            if (passphrase == "") { passphrase = "\"\""; }
            
            //select host
            Host selectedHost = null!;
            while(true)
            {
                List<Host> tmpHosts = new List<Host>(Program.hosts);
                tmpHosts.Add(new Host(-1, "Add new host ->"));

                bool selected = false;
                bool result = ListSelector<Host>(tmpHosts, (int index) => {
                    //create new
                    if(tmpHosts[index]._id == -1)
                    {
                        string? hostURL = "";
                        Console.Write("Please input the new host URL (Leave empty to cancel) >>> ");
                        hostURL = Console.ReadLine();
                        if(hostURL != null && hostURL != "") { AddHost(new string[] { "host", "add", hostURL }); }
                    }
                    else
                    {
                        selectedHost = Program.hosts[index];
                        selected = true;
                    }
                }, "Select a host");

                //cancelled host selection -> cancel account creation
                if(!result) { Console.WriteLine("Account creation canceled"); return; }
                if(selected) { break; }
            }

            commands.CreateAccount(args[1], args[2], fileName, selectedHost, passphrase);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Take the public key at '" + fileName + ".pub' and add it to your git remote account");
            Console.WriteLine("Public key:\n" + File.ReadAllText(fileName + ".pub"));
            Console.ResetColor();
        }
        public void EditAccount(string[] args)
        {
            ListSelector<Account>(Program.accounts, (int index) =>
            {
                //get new values
                Console.Write("Enter new username (leave empty to keep current) >>> ");
                string? username = Console.ReadLine();
                Console.Write("Enter new email (leave empty to keep current) >>> ");
                string? email = Console.ReadLine();
                Console.Write("Enter new private key path (leave empty to keep current) >>> ");
                string? privateKeyPath = Console.ReadLine();

                //change host
                Host? newHost = null;
                while(true)
                {
                    List<Host> tmpHosts = new List<Host>(Program.hosts);
                    tmpHosts.Add(new Host(-1, "Add new host ->"));

                    bool selected = false;
                    bool result = ListSelector<Host>(tmpHosts, (int index) => {
                        //create new
                        if(tmpHosts[index]._id == -1)
                        {
                            string? hostURL = "";
                            Console.Write("Please input the new host URL (Leave empty to cancel) >>> ");
                            hostURL = Console.ReadLine();
                            if(hostURL != null && hostURL != "") { AddHost(new string[] { "host", "add", hostURL }); }
                        }
                        else
                        {
                            newHost = Program.hosts[index];
                            selected = true;
                        }
                    }, "Select a host");

                    if(selected || !result) { break; }
                }

                commands.EditAccount(Program.accounts[index], username, email, privateKeyPath, newHost);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Account successfully edited");
                Console.ResetColor();
            }, "Select an account");
        }
        public void RemoveAccount(string[] args)
        {
            ListSelector<Account>(Program.accounts, (int index) =>
            {
                //check if the user is sure
                string code = new Random().Next(0, 6000).ToString();
                Console.Write("To confirm removal please input this confirmation code '" + code + "' >>> ");
                string? checkString = Console.ReadLine();
                if (checkString == null || checkString != code) { Console.WriteLine("Entered incorrect confirmation code"); return; }

                bool result = commands.RemoveAccount(Program.accounts[index]);
                Console.ForegroundColor = (result) ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine((result) ? "Account removed" : "Account doesn't exist");
                Console.ResetColor();
            }, "Select an account", (string toPrint) =>
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                toPrint = "Selected: " + toPrint;
                return toPrint;
            });
        }
        public void ImportAccount(string[] args)
        {
            if (args.Length < 2) { Console.WriteLine("Not enough arguments"); return; }
            else
            {
                (bool, int) result = commands.ImportAccount(args[1]);
                Console.WriteLine((result.Item1) ? result.Item2 + " Account(s) imported" : "No accounts imported");
            }
        }

        public void AddHost(string[] args)
        {
            if (args.Length < 3) { Console.WriteLine("Not enough arguments"); return; }
            commands.CreateHost(args[2]);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Hostname added");
            Console.ResetColor();
        }
        public void RemoveHost()
        {
            ListSelector<Host>(Program.hosts, (int index) => {
                //check if the user is sure
                string code = new Random().Next(0, 6000).ToString();
                Console.Write("To confirm removal please input this confirmation code '" + code + "' >>> ");
                string? checkString = Console.ReadLine();
                if (checkString == null || checkString != code) { Console.WriteLine("Entered incorrect confirmation code"); return; }

                bool result = commands.RemoveHost(Program.hosts[index]); 
                Console.ForegroundColor = (result) ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine((result) ? "Account removed" : "Account doesn't exist");
                Console.ResetColor();
            }, "Select a host");
        }
        public void EditHost()
        {
            ListSelector<Host>(Program.hosts, (int index) => {
                //get new values
                Console.Write("Enter new host (leave empty to keep current) >>> ");
                string? host = Console.ReadLine();

                commands.EditHost(Program.hosts[index], host);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Host successfully edited");
                Console.ResetColor();
            }, "Select a host");
        }

        public string GetPassphrase()
        {
            string passphrase = "";
            while (true)
            {
                ConsoleKeyInfo i = Console.ReadKey(true);
                if (i.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else if (i.Key == ConsoleKey.Backspace)
                {
                    if (passphrase.Length > 0)
                    {
                        passphrase = passphrase.Remove(passphrase.Length - 1);
                        Console.Write("\b \b");
                    }
                }
                else if (i.KeyChar != '\u0000')
                {
                    passphrase += i.KeyChar;
                    Console.Write("*");
                }
            }
            return passphrase;
        }
        /// <param name="onConfirm">OnEnter(User index in list)</param>
        /// <param name="onIsCurrentAccount">OnAccountIsCurrent(account.ToString()) | returns edited string</param>
        /// <returns>Whether selection was canceled</returns>
        public bool ListSelector<T>(List<T> collection, Action<int> onConfirm, string? title = null, Func<string, string>? onIsCurrentAccount = null) where T : class, IPrintable
        {
            //selection
            int curIndex = 0;
            Console.CursorVisible = false;
            while (true)
            {
                bool selected = false;

                //print accounts
                Console.Clear();
                if(title != null) { Console.WriteLine("- " + title + " -"); }
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("? Press Esc to cancel ?\n");
                Console.ResetColor();
                for (int i = 0; i < collection.Count; i++)
                {
                    string toPrint = collection[i].ToString(i, true);
                    if (Program.currentAccount == collection[i] && onIsCurrentAccount != null) { toPrint = onIsCurrentAccount(toPrint); }
                    if (i == curIndex)
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
                        if (curIndex < 0) { curIndex = collection.Count - 1; }
                        break;
                    case ConsoleKey.DownArrow:
                        curIndex++;
                        if (curIndex > collection.Count - 1) { curIndex = 0; }
                        break;
                    case ConsoleKey.Enter:
                        Console.Clear();
                        Console.CursorVisible = true;

                        onConfirm(curIndex);
                        selected = true;
                        break;
                    case ConsoleKey.Escape:
                        Console.Clear();
                        Console.CursorVisible = true;
                        return false;
                }

                if (selected) { break; }
            }
            Console.CursorVisible = true;
            return true;
        }
    }
}
