﻿using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Web.Models;
using System.Data.Entity;
using Web.Helper;
using Web.Models.Json;
using System.Threading.Tasks;
using System.Web.Http.Cors;

namespace Web.Controllers
{
    [Authorize]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ServiceMembershipController : ApiController
    {
        private InterceptDB db = new InterceptDB();

        [Route("api/v1/service_membership/{id}")]
        [HttpPost]
        public async Task<HttpResponseMessage> AddServiceMembership([FromBody] JServiceUpdate model)    //Service GUID
        {
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            Guid ServiceID;
            Guid.TryParse(model.id.ToString().ToUpper(), out ServiceID);
            //Check if Service is Present
            int count = await db.Services.CountAsync(x => x.ServiceGUID == ServiceID && x.IsDeleted == false);
            if (count == 0)
                return Request.CreateResponse(HttpStatusCode.NotFound);
            var rUser = this.ApiUser().RUser;
            //Check if the Service Entry Already Exists
            var entry = await db.UserServices.FirstOrDefaultAsync(x => x.ServiceGUID == ServiceID && x.UserGUID == rUser.SubjectID);

            if (entry == null)
            {
                var newUserService = new UserService
                {
                    AccessID = Guid.NewGuid(),
                    ServiceGUID = model.id,
                    AccessToken = model.accessToken,
                    TokenExpiration = model.tokenExpiresAt,
                    UserGUID = this.ApiUser().RUser.SubjectID,
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    CreatedBy = this.ApiUser().RUser.SubjectID.ToString()
                };

                db.UserServices.Add(newUserService);
            }
            else
            {
                entry.IsDeleted = false;
            }

            //Save
            await db.SaveChangesAsync();

            return Request.CreateResponse(HttpStatusCode.Created);
        }


        [Route("api/v1/service_membership/{id}")]
        [HttpDelete]
        public async Task<IHttpActionResult> RemoveServiceMembership(Guid id)
        {
            var ruser = this.ApiUser().RUser;

            var entry = await db.UserServices.FirstOrDefaultAsync(x => x.UserGUID == ruser.SubjectID && x.ServiceGUID == id && x.IsDeleted == false);

            if (entry == null)
                return NotFound();

            entry.IsDeleted = true;
            db.Entry(entry).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return Ok();
        }

        [Route("api/v1/service_membership/{id}")]
        [HttpPut]
        public async Task<IHttpActionResult> UpdateServiceMembership(Guid id, JServiceUpdate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ruser = this.ApiUser().RUser;

            var entry = await db.UserServices.FirstOrDefaultAsync(x => x.UserGUID == ruser.SubjectID && x.ServiceGUID == id && x.IsDeleted == false);

            if (entry == null)
                return NotFound();

            entry.AccessToken = model.accessToken;
            entry.TokenExpiration = model.tokenExpiresAt;

            db.Entry(entry).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return Ok();
        }


        [Route("api/v1/service_membership")]
        [HttpGet]
        public async Task<IEnumerable<JService>> GetServiceMembership()
        {
            var ruser = this.ApiUser().RUser;
            var ret= await db.UserServices.Where(x => x.UserGUID == ruser.SubjectID && x.IsDeleted == false).Select(x => new JService { name = x.Service.Name, ID = x.ServiceGUID, authenticationMethod = x.Service.AuthMethod, serviceProviderInfo = x.Service.ServiceProviderInfo }).ToListAsync();
            return ret;
        }

        private string GetHeaderValue(string header)
        {
            return Request.Headers.GetValues(header).FirstOrDefault();
        }
    }
}
