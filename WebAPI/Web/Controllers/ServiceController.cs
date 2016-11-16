using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Web.Models.Json;
using System.Data.Entity;
using Web.Models;
using Web;

namespace Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ServiceController : ApiController
    {
        private InterceptDB db = new InterceptDB();
                
        [Route("api/v1/service")]
        [HttpPost]
        public async Task<HttpResponseMessage> AddNewService([FromBody] JService model)
        {
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            

            //Create Service and save to database
            /////////////////////////////////////
            Service service = null;
            
            service = new Service
            {
                ServiceGUID = Guid.NewGuid(),
                Name = model.name,
                AuthMethod = model.authenticationMethod,
                ServiceProviderInfo = model.serviceProviderInfo,
                CreatedDate = DateTime.Now,
                CreatedBy = this.ApiUser().Email,
                IsDeleted = false
            };


            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    //Check if Application Already Exists
                    int count = await db.Services.CountAsync(u => u.Name.ToLower().Trim() == model.name.ToLower().Trim() && u.IsDeleted == false);
                    if (count > 0)
                        return Request.CreateResponse(HttpStatusCode.Conflict, "Service already exists.");

                    db.Services.Add(service);
                    db.SaveChanges();

                    //Complete Transaction
                    transaction.Commit();

                    return Request.CreateResponse(HttpStatusCode.Created);
                }
                catch
                {
                    //Rollback
                    transaction.Rollback();

                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Please try again");
                }
            }
        }

        [Route("api/v1/service/{name}")]
        [HttpGet]        
        public async Task<IEnumerable<JService>> GetService(string name)
        {
            return await db.Services.Where(x => x.Name == name).Select(x => new JService { name = x.Name, authenticationMethod = x.AuthMethod, serviceProviderInfo = x.ServiceProviderInfo }).ToListAsync();
        }

        [Route("api/v1/service")]
        [HttpGet]
        public async Task<IEnumerable<JService>> GetServices()
        {
            return await db.Services.Select(x => new JService { authenticationMethod = x.AuthMethod, serviceProviderInfo = x.ServiceProviderInfo }).ToListAsync();
        }

        [Route("api/v1/service/{name}")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteService(string name)
        {
            Service service = await db.Services.FirstOrDefaultAsync(x => x.Name == name);
            if (service == null)
                return NotFound();

            //Mark as Deleted
            service.IsDeleted = true;
            await db.SaveChangesAsync();

            return Ok();
        }

        [Route("api/v1/service/{name}")]
        [HttpPut]
        public async Task<IHttpActionResult> PutService(string name, [FromBody] JService jService)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            Service service = await db.Services.FirstOrDefaultAsync(x => x.Name == name);
            if (service == null)
                return NotFound();

            //Update Settings
            if (String.Compare(service.ServiceProviderInfo, service.ServiceProviderInfo, true) != 0 || String.Compare(service.AuthMethod, service.AuthMethod, true) != 0)
            {
                service.AuthMethod = jService.authenticationMethod;
                service.ServiceProviderInfo = jService.serviceProviderInfo;
                service.ModifiedBy = this.ApiUser().Email;
                service.ModifiedDate = DateTime.UtcNow;
            }
            
            db.Entry(service).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}
