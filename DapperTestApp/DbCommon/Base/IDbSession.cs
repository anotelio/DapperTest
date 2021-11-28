using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace DapperTestApp.DbCommon.Base
{
    public interface IDbSession<TC> : IAsyncDisposable
        where TC : DbConnection
    {
        public TC Connection { get; }

        public async Task<DbTransaction> OpenAndBeginTransactionAsync()
        {
            var cnn = this.Connection;
            await cnn.OpenAsync();
            return await cnn.BeginTransactionAsync();
        }

        public async ValueTask DisposeAsync(DbTransaction dbTransaction)
        {
            await dbTransaction.Connection.DisposeAsync();
        }
    }
}