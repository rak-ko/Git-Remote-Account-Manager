namespace GAM
{
    class Program
    {
        public static Account? currentAccount;
        public static List<Account> accounts = new List<Account>();
        public static List<string> hostnames = new List<string>();

        static void Main(string[] args)
        {
            //configure
            Commands commands = new Commands();
            GAMConsole console = new GAMConsole(commands);
            GUI gui = new GUI();

            //setup
            commands.LoadAccounts();
            commands.LoadCurrentAccount();
            commands.LoadHostnames();

            //run cli
            if (args.Length > 0) { console.RunCommand(args); }
            //run gui
            else { gui.LaunchGUI(); }
        }
    }
}
