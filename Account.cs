using Newtonsoft.Json;

namespace GAM
{
    public class Account
    {
        [JsonProperty]
        ulong ID;
        [JsonProperty]
        string username;
        [JsonProperty]
        string email;
        [JsonProperty]
        string publicKeyPath;
        [JsonProperty]
        string privateKeyPath;

        [JsonIgnore]
        public ulong _ID
        {
            get
            {
                return ID;
            }
        }
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
        public string _publicKeyPath
        {
            get
            {
                return publicKeyPath;
            }
        }
        [JsonIgnore]
        public string _privateKeyPath
        {
            get
            {
                return privateKeyPath;
            }
        }

        public Account(ulong ID, string username, string email, string publicKeyPath, string privateKeyPath)
        {
            this.ID = ID;
            this.username = username;
            this.email = email;
            this.publicKeyPath = publicKeyPath;
            this.privateKeyPath = privateKeyPath;
        }
        public override string ToString()
        {
            return string.Format("[{0}] User: {1} Email: {2}", ID, username, email);
        }
    }
}