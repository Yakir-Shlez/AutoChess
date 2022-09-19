using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Client
{
    internal class PieceToMove : IEquatable<PieceToMove>
    {
        public int rowIndex;
        public int colIndex;
        public PieceToMove(int rowIndex, int colIndex)
        {
            this.rowIndex = rowIndex;
            this.colIndex = colIndex;
        }
        public PieceToMove(Move move)
        {
            this.rowIndex = move.destRowIndex;
            this.colIndex = move.destColIndex;
        }
        public PieceToMove(string square)
        {
            if (square.Length != 2)
                throw new ApplicationException("Wrong PieceToMove input");

            if (square[0] < 'a' || square[0] > 'h')
                throw new ApplicationException("Wrong PieceToMove input");
            colIndex = (int)square[0] - (int)'a';

            if (square[1] < '1' || square[1] > '8')
                throw new ApplicationException("Wrong move input");
            rowIndex = (int)square[1] - (int)'1';
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as PieceToMove);
        }

        public bool Equals(PieceToMove other)
        {
            return other != null &&
                   rowIndex == other.rowIndex &&
                   colIndex == other.colIndex;
        }
        public override string ToString()
        {
            return ((char)(colIndex + (int)'a') + (rowIndex + 1).ToString());
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
