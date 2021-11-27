using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DapperTestApp.DbCommon.DbContracts.Enums;
using DapperTestApp.DbCommon.DbContracts.Inputs;
using DapperTestApp.DbCommon.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DapperTestApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ShipmentProductController : ControllerBase
    {
        private readonly IShipmentProductRepository repository;

        public ShipmentProductController(IShipmentProductRepository repository)
        {
            this.repository = repository;
        }

        [HttpGet("shipments_query_get")]
        public async Task<IActionResult> ShipmentsGet()
        {
            return Ok(await this.repository.ShipmentsQueryGet());
        }

        [HttpGet("shipments_query_w-parameter_get")]
        public async Task<IActionResult> ShipmentsGet(decimal quantity)
        {
            return Ok(await this.repository.ShipmentsGet(quantity));
        }

        [HttpPost("shipments_query_w-parameter_list_get")]
        public async Task<IActionResult> ShipmentsGet(IList<int> productIds)
        {
            return Ok(await this.repository.ShipmentsGet(productIds));
        }

        [HttpGet("products_proc_get")]
        public async Task<IActionResult> ProductsGet()
        {
            return Ok(await this.repository.ProductsProcGet());
        }

        [HttpGet("products_filter_proc_get")]
        public async Task<IActionResult> ProductsByFilterGet(string name_filter, ProductType? product_type)
        {
            return Ok(await this.repository.ProductsByFilterGet(name_filter, product_type));
        }

        [HttpGet("shipment_numbers_proc_get")]
        public async Task<IActionResult> ShipmentNumbersGet(int productId, int numberCount)
        {
            (int Count, decimal Quantity, bool Overweight) =
                await this.repository.ShipmentsNumbers(productId, numberCount);

            return new JsonResult(new { Count, Quantity, Overweight });
        }

        [HttpGet("product_shipments_multiple")]
        public async Task<IActionResult> ProductShipmentsGet(int productId)
        {
            return Ok(await this.repository.ProductShipmentsGet(productId));
        }

        [HttpPost("products_tvp_add")]
        public async Task<IActionResult> ProductsTvpAdd(IList<ProductTvp> products)
        {
            await this.repository.ProductsAdd(products);
            return Ok();
        }

        [HttpPost("shipment_tvp_add")]
        public async Task<IActionResult> ShipmentTvpAdd(ShipmentTvp shipment)
        {
            return Ok(await this.repository.ShipmentAdd(shipment));
        }

        [HttpGet("product_shipments_multi_mapping")]
        public async Task<IActionResult> ProductShipmentsMapGet(int productId)
        {
            return Ok(await this.repository.ProductShipmentsMapGet(productId));
        }

        [HttpGet("product_shipments_all_multi_mapping")]
        public async Task<IActionResult> ProductShipmentsMapGetAll()
        {
            return Ok(await this.repository.ProductShipmentsMapGetAll());
        }

        [HttpGet("products_table_get")]
        public async Task<IActionResult> ProductsTableGet()
        {
            return Ok(await this.repository.ProductsTableGet());
        }

        [HttpGet("products_table_to_object_get")]
        public async Task<IActionResult> ProductsTableToObjectGet()
        {
            return Ok(await this.repository.ProductsTableToObjectGet());
        }

        [HttpGet("shipments_get_json")]
        public async Task<IActionResult> ShipmentsGetJson()
        {
            return Ok(await this.repository.ShipmentsGetJson());
        }

        [HttpGet("shipments_get_directly_json")]
        public async Task<IActionResult> ShipmentsGetDirectlyJson()
        {
            var shipments = await this.repository.ShipmentsGetDirectlyJson();

            return Content(shipments, "application/json", Encoding.UTF8);
        }

        [HttpGet("product_shipments_get_json")]
        public async Task<IActionResult> ProductShipmentsGetJson()
        {
            return Ok(await this.repository.ProductShipmentsGetJson());
        }

        [HttpGet("product_shipmentsJson_get")]
        public async Task<IActionResult> ProductShipmentsJsonGet()
        {
            return Ok(await this.repository.ProductShipmentsJsonGet());
        }

        [HttpGet("product_shipmentsJsonDocument_get")]
        public async Task<IActionResult> ProductShipmentsJsonDocumentGet()
        {
            return Ok(await this.repository.ProductShipmentsJsonDocumentGet());
        }

        [HttpGet("get_all_parallel")]
        public async Task<IActionResult> GetAllParallel()
        {
            // 2 sessions will be opened, tasks execute in parallel
            // when 1st query is too fast, only 1 session will be opened and used by 2nd

            var productsTask = this.repository.ProductsGet();
            var shipmentsTask = this.repository.ShipmentsGet();

            var products = await productsTask;
            var shipments = await shipmentsTask;

            return new JsonResult(new { products, shipments });
        }

        [HttpGet("get_all_with_one_tran")]
        public async Task<IActionResult> GetAllWithOneTran()
        {
            // 1 session will be opened, tasks do not execute in parallel
            // use 2 transactions for each method to execute in parallel

            var cnn = this.repository.RetailDbSession.Connection;
            await cnn.OpenAsync();
            var tran = await cnn.BeginTransactionAsync();

            var productsTask = this.repository.ProductsGet(tran);
            var shipmentsTask = this.repository.ShipmentsGet(tran);

            var products = await productsTask;
            var shipments = await shipmentsTask;

            await tran.CommitAsync();
            // Do not forget to dispose new connection and transaction
            await tran.DisposeAsync();
            await cnn.DisposeAsync();

            return new JsonResult(new { products, shipments });
        }

        [HttpGet("get_all_using_with_one_tran")]
        public async Task<IActionResult> GetAllInUsingWithOneTran()
        {
            // you can use async DbConnection methods and using if you want
            using var cnn = this.repository.RetailDbSession.Connection;
            cnn.Open();
            using var tran = cnn.BeginTransaction();

            var productsTask = this.repository.ProductsGet(tran);
            var shipmentsTask = this.repository.ShipmentsGet(tran);

            var products = await productsTask;
            var shipments = await shipmentsTask;

            tran.Commit();

            return new JsonResult(new { products, shipments });
        }

        [HttpGet("get_all_dbsession_methods_with_one_tran")]
        public async Task<IActionResult> GetAllDbSessionMethodsWithOneTran()
        {
            // run tasks in parallel and open 2 sessions, dispose and commit automatically
            await using var tran =
                await this.repository.RetailDbSession.OpenAndBeginTransactionAsync();

            var productsTask = this.repository.ProductsGet(tran);
            var shipmentsTask = this.repository.ShipmentsGet(tran);

            var products = await productsTask;
            var shipments = await shipmentsTask;

            return new JsonResult(new { products, shipments });
        }
    }
}
