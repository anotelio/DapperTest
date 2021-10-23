using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using DapperTestApp.DbCommon.Base;

namespace DapperTestApp.DbCommon.Extensions
{
    public static class DbConnectionExtensions
    {
        /// <summary>
        /// Generic connect to transactional connection otherwise get new connection
        /// </summary>
        /// <typeparam name="TC"></typeparam>
        /// <param name="dbSession"></param>
        /// <param name="dbTransaction"></param>
        /// <returns></returns>
        public static TC DbConnect<TC>(this TC connection, IDbTransaction dbTransaction)
            where TC : DbConnection
        {
            return (TC)dbTransaction?.Connection ?? connection;
        }

        /// <summary>
        /// General connect to transactional connection otherwise get new connection
        /// </summary>
        /// <typeparam name="TC"></typeparam>
        /// <param name="dbSession"></param>
        /// <param name="dbTransaction"></param>
        /// <returns></returns>
        public static DbConnection DbConnect(this DbConnection connection, DbTransaction dbTransaction)
        {
            return dbTransaction?.Connection ?? connection;
        }

        /// <summary>
        /// SQLServer-typed connect to transactional connection otherwise get new connection
        /// </summary>
        /// <param name="sqlSession"></param>
        /// <param name="sqlTransaction"></param>
        /// <returns></returns>
        public static SqlConnection DbConnect(this SqlConnection connection, SqlTransaction sqlTransaction)
        {
            return sqlTransaction?.Connection ?? connection;
        }
        
        /// <summary>
        /// Generic connect to transactional connection otherwise get new connection from db session
        /// </summary>
        /// <typeparam name="TC"></typeparam>
        /// <param name="dbSession"></param>
        /// <param name="dbTransaction"></param>
        /// <returns></returns>
        public static TC DbConnect<TC>(this IDbSession<TC> session, IDbTransaction dbTransaction)
            where TC : DbConnection
        {
            return (TC)dbTransaction?.Connection ?? session.Connection;
        }

        /// <summary>
        /// General connect to transactional connection otherwise get new connection from db session
        /// </summary>
        /// <typeparam name="TC"></typeparam>
        /// <param name="dbSession"></param>
        /// <param name="dbTransaction"></param>
        /// <returns></returns>
        public static DbConnection DbConnect(this IDbSession<DbConnection> session, DbTransaction dbTransaction)
        {
            return dbTransaction?.Connection ?? session.Connection;
        }

        /// <summary>
        /// SQLServer-typed connect to transactional connection otherwise get new connection from db session
        /// </summary>
        /// <param name="sqlSession"></param>
        /// <param name="sqlTransaction"></param>
        /// <returns></returns>
        public static SqlConnection DbConnect(this IDbSession<SqlConnection> session, SqlTransaction sqlTransaction)
        {
            return sqlTransaction?.Connection ?? session.Connection;
        }
    }
}
