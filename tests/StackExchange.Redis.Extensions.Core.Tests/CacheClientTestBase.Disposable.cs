using System;
using StackExchange.Redis.Extensions.Tests.Extensions;

namespace StackExchange.Redis.Extensions.Core.Tests
{
    public abstract partial class CacheClientTestBase : IDisposable
    {
        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                db.FlushDatabase();
                db.Multiplexer.GetSubscriber().UnsubscribeAll();
                connectionPoolManager.Dispose();
            }

            disposed = true;
        }
    }
}
