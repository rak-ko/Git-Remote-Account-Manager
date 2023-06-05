using System.Security;

namespace GAM
{
    class GAMConsole
    {
        const string helpString = "create [username] [email (connected to your remote git account)] Create new account\n" +
        "current Current use account\n" +
        "select Set current account\n" +
        "list List all accounts\n" +
        "edit Edit user\n" +
        "remove Remove user\n" +
        "saveLoc Get .json account file location\n" +
        "rConf Restores ssh config \n" +
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
                    CreateAccountConsole(args);
                    break;
                case "list":
                    if (Program.accounts.Count == 0) { Console.WriteLine("No accounts registered yet"); }
                    else { for (int i = 0; i < Program.accounts.Count; i++) { Console.WriteLine(Program.accounts[i].ToString(i)); } }
                    break;
                case "edit":
                    EditAccountConsole(args);
                    break;
                case "remove":
                    RemoveAccountConsole(args);
                    break;
                case "saveLoc":
                    Console.WriteLine(commands.GetSaveFileLocation());
                    break;
                case "rConf":
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
                default:
                    Console.WriteLine("Unknown command");
                    break;
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

            AccountSelector((int index) =>
            {
                bool result = commands.SetCurrentAccount(Program.accounts[index]);
                Console.Clear();
                Console.ForegroundColor = (result) ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine((result) ? "Account changed" : "Account wasn't changed");
                Console.ResetColor();
            }, (string toPrint) =>
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                toPrint = "Selected: " + toPrint;
                return toPrint;
            });
        }
        public void CreateAccountConsole(string[] args)
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
            Console.Write("Input a passphrase >>> ");
            SecureString passphrase = GetPassphrase();
            Console.WriteLine();

            commands.CreateAccount(args[1], args[2], fileName, "Github.com", passphrase);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Take the public key at '" + fileName + ".pub' and add it to your git remote account");
            Console.ResetColor();
        }
        public void EditAccountConsole(string[] args)
        {
            AccountSelector((int index) =>
            {
                Console.Clear();

                //get new values
                Console.Write("Enter new username (leave empty to keep current) >>> ");
                string? username = Console.ReadLine();
                Console.Write("Enter new email (leave empty to keep current) >>> ");
                string? email = Console.ReadLine();
                Console.Write("Enter new private key path (leave empty to keep current) >>> ");
                string? privateKeyPath = Console.ReadLine();

                commands.EditAccount(Program.accounts[index], username, email, privateKeyPath);
                Console.ResetColor();
            });
        }
        public void RemoveAccountConsole(string[] args)
        {
            AccountSelector((int index) =>
            {
                Console.Clear();
                Console.CursorVisible = true;

                //check if the user is sure
                string code = new Random().Next(0, 6000).ToString();
                Console.Write("To confirm removal please input this confirmation code '" + code + "' >>> ");
                string? checkString = Console.ReadLine();
                if (checkString == null || checkString != code) { Console.WriteLine("Entered incorrect confirmation code"); return; }

                bool result = commands.RemoveAccount(Program.accounts[index]);
                Console.ForegroundColor = (result) ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine((result) ? "Account removed" : "Account doesn't exist");
                Console.ResetColor();
            }, (string toPrint) =>
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                toPrint = "Selected: " + toPrint;
                return toPrint;
            });
        }
        public void ImportAccountConsole(string[] args)
        {
            if (args.Length < 2) { Console.WriteLine("Not enough arguments"); return; }
            else
            {
                (bool, int) result = commands.ImportAccount(args[1]);
                Console.WriteLine((result.Item1) ? result.Item2 + " Account(s) imported" : "No accounts imported");
            }
        }
        /// <param name="onEnter">OnEnter(User index in list)</param>
        /// <param name="onAccountIsCurrent">OnAccountIsCurrent(account.ToString()) | returns edited string</param>
        public void AccountSelector(Action<int> onEnter, Func<string, string>? onAccountIsCurrent = null)
        {
            //selection
            int curIndex = 0;
            Console.CursorVisible = false;
            while (true)
            {
                bool selected = false;

                //print accounts
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("? Press Esc to cancel ?\n");
                Console.ResetColor();
                for (int i = 0; i < Program.accounts.Count; i++)
                {
                    string toPrint = Program.accounts[i].ToString(i, true);
                    if (Program.currentAccount == Program.accounts[i] && onAccountIsCurrent != null) { toPrint = onAccountIsCurrent(toPrint); }
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
                        if (curIndex < 0) { curIndex = Program.accounts.Count - 1; }
                        break;
                    case ConsoleKey.DownArrow:
                        curIndex++;
                        if (curIndex > Program.accounts.Count - 1) { curIndex = 0; }
                        break;
                    case ConsoleKey.Enter:
                        onEnter(curIndex);
                        selected = true;
                        break;
                    case ConsoleKey.Escape:
                        Console.Clear();
                        selected = true;
                        break;
                }

                if (selected) { break; }
            }
            Console.CursorVisible = true;
        }

        public SecureString GetPassphrase()
        {
            SecureString pwd = new SecureString();
            while (true)
            {
                ConsoleKeyInfo i = Console.ReadKey(true);
                if (i.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else if (i.Key == ConsoleKey.Backspace)
                {
                    if (pwd.Length > 0)
                    {
                        pwd.RemoveAt(pwd.Length - 1);
                        Console.Write("\b \b");
                    }
                }
                else if (i.KeyChar != '\u0000')
                {
                    pwd.AppendChar(i.KeyChar);
                    Console.Write("*");
                }
            }
            return pwd;
        }
    }
}
