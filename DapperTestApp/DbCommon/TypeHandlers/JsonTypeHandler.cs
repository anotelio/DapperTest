using Dapper;
using System.Data;
using System.Text.Json;
using System;

namespace DapperTestApp.DbCommon.TypeHandlers
{
    public class JsonTypeHandler : SqlMapper.ITypeHandler
    {
        public void SetValue(IDbDataParameter parameter, object value)
        {
            parameter.Value = JsonSerializer.Serialize(value);
        }

        public object Parse(Type destinationType, object value)
        {
            return JsonSerializer.Deserialize(value as string, destinationType);
        }
    }
}
