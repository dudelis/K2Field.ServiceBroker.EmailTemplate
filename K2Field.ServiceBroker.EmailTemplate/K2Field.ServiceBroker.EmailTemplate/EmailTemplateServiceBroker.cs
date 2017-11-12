﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using SourceCode.SmartObjects.Services.ServiceSDK;
using SourceCode.SmartObjects.Services.ServiceSDK.Objects;
using SourceCode.SmartObjects.Services.ServiceSDK.Types;
using K2Field.ServiceBroker.EmailTemplate.Constants;
using K2Field.ServiceBroker.EmailTemplate.Properties;
using K2Field.ServiceBroker.EmailTemplate.ServiceObjects;
using K2Field.ServiceBroker.EmailTemplate.ServiceObjects.EmailTemplate;
using SourceCode.Hosting.Server.Interfaces;

namespace K2Field.ServiceBroker.EmailTemplate
{
    public class EmailTemplateServiceBroker : ServiceAssemblyBase, IHostableType
    {
        #region Private Properties
        private static readonly object serviceObjectToTypeLock = new object();
        private static readonly object serviceObjectLock = new object();
        private static Dictionary<string, Type> _serviceObjectToType = new Dictionary<string, Type>();
        private List<ServiceObjectBase> _serviceObjects;
        private object syncobject = new object();
        #endregion Private Properties
        #region Internal properties for ServiceObjectBase's child classes.
        internal K2Connection K2Connection { get; private set; }
        internal static ISecurityManager SecurityManager { get; private set; }
        #endregion Internal properties for ServiceObjectBase's child classes.

        #region Public overrides for ServiceAssemblyBase
        public override string GetConfigSection()
        {
            Service.ServiceConfiguration.Add(ServiceConfig.DelimitedInputIDs, true, "Id1;Id2");
            Service.ServiceConfiguration.Add(ServiceConfig.PlaceholderSmoSystemName, true, "SmoSystemName");
            Service.ServiceConfiguration.Add(ServiceConfig.ListMethodName, true, "List");
            Service.ServiceConfiguration.Add(ServiceConfig.PlaceholderPropertyName, true, "Placeholder");
            Service.ServiceConfiguration.Add(ServiceConfig.AdoNetPropertyName, true, "AdoNetQuery");
            Service.ServiceConfiguration.Add(ServiceConfig.PlaceholderWrapper, true, "%");
            return base.GetConfigSection();
        }

        public override string DescribeSchema()
        {
            Service.Name = "K2Field.ServiceBroker.EmailTemplate";
            Service.MetaData.DisplayName = "Email Templating";
            Service.MetaData.Description = "A Service Broker that provides email templating functionality for the various functional service objects that aid the implementation of a K2 project.";

            bool requireServiceFolders = false;
            foreach (var entry in ServiceObjectClasses)
            {
                if (!string.IsNullOrEmpty(entry.ServiceFolder))
                {
                    requireServiceFolders = true;
                }
            }

            foreach (var entry in ServiceObjectClasses)
            {
                var serviceObjects = entry.DescribeServiceObjects();
                foreach (var so in serviceObjects)
                {
                    Service.ServiceObjects.Create(so);
                    if (requireServiceFolders)
                    {
                        var sFolder = InitializeServiceFolder(entry.ServiceFolder, entry.ServiceFolder);
                        sFolder.Add(so);
                    }
                }
            }

            return base.DescribeSchema();
        }

        public override void Execute()
        {
            // Value can't be set in K2Connection constructor, because the SmartBroker sets the UserName value after Init
            K2Connection.UserName = Service.ServiceConfiguration.ServiceAuthentication.UserName;

            var sObject = Service.ServiceObjects[0];
            try
            {
                
                if (sObject == null || string.IsNullOrEmpty(sObject.Name))
                {
                    throw new ApplicationException(Resources.SOIsNotSet);
                }
                if (!ServiceObjectToType.ContainsKey(sObject.Name))
                {
                    throw new ApplicationException(string.Format(Resources.IsNotValidSO, sObject.Name));
                }
                var soType = ServiceObjectToType[sObject.Name];
                var constParams = new object[] { this };
                var soInstance = Activator.CreateInstance(soType, constParams) as ServiceObjectBase;

                soInstance.Execute();
                ServicePackage.IsSuccessful = true;
            }
            catch (Exception ex)
            {
                var error = new StringBuilder();
                error.AppendFormat("Exception.Message: {0}\n", ex.Message);
                error.AppendFormat("Exception.StackTrace: {0}\n", ex.StackTrace);

                var innerEx = ex;
                int i = 0;
                while (innerEx.InnerException != null)
                {
                    error.AppendFormat("{0} InnerException.Message: {1}\n", i, innerEx.InnerException.Message);
                    error.AppendFormat("{0} InnerException.StackTrace: {1}\n\n", i, innerEx.InnerException.StackTrace);
                    innerEx = innerEx.InnerException;
                    i++;
                }
                error.AppendLine();
                if (Service.ServiceObjects.Count > 0)
                {
                    foreach (var executingSo in Service.ServiceObjects)
                    {
                        error.AppendFormat("Service Object Name: {0}\n", executingSo.Name);
                        foreach (var method in executingSo.Methods)
                        {
                            error.AppendFormat("Service Object Methods: {0}\n", method.Name);
                        }

                        foreach (var prop in executingSo.Properties)
                        {
                            var val = prop.Value as string;
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
                if (SecurityManager == null)
                {
                    SecurityManager = serverMarshaling.GetSecurityManagerContext();
                }
                K2Connection = new K2Connection(serviceMarshalling, serverMarshaling);
            }
        }

        public override void Extend() { }
        public void Unload() { }
        #endregion Public overrides for ServiceAssemblyBase

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
                                new EmailTemplateSO(this)
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
                        var sObjects = soBase.DescribeServiceObjects();
                        foreach (ServiceObject so in sObjects)
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
            foreach (var sFolder in Service.ServiceFolders)
            {
                if (string.Compare(sFolder.Name, folderName) == 0)
                {
                    return sFolder;
                }
            }
            var newSFolder = new ServiceFolder(folderName, new MetaData(folderName, description));
            Service.ServiceFolders.Create(newSFolder);
            return newSFolder;
        }
        #endregion
    }
}
