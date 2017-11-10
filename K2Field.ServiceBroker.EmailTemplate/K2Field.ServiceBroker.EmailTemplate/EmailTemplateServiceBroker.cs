using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using SourceCode.SmartObjects.Services.ServiceSDK;
using SourceCode.SmartObjects.Services.ServiceSDK.Objects;
using SourceCode.SmartObjects.Services.ServiceSDK.Types;
using System.Transactions;
using K2Field.ServiceBroker.EmailTemplate.Constants;

namespace K2Field.ServiceBroker.EmailTemplate
{
    public class EmailTemplateServiceBroker : ServiceAssemblyBase
    {


        public override string GetConfigSection()
        {
            Service.ServiceConfiguration.Add(ServiceConfiguration.ServerName, true, "localhost");
            Service.ServiceConfiguration.Add(Resources.Port, true, "5555");
            Service.ServiceConfiguration.Add(Resources.DelimitedInputIDs, true, "Id1;Id2");
            return base.GetConfigSection();
        }

        public override string DescribeSchema()
        {
            Service.Name = "K2Field.ServiceBroker.EmailTemplate";
            Service.MetaData.DisplayName = "Email Templating";
            Service.MetaData.Description = "A Service Broker that provides email templating functionality for the various functional service objects that aid the implementation of a K2 project.";

            bool requireServiceFolders = false;
            foreach (ServiceObjectBase entry in ServiceObjectClasses)
            {
                if (!string.IsNullOrEmpty(entry.ServiceFolder))
                {
                    requireServiceFolders = true;
                }
            }

            foreach (ServiceObjectBase entry in ServiceObjectClasses)
            {
                List<ServiceObject> serviceObjects = entry.DescribeServiceObjects();
                foreach (ServiceObject so in serviceObjects)
                {
                    Service.ServiceObjects.Create(so);
                    if (requireServiceFolders)
                    {
                        ServiceFolder sf = InitializeServiceFolder(entry.ServiceFolder, entry.ServiceFolder);
                        sf.Add(so);
                    }
                }
            }

            return base.DescribeSchema();
        }


        #region Private Methods
        /// <summary>
        /// Cache of all service object that we have. We load this into a static object in the hope to re-use it as often as possible.
        /// </summary>
        private IEnumerable<ServiceObjectBase> ServiceObjectClasses
        {
            get
            {
                if (_serviceObjects == null)
                {
                    lock (serviceObjectLock)
                    {
                        if (_serviceObjects == null)
                        {
                            _serviceObjects = new List<ServiceObjectBase>
                            {
                                new ManagementWorklistSO(this),
                                new ErrorLogSO(this),
                                new IdentitySO(this),
                                new WorklistSO(this),
                                new OutOfOfficeClientSO(this),
                                new OutOfOfficeSO(this),
                                new ProcessInstanceManagementSO(this),
                                new ProcessInstanceClientSO(this),
                                new RoleManagementSO(this),
                                new ActiveDirectorySO(this),
                                new WorkingHoursConfigurationSO(this),
                                new GroupSO(this),
                                new UserSO(this),
                                new ADOSMOQuerySO(this),
                                new PowerShellVariablesSO(this),
                                new SimplePowerShellSO(this),
                                new DynamicPowerShellSO(this)
                            };

                        }
                    }
                }
                return _serviceObjects;
            }
        }

        /// <summary>
        /// helper property to get the type of the service object, to be able to initialize a specific instance of it.
        /// </summary>
        private Dictionary<string, Type> ServiceObjectToType
        {
            get
            {
                lock (serviceObjectToTypeLock)
                {
                    if (_serviceObjectToType.Count != 0)
                    {
                        return _serviceObjectToType;
                    }

                    _serviceObjectToType = new Dictionary<string, Type>();
                    foreach (ServiceObjectBase soBase in ServiceObjectClasses)
                    {
                        List<ServiceObject> serviceObjs = soBase.DescribeServiceObjects();
                        foreach (ServiceObject so in serviceObjs)
                        {
                            _serviceObjectToType.Add(so.Name, soBase.GetType());
                        }
                    }
                }
                return _serviceObjectToType;
            }
        }
        private ServiceFolder InitializeServiceFolder(string folderName, string description)
        {
            if (string.IsNullOrEmpty(folderName))
            {
                folderName = "Other";
                description = "Other";
            }
            foreach (ServiceFolder sf in Service.ServiceFolders)
            {
                if (string.Compare(sf.Name, folderName) == 0)
                {
                    return sf;
                }
            }
            ServiceFolder newSf = new ServiceFolder(folderName, new MetaData(folderName, description));
            Service.ServiceFolders.Create(newSf);
            return newSf;
        }

        #endregion

        #region Constructor
        /// <summary>
        /// A new instance is called for every new connection that is created to the K2 server. A new instance of this class is not created
        /// when the connection remains open. One connection can be used for multiple things.
        /// </summary>
        public K2NEServiceBroker()
        {
        }
        #endregion

        #region Public overrides for ServiceAssemblyBase
        
        public override void Execute()
        {
            // Value can't be set in K2Connection constructor, because the SmartBroker sets the UserName value after Init
            K2Connection.UserName = Service.ServiceConfiguration.ServiceAuthentication.UserName;

            ServiceObject so = Service.ServiceObjects[0];
            try
            {
                //TODO: improve performance? http://bloggingabout.net/blogs/vagif/archive/2010/04/02/don-t-use-activator-createinstance-or-constructorinfo-invoke-use-compiled-lambda-expressions.aspx

                // This creates an instance of the object responsible to handle the execution.
                // We can't cache the instance itself, as that gives threading issue because the 
                // object can be re-used by the k2 host server for multiple different SMO calls
                // so we always need to know which ServiceObject we actually want to execute and 
                // create an instance first. This is  "late" initalization. We can also not keep a list of 
                // service objects that have been instanciated around in memory as this would be to resource 
                // intensive and slow (as we would constantly initialize all).
                if (so == null || string.IsNullOrEmpty(so.Name))
                {
                    throw new ApplicationException(Resources.SOIsNotSet);
                }
                if (!ServiceObjectToType.ContainsKey(so.Name))
                {
                    throw new ApplicationException(string.Format(Resources.IsNotValidSO, so.Name));
                }
                Type soType = ServiceObjectToType[so.Name];
                object[] constParams = new object[] { this };
                ServiceObjectBase soInstance = Activator.CreateInstance(soType, constParams) as ServiceObjectBase;

                soInstance.Execute();
                ServicePackage.IsSuccessful = true;
            }
            catch (Exception ex)
            {
                StringBuilder error = new StringBuilder();
                error.AppendFormat("Exception.Message: {0}\n", ex.Message);
                error.AppendFormat("Exception.StackTrace: {0}\n", ex.StackTrace);

                Exception innerEx = ex;
                int i = 0;
                while (innerEx.InnerException != null)
                {
                    error.AppendFormat("{0} InnerException.Message: {1}\n", i, innerEx.InnerException.Message);
                    error.AppendFormat("{0} InnerException.StackTrace: {1}\n\n", i, innerEx.InnerException.StackTrace);
                    innerEx = innerEx.InnerException;
                    i++;
                }

                error.AppendLine();
                if (base.Service.ServiceObjects.Count > 0)
                {
                    foreach (ServiceObject executingSo in base.Service.ServiceObjects)
                    {
                        error.AppendFormat("Service Object Name: {0}\n", executingSo.Name);
                        foreach (Method method in executingSo.Methods)
                        {
                            error.AppendFormat("Service Object Methods: {0}\n", method.Name);
                        }

                        foreach (Property prop in executingSo.Properties)
                        {
                            string val = prop.Value as string;
                            if (!string.IsNullOrEmpty(val))
                            {
                                error.AppendFormat("[{0}].{1}: {2}\n", executingSo.Name, prop.Name, val);
                            }
                            else
                            {
                                error.AppendFormat("[{0}].{1}: [String.Empty]\n", executingSo.Name, prop.Name);
                            }
                        }
                    }
                }


                ServicePackage.ServiceMessages.Add(error.ToString(), MessageSeverity.Error);
                ServicePackage.IsSuccessful = false;
            }
        }
        
        public void Init(IServiceMarshalling serviceMarshalling, IServerMarshaling serverMarshaling)
        {
            lock (syncobject)
            {
                if (HostServiceLogger == null)
                {
                    HostServiceLogger = new Logger(serviceMarshalling.GetHostedService(typeof(SourceCode.Logging.ILogger)) as SourceCode.Logging.ILogger);
                    HostServiceLogger.LogDebug("Logger loaded from ServiceMarshalling");
                }

                if (IdentityService == null)
                {
                    IdentityService = serviceMarshalling.GetHostedService(typeof(IIdentityService)) as IIdentityService;
                }
                if (SecurityManager == null)
                {
                    SecurityManager = serverMarshaling.GetSecurityManagerContext();
                }

                K2Connection = new K2Connection(serviceMarshalling, serverMarshaling);
            }
        }

        public override void Extend() { }
        public void Unload()
        {
            HostServiceLogger.Dispose();
        }
        #endregion Public overrides for ServiceAssemblyBase


    }
}
