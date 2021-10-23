using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DapperTestApp.DbCommon.DbContracts.Outputs
{
    public class ProductShipments : Product
    {
        [JsonPropertyName("shipments")]
        public IEnumerable<Shipment> Shipments { get; set; }
    }
}
