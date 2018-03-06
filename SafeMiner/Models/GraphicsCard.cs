using SafeMiner.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeMiner.Models
{
    class GraphicsCard
    {
        public int CardNumber { get; set; }
        public GraphicsCardType Type { get; set; }
        public string CardName { get; set; }
    }
}
