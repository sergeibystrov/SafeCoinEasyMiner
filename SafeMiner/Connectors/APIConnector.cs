using RestSharp;
using SafeMiner.Connectors.Interfaces;
using SafeMiner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



/// <summary>
/// API CALL FOR ALL POOLS: https://raw.githubusercontent.com/icsta/SafeCoinEasyMiner/master/SafeMiner/Models/Pools.json
/// </summary>

namespace SafeMiner.Connectors
{
    public class APIConnector
    {

        public APIConnector()
        {

        }

        public List<PoolByCountry> Get()
        {
            try
            {
                var client = new RestClient("https://raw.githubusercontent.com/icsta/SafeCoinEasyMiner/master/SafeMiner/Models/Pools.json");

                var request = new RestRequest(Method.GET);

                var result = client.Execute(request);
                var x = Newtonsoft.Json.JsonConvert.DeserializeObject<PoolByCountry>(result.Content);
                return null;
            }
            catch
            {
                return null;
            }
        }
        
    }
}
