using Newtonsoft.Json;

namespace GAM
{
    public class Host : IPrintable
    {
        [JsonProperty]
        int id;
        [JsonProperty]
        string host = "";

        [JsonIgnore]
        public int _id
        {
            get
            {
                return id;
            }
        }
        [JsonIgnore]
        public string _host
        {
            get
            {
                return host;
            }
            set
            {
                host = value;
            }
        }

        public Host(int id, string host)
        {
            this.id = id;
            this.host = host;
        }
        public string ToString(int index, bool compact = false)
        {
            return String.Format("[{0}] {1}", index, host);
        }
    }
}
