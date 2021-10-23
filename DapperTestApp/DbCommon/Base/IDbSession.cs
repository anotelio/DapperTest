using System;
using System.Data.Common;

namespace DapperTestApp.DbCommon.Base
{
    public interface IDbSession<TC> : IAsyncDisposable
        where TC : DbConnection
    {
        public TC Connection { get; }
    }
}