using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Client
{
    class AINode
    {
        public List<Move> allPossibleMoves;
        public List<int> allScores;
        public List<AINode> allPossibleNodes;
        public int depth;
        public bool max;
        public Move bestMove;
        public int bestScore;
        public AINode(int depth)
        {
            this.depth = depth;
        }
    }
}
