using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using K2Field.ServiceBroker.EmailTemplate.Constants;
using K2Field.ServiceBroker.EmailTemplate.Properties;
using SourceCode.SmartObjects.Services.ServiceSDK.Objects;

namespace K2Field.ServiceBroker.EmailTemplate
{
    public abstract class ServiceObjectBase
    {
        public virtual string ServiceFolder => string.Empty;
        

        protected EmailTemplateServiceBroker ServiceBroker
        {
            get;
            private set;
        }

        protected ServiceObjectBase(EmailTemplateServiceBroker broker)
        {
            ServiceBroker = broker;

        }

        public abstract List<ServiceObject> DescribeServiceObjects();
        public abstract void Execute();

        protected List<string> GetInputIds()
        {
            var delimitedInputIds =
                ServiceBroker.Service.ServiceConfiguration[ServiceConfig.DelimitedInputIDs] as string;

            var l = delimitedInputIds.Split(';').Select(x => x).ToList();
            
            return l;
        }
        protected string GetStringProperty(string name, bool isRequired = false)
        {
            var p = ServiceBroker.Service.ServiceObjects[0].Properties[name];
            if (p == null)
            {
                if (isRequired)
                    throw new ArgumentException(string.Format(Resources.RequiredPropertyNotFound, name));
                return string.Empty;
            }
            var val = p.Value as string;
            if (isRequired && string.IsNullOrEmpty(val))
                throw new ArgumentException(string.Format(Resources.RequiredPropertyIsEmpty, name));

            return val;
        }
        protected string GetStringParameter(string name, bool isRequired = false)
        {
            MethodParameter p = ServiceBroker.Service.ServiceObjects[0].Methods[0].MethodParameters[name];
            if (p == null)
            {
                if (isRequired)
                    throw new ArgumentException(string.Format(Resources.RequiredParameterNotFound, name));
                return string.Empty;
            }
            string val = p.Value as string;
            if (isRequired && string.IsNullOrEmpty(val))
                throw new ArgumentException(string.Format(Resources.RequiredParameterIsEmpty, name));

            return val;
        }
    }
}
