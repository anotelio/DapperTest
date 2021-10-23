using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper.ColumnMapper;
using DapperTestApp.DbCommon.DbContracts.Enums;

namespace DapperTestApp.DbCommon.DbContracts.Outputs
{
    public class ProductShipmentsJson
    {
        [ColumnMapping("product_id")]
        [JsonPropertyName("product_id")]
        public int ProductId { get; set; }

        [ColumnMapping("product_name")]
        [JsonPropertyName("product_name")]
        public string ProductName { get; set; }

        [ColumnMapping("product_type")]
        [JsonPropertyName("product_type")]
        public ProductType ProductType { get; set; }

        [JsonPropertyName("shipments")]
        public JsonDocument Shipments { get; set; }
    }
}
