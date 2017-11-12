using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using K2Field.ServiceBroker.EmailTemplate.Properties;
using SourceCode.SmartObjects.Client;

namespace K2Field.ServiceBroker.EmailTemplate.ServiceObjects.EmailTemplate
{
    public class PlaceholderItemCollection
    {
        public string Wrapper;
        public List<PlaceholderItem> Items;

        public PlaceholderItemCollection()
        {
            Items = new List<PlaceholderItem>();
        }

        public void AddItem(string name, string adoQuery)
        {
            var item = new PlaceholderItem()
            {
                Name = name,
                AdoQuery = adoQuery
            };
            Items.Add(item);
        }

        public void GetAllValues(SmartObjectClientServer smoServer, Dictionary<string, string> inputIds)
        {
            foreach (var p in Items)
            {
                
                var query = p.AdoQuery;
                if (query.ToLower().StartsWith("delete ") || query.ToLower().StartsWith("update "))
                {
                    throw new ArgumentException(String.Format(Resources.QueryCannotStartDeleteUpdate, query));
                }
                foreach (var item in inputIds)
                {
                    var searchValue = "@" + item.Key;
                    query = query.Replace(searchValue, item.Value);
                }
                DataTable results = smoServer.ExecuteSQLQueryDataTable(query);
                p.Value = results.Rows[0][0].ToString();
            }
        }

        public string ReplacePlaceholders(string input)
        {
            foreach (var item in Items)
            {
                var placeholder = Wrapper + item.Name + Wrapper;
            }
            throw new NotImplementedException();
        }
    }
}
