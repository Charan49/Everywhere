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
using System.Web.Http.Cors;
using WebApi.ErrorHelper;

namespace Web.Controllers
{
    [Authorize]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ServiceController : ApiController
    {
        private InterceptDB db = new InterceptDB();
                
        [Route("api/v1/service")]
        [HttpPost]
        public async Task<HttpResponseMessage> AddNewService([FromBody] JService model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                throw new ApiException() { ErrorCode = (int)HttpStatusCode.BadRequest, ErrorDescription = "Bad Request..." };
            }
            

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
                    {
                        return Request.CreateResponse(HttpStatusCode.Conflict, "Service already exists.");
                        throw new ApiDataException(1002, "Service already exists.", HttpStatusCode.Conflict);
                    }

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
                    throw new ApiDataException(1002, "Please try again.", HttpStatusCode.InternalServerError);
                }
            }
        }

        [Route("api/v1/service/{name}")]
        [HttpGet]        
        public async Task<IEnumerable<JService>> GetService(string name)
        {
          
            var services = await db.Services.Where(x => x.Name == name && x.IsDeleted == false).Select(x => new JService { ID=x.ServiceGUID, name = x.Name, authenticationMethod = x.AuthMethod, serviceProviderInfo = x.ServiceProviderInfo, IsTestUsersExists= false, }).ToListAsync();
            
            return services;
            throw new ApiDataException(1002, "Service not found.", HttpStatusCode.NotFound);

        }

        [Route("api/v1/service")]
        [HttpGet]
        public async Task<IEnumerable<JService>> GetServices()
        {
            var rUser = this.ApiUser().RUser;
            bool IsTestUsers = true;
            var testUsers = await db.TestUsers.FirstOrDefaultAsync(x => x.UserGUID == rUser.SubjectID && x.IsDeleted == false && x.IsLinked == false);
            if (testUsers != null)
            {
                if (string.IsNullOrEmpty(testUsers.FaceBookID))
                {
                    IsTestUsers = false;
                }
            }
            else { IsTestUsers = false; }

            var ret=await db.Services.Where(x => x.IsDeleted == false).Select(x => new JService { name = x.Name, ID = x.ServiceGUID, authenticationMethod = x.AuthMethod, serviceProviderInfo = x.ServiceProviderInfo, IsDeleted=x.IsDeleted.ToString(), IsTestUsersExists=IsTestUsers }).ToListAsync();
            return ret;
            throw new ApiDataException(1002, "Service not found.", HttpStatusCode.NotFound);
        }

        [Route("api/v1/service/{name}")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteService(string name)
        {
            Service service = await db.Services.FirstOrDefaultAsync(x => x.Name == name);
            if (service == null)
            {
                return NotFound();
                throw new ApiDataException(1002, "Service not found.", HttpStatusCode.NotFound);
            }

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
            {
                return BadRequest(ModelState);
                throw new ApiException() { ErrorCode = (int)HttpStatusCode.BadRequest, ErrorDescription = "Bad Request..." };
            }


            Service service = await db.Services.FirstOrDefaultAsync(x => x.Name == name);
            if (service == null)
            {
                return NotFound();
                throw new ApiDataException(1002, "Service not found.", HttpStatusCode.NotFound);
            }

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
