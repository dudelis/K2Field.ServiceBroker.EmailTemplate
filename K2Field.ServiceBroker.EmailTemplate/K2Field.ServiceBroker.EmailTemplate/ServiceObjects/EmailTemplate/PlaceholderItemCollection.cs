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
        public List<PlaceholderItem> Items;

        public string Wrapper { get; set; }
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
        public void AddItemWithValue(string name, string value)
        {
            var item = new PlaceholderItem()
            {
                Name = name,
                Value = value
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
                    throw new ArgumentException(string.Format(Resources.QueryCannotStartDeleteUpdate, query));
                }
                foreach (var item in inputIds)
                {
                    var searchValue = "@" + item.Key;
                    query = query.Replace(searchValue, item.Value);
                }
                DataTable results = smoServer.ExecuteSQLQueryDataTable(query);
                if (results.Rows.Count > 0)
                {
                    p.Value = results.Rows[0][0].ToString();
                }
            }
        }
        public string ReplacePlaceholders(string input)
        {
            var output = input;
            if (string.IsNullOrEmpty(input)) return output;
            foreach (var item in Items)
            {
                var placeholder = Wrapper + item.Name + Wrapper;
                output = output.Replace(placeholder, item.Value);
            }
            return output;
        }
    }
}
