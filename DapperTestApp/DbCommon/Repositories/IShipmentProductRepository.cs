using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using DapperTestApp.DbCommon.DbContracts.Enums;
using DapperTestApp.DbCommon.DbContracts.Inputs;
using DapperTestApp.DbCommon.DbContracts.Outputs;
using DapperTestApp.DbCommon.Sessions;

namespace DapperTestApp.DbCommon.Repositories
{
    public interface IShipmentProductRepository
    {
        IRetailDbSession RetailDbSession { get; }

        Task<IEnumerable<Shipment>> ShipmentsQueryGet();

        Task<IEnumerable<Shipment>> ShipmentsGet(decimal quantity);

        Task<IEnumerable<Shipment>> ShipmentsGet(IList<int> productIds);

        Task<IEnumerable<Product>> ProductsProcGet();

        Task<IEnumerable<Product>> ProductsByFilterGet(string name_filter, ProductType? product_type);

        Task<(int, decimal, bool)> ShipmentsNumbers(int productId, int numberCount);

        Task<ProductShipments> ProductShipmentsGet(int productId);

        Task ProductsAdd(IList<ProductTvp> products);

        Task<int> ShipmentAdd(ShipmentTvp shipment);

        Task<ProductShipments> ProductShipmentsMapGet(int productId);

        Task<IEnumerable<ProductShipments>> ProductShipmentsMapGetAll();

        Task<IEnumerable<Product>> ProductsTableGet();

        Task<object> ProductsTableToObjectGet();

        Task<IEnumerable<Shipment>> ShipmentsGetJson();

        Task<string> ShipmentsGetDirectlyJson();

        Task<IEnumerable<ProductShipments>> ProductShipmentsGetJson();

        Task<IEnumerable<ProductShipments>> ProductShipmentsJsonGet();

        Task<IEnumerable<ProductShipmentsJson>> ProductShipmentsJsonDocumentGet();

        Task<IEnumerable<Shipment>> ShipmentsGet(IDbTransaction dbTransaction = null);

        Task<IEnumerable<Product>> ProductsGet(IDbTransaction dbTransaction = null);
    }
}