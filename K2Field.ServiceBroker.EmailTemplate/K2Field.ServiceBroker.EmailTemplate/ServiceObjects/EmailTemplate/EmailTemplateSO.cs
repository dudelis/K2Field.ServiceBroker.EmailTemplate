﻿using System;
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
        private PlaceholderItemCollection _placeholders;
        private Dictionary<string, string> _inputIds; 
        private readonly string _pSmoSystemName;
        private readonly string _pSmoListName;
        private readonly string _pNameProperty;
        private readonly string _pAdoNetProperty;
        private readonly string _pReturnProperty;

        public EmailTemplateSO(EmailTemplateServiceBroker broker) : base(broker)
        {
            _placeholders = new PlaceholderItemCollection()
            {
                Wrapper = ServiceBroker.Service.ServiceConfiguration[ServiceConfig.PlaceholderWrapperSymbol].ToString()
            };
            _inputIds = new Dictionary<string, string>();
            _pSmoSystemName =
                ServiceBroker.Service.ServiceConfiguration[ServiceConfig.PlaceholderSmoSystemName].ToString();
            _pSmoListName = ServiceBroker.Service.ServiceConfiguration[ServiceConfig.ListMethodName].ToString();
            _pNameProperty = ServiceBroker.Service.ServiceConfiguration[ServiceConfig.PlaceholderPropertyName].ToString();
            _pAdoNetProperty = ServiceBroker.Service.ServiceConfiguration[ServiceConfig.AdoNetPropertyName].ToString();
            _pReturnProperty = ServiceBroker.Service.ServiceConfiguration[ServiceConfig.ReturnProperty].ToString();

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
            so.Properties.Add(Helper.CreateProperty(Constants.Properties.Placeholder, "Placeholder", SoType.Text));
            so.Properties.Add(Helper.CreateProperty(Constants.Properties.PlaceholderWithWrapper, "Placeholder with Wrapper", SoType.Text));

            var mGetEmailTemplate = Helper.CreateMethod(Constants.Methods.GetEmailTemplate, "Returns the Email Template with changed placholders", MethodType.Execute);
            mGetEmailTemplate.InputProperties.Add(Constants.Properties.InputEmailBody);
            mGetEmailTemplate.InputProperties.Add(Constants.Properties.InputEmailSubject);
            mGetEmailTemplate.ReturnProperties.Add(Constants.Properties.OutputEmailBody);
            mGetEmailTemplate.ReturnProperties.Add(Constants.Properties.OutputEmailSubject);
            mGetEmailTemplate.MethodParameters = Helper.GetMethodParamaters(GetInputIds(), MethodParameterType.Dynamic);
            MethodParameters staticParams = Helper.GetMethodParamaters(GetStaticPlaceholders(), MethodParameterType.Static);
            foreach (var param in staticParams)
            {
                mGetEmailTemplate.MethodParameters.Create(param);
            }
            so.Methods.Add(mGetEmailTemplate);

            var mListPlaceholders = Helper.CreateMethod(Constants.Methods.ListPlaceholders, "Returns a list of all placeholders", MethodType.List);
            mListPlaceholders.ReturnProperties.Add(Constants.Properties.Placeholder);
            mListPlaceholders.ReturnProperties.Add(Constants.Properties.PlaceholderWithWrapper);
            so.Methods.Add(mListPlaceholders);
            return new List<ServiceObject> {so};
            
        }

        public override void Execute()
        {
            switch (base.ServiceBroker.Service.ServiceObjects[0].Methods[0].Name)
            {
                case Constants.Methods.GetEmailTemplate:
                    GetEmailTemplate();
                    break;
                case Constants.Methods.ListPlaceholders:
                    ListPlaceholders();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void GetEmailTemplate()
        {
            //Getting all the ServiceConfig and Input properties
            var _inputSubject = GetStringProperty(Constants.Properties.InputEmailSubject) ?? string.Empty;
            var _inputBody = GetStringProperty(Constants.Properties.InputEmailBody) ?? string.Empty;
            
            foreach ( var idName in GetInputIds())
            {
                _inputIds.Add(idName, GetStringParameter(idName));
            }

            if (!string.IsNullOrEmpty(_pSmoSystemName))
            {

                SmartObjectClientServer smoServer = ServiceBroker.K2Connection.GetConnection<SmartObjectClientServer>();
                using (smoServer.Connection)
                {
                    var smo = smoServer.GetSmartObject(_pSmoSystemName);
                    smo.MethodToExecute = _pSmoListName;
                    var dt = smoServer.ExecuteListDataTable(smo);
                    foreach (DataRow row in dt.Rows)
                    {
                        var placeholder = _placeholders.Wrapper + row[_pNameProperty] + _placeholders.Wrapper;
                        //Getting only the placholders, which are used in the EmailSubject/EmailBody
                        if (_inputSubject.Contains(placeholder) || _inputBody.Contains(placeholder))
                        {
                            _placeholders.AddItem(row[_pNameProperty].ToString(), row[_pAdoNetProperty].ToString(), row[_pReturnProperty].ToString());
                        }
                    }
                }
                //Getting the values of the placeholders
                _placeholders.GetAllValues(_inputIds, ServiceBroker);
                
            }
            //Adding static placeholders
            foreach (var item in GetStaticPlaceholders())
            {
                _placeholders.AddItemWithValue(item, GetStringParameter(item));
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

        private void ListPlaceholders()
        {
            //Adding dynamic placeholders
            if (!string.IsNullOrEmpty(_pSmoSystemName))
            {
                SmartObjectClientServer smoServer = ServiceBroker.K2Connection.GetConnection<SmartObjectClientServer>();
                using (smoServer.Connection)
                {
                    var smo = smoServer.GetSmartObject(_pSmoSystemName);
                    smo.MethodToExecute = _pSmoListName;
                    var dt = smoServer.ExecuteListDataTable(smo);
                    foreach (DataRow row in dt.Rows)
                    {
                        _placeholders.AddItem(row[_pNameProperty].ToString());
                    }
                }
            }
            //Adding static placeholders
            //Adding static placeholders
            foreach (var item in GetStaticPlaceholders())
            {
                _placeholders.AddItem(item);
            }
            ServiceBroker.Service.ServiceObjects[0].Properties.InitResultTable();
            DataTable results = ServiceBroker.ServicePackage.ResultTable;
            foreach (var item in _placeholders.Items)
            {
                DataRow dr = results.NewRow();
                dr[Constants.Properties.Placeholder] = item.Name;
                dr[Constants.Properties.PlaceholderWithWrapper] = _placeholders.Wrapper + item.Name + _placeholders.Wrapper;
                results.Rows.Add(dr);
            }
        }
    }
}
