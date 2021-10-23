using System.Data;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Dapper;

namespace DapperTestApp.DbCommon.Extensions
{
    public static class SqlQueryExtensions
    {
        public static async Task<TResult> QueryFromJson<TResult>(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var jsonChunks = await cnn.QueryAsync<string>(sql, param,
                transaction, commandTimeout, commandType);

            var completeJsonPayload = string.Concat(jsonChunks);

            return JsonSerializer.Deserialize<TResult>(completeJsonPayload);
        }

        public static async Task<string> QueryFromJson(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var jsonChunks = await cnn.QueryAsync<string>(sql, param,
                transaction, commandTimeout, commandType);

            // other variant to concat json
            StringBuilder jsonSb = new();
            await using StringWriter jsonSw = new(jsonSb);
            foreach (string chunk in jsonChunks)
            {
                await jsonSw.WriteAsync(chunk);
            }

            return jsonSb.ToString();
        }
    }
}
