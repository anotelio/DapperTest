using System.Text.Json.Serialization;
using DapperTestApp.DbCommon.DbContracts.Enums;

namespace DapperTestApp.DbCommon.DbContracts.Inputs
{
    public class ProductTvp
    {
        [JsonPropertyName("product_id")]
        public int ProductId { get; set; }

        [JsonPropertyName("product_name")]
        public string ProductName { get; set; }

        [JsonPropertyName("product_type")]
        public byte ProductType { get; set; }
    }
}
