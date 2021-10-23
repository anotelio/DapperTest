using System.Text.Json.Serialization;

namespace DapperTestApp.DbCommon.DbContracts.Inputs
{
    public class ShipmentTvp
    {
        [JsonPropertyName("product_id")]
        public int ProductId { get; set; }

        [JsonPropertyName("quantity")]
        public decimal Quantity { get; set; }
    }
}
