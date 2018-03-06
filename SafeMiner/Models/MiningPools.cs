using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeMiner.Models
{
    public class MiningPools
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string uri { get; set; }
        public string port { get; set; }
        public string fee { get; set; }
    }
}
