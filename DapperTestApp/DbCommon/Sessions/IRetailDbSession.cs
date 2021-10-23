using System.Data.SqlClient;
using DapperTestApp.DbCommon.Base;

namespace DapperTestApp.DbCommon.Sessions
{
    public interface IRetailDbSession : IDbSession<SqlConnection>
    {
    }
}