using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SourceCode.SmartObjects.Services.ServiceSDK.Objects;

namespace K2Field.ServiceBroker.EmailTemplate.ServiceObjects
{
    public class EmailTemplateSO : ServiceObjectBase
    {
        public EmailTemplateSO(EmailTemplateServiceBroker broker) : base(broker)
        {
            
        }

        public override void Execute()
        {
            throw new NotImplementedException();
        }

        public override List<ServiceObject> DescribeServiceObjects()
        {
            throw new NotImplementedException();
        }
    }
}
