using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using K2Field.ServiceBroker.EmailTemplate.Constants;
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
            string delimitedInputIds =
                ServiceBroker.Service.ServiceConfiguration[ServiceConfig.DelimitedInputIDs] as string;

            var l = delimitedInputIds.Split(';').Select(x => x).ToList();
            
            return l;
        }
    }
}
