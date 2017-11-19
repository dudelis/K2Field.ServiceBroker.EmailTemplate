using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using K2Field.ServiceBroker.EmailTemplate.Properties;
using SourceCode.Data.SmartObjectsClient;
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
        public void AddItem(string name)
        {
            var item = new PlaceholderItem()
            {
                Name = name
            };
            Items.Add(item);
        }
        public void AddItem(string name, string adoQuery, string returnProperty)
        {
            var item = new PlaceholderItem()
            {
                Name = name,
                AdoQuery = adoQuery,
                ReturnProperty = returnProperty
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
        public void GetAllValues(Dictionary<string, string> inputIds, string connectionString)
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
                    var searchValue = Wrapper + item.Key + Wrapper;
                    query = query.Replace(searchValue, item.Value);
                }
                DataTable dt = new DataTable();
                using (SOConnection soConnection = new SOConnection(connectionString))
                using (SOCommand soCommand = new SOCommand(query, soConnection))
                using (SODataAdapter soDataAdapter = new SODataAdapter(soCommand))
                {
                    soDataAdapter.Fill(dt);
                }
                if (dt.Rows.Count > 0)
                {
                    p.Value = dt.Rows[0][p.ReturnProperty].ToString();
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
