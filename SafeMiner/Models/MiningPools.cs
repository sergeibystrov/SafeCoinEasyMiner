using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeMiner.Models
{
    public class MiningPool
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string uri { get; set; }
        public string port { get; set; }
        public string fee { get; set; }

        public static List<MiningPool> Get() //This will eventually be changed to an API call to get all of the pools.. For now hard coded :(
        {
            return new List<MiningPool> {
                new MiningPool
                {
                    ID = 0,
                    Name = "Equipool USA",
                    fee = "0.2%",
                    port = "50111",
                    uri = "mine.equipool.1ds.us"
                },
                new MiningPool
                {
                    ID = 1,
                    Name = "Cats Pool EU",
                    fee = "0.5%",
                    port = "3432",
                    uri = "safecoin.catspool.org"
                }
            };

        }
    }
}
