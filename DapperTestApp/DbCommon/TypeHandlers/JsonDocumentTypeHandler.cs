using System.Data;
using System.Text.Json;
using Dapper;

namespace DapperTestApp.DbCommon.TypeHandlers
{
    public class JsonDocumentTypeHandler : SqlMapper.TypeHandler<JsonDocument>
    {
        public override JsonDocument Parse(object value)
        {
            return JsonDocument.Parse(value.ToString());
        }

        public override void SetValue(IDbDataParameter parameter, JsonDocument value)
        {
            parameter.DbType = DbType.String;
            parameter.Size = int.MaxValue;
            parameter.Value = value.ToString();
        }
    }
}
