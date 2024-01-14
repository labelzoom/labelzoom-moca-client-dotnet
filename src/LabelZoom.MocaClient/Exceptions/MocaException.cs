using System;

namespace LabelZoom.MocaClient.Exceptions
{
    public class MocaException : Exception
    {
        public int ErrorCode { get; private set; }

        public MocaException(int errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
