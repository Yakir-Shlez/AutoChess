using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Client
{
    public class Move : IEquatable<Move>
    {
        public int sourceRowIndex;
        public int sourceColIndex;
        public int destRowIndex;
        public int destColIndex;
        public int destWeight;
        public Move()
        {
            sourceRowIndex = -1;
            sourceColIndex = -1;
            destRowIndex = -1;
            destColIndex = -1;
            this.destWeight = 0;
        }
        public Move(int sourceRowIndex, int sourceColIndex, int destRowIndex, int destColIndex)
        {
            this.sourceRowIndex = sourceRowIndex;
            this.sourceColIndex = sourceColIndex;
            this.destRowIndex = destRowIndex;
            this.destColIndex = destColIndex;
        }
        public Move(string move)
        {
            if (move.Length != 5)
                throw new ApplicationException("Wrong move input");
            string[] moveArr = move.Split('-');
            if (moveArr.Length != 2 || moveArr[0].Length != 2 || moveArr[1].Length != 2)
                throw new ApplicationException("Wrong move input");

            if (moveArr[0][0] < 'a' || moveArr[0][0] > 'h')
                throw new ApplicationException("Wrong move input");
            sourceColIndex = (int)moveArr[0][0] - (int)'a';

            if (moveArr[0][1] < '1' || moveArr[0][1] > '8')
                throw new ApplicationException("Wrong move input");
            sourceRowIndex = (int)moveArr[0][1] - (int)'1';

            if (moveArr[1][0] < 'a' || moveArr[1][0] > 'h')
                throw new ApplicationException("Wrong move input");
            destColIndex = (int)moveArr[1][0] - (int)'a';

            if (moveArr[1][1] < '1' || moveArr[1][1] > '8')
                throw new ApplicationException("Wrong move input");
            destRowIndex = (int)moveArr[1][1] - (int)'1';
        }
        public Move ReverseMove()
        {
            return new Move(this.destRowIndex, this.destColIndex, this.sourceRowIndex, this.sourceColIndex);
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as Move);
        }
        public bool Equals(Move other)
        {
            return other != null &&
                   sourceRowIndex == other.sourceRowIndex &&
                   sourceColIndex == other.sourceColIndex &&
                   destRowIndex == other.destRowIndex &&
                   destColIndex == other.destColIndex;
        }
        public override string ToString()
        {
            return ((char)(sourceColIndex + (int)'a') + (sourceRowIndex + 1).ToString() + "-" + (char)(destColIndex + (int)'a') + (destRowIndex + 1).ToString());
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
