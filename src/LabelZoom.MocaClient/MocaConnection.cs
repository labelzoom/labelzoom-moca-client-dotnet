using System;
using System.Collections.Generic;
using System.Data;

namespace LabelZoom.MocaClient
{
    public abstract class MocaConnection : IDisposable
    {
        protected const string MOCA_DATE_FORMAT = "yyyyMMddHHmmss";

        protected readonly IDictionary<string, string> environment = new Dictionary<string, string>();
        protected string? sessionKey;
        protected string? userId;

        public abstract string ConnectionString { get; protected set; }

        public int ConnectionTimeout => 15;

        #region Dispose Pattern
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~MocaConnection()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public virtual void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
