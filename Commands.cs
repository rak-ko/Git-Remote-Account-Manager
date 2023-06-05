using System.Diagnostics;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace GAM
{
    public class Commands
    {
        const string accountsFileName = "gamAccounts.json";
        const string configFileName = "gamConfig.json";
        const string sshConfigIncludesFolderName = "customConfigs";
        static string accountsFilePath = "";
        static string configFilePath = "";

        const string windowsTerminalName = "cmd.exe";
        const string linuxTerminalName = "/bin/bash";
        string terminalName = windowsTerminalName;

        public string sshPath = "";

        public Commands()
        {
            //setup shell
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) { terminalName = linuxTerminalName; }

            //setup sshPath
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) { sshPath = "/home/" + Environment.UserName + "/.ssh/"; }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) { sshPath = "C:/Users/" + Environment.UserName + "/.ssh/"; }
            else { sshPath = "/home/" + Environment.UserName + "/.ssh/"; } //linux again
        }

        public void CreateAccount(string username, string email, string keyFileName, string hostname, string passphrase)
        {
            //generate ssh keys
            string args = "-t ed25519 -C \"" + email + "\" -f \"" + keyFileName + "\" -N " + passphrase;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) { RunCommand("ssh-keygen", args); }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) { RunCommand("ssh-keygen " + args); }
            else { RunCommand("ssh-keygen", args); } //linux

            //add account
            Account newAccount = new Account(username, email, keyFileName, hostname);
            Program.accounts.Add(newAccount);
            if (Program.currentAccount == null) { SetCurrentAccount(Program.accounts[Program.accounts.Count - 1]); }
            SaveAccounts();
        }
        public bool RemoveAccount(Account account)
        {
            //set new current account
            if (Program.currentAccount == account && Program.accounts.Count > 0)
            {
                for (int i = 0; i < Program.accounts.Count; i++)
                {
                    if (Program.accounts[i] != Program.currentAccount)
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
            if (username != null && username != "") { account._username = username; }
            if (email != null && email != "") { account._email = email; }
            if (privateKeyPath != null && privateKeyPath != "") { account._privateKeyPath = privateKeyPath; }

            //update accounts
            SaveAccounts();
            if (account == Program.currentAccount) { SetCurrentAccount(account); }
            RestoreSSHConfig();
        }
        public (bool, int) ImportAccount(string path)
        {
            if (!File.Exists(path) || File.GetAttributes(path).HasFlag(FileAttributes.Directory)) { return (false, 0); }
            string json = File.ReadAllText(path);
            List<Account>? newAccounts = JsonConvert.DeserializeObject<List<Account>>(json);
            if (newAccounts == null) { return (false, 0); }
            Program.accounts.AddRange(newAccounts);
            SaveAccounts();
            return (true, newAccounts.Count);
        }

        public void SaveAccounts()
        {
            string json = JsonConvert.SerializeObject(Program.accounts, Formatting.Indented);
            using (StreamWriter streamWriter = new StreamWriter(accountsFilePath)) { }
            File.WriteAllText(accountsFilePath, json);
        }
        public bool RestoreSSHConfig()
        {
            string configName = "config";
            Directory.CreateDirectory(sshPath);
            using (StreamWriter wrt = new StreamWriter(sshPath + configName)) { }

            string newConfigText = "Host github.com \n";
            if (Program.currentAccount == null) { return false; }
            newConfigText += "IdentityFile " + Program.currentAccount._privateKeyPath + "\n";
            if (Directory.Exists(sshPath + sshConfigIncludesFolderName)) { newConfigText += "Include " + sshConfigIncludesFolderName + "/*"; }
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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                RunCommand("git", "config --global user.name " + Program.currentAccount._username);
                RunCommand("git", "config --global --replace-all user.email " + Program.currentAccount._email);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
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
            if (File.Exists(accountsFilePath))
            {
                string json = File.ReadAllText(accountsFilePath);
                List<Account>? tmp = JsonConvert.DeserializeObject<List<Account>>(json);
                if (tmp != null) { Program.accounts = tmp; }
            }
        }
        public void LoadCurrentAccount()
        {
            string username = "";
            string email = "";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                username = RunCommand("git", "config --global user.name", true);
                email = RunCommand("git", "config --global user.email", true);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                username = RunCommand("git config --global user.name", true);
                email = RunCommand("git config --global user.email", true);
            }

            username = username.Replace("\n", "");
            email = email.Replace("\n", "");
            for (int i = 0; i < Program.accounts.Count; i++)
            {
                if (Program.accounts[i]._email == email && Program.accounts[i]._username == username)
                {
                    Program.currentAccount = Program.accounts[i];
                    break;
                }
            }
        }

        public void LoadHostnames()
        {
            configFilePath = AppDomain.CurrentDomain.BaseDirectory + "/" + configFileName;
            if (File.Exists(configFilePath))
            {
                string json = File.ReadAllText(configFilePath);
                Dictionary<string, List<string>>? tmp = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
                if (tmp != null && tmp.ContainsKey("hostnames")) { Program.hostnames = tmp["hostnames"]; }
            }
            else
            {
                File.Create(configFilePath);
                Dictionary<string, List<string>> newConfig = new Dictionary<string, List<string>>();
                newConfig.Add("hostnames", new List<string>() {
                    "github.com",
                    "gitlab.com"
                });
                File.WriteAllText(configFilePath, JsonConvert.SerializeObject(newConfig));
            }
        }

        public string RunCommand(string command, bool getOutput = false)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) { command = "-c " + command; }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) { command = "/c " + command; }

            return RunCommand(terminalName, command, getOutput);
        }
        public string RunCommand(string appName, string command, bool getOutput = false)
        {
            Process p = new Process
            {
                StartInfo =
                {
                    FileName = appName,
                    Arguments = command
                }
            };

            if (getOutput)
            {
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();
                string stdOut = "";
                while (!p.StandardOutput.EndOfStream) { stdOut = p.StandardOutput.ReadToEnd(); }
                p.WaitForExit();
                return stdOut;
            }
            p.Start();
            p.WaitForExit();
            return "";
        }
    }
}
