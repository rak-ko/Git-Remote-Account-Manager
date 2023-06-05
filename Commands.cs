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

        public void CreateAccount(string username, string email, string keyFileName, Host host, string passphrase)
        {
            //generate ssh keys
            string args = "-t ed25519 -C \"" + email + "\" -f \"" + keyFileName + "\" -N " + passphrase;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) { RunCommand("ssh-keygen", args); }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) { RunCommand("ssh-keygen " + args); }
            else { RunCommand("ssh-keygen", args); } //linux

            //add account
            Account newAccount = new Account(username, email, keyFileName, host._id);
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
        public void EditAccount(Account account, string? username, string? email, string? privateKeyPath, Host? newHost)
        {
            if (username != null && username != "") { account._username = username; }
            if (email != null && email != "") { account._email = email; }
            if (privateKeyPath != null && privateKeyPath != "") { account._privateKeyPath = privateKeyPath; }
            if(newHost != null) { account._sshHostId = newHost._id; }

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
            string configName = "gamConfig";
            string mainConfigName = "config";
            string configPath = sshPath + configName;
            string mainConfigPath = sshPath + mainConfigName;

            //create gamConfig
            Directory.CreateDirectory(sshPath);
            using (StreamWriter wrt = new StreamWriter(configPath)) { }
            if (Program.currentAccount == null) { return false; }

            int index = Program.hosts.FindIndex(x => x._id == Program.currentAccount._sshHostId);
            string hostURL = (index != -1) ? Program.hosts[index]._host : "Github.com";
            string newConfigText = "Host " + hostURL + " \n";
            newConfigText += "IdentityFile " + Program.currentAccount._privateKeyPath + "\n";
            File.WriteAllText(configPath, newConfigText);

            //add to main config
            if(!File.Exists(mainConfigPath)) { File.Create(mainConfigPath).Close(); }
            string configString = File.ReadAllText(mainConfigPath);
            if(!configString.Contains("Include " + configPath)) { configString = "Include " + configPath + "\n" + configString; }
            File.WriteAllText(mainConfigPath, configString);

            return true;
        }
        public string GetSaveFileLocation()
        {
            return Path.GetFullPath(accountsFilePath);
        }
        public string GetConfigFileLocation()
        {
            return Path.GetFullPath(configFilePath);
        }

        /// <returns>1 -> successfully set | 0 -> generic error | 2 -> Account doesn't have ssh id set, using default</returns>
        public int SetCurrentAccount(Account account)
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
            return (account._sshHostId == -1) ? 2 : 1;
        }
        public void LoadAccounts()
        {
            accountsFilePath = AppDomain.CurrentDomain.BaseDirectory + accountsFileName;
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
                    SetCurrentAccount(Program.accounts[i]);
                    break;
                }
            }
        }

        public void CreateHost(string host)
        {
            int id = (Program.hosts.Count > 0) ? Program.hosts.Max(x => x._id) : 0;
            Program.hosts.Add(new Host(id + 1, host));
            SaveConfig();
        }
        public bool RemoveHost(Host host)
        {
            if(!Program.hosts.Contains(host)) { return false; }

            //change any occurrences in accounts
            for (int i = 0; i < Program.accounts.Count; i++) { if(Program.accounts[i]._sshHostId == host._id) { Program.accounts[i]._sshHostId = -1; }}
            SaveAccounts();

            Program.hosts.Remove(host);
            SaveConfig();
            return true;
        }
        public void EditHost(Host host, string? hostURL)
        {
            if(hostURL != null) { host._host = hostURL; }
            SaveConfig();
            RestoreSSHConfig(); //in case host was in active use
        }

        public void LoadConfig()
        {
            //no config -> create default
            configFilePath = AppDomain.CurrentDomain.BaseDirectory + configFileName;
            if (!File.Exists(configFilePath)) { ResetConfigToDefault(); }

            //load config
            string json = File.ReadAllText(configFilePath);
            if(json.Replace(" ", "") == "") { ResetConfigToDefault(); }
            Dictionary<string, string>? config = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            if(config != null)
            {
                //load hosts
                if(config.ContainsKey("hosts")) 
                { 
                    List<Host>? tmpHosts = JsonConvert.DeserializeObject<List<Host>>(config["hosts"]);
                    if(tmpHosts != null) { Program.hosts = tmpHosts; }
                }
                else
                {
                    config.Add("hosts", DefaultHosts());
                }
            }
        }
        public void SaveConfig()
        {
            //compile config
            Dictionary<string, string> config = new Dictionary<string, string>() {
                { "hosts", JsonConvert.SerializeObject(Program.hosts, Formatting.Indented) }
            };

            File.WriteAllText(configFilePath, JsonConvert.SerializeObject(config, Formatting.Indented));
        }
        public void ResetConfigToDefault()
        {
            using (StreamWriter writer = new StreamWriter(configFilePath)) 
            {
                Dictionary<string, string> newConfig = new Dictionary<string, string>() {
                    { "hosts", DefaultHosts() }
                };
                writer.Write(JsonConvert.SerializeObject(newConfig, Formatting.Indented));
            }
        }
        public string DefaultHosts()
        {
            return JsonConvert.SerializeObject(new List<Host>() { 
                new Host(0, "Github.com"),
                new Host(1, "Gitlab.com")
            }, Formatting.Indented);
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