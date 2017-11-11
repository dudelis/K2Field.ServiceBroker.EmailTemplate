using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SourceCode.SmartObjects.Services.ServiceSDK.Objects;
using K2Field.ServiceBroker.EmailTemplate.Constants;
using K2Field.ServiceBroker.EmailTemplate.Helpers;
using SourceCode.SmartObjects.Services.ServiceSDK.Types;

namespace K2Field.ServiceBroker.EmailTemplate.ServiceObjects
{
    public class EmailTemplateSO : ServiceObjectBase
    {
        public EmailTemplateSO(EmailTemplateServiceBroker broker) : base(broker)
        {
            
        }
        public override List<ServiceObject> DescribeServiceObjects()
        {
            var so = new ServiceObject()
            {
                Name = "EmailTemplateSO",
                MetaData = new MetaData("EmailTemplateServiceObject", "Service Object for creating templates in the Emails"),
                Active = true
            };
            so.Properties.Add(Helper.CreateProperty(Constants.Properties.InputEmailSubject, "Input subject of the Email", SoType.Memo));
            so.Properties.Add(Helper.CreateProperty(Constants.Properties.InputEmailBody, "Input body of the email", SoType.Memo));
            so.Properties.Add(Helper.CreateProperty(Constants.Properties.OutputEmailBody, "Output body of the email", SoType.Memo));
            so.Properties.Add(Helper.CreateProperty(Constants.Properties.OutputEmailSubject, "Output body of the email", SoType.Memo));

            Method mGetEmailTemplate = Helper.CreateMethod(Constants.Methods.GetEmailTemplate, "Returns the Email Template with changed placholders", MethodType.Execute);
            mGetEmailTemplate.InputProperties.Add(Constants.Properties.InputEmailBody);
            mGetEmailTemplate.InputProperties.Add(Constants.Properties.InputEmailBody);
            mGetEmailTemplate.ReturnProperties.Add(Constants.Properties.OutputEmailBody);
            mGetEmailTemplate.ReturnProperties.Add(Constants.Properties.OutputEmailSubject);
            mGetEmailTemplate.MethodParameters = Helper.GetMethodParamaters(GetInputIds());
            so.Methods.Add(mGetEmailTemplate);

            return new List<ServiceObject> {so};
            
        }

        public override void Execute()
        {
            throw new NotImplementedException();
        }

       
    }
}
