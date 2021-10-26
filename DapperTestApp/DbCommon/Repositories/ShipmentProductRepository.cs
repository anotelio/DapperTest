using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DapperParameters;
using DapperTestApp.DbCommon.DbContracts.Enums;
using DapperTestApp.DbCommon.DbContracts.Inputs;
using DapperTestApp.DbCommon.DbContracts.Outputs;
using DapperTestApp.DbCommon.Extensions;
using DapperTestApp.DbCommon.Sessions;

namespace DapperTestApp.DbCommon.Repositories
{
    public sealed class ShipmentProductRepository : IShipmentProductRepository
    {
        public ShipmentProductRepository(IRetailDbSession retailDbSession)
        {
            RetailDbSession = retailDbSession;
        }

        public IRetailDbSession RetailDbSession { get; }

        /// <summary>
        /// Get all shipments query
        /// </summary>
        public async Task<IEnumerable<Shipment>> ShipmentsQueryGet()
        {
            const string query =
                @"SELECT
                    s.[shipment_id]       AS ShipmentId,
                    s.[product_id]        AS ProductId,
                    s.[quantity]          AS Quantity,
                    s.[is_delete]         AS IsDelete,
                    s.[created_at_utc]    AS CreatedAtUtc
                FROM [sale].[shipments] s";

            // CommandType.Text by default
            return await RetailDbSession.Connection.QueryAsync<Shipment>(query, commandTimeout: 40);
        }

        /// <summary>
        /// Get all shipments query with parameter
        /// </summary>
        public async Task<IEnumerable<Shipment>> ShipmentsGet(decimal quantity)
        {
            // add parameters divided by comma as anonymous types
            var parameters = new { Quantity = quantity };

            const string query =
                @"SELECT
                    s.[shipment_id]       AS ShipmentId,
                    s.[product_id]        AS ProductId,
                    s.[quantity]          AS Quantity,
                    s.[is_delete]         AS IsDelete,
                    s.[created_at_utc]    AS CreatedAtUtc
                FROM [sale].[shipments] s
                WHERE s.[quantity] > @Quantity";

            // CommandType.Text explicitly
            return await RetailDbSession.Connection
                .QueryAsync<Shipment>(query, parameters, commandType: CommandType.Text);
        }

        /// <summary>
        /// Get all shipments query with parameter list
        /// </summary>
        public async Task<IEnumerable<Shipment>> ShipmentsGet(IList<int> productIds)
        {
            var parameters = new { ProductIds = productIds };

            const string query =
                @"SELECT
                    s.[shipment_id],
                    s.[product_id],
                    s.[quantity],
                    s.[is_delete],
                    s.[created_at_utc]
                FROM[sale].[shipments] s
                WHERE s.[product_id] IN @ProductIds";

            return await RetailDbSession.Connection.QueryAsync<Shipment>(query, parameters);
        }

        /// <summary>
        /// Get all products proc
        /// </summary>
        public async Task<IEnumerable<Product>> ProductsProcGet()
        {
            const string proc = "[sale].[products_get]";

            return await RetailDbSession.Connection
                .QueryAsync<Product>(proc, commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Get products by filter proc
        /// </summary>
        public async Task<IEnumerable<Product>> ProductsByFilterGet(string nameFilter, ProductType? productType)
        {
            const string proc = "[sale].[products_filter_get]";

            DynamicParameters parameters = new();
            parameters.Add("@product_name_filter", nameFilter, DbType.String);
            parameters.Add("@product_type", productType, DbType.Byte);

            return await RetailDbSession.Connection
                .QueryAsync<Product>(proc, parameters, commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Get shipments numbers with different parameters proc
        /// </summary>
        public async Task<(int, decimal, bool)> ShipmentsNumbers(int productId, int numberCount)
        {
            const string proc = "[sale].[shipment_diff_params_numbers]";

            DynamicParameters parameters = new(new { product_id = productId });
            // or you can put product_id like above (ParameterDirection.Input explicitly)
            //parameters.Add("@product_id", productId, DbType.Int32, ParameterDirection.Input); 
            parameters.Add("@number_count", numberCount, DbType.Int32, ParameterDirection.InputOutput);
            parameters.Add("@quantity_sum", dbType: DbType.Decimal, direction: ParameterDirection.Output, precision: 10, scale: 3);
            parameters.Add("@overweight", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue); // only Int32

            await RetailDbSession.Connection
                .ExecuteAsync(proc, parameters, commandType: CommandType.StoredProcedure);

            numberCount = parameters.Get<int>("@number_count");
            decimal quantitySum = parameters.Get<decimal>("@quantity_sum");
            bool overweight = Convert.ToBoolean(parameters.Get<int>("@overweight"));

            return (numberCount, quantitySum, overweight);
        }

        /// <summary>
        /// Get product shipments multiple proc
        /// </summary>
        /// <see cref="https://dapper-tutorial.net/querymultiple"/>
        public async Task<ProductShipments> ProductShipmentsGet(int productId)
        {
            const string proc = "[sale].[product_shipments_multiple]";

            var parameters = new { product_id = productId };

            var gridReader = await RetailDbSession.Connection
                .QueryMultipleAsync(proc, parameters, commandType: CommandType.StoredProcedure);

            ProductShipments productShipments =
                await gridReader.ReadSingleOrDefaultAsync<ProductShipments>();
            productShipments.Shipments = await gridReader.ReadAsync<Shipment>();

            return productShipments;
        }

        /// <summary>
        /// Add products tvp proc
        /// </summary>
        /// <see cref="https://github.com/RasicN/Dapper-Parameters"/>
        /// <seealso cref="https://dapper-tutorial.net/parameter-table-valued-parameter"/>
        public async Task ProductsAdd(IList<ProductTvp> products)
        {
            const string proc = "[sale].[products_add_tvp]";

            var parameters = new DynamicParameters();
            parameters.AddTable("@products", "[sale].[t_products]", products);

            await RetailDbSession.Connection
                .ExecuteAsync(proc, parameters, commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Add shipments tvp proc
        /// </summary>
        /// <see cref="https://github.com/RasicN/Dapper-Parameters"/>
        /// <seealso cref="https://dapper-tutorial.net/parameter-table-valued-parameter"/>
        public async Task<int> ShipmentAdd(ShipmentTvp shipment)
        {
            const string proc = "[sale].[shipments_add_tvp]";

            IEnumerable<ShipmentTvp> shipments = new[] { shipment }; // IEnumerable as example to use AsList()

            var parameters = new DynamicParameters();
            parameters.AddTable("@shipments", "[sale].[t_shipments]", shipments.AsList());

            return await RetailDbSession.Connection
                .QuerySingleAsync<int>(proc, parameters, commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Get product shipments multi-mapping proc
        /// </summary>
        /// <seealso cref="https://dapper-tutorial.net/result-multi-mapping"/>
        public async Task<ProductShipments> ProductShipmentsMapGet(int productId)
        {
            const string proc = "[sale].[product_shipments_multi_mapping]";

            var parameters = new { product_id = productId };

            var lookupPairs = new Dictionary<int, ProductShipments>();

            (await RetailDbSession.Connection
                .QueryAsync(
                    sql: proc,
                    map: MapSelectedShipments(lookupPairs),
                    splitOn: "shipment_id",
                    param: parameters,
                    commandType: CommandType.StoredProcedure))
            .AsQueryable();

            return lookupPairs.Values.FirstOrDefault();

            static Func<ProductShipments, Shipment, ProductShipments> MapSelectedShipments(Dictionary<int, ProductShipments> lookupPairs)
            {
                return (ps, sh) =>
                {
                    if (!lookupPairs.TryGetValue(ps.ProductId, out ProductShipments productShipments))
                    {
                        lookupPairs.Add(ps.ProductId, productShipments = ps);
                    }

                    if (sh is not null)
                    {
                        sh.ProductId = ps.ProductId;
                        (productShipments.Shipments ??= new List<Shipment>()).AsList().Add(sh);
                    }

                    return productShipments;
                };
            }
        }

        /// <summary>
        /// Get all product shipments multi-mapping proc
        /// </summary>
        /// <seealso cref="https://dapper-tutorial.net/result-multi-mapping"/>
        public async Task<IEnumerable<ProductShipments>> ProductShipmentsMapGetAll()
        {
            const string proc = "[sale].[product_shipments_all_multi_mapping]";

            var lookupPairs = new Dictionary<int, ProductShipments>();

            (await RetailDbSession.Connection
                .QueryAsync(
                    sql: proc,
                    map: MapSelectedShipments(lookupPairs),
                    // splitOn: split resultset by columns from left to right.
                    // Care about columns positions for right mapping to not lose data
                    splitOn: "shipment_id", // also works "product_id, shipment_id"
                    commandType: CommandType.StoredProcedure))
            .AsQueryable();

            return lookupPairs.Values;

            static Func<ProductShipments, Shipment, ProductShipments> MapSelectedShipments(Dictionary<int, ProductShipments> lookupPairs)
            {
                return (ps, sh) =>
                {
                    if (!lookupPairs.TryGetValue(ps.ProductId, out ProductShipments productShipments))
                    {
                        lookupPairs.Add(ps.ProductId, productShipments = ps);
                    }

                    if (sh is not null)
                    {
                        sh.ProductId = ps.ProductId;
                        (productShipments.Shipments ??= new List<Shipment>()).AsList().Add(sh);
                    }

                    return ps;
                };
            }
        }

        /// <summary>
        /// Get all products by table
        /// </summary>
        /// remarks
        /// The result set of this method use Dapper.ColumnMapper nuget for mapping.
        /// But you can use more popular FluentMapper, it is up to you.
        /// <see cref="https://github.com/dturkenk/Dapper.ColumnMapper"/>
        /// <seealso cref="https://github.com/henkmollema/Dapper-FluentMap"/>
        public async Task<IEnumerable<Product>> ProductsTableGet()
        {
            const string table = "[sale].[products]";

            const string connString = "Provider=MSOLEDBSQL;Server=localhost\\MSSQLSERVER01;Database=retail_db;Trusted_Connection=yes";

            // only supported by oledb provider and windows only
            await using var oledbConn = new OleDbConnection(connString);

            return await oledbConn.QueryAsync<Product>(table, commandType: CommandType.TableDirect);
        }

        /// <summary>
        /// Get all products by table to Object. Return table as it is.
        /// </summary>
        public async Task<object> ProductsTableToObjectGet()
        {
            const string table = "[sale].[products]";

            const string connString = "Provider=MSOLEDBSQL;Server=localhost\\MSSQLSERVER01;Database=retail_db;Trusted_Connection=yes";

            // only supported by oledb provider and windows only
            await using var oledbConn = new OleDbConnection(connString);

            // return IEnumerable<dynamic>
            return await oledbConn.QueryAsync(table, commandType: CommandType.TableDirect);
        }

        /// <summary>
        /// Get shipments as json and deserialize
        /// </summary>
        public async Task<IEnumerable<Shipment>> ShipmentsGetJson()
        {
            const string query = "SELECT * FROM [sale].[shipments] FOR JSON PATH;";

            return await RetailDbSession.Connection
                .QueryFromJson<IEnumerable<Shipment>>(query);
        }

        /// <summary>
        /// Get shipments as json and return
        /// </summary>
        public async Task<string> ShipmentsGetDirectlyJson()
        {
            const string query = "SELECT * FROM [sale].[shipments] FOR JSON PATH;";

            return await RetailDbSession.Connection.QueryFromJson(query);
        }

        /// <summary>
        /// Get product shipments as json and deserialize
        /// </summary>
        public async Task<IEnumerable<ProductShipments>> ProductShipmentsGetJson()
        {
            const string proc = "[sale].[product_shipments_get_json]";

            return await RetailDbSession.Connection
                .QueryFromJson<IEnumerable<ProductShipments>>(proc, commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Get product with shipments as json
        /// </summary>
        /// <see cref="TypeHandlers.JsonTypeHandler"/>
        public async Task<IEnumerable<ProductShipments>> ProductShipmentsJsonGet()
        {
            const string proc = "[sale].[product_shipmentsJson_get]";

            // Care - limit to visualize data in shipments column in SSMS is 65536 symbols per row
            // When service (ADO.NET) is used, all data will be returned
            return await RetailDbSession.Connection
                .QueryAsync<ProductShipments>(proc, commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Get product with shipments as json document
        /// </summary>
        /// remarks: ColumnMapping does not work in this scenario
        /// <see cref="TypeHandlers.JsonDocumentTypeHandler"/>
        public async Task<IEnumerable<ProductShipmentsJson>> ProductShipmentsJsonDocumentGet()
        {
            const string proc = "[sale].[product_shipmentsJson_get]";

            return await RetailDbSession.Connection
                .QueryAsync<ProductShipmentsJson>(proc, commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Get all shipments query with transaction
        /// </summary>
        public async Task<IEnumerable<Shipment>> ShipmentsGet(IDbTransaction dbTransaction)
        {
            const string query =
                @"SELECT
                    s.[shipment_id]       AS ShipmentId,
                    s.[product_id]        AS ProductId,
                    s.[quantity]          AS Quantity,
                    s.[is_delete]         AS IsDelete,
                    s.[created_at_utc]    AS CreatedAtUtc
                FROM [sale].[shipments] s";

            return await RetailDbSession.Connection.DbConnect(dbTransaction)
                .QueryAsync<Shipment>(query, transaction: dbTransaction);
        }

        /// <summary>
        /// Get all products proc with transaction
        /// </summary>
        public async Task<IEnumerable<Product>> ProductsGet(IDbTransaction dbTransaction)
        {
            const string proc = "[sale].[products_get]";

            return await RetailDbSession.Connection.DbConnect(dbTransaction)
                .QueryAsync<Product>(proc, commandType: CommandType.StoredProcedure, transaction: dbTransaction);
        }
    }
}
