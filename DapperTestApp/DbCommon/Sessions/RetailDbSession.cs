using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using DapperTestApp.DbCommon.Options;
using Microsoft.Extensions.Options;

namespace DapperTestApp.DbCommon.Sessions
{
    public sealed class RetailDbSession : IRetailDbSession
    {
        private readonly string dbConnectionOption;

        public RetailDbSession(IOptions<DbConnectionOption> dbConnectionOption)
        {
            this.dbConnectionOption = dbConnectionOption.Value.RetailDb;
        }

        public SqlConnection Connection => new(this.dbConnectionOption);

        public async ValueTask DisposeAsync()
        {
            await Connection.DisposeAsync();
        }
    }
}