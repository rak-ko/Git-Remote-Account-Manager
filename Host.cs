namespace GAM
{
    public class Host
    {
        Guid id;
        string host = "";

        public Host(string host)
        {
            id = Guid.NewGuid();
            this.host = host;
        }
    }
}
