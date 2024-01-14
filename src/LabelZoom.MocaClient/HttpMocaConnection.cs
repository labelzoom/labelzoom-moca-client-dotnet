using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security;
using System.Threading.Tasks;
using System.Threading;
using System.Xml;
using LabelZoom.MocaClient.Exceptions;
using System.Reflection;

namespace LabelZoom.MocaClient
{
    public class HttpMocaConnection : MocaConnection
    {
        private readonly HttpClient httpClient;

        public HttpMocaConnection(string connectionString, bool suppressCertificateWarnings = false)
        {
            ConnectionString = connectionString;

            string userAgent = "LabelZoom.MocaClient/" + Assembly.GetExecutingAssembly().GetName().Version.ToString();
            if (suppressCertificateWarnings)
            {
                var certIgnoreHandler = new HttpClientHandler();
                certIgnoreHandler.ServerCertificateCustomValidationCallback = (a, b, c, d) => true;
                httpClient = new HttpClient(certIgnoreHandler);
            }
            else
            {
                httpClient = new HttpClient();
            }
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
            httpClient.Timeout = TimeSpan.FromSeconds(ConnectionTimeout);
        }

        public override string ConnectionString { get; set; }

        private async Task<HttpResponseMessage> PostXml(string xml, CancellationToken token)
        {
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, ConnectionString))
            {
                using (HttpContent content = new MocaRequestContent(xml))
                {
                    request.Content = content;
                    HttpResponseMessage response = await httpClient.SendAsync(request, token);
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"HTTP error {response.StatusCode}");
                    }
                    return response;
                }
            }
        }

        public async Task<bool> Login(string userId, string password)
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                return await Login(userId, password, cts.Token);
            }
        }
        public async Task<bool> Login(string userId, string password, CancellationToken token)
        {
            try
            {
                using (HttpResponseMessage response = await PostXml($"<moca-request autocommit=\"True\"><environment><var name=\"USR_ID\" value=\"RFAUST\"/></environment><query>login user where usr_id = &apos;{SecurityElement.Escape(userId)}&apos; and usr_pswd = &apos;{SecurityElement.Escape(password)}&apos;</query></moca-request>", token))
                {
                    MocaResponse mocaResponse = MocaResponse.FromXml(await response.Content.ReadAsStringAsync());
                    if (mocaResponse.IsError)
                    {
                        throw MocaExceptionFactory.Generate(mocaResponse.StatusCode, mocaResponse.StatusMessage);
                    }
                    sessionKey = mocaResponse.ResponseData?.Rows[0]["session_key"].ToString();
                    this.userId = userId;
                }
            }
            catch (TaskCanceledException ex)
            {
                // Don't log TaskCanceledException, just re-throw it
                throw ex;
            }
            catch (MocaException ex)
            {
                //LOG.Warn("MOCA Exception in MocaConnection.Login", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                //LOG.Error("Unhandled exception in MocaConnection.Login", ex);
                throw ex;
            }
            return true;
        }

        public void LogOut()
        {
            if (sessionKey != null)
            {
                Execute($"logout user where usr_id = '{userId}'").Wait();
            }
            sessionKey = null;
            userId = null;
        }

        public async Task<MocaResponse> Execute(string command, IDictionary<string, object> context = null)
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                return await Execute(command, cts.Token, context);
            }
        }
        public async Task<MocaResponse> Execute(string command, CancellationToken token, IDictionary<string, object> context = null)
        {
            if (sessionKey == null)
            {
                throw new InvalidOperationException("Not logged in");
            }

            try
            {
                XmlDocument doc = GetXmlForRequest(command, context);
                using (HttpResponseMessage response = await PostXml(doc.OuterXml, token))
                {
                    MocaResponse mocaResponse = MocaResponse.FromXml(await response.Content.ReadAsStringAsync());
                    if (mocaResponse.IsError)
                    {
                        throw MocaExceptionFactory.Generate(mocaResponse.StatusCode, mocaResponse.StatusMessage);
                    }
                    return mocaResponse;
                }
            }
            catch (TaskCanceledException ex)
            {
                // Don't log TaskCanceledException, just re-throw it
                throw ex;
            }
            catch (MocaException ex)
            {
                //LOG.Warn("MOCA Exception in MocaConnection.Execute", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                //LOG.Error($"Unhandled exception in MocaConnection.Execute", ex);
                throw ex;
            }
        }

        /// <summary>
        /// Load environment variables from connection profile
        /// </summary>
        /// <param name="vars"></param>
        public void LoadEnvironmentVariables(IDictionary<string, string> vars)
        {
            if (vars == null)
            {
                return;
            }
            foreach (KeyValuePair<string, string> kvp in vars)
            {
                if ("SESSION_KEY".Equals(kvp.Key))
                {
                    throw new InvalidOperationException("Tried to set protected environment variable");
                }
                environment.Add(kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// Build MOCA XML request
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private XmlDocument GetXmlForRequest(string query, IDictionary<string, object> context = null)
        {
            XmlDocument doc = new XmlDocument();

            XmlElement mocaNode = doc.CreateElement(string.Empty, "moca-request", string.Empty);
            mocaNode.SetAttribute("autocommit", "True");
            mocaNode.AppendChild(GetEnvironmentNode(doc));

            if (context != null && context.Count > 0)
            {
                XmlElement contextNode = doc.CreateElement(string.Empty, "context", string.Empty);

                foreach (KeyValuePair<string, object> kvp in context)
                {
                    XmlElement variableNode = doc.CreateElement(string.Empty, "field", string.Empty);
                    variableNode.SetAttribute("name", kvp.Key);
                    variableNode.SetAttribute("oper", "EQ");

                    XmlText? variableText = null;
                    if (kvp.Value is string)
                    {
                        variableNode.SetAttribute("type", "STRING");
                    }
                    else if (kvp.Value is int)
                    {
                        variableNode.SetAttribute("type", "INTEGER");
                    }
                    else if (kvp.Value is float || kvp.Value is double)
                    {
                        variableNode.SetAttribute("type", "DOUBLE");
                    }
                    else if (kvp.Value is bool b)
                    {
                        variableNode.SetAttribute("type", "BOOLEAN");
                        variableText = doc.CreateTextNode(b ? "true" : "false");
                    }
                    else if (kvp.Value is DateTime dt)
                    {
                        variableNode.SetAttribute("type", "DATETIME");
                        variableText = doc.CreateTextNode(dt.ToString(MOCA_DATE_FORMAT));
                    }
                    else
                    {
                        throw new ArgumentException("Unrecognized parameter type");
                    }

                    // If text not set manually, set it automatically using object.ToString()
                    if (variableText == null)
                    {
                        variableText = doc.CreateTextNode(kvp.Value.ToString());
                    }
                    variableNode.AppendChild(variableText);

                    contextNode.AppendChild(variableNode);
                }
                mocaNode.AppendChild(contextNode);
            }

            XmlElement queryNode = doc.CreateElement(string.Empty, "query", string.Empty);
            XmlText queryText = doc.CreateTextNode(query);
            queryNode.AppendChild(queryText);
            mocaNode.AppendChild(queryNode);

            doc.AppendChild(mocaNode);
            return doc;
        }

        /// <summary>
        /// Build environment node for MOCA XML request
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private XmlElement GetEnvironmentNode(XmlDocument doc)
        {
            XmlElement envNode = doc.CreateElement(string.Empty, "environment", string.Empty);

            IDictionary<string, string?> vars = new SortedDictionary<string, string?>();
            foreach (KeyValuePair<string, string> kvp in environment)
            {
                vars.Add(kvp.Key, kvp.Value);
            }
            vars.Add("SESSION_KEY", sessionKey);
            if (!vars.ContainsKey("USR_ID"))
            {
                vars.Add("USR_ID", userId);
            }
            foreach (KeyValuePair<string, string?> kvp in vars)
            {
                XmlElement varNode = doc.CreateElement(string.Empty, "var", string.Empty);
                varNode.SetAttribute("name", kvp.Key);
                varNode.SetAttribute("value", kvp.Value);
                envNode.AppendChild(varNode);
            }

            return envNode;
        }

        #region Dispose Pattern
        private bool disposedValue;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    httpClient.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~HttpMocaConnection()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public override void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
