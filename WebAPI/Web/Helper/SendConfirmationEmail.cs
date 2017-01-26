using Exceptions;
using SendGrid;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Twilio;
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

                    throw new ApiException() { ErrorCode = (int)HttpStatusCode.InternalServerError, ErrorDescription = "Bad Request...  " + ex.InnerException };
                }
            }
            else
            {
                return Task.FromResult(0);
                throw new ApiException() { ErrorCode = (int)HttpStatusCode.Unauthorized, ErrorDescription = "Bad Request..." };
            }
        }
    }
    public static class SendEmail
    {
        public static Task sendMail(MailMessage message)
        {


            string emailAccount = ConfigurationManager.AppSettings["mailAccount"].ToString();
            string emailAccountpassword = ConfigurationManager.AppSettings["mailPassword"].ToString();
            string smtp = ConfigurationManager.AppSettings["smtpAddress"].ToString();
            string port = ConfigurationManager.AppSettings["smtpport"].ToString();

            MailMessage sendmessage = new MailMessage(emailAccount, message.To.ToString());
            sendmessage.Subject = message.Subject;
            sendmessage.IsBodyHtml = true;
            sendmessage.Body = message.Body;
            SmtpClient client = new SmtpClient(smtp, int.Parse(port));
            // Credentials are necessary if the server requires the client 
            // to authenticate before it will send e-mail on the client's behalf.
            System.Net.NetworkCredential basicAuthenticationInfo = new
            System.Net.NetworkCredential(emailAccount, emailAccountpassword);
            client.Credentials = basicAuthenticationInfo;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;

            try
            {
                client.Send(sendmessage);
            }
            catch (InvalidApiRequestException ex)
            {
                var detalle = new StringBuilder();

                detalle.Append("ResponseStatusCode: " + ex.ResponseStatusCode + ".   ");
                for (int i = 0; i < ex.Errors.Count(); i++)
                {
                    detalle.Append(" -- Error #" + i.ToString() + " : " + ex.Errors[i]);
                }

                throw new ApiException() { ErrorCode = (int)HttpStatusCode.InternalServerError, ErrorDescription = "Bad Request...  " + ex.InnerException };
            }
            return Task.FromResult(0);


        }
    }

    public static class SMS
    {
        public static Task SendSMS(string cellNumber, string message)
        {
            string twilioSID = ConfigurationManager.AppSettings["twilioSID"].ToString();
            string twilioAuthToken = ConfigurationManager.AppSettings["twilioAuthToken"].ToString();
            string twilioPhoneNumber = ConfigurationManager.AppSettings["twilioPhoneNumber"].ToString();

            var client = new TwilioRestClient(twilioSID, twilioAuthToken);
            
            try
            {
                var result = client.SendMessage(twilioPhoneNumber, cellNumber, "Your Everywhere verification code is  " + message);
            }
            catch (InvalidApiRequestException ex)
            {
                var detalle = new StringBuilder();

                detalle.Append("ResponseStatusCode: " + ex.ResponseStatusCode + ".   ");
                for (int i = 0; i < ex.Errors.Count(); i++)
                {
                    detalle.Append(" -- Error #" + i.ToString() + " : " + ex.Errors[i]);
                }

                throw new ApiException() { ErrorCode = (int)HttpStatusCode.InternalServerError, ErrorDescription = "Bad Request...  " + ex.InnerException };
            }
            return Task.FromResult(0);
        }
    }
}