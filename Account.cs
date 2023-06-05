using Newtonsoft.Json;

namespace GAM
{
    public class Account : IPrintable
    {
        [JsonProperty]
        string username;
        [JsonProperty]
        string email;
        [JsonProperty]
        string privateKeyPath;
        [JsonProperty]
        int sshHostId;

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
        public int _sshHostId
        {
            get
            {
                return sshHostId;
            }
            set
            {
                sshHostId = value;
            }
        }

        public Account(string username, string email, string privateKeyPath, int sshHostId)
        {
            this.username = username;
            this.email = email;
            this.privateKeyPath = privateKeyPath;
            this.sshHostId = sshHostId;
        }
        public string ToString(int index, bool compact = false)
        {
            string toReturn = "";
            int hostIndex = Program.hosts.FindIndex(x => x._id == sshHostId);
            string host = (hostIndex != -1) ? Program.hosts[hostIndex]._host : "Github.com";
            if (compact) { toReturn = string.Format("[{0}] User: {1} Email: {2} Host: {3}", index, username, email, host); }
            else { toReturn = string.Format("[{0}] \n - User: {1} \n - Email: {2} \n - Private Key Path: {3} \n - Host: {4}", index, username, email, privateKeyPath, host); }
            return toReturn;
        }
    }
}
