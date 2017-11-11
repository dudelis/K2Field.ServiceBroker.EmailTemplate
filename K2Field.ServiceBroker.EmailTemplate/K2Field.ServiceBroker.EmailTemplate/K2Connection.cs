using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using SourceCode.Hosting.Client.BaseAPI;
using SourceCode.Hosting.Server.Interfaces;
using SourceCode.Workflow.Client;

namespace K2Field.ServiceBroker.EmailTemplate
{
    internal class K2Connection
    {
        private static string _defaultWorkflowServerConnectionString;
        private static int _workflowServerPort;
        private readonly string _sessionConnectionString;
        private ISessionManager _sessionManager;
        private ConnectionSetup sessionWorkflowConnectionSetup;

        public string SessionConnectionString => _sessionConnectionString;
        
        public ISessionManager SessionManager => _sessionManager;
        
        public K2Connection(IServiceMarshalling serviceMarshalling, IServerMarshaling serverMarshaling)
        {
            // These values are static because they won't change on the server.
            // This code will only execute once after K2HostServer starts
            if (string.IsNullOrEmpty(_defaultWorkflowServerConnectionString) || _workflowServerPort == 0)
            {
                var appSettings = ConfigurationManager.AppSettings;
                _defaultWorkflowServerConnectionString = ConfigurationManager.ConnectionStrings["WorkflowServer"].ConnectionString;

                ConnectionSetup workflowConnectionSetup = new ConnectionSetup();
                workflowConnectionSetup.ParseConnectionString(_defaultWorkflowServerConnectionString);

                _workflowServerPort = int.Parse(workflowConnectionSetup.ConnectionParameters[SourceCode.Workflow.Client.ConnectionSetup.ParamKeys.Port]);
            }

            _sessionManager = serverMarshaling.GetSessionManagerContext();

            var sessionCookie = SessionManager.CurrentSessionCookie;
            _sessionConnectionString = EmailTemplateServiceBroker.SecurityManager.GetSessionConnectionString(sessionCookie);
        }



        public string UserName { get; set; }

        private ConnectionSetup SessionWorkflowConnectionSetup
        {
            get
            {
                if (sessionWorkflowConnectionSetup == null && !string.IsNullOrEmpty(this.SessionConnectionString) && _workflowServerPort != 0)
                {
                    sessionWorkflowConnectionSetup = new ConnectionSetup();
                    sessionWorkflowConnectionSetup.ParseConnectionString(this.SessionConnectionString);
                    sessionWorkflowConnectionSetup.ConnectionParameters[SourceCode.Workflow.Client.ConnectionSetup.ParamKeys.Port] = _workflowServerPort.ToString();
                    sessionWorkflowConnectionSetup.ConnectionParameters.Remove(SourceCode.Workflow.Client.ConnectionSetup.ParamKeys.ConnectionString);
                }

                return sessionWorkflowConnectionSetup;
            }
        }

        public T GetConnection<T>() where T : BaseAPI, new()
        {
            var server = new T();
            server.CreateConnection();
            server.Connection.Open(SessionConnectionString);

            return server;
        }

        public Connection GetWorkflowClientConnection()
        {
            Connection connection = new Connection();

            try
            {
                connection.Open(SessionWorkflowConnectionSetup);
                return connection;
            }
            catch (Exception ex)
            {
                if (connection != null)
                {
                    connection.Dispose();
                }

                throw new Exception("Failed to create Connection to K2.", ex);
            }
        }
    }
}
