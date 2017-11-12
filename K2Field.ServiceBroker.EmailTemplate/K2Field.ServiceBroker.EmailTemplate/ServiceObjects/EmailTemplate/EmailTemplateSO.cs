using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using K2Field.ServiceBroker.EmailTemplate;
using SourceCode.SmartObjects.Services.ServiceSDK.Objects;
using K2Field.ServiceBroker.EmailTemplate.Constants;
using K2Field.ServiceBroker.EmailTemplate.Helpers;
using SourceCode.Hosting.Client.BaseAPI;
using SourceCode.SmartObjects.Client;
using SourceCode.SmartObjects.Services.ServiceSDK.Types;
using MetaData = SourceCode.SmartObjects.Services.ServiceSDK.Objects.MetaData;
using MethodType = SourceCode.SmartObjects.Services.ServiceSDK.Types.MethodType;

namespace K2Field.ServiceBroker.EmailTemplate.ServiceObjects.EmailTemplate
{
    public class EmailTemplateSO : ServiceObjectBase
    {
        private string _inputSubject;
        private string _inputBody;
        private PlaceholderItemCollection _placeholders;
        private Dictionary<string, string> _inputIds; 
        private string _pSmoSystemName;
        private string _pSmoListName;
        private string _pNameProperty;
        private string _pAdoNetProperty;

        public EmailTemplateSO(EmailTemplateServiceBroker broker) : base(broker)
        {
            _placeholders = new PlaceholderItemCollection();
            _placeholders.Wrapper = ServiceBroker.Service.ServiceConfiguration[ServiceConfig.PlaceholderWrapper].ToString();
            _inputIds = new Dictionary<string, string>();
            
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
            switch (base.ServiceBroker.Service.ServiceObjects[0].Methods[0].Name)
            {
                case Constants.Methods.GetEmailTemplate:
                    GetEmailTemplate();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void GetEmailTemplate()
        {
            //Getting all the ServiceConfig and Input properties
            _inputSubject = GetStringProperty(Constants.Properties.InputEmailSubject);
            _inputBody = GetStringProperty(Constants.Properties.InputEmailBody);
            _pSmoSystemName =
                ServiceBroker.Service.ServiceConfiguration[ServiceConfig.PlaceholderSmoSystemName].ToString();
            _pSmoListName = ServiceBroker.Service.ServiceConfiguration[ServiceConfig.ListMethodName].ToString();
            _pNameProperty = ServiceBroker.Service.ServiceConfiguration[ServiceConfig.PlaceholderPropertyName].ToString();
            _pAdoNetProperty = ServiceBroker.Service.ServiceConfiguration[ServiceConfig.AdoNetPropertyName].ToString();
            foreach ( var idName in GetInputIds())
            {
                _inputIds.Add(idName, GetStringParameter(idName));
            }
            

            SmartObjectClientServer smoServer = ServiceBroker.K2Connection.GetConnection<SmartObjectClientServer>();
            using (smoServer.Connection)
            {
                var smo = smoServer.GetSmartObject(_pSmoSystemName);
                smo.MethodToExecute = _pSmoListName;
                var dt = smoServer.ExecuteListDataTable(smo);
                //Getting only the placholders, which are used in the EmailSubject/EmailBody
                foreach (DataRow row in dt.Rows)
                {
                    var placeholder = _pWrapper + row[_pNameProperty] + _pWrapper;
                    if (_inputSubject.Contains(placeholder) || _inputBody.Contains(placeholder))
                    {
                        _placeholders.AddItem(row[_pNameProperty].ToString(), row[_pAdoNetProperty].ToString());
                    }
                }
                //Getting the values of the placeholders
                _placeholders.GetAllValues(smoServer, _inputIds);
            }
            //Replacing all the values
            
            var outputSubject = _placeholders.ReplacePlaceholders(_inputSubject);
            var outputBody = _placeholders.ReplacePlaceholders(_inputBody);

            ServiceBroker.Service.ServiceObjects[0].Properties.InitResultTable();
            DataTable results = ServiceBroker.ServicePackage.ResultTable;

            DataRow dr = results.NewRow();
            dr[Constants.Properties.OutputEmailBody] = outputBody;
            dr[Constants.Properties.OutputEmailSubject] = outputSubject;
            results.Rows.Add(dr);
        }
    }
}
