using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ui5.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Ui5.Controllers
{
    public class Supplier
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<Product> Products { get; set; }
    }

    [EnableCors("UI5Policy")]
    public class SuppliersController : ODataController
    {
        private readonly TestDbContext db;
        public SuppliersController(TestDbContext _db)
        {
            db = _db;
        }

        [EnableQuery]
        public IQueryable<Supplier> Get()
        {
            return db.Suppliers;
        }
        [EnableQuery]
        public SingleResult<Supplier> Get([FromODataUri] int key)
        {
            IQueryable<Supplier> result = db.Suppliers.Where(p => p.Id == key);
            return SingleResult.Create(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post(Supplier supplier)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            db.Suppliers.Add(supplier);
            await db.SaveChangesAsync();
            return Created(supplier);
        }

        [HttpPatch]
        public async Task<IActionResult> Patch([FromODataUri] int key, Delta<Supplier> suplier)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var entity = await db.Suppliers.FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }
            suplier.Patch(entity);
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SupplierExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return Updated(entity);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromODataUri] int key, Supplier update)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (key != update.Id)
            {
                return BadRequest();
            }
            db.Entry(update).State = EntityState.Modified;
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SupplierExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return Updated(update);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromODataUri] int key)
        {
            var supplier = await db.Suppliers.FindAsync(key);
            if (supplier == null)
            {
                return NotFound();
            }
            db.Suppliers.Remove(supplier);
            await db.SaveChangesAsync();
            return StatusCode((int)HttpStatusCode.NoContent);
        }

        [EnableQuery]
        public IQueryable<Product> GetProducts([FromODataUri] int key)
        {
            return db.Suppliers.Where(m => m.Id.Equals(key)).SelectMany(m => m.Products);
        }

        private bool SupplierExists(int key)
        {
            return db.Suppliers.Any(p => p.Id == key);
        }

    }
}
