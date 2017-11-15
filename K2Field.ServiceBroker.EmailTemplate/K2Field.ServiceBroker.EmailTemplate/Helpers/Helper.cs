using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SourceCode.SmartObjects.Services.ServiceSDK.Objects;
using SourceCode.SmartObjects.Services.ServiceSDK.Types;

namespace K2Field.ServiceBroker.EmailTemplate.Helpers
{
    public class Helper
    {
        public static Property CreateProperty(string name, string description, SoType type)
        {
            Property property = new Property
            {
                Name = name,
                SoType = type,
                Type = MapHelper.GetTypeBySoType(type),
                MetaData = new MetaData(name, description)
            };
            return property;
        }
        public static Method CreateMethod(string name, string description, MethodType methodType)
        {
            Method m = new Method
            {
                Name = name,
                Type = methodType,
                MetaData = new MetaData(name, description)
            };
            return m;
        }
        public static MethodParameter CreateParameter(string name, SoType soType, bool isRequired, MethodParameterType type)
        {
            MethodParameter methodParam = new MethodParameter
            {
                Name = name,
                IsRequired = isRequired,
                MetaData = new MetaData
                {
                    Description = type.ToString(),
                    DisplayName = name
                },
                SoType = soType,
                Type = MapHelper.GetTypeBySoType(soType)
            };
            return methodParam;
        }
        public static MethodParameters GetMethodParamaters(List<string> inputIds, MethodParameterType type)
        {
            MethodParameters paramCollection = new MethodParameters();
            if (inputIds.Count == 0) return paramCollection;
            foreach (var param in inputIds)
            {
                paramCollection.Create(CreateParameter(param, SoType.Text, false, type));
            }
            return paramCollection;
        }
    }
}
