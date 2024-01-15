using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

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

        public abstract Task<bool> Login(string userId, string password);
        public abstract Task<bool> Login(string userId, string password, CancellationToken token);
        public abstract void LogOut();
        public abstract Task<MocaResponse> Execute(string command, IDictionary<string, object>? context = null);
        public abstract Task<MocaResponse> Execute(string command, CancellationToken token, IDictionary<string, object>? context = null);

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
