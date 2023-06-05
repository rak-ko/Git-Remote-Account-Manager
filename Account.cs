using Newtonsoft.Json;

namespace GAM
{
    public class Account
    {
        [JsonProperty]
        string username;
        [JsonProperty]
        string email;
        [JsonProperty]
        string privateKeyPath;
        [JsonProperty]
        string sshHostname;

        [JsonIgnore]
        public string _username
        {
            get
            {
                return username;
            }
            set
            {
                username = value;
            }
        }
        [JsonIgnore]
        public string _email
        {
            get
            {
                return email;
            }
            set
            {
                email = value;
            }
        }
        [JsonIgnore]
        public string _privateKeyPath
        {
            get
            {
                return privateKeyPath;
            }
            set
            {
                privateKeyPath = value;
            }
        }
        [JsonIgnore]
        public string _sshHostname
        {
            get
            {
                return sshHostname;
            }
            set
            {
                sshHostname = value;
            }
        }

        public Account(string username, string email, string privateKeyPath, string sshHostname)
        {
            this.username = username;
            this.email = email;
            this.privateKeyPath = privateKeyPath;
            this.sshHostname = sshHostname;
        }
        public string ToString(int index, bool compact = false)
        {
            string toReturn = "";
            if (compact) { toReturn = string.Format("[{0}] User: {1} Email: {2}", index, username, email); }
            else { toReturn = string.Format("[{0}] \n - User: {1} \n - Email: {2} \n - Private Key Path: {3}", index, username, email, privateKeyPath); }
            return toReturn;
        }
    }
}
