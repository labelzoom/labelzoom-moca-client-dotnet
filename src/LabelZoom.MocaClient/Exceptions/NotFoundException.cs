namespace LabelZoom.MocaClient.Exceptions
{
    public class NotFoundException : MocaException
    {
        public const int STATUS_CODE = 510;

        public NotFoundException(string message) : base(STATUS_CODE, message) { }
    }
}
