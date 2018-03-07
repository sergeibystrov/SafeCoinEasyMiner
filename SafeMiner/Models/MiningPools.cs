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
                    Name = "Minadores Pool",
                    fee = "0.2%",
                    port = "3032",
                    uri = "equi.minadorespool.gq"
                },
                new MiningPool
                {
                    ID = 2,
                    Name = "Cats Pool EU",
                    fee = "0.5%",
                    port = "3432",
                    uri = "safecoin.catspool.org"
                },
                new MiningPool
                {
                    ID = 3,
                    Name = "Cats Pool USA",
                    fee = "0.5%",
                    port = "3432",
                    uri = "safecoin-us.catspool.org"
                },
                new MiningPool
                {
                    ID = 4,
                    Name = "Sexy Pool",
                    fee = "0.5%",
                    port = "21002",
                    uri = "safe.pool.sexy"
                },
                new MiningPool
                {
                    ID = 5,
                    Name = "PcMining XYZ EU",
                    fee = "1%!!",
                    port = "3457",
                    uri = "pcmining.xyz"
                },
                new MiningPool
                {
                    ID = 6,
                    Name = "PcMining XYZ USA",
                    fee = "1%!!",
                    port = "3457",
                    uri = "us.pcmining.xyz"
                },
                new MiningPool
                {
                    ID = 7,
                    Name = "PcMining XYZ Asia",
                    fee = "1%!!",
                    port = "3457",
                    uri = "asia.pcmining.xyz"
                }
            };

        }
    }
}
