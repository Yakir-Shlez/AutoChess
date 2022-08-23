using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Client
{
    class CheckedBoard
    {
        public int hashCode;
        public List<Move> allMoves;
        public int numberOfPieces;
        public ChessBoard board;
        public CheckedBoard(int hashCode, List<Move> allMoves, int numberOfPieces, ChessBoard board)
        {
            this.hashCode = hashCode;
            this.allMoves = allMoves;
            this.numberOfPieces = numberOfPieces;
            this.board = board;
        }
    }
}
