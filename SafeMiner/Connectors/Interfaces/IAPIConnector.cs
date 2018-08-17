using System.Collections.Generic;
using SafeMiner.Models;

namespace SafeMiner.Connectors.Interfaces
{
    public interface IAPIConnector
    {
        List<Pool> Get();
    }
}