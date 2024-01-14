namespace LabelZoom.MocaClient.Exceptions
{
    public class CommandNotFoundException : MocaException
    {
        public const int STATUS_CODE = 501;

        public CommandNotFoundException(string message) : base(STATUS_CODE, message) { }
    }
}
