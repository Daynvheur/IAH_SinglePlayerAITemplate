using System.Collections.Generic;

namespace IAH_SinglePlayerAutomation.Class.Response
{
    public class TpCard
    {
        public string? type;
    }

    public class TpScreenResponse
    {
        public List<TpCard> chaosCards = new();
        public List<TpCard> tpCards = new();
    }
}