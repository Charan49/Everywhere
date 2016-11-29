using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http.Tracing;
using WebApi.ErrorHelper;

namespace Web.Helper
{
    public sealed class NLogger : ITraceWriter
    {
        private static readonly Logger classLogger = LogManager.GetCurrentClassLogger();

        private static readonly Lazy<Dictionary<TraceLevel, Action<string>>> loggingMap =
            new Lazy<Dictionary<TraceLevel, Action<string>>>(() => new Dictionary<TraceLevel, Action<string>>
                {
                    {TraceLevel.Info, classLogger.Info},
                    {TraceLevel.Debug, classLogger.Debug},
                    {TraceLevel.Error, classLogger.Error},
                    {TraceLevel.Fatal, classLogger.Fatal},
                    {TraceLevel.Warn, classLogger.Warn}
                });

        private Dictionary<TraceLevel, Action<string>> Logger
        {
            get { return loggingMap.Value; }
        }

        public void Trace(HttpRequestMessage request, string category, TraceLevel level, Action<TraceRecord> traceAction)
        {
            if (level != TraceLevel.Off)
            {
                var record = new TraceRecord(request, category, level);
                traceAction(record);
                Log(record);
            }
        }

        private void Log(TraceRecord record)
        {
            var message = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(record.Message))
                message.Append("").Append(record.Message + Environment.NewLine);

            if (record.Request != null)
            {
                if (record.Request.Method != null)
                    message.Append("Method: " + record.Request.Method + Environment.NewLine);

                if (record.Request.RequestUri != null)
                    message.Append("").Append("URL: " + record.Request.RequestUri + Environment.NewLine);

                if (record.Request.Headers != null && record.Request.Headers.Contains("Token") && record.Request.Headers.GetValues("Token") != null && record.Request.Headers.GetValues("Token").FirstOrDefault() != null)
                    message.Append("").Append("Token: " + record.Request.Headers.GetValues("Token").FirstOrDefault() + Environment.NewLine);
            }

            if (!string.IsNullOrWhiteSpace(record.Category))
                message.Append("").Append(record.Category);

            if (!string.IsNullOrWhiteSpace(record.Operator))
                message.Append(" ").Append(record.Operator).Append(" ").Append(record.Operation);

            if (record.Exception != null && !string.IsNullOrWhiteSpace(record.Exception.GetBaseException().Message))
            {
                var exceptionType = record.Exception.GetType();
                message.Append(Environment.NewLine);
                if (exceptionType == typeof(ApiException))
                {
                    var exception = record.Exception as ApiException;
                    if (exception != null)
                    {
                        message.Append("").Append("Error: " + exception.ErrorDescription + Environment.NewLine);
                        message.Append("").Append("Error Code: " + exception.ErrorCode + Environment.NewLine);
                    }
                }
                else if (exceptionType == typeof(ApiBusinessException))
                {
                    var exception = record.Exception as ApiBusinessException;
                    if (exception != null)
                    {
                        message.Append("").Append("Error: " + exception.ErrorDescription + Environment.NewLine);
                        message.Append("").Append("Error Code: " + exception.ErrorCode + Environment.NewLine);
                    }
                }
                else if (exceptionType == typeof(ApiDataException))
                {
                    var exception = record.Exception as ApiDataException;
                    if (exception != null)
                    {
                        message.Append("").Append("Error: " + exception.ErrorDescription + Environment.NewLine);
                        message.Append("").Append("Error Code: " + exception.ErrorCode + Environment.NewLine);
                    }
                }
                else
                    message.Append("").Append("Error: " + record.Exception.GetBaseException().Message + Environment.NewLine);
            }

            Logger[record.Level](Convert.ToString(message) + Environment.NewLine);
        }
        
    }
}