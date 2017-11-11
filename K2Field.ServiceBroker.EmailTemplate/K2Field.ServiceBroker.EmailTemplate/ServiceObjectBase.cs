using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
