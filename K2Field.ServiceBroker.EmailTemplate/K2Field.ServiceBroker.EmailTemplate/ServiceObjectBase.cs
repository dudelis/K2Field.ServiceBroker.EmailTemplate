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

        protected EmailTemplateServiceBroker ServiceBroker
        {
            get;
            private set;
        }

        public ServiceObjectBase(EmailTemplateServiceBroker broker)
        {

        }

        public abstract List<ServiceObject> DescribeServiceObjects();
    }
}
