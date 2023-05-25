namespace GAM
{
    //TODO: simpler key generation (ask for arguments and then pass them in yourself)
    //TODO: use arrow keys to navigate instead of passing in the id
    //TODO: when editing an account, use the same arrow key navigation for picking properties to edit

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
