namespace GAM
{
    public class Account
    {
        ulong ID;
        string username = null!;
        string email = null!;

        public ulong _ID
        {
            get
            {
                return ID;
            }
        }

        public Account(ulong ID, string username, string email)
        {
            this.ID = ID;
            this.username = username;
            this.email = email;
        }
        public override string ToString()
        {
            return string.Format("[{0}] \nUser: {1} \nEmail: {2}", ID, username, email);
        }
    }
}