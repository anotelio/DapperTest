using System;
using System.Text.Json.Serialization;
using Dapper.ColumnMapper;

namespace DapperTestApp.DbCommon.DbContracts.Outputs
{
    public class Shipment
    {
        [ColumnMapping("shipment_id")]
        [JsonPropertyName("shipment_id")]
        public int ShipmentId { get; set; }

        [ColumnMapping("product_id")]
        [JsonPropertyName("product_id")]
        public int ProductId { get; set; }

        [ColumnMapping("quantity")]
        [JsonPropertyName("quantity")]
        public decimal Quantity { get; set; }

        [ColumnMapping("is_delete")]
        [JsonPropertyName("is_delete")]
        public bool IsDelete { get; set; }

        [ColumnMapping("created_at_utc")]
        [JsonPropertyName("created_at_utc")]
        public DateTime CreatedAtUtc { get; set; }
    }
}
