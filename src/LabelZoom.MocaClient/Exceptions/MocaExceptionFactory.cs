namespace LabelZoom.MocaClient.Exceptions
{
    internal class MocaExceptionFactory
    {
        public static MocaException Generate(int statusCode, string message)
        {
            switch (statusCode)
            {
                case -1403:
                case NotFoundException.STATUS_CODE:
                    return new NotFoundException(message);
                case CommandNotFoundException.STATUS_CODE:
                    return new CommandNotFoundException(message);
                default:
                    return new MocaException(statusCode, message);
            }
        }
    }
}
