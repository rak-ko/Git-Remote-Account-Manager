using System.Diagnostics;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace GAM
{
    public class Commands
    {
        const string accountsFileName = "gamAccounts.json";
        const string sshConfigIncludesFolderName = "customConfigs";
        static string accountsFilePath = "";

        const string windowsTerminalName = "cmd.exe";
        const string linuxTerminalName = "/bin/bash";
        string terminalName = windowsTerminalName;

        public string sshPath = "";

        public Commands()
        {
            //setup shell
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) { terminalName = linuxTerminalName; }
            
            //setup sshPath
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) { sshPath = "/home/"+ Environment.UserName +"/.ssh/"; }
            else if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) { sshPath = "C:/Users/"+ Environment.UserName +"/.ssh/"; }
            else { sshPath = "/home/"+ Environment.UserName +"/.ssh/"; } //linux again
        }
        
        public void CreateAccount(string username, string email, string keyFileName)
        {
            //generate ssh keys
            RunCommand("ssh-keygen -t ed25519 -C \""+ email +"\"", false, new Dictionary<string, string>() { 
                {"Enter file in which to save the key (/home/vojta/.ssh/id_rsa)", keyFileName} 
            });

            //add account
            Account newAccount = new Account(username, email, keyFileName);
            Program.accounts.Add(newAccount);
            if(Program.currentAccount == null) { SetCurrentAccount(Program.accounts[Program.accounts.Count - 1]); }
            SaveAccounts();
        }
        public bool RemoveAccount(Account account)
        {
            //set new current account
            if(Program.currentAccount == account && Program.accounts.Count > 0) 
            { 
                for (int i = 0; i < Program.accounts.Count; i++)
                {
                    if(Program.accounts[i] != Program.currentAccount)
                    {
                        SetCurrentAccount(Program.accounts[i]);
                        break;
                    }
                }
            }
            //remove ssh key
            File.Delete(account._privateKeyPath);
            File.Delete(account._privateKeyPath + ".pub");

            Program.accounts.Remove(account);
            RestoreSSHConfig();
            SaveAccounts();
            return true;
        }
        public void EditAccount(Account account, string? username, string? email, string? privateKeyPath)
        {
            if(username != null && username != "") { account._username = username; }
            if(email != null && email != "") { account._email = email; }
            if(privateKeyPath != null && privateKeyPath != "") { account._privateKeyPath = privateKeyPath; }

            //update accounts
            SaveAccounts();
            if(account == Program.currentAccount) { SetCurrentAccount(account); }
            RestoreSSHConfig();
        }
        public (bool, int) ImportAccount(string path)
        {
            if(!File.Exists(path) || File.GetAttributes(path).HasFlag(FileAttributes.Directory)) { return (false, 0); }
            string json = File.ReadAllText(path);
            List<Account>? newAccounts = JsonConvert.DeserializeObject<List<Account>>(json);
            if(newAccounts == null) { return (false, 0); }
            Program.accounts.AddRange(newAccounts);
            SaveAccounts();
            return (true, newAccounts.Count);
        }
        
        public void SaveAccounts()
        {
            string json = JsonConvert.SerializeObject(Program.accounts, Formatting.Indented);
            using(StreamWriter streamWriter = new StreamWriter(accountsFilePath)) {}
            File.WriteAllText(accountsFilePath, json);
        }
        public bool RestoreSSHConfig()
        {
            string configName = "config";
            Directory.CreateDirectory(sshPath);
            using(StreamWriter wrt = new StreamWriter(sshPath + configName)) {}

            string newConfigText = "Host github.com \n";
            if(Program.currentAccount == null) { return false; }
            newConfigText += "IdentityFile " + Program.currentAccount._privateKeyPath + "\n";
            if(Directory.Exists(sshPath + sshConfigIncludesFolderName)) { newConfigText += "Include " + sshConfigIncludesFolderName + "/*"; }
            File.WriteAllText(sshPath + configName, newConfigText);
            return true;
        }   
        public string GetSaveFileLocation()
        {
            return Path.GetFullPath(accountsFilePath);
        }
        
        public bool SetCurrentAccount(Account account)
        {
            Program.currentAccount = account;

            //change git
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                RunCommand("git", "config --global user.name " + Program.currentAccount._username);
                RunCommand("git", "config --global --replace-all user.email " + Program.currentAccount._email);
            }
            else if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                RunCommand("git config --global user.name " + Program.currentAccount._username);
                RunCommand("git config --global --replace-all user.email " + Program.currentAccount._email);
            }
            
            RestoreSSHConfig();
            return true;
        }
        public void LoadAccounts()
        {
            accountsFilePath = AppDomain.CurrentDomain.BaseDirectory + "/" + accountsFileName;
            if(File.Exists(accountsFilePath)) 
            { 
                string json = File.ReadAllText(accountsFilePath);
                List<Account>? tmp = JsonConvert.DeserializeObject<List<Account>>(json);
                if(tmp != null) { Program.accounts = tmp; }
            }
        }
        public void LoadCurrentAccount()
        {
            string username = "";
            string email = "";
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                username = RunCommand("git", "config --global user.name", true);
                email = RunCommand("git", "config --global user.email", true);
            }
            else if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                username = RunCommand("git config --global user.name", true);
                email = RunCommand("git config --global user.email", true);
            }
            
            username = username.Replace("\n", "");
            email = email.Replace("\n", "");
            for (int i = 0; i < Program.accounts.Count; i++)
            {
                if(Program.accounts[i]._email == email && Program.accounts[i]._username == username)
                {
                    Program.currentAccount = Program.accounts[i];
                    break;
                }
            }
        }

        public string RunCommand(string command, bool getOutput = false, Dictionary<string, string>? inputArguments = null)
        {
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) { command = "-c " + command; }
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) { command = "/c " + command; }

            return RunCommand(terminalName, command, getOutput, inputArguments);
        }
        public string RunCommand(string appName, string command, bool getOutput = false, Dictionary<string, string>? inputArguments = null)
        {
            Process p = new Process
            {
                StartInfo = 
                {
                    FileName = appName,
                    Arguments = command
                }
            };
            if(getOutput)
            {
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();
                string stdOut = "";
                while (!p.StandardOutput.EndOfStream) { stdOut = p.StandardOutput.ReadToEnd(); }
                p.WaitForExit();
                return stdOut;
            }
            if(inputArguments != null)
            {
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardError = true;

                int index = 0;
                p.OutputDataReceived += (s, e) => {
                    Console.WriteLine("(Received) " + e.Data + " (END)");
                    if(e.Data != null && inputArguments.TryGetValue(e.Data, out string? response) && response != null)
                    {
                        Console.WriteLine("(RESPONDED)");
                        p.StandardInput.WriteLine(response);
                        index++;
                    }
                };

                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                p.WaitForExit();
                p.StandardOutput.Dispose();
                p.StandardInput.Dispose();
                p.StandardError.Dispose();
                return "";
            }

            p.Start();
            p.WaitForExit();
            return "";
        }
    }
}