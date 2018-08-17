using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeMiner.Models
{
    public class PoolByCountry
    {
        public string Region { get; set; }
        public List<Pool> Pools { get; set; }
    }
    public class Pool
    {
        public string Name { get; set; }
        public string uri { get; set; }
        public string port { get; set; }
        public string WorkerPage { get; set; }

    }
}
