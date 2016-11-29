using Exceptions;
using SendGrid;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using WebApi.ErrorHelper;

namespace Web.Helper
{
    public static class SendConfirmationEmail
    {
        public static Task sendMail(SendGridMessage message)
        {


            var credentials = new NetworkCredential(
                       ConfigurationManager.AppSettings["mailAccount"],
                       ConfigurationManager.AppSettings["mailPassword"]
                       );

            // Create a Web transport for sending email.
            var transportWeb = new SendGrid.Web(credentials);

            // Send the email.
            if (transportWeb != null)
            {
                try
                {
                    return transportWeb.DeliverAsync(message);
                }
                catch (InvalidApiRequestException ex)
                {
                    var detalle = new StringBuilder();

                    detalle.Append("ResponseStatusCode: " + ex.ResponseStatusCode + ".   ");
                    for (int i = 0; i < ex.Errors.Count(); i++)
                    {
                        detalle.Append(" -- Error #" + i.ToString() + " : " + ex.Errors[i]);
                    }

                    throw new ApiException() { ErrorCode = (int)HttpStatusCode.InternalServerError, ErrorDescription = "Bad Request...  "+ ex.InnerException };
                }
            }
            else
            {
                return Task.FromResult(0);
                throw new ApiException() { ErrorCode = (int)HttpStatusCode.Unauthorized, ErrorDescription = "Bad Request..." };
            }
        }
    }
}