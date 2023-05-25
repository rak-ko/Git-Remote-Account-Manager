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

        public Account(ulong ID, string username, string email, string privateKeyPath)
        {
            this.ID = ID;
            this.username = username;
            this.email = email;
            this.privateKeyPath = privateKeyPath;
        }
        public string ToString(bool compact = false)
        {
            string toReturn = "";
            if(compact) { toReturn = string.Format("[{0}] User: {1} Email: {2}", ID, username, email); }
            else { toReturn = string.Format("[{0}] \nUser: {1} \nEmail: {2} \nPrivate Key Path: {3}", ID, username, email, privateKeyPath); }
            return toReturn;
        }
    }
}