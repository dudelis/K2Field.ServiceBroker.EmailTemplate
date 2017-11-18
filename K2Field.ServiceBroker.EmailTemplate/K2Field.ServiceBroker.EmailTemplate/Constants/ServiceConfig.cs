using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K2Field.ServiceBroker.EmailTemplate.Constants
{
    public static class ServiceConfig
    {
        public const string PlaceholderSmoSystemName = "Smo System Name (Dynamic Placeholders)";
        public const string ListMethodName = "Smo List Method Name";
        public const string PlaceholderPropertyName = "Dynamic Placeholder Property Name";
        public const string AdoNetPropertyName = "Ado.Net Property Name";
        public const string ReturnProperty = "Return Property - contains the column to return from Ado.Net";
        public const string DelimitedInputIDs = "Input Parameters for Dynamic ADO.NET queries, delimited with (;)";
        public const string StaticPlaceholders = "Static Placeholders, delimited with (;)";
        public const string PlaceholderWrapperSymbol = "Placeholder Wrapper in the Body/Subject and Queries - e.g. $text$";
    }
}
