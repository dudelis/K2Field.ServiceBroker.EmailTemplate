using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using SourceCode.Data.SmartObjectsClient;
using SourceCode.Hosting.Client.BaseAPI;
using SourceCode.Hosting.Server.Interfaces;
using SourceCode.Workflow.Client;

namespace K2Field.ServiceBroker.EmailTemplate
{
    internal class K2Connection
    {
        private readonly string _sessionConnectionString;
        private ISessionManager _sessionManager;
        
        public string SessionConnectionString => _sessionConnectionString;
        
        public ISessionManager SessionManager => _sessionManager;
        public string UserName { get; set; }

        public K2Connection(IServiceMarshalling serviceMarshalling, IServerMarshaling serverMarshaling)
        {
            _sessionManager = serverMarshaling.GetSessionManagerContext();
            var sessionCookie = SessionManager.CurrentSessionCookie;
            _sessionConnectionString = EmailTemplateServiceBroker.SecurityManager.GetSessionConnectionString(sessionCookie);
        }
        public T GetConnection<T>() where T : BaseAPI, new()
        {
            var server = new T();
            server.CreateConnection();
            server.Connection.Open(SessionConnectionString);
            return server;
        }

        public string GetSOConnectionString()
        {
            SCConnectionStringBuilder scConStr = new SCConnectionStringBuilder(_sessionConnectionString);
            SOConnectionStringBuilder soConStr = new SOConnectionStringBuilder()
            {
                Server = scConStr.Host,
                Port = Convert.ToInt32(scConStr.Port)
            };
            return soConStr.ConnectionString;
        }
    }
}
