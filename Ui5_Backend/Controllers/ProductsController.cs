using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ui5_Backend.Data;

namespace Ui5_Backend.Controllers
{
    
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }

        // New code:    
        [ForeignKey("Supplier")]
        public int? SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; }
    }

    public class ProductRating
    {
        public int ID { get; set; }
        public int Rating { get; set; }
        public int ProductID { get; set; }
        public virtual Product Product { get; set; }
    }


    public class ProductsController : ODataController
    {
        private readonly TestDbContext db;
        public ProductsController(TestDbContext _db)
        {
            db = _db;
        }

        [EnableQuery]
        public IQueryable<Product> Get()
        {
            return db.Products;
        }
        [EnableQuery]
        public SingleResult<Product> Get([FromODataUri] int key)
        {
            IQueryable<Product> result = db.Products.Where(p => p.Id == key);
            return SingleResult.Create(result);
        }

        [HttpGet]
        public IActionResult MostExpensive()
        {
            var product = db.Products.Max(x => x.Price);
            return Ok(product);
        }


        [EnableQuery]
        public SingleResult<Supplier> GetSupplier([FromODataUri] int key)
        {
            var result = db.Products.Where(m => m.Id == key).Select(m => m.Supplier);
            return SingleResult.Create(result);
        }

        private bool ProductExists(int key)
        {
            return db.Products.Any(p => p.Id == key);
        }
    }
}
