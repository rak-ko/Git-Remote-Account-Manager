namespace GAM
{
    class Program
    {
        public static Account? currentAccount;
        public static List<Account> accounts = new List<Account>();

        static void Main(string[] args)
        {
            //configure
            Commands commands = new Commands();
            GAMConsole console = new GAMConsole(commands);
            GUI gui = new GUI();

            //setup
            commands.LoadAccounts();
            commands.LoadCurrentAccount();

            //run cli
            if(args.Length > 0) { console.RunCommand(args); }
            //run gui
            else { gui.LaunchGUI(); }
        }
    }
}
