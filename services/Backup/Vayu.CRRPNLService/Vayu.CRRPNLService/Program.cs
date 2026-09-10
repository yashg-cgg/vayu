namespace Vayu.CRRPNLService
{
    class Program
    {
        static void Main(string[] args)
        {
            CRRServer cRRServer = new CRRServer();
            cRRServer.Connect();
        }
    }
}
