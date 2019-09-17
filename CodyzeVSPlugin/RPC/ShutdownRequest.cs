namespace CodyzeVSPlugin.RPC
{
    class ShutdownRequest : RequestMessage
    {
        public ShutdownRequest() : base("shutdown", true)
        {
        }
    }
}
