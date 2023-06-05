namespace GAM
{
    class Program
    {
        public static Account? currentAccount;
        public static List<Account> accounts = new List<Account>();
        public static List<Host> hosts = new List<Host>();

        static void Main(string[] args)
        {
            //configure
            Commands commands = new Commands();
            GAMConsole console = new GAMConsole(commands);
            GUI gui = new GUI();

            //setup
            commands.LoadAccounts();
            commands.LoadCurrentAccount();
            commands.LoadConfig();

            //run cli
            if (args.Length > 0) { console.RunCommand(args); }
            //run gui
            else { gui.LaunchGUI(); }
        }
    }
}
