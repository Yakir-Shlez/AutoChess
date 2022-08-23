using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Client
{
    public class Config
    {
        public string Ip_Address { get; set; }
        public bool Testing { get; set; }
        public bool TestingWithoutServer { get; set; }
        public bool Testing_AI { get; set; }
        public bool Testing_AI_Brain { get; set; }
        public bool Testing_Ai_Vs_Ai { get; set; }
        public int Testing_AI_Side { get; set; }
        public List<Move> Testing_AI_Moves { get; set; }

    }
}
