using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Client
{
    public enum PieceType
    {
        King = 0,
        Queen = 1,
        Rook = 2,
        Bishop = 3,
        Knight = 4,
        Pawn = 5,
        Null = 6
    }
    public enum PieceColor
    {
        Black = 0,
        White = 1,
        Null = 2
    }
    public enum GameState
    {
        MyTurn,
        OpTurn,
        Resigned,
        Null
    }
}
