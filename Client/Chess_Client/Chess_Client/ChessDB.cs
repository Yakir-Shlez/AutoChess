using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Client
{
    static class ChessDB
    {
        public static readonly string wKing = "\u2654";
        public static readonly string wQueen = "\u2655";
        public static readonly string wRook = "\u2656";
        public static readonly string wBishop = "\u2657";
        public static readonly string wKnight = "\u2658";
        public static readonly string wPawn = "\u2659";
        public static readonly string bKing = "\u265a";
        public static readonly string bQueen = "\u265b";
        public static readonly string bRook = "\u265c";
        public static readonly string bBishop = "\u265d";
        public static readonly string bKnight = "\u265e";
        public static readonly string bPawn = "\u265f";
        public static readonly string empty = "   ";
        public static readonly int[,] whiteKingPositionWeights = new int[,] {
            { 20, 30, 10, 0, 0, 10, 30, 20 },
            { 20, 20, 0, 0, 0, 0, 20, 20 },
            { -10, -20, -20, -20, -20, -20, -20, -10 },
            { -20, -30, -30, -40, -40, -30, -30, -20 },
            { -30, -40, -40, -50, -50, -40, -40, -30 },
            { -30, -40, -40, -50, -50, -40, -40, -30 },
            { -30, -40, -40, -50, -50, -40, -40, -30 },
            { -30, -40, -40, -50, -50, -40, -40, -30 } };
        public static readonly int[,] blackKingPositionWeights = new int[,] {
            { -30, -40, -40, -50, -50, -40, -40, -30 },
            { -30, -40, -40, -50, -50, -40, -40, -30 },
            { -30, -40, -40, -50, -50, -40, -40, -30 },
            { -30, -40, -40, -50, -50, -40, -40, -30 },
            { -20, -30, -30, -40, -40, -30, -30, -20 },
            { -10, -20, -20, -20, -20, -20, -20, -10 },
            { 20, 20, 0, 0, 0, 0, 20, 20 },
            { 20, 30, 10, 0, 0, 10, 30, 20 } };
        public static readonly int[,] whiteQueenPositionWeights = new int[,] {
            { -20, -10, -10, -5, -5, -10, -10, -20 },
            { -10, 0, 5, 0, 0, 0, 0, -10 },
            { -10, 5, 5, 5, 5, 5, 0, -10 },
            { 0, 0, 5, 5, 5, 5, 0, -5 },
            { -5, 0, 5, 5, 5, 5, 0, -5 },
            { -10, 0, 5, 5, 5, 5, 0, -10 },
            { -10, 0, 0, 0, 0, 0, 0, -10 },
            { -20, -10, -10, -5, -5, -10, -10, -20 } };
        public static readonly int[,] blackQueenPositionWeights = new int[,] {
            { -20, -10, -10, -5, -5, -10, -10, -20 },
            { -10, 0, 0, 0, 0, 0, 0, -10 },
            { -10, 0, 5, 5, 5, 5, 0, -10 },
            { -5, 0, 5, 5, 5, 5, 0, -5 },
            { 0, 0, 5, 5, 5, 5, 0, -5 },
            { -10, 5, 5, 5, 5, 5, 0, -10 },
            { -10, 0, 5, 0, 0, 0, 0, -10 },
            { -20, -10, -10, -5, -5, -10, -10, -20 } };
        public static readonly int[,] whiteRookPositionWeights = new int[,] {
            { 0, 0, 0, 5, 5, 0, 0, 0 },
            { -5, 0, 0, 0, 0, 0, 0, -5 },
            { -5, 0, 0, 0, 0, 0, 0, -5 },
            { -5, 0, 0, 0, 0, 0, 0, -5 },
            { -5, 0, 0, 0, 0, 0, 0, -5 },
            { -5, 0, 0, 0, 0, 0, 0, -5 },
            { 5, 10, 10, 10, 10, 10, 10, 5 },
            { 0, 0, 0, 0, 0, 0, 0, 0 } };
        public static readonly int[,] blackRookPositionWeights = new int[,] { 
            { 0, 0, 0, 0, 0, 0, 0, 0 }, 
            { 5, 10, 10, 10, 10, 10, 10, 5 }, 
            { -5, 0, 0, 0, 0, 0, 0, -5 },
            { -5, 0, 0, 0, 0, 0, 0, -5 }, 
            { -5, 0, 0, 0, 0, 0, 0, -5 }, 
            { -5, 0, 0, 0, 0, 0, 0, -5 }, 
            { -5, 0, 0, 0, 0, 0, 0, -5 },
            { 0, 0, 0, 5, 5, 0, 0, 0 } };
        public static readonly int[,] whiteBishopPositionWeights = new int[,] {
            { -20, -10, -10, -10, -10, -10, -10, -20 },
            { -10, 5, 0, 0, 0, 0, 5, -10 },
            { -10, 10, 10, 10, 10, 10, 10, -10 },
            { -10, 0, 10, 10, 10, 10, 0, -10 },
            { -10, 5, 5, 10, 10, 5, 5, -10 },
            { -10, 0, 5, 10, 10, 5, 0, -10 },
            { -10, 0, 0, 0, 0, 0, 0, -10 },
            { -20, -10, -10, -10, -10, -10, -10, -20 }};
        public static readonly int[,] blackBishopPositionWeights = new int[,] {
            { -20, -10, -10, -10, -10, -10, -10, -20 }, 
            { -10, 0, 0, 0, 0, 0, 0, -10 }, 
            { -10, 0, 5, 10, 10, 5, 0, -10 },
            { -10, 5, 5, 10, 10, 5, 5, -10 },
            { -10, 0, 10, 10, 10, 10, 0, -10 }, 
            { -10, 10, 10, 10, 10, 10, 10, -10 },
            { -10, 5, 0, 0, 0, 0, 5, -10 }, 
            { -20, -10, -10, -10, -10, -10, -10, -20 } };
        public static readonly int[,] whiteKnightPositionWeights = new int[,] {
            { -50, -40, -30, -30, -30, -30, -40, -50 },
            { -40, -20, 0, 5, 5, 0, -20, -40 },
            { -30, 5, 10, 15, 15, 10, 5, -30 },
            { -30, 0, 15, 20, 20, 15, 0, -30 },
            { -30, 5, 15, 20, 20, 15, 5, -30 },
            { -30, 5, 15, 20, 20, 15, 5, -30 },
            { -40, -20, 0, 0, 0, 0, -20, -40 },
            { -50, -40, -30, -30, -30, -30, -40, -50 } };
        public static readonly int[,] blackKnightPositionWeights = new int[,] {
            { -50, -40, -30, -30, -30, -30, -40, -50 }, 
            { -40, -20, 0, 0, 0, 0, -20, -40 }, 
            { -30, 5, 15, 20, 20, 15, 5, -30 },
            { -30, 5, 15, 20, 20, 15, 5, -30 },
            { -30, 0, 15, 20, 20, 15, 0, -30 }, 
            { -30, 5, 10, 15, 15, 10, 5, -30 }, 
            { -40, -20, 0, 5, 5, 0, -20, -40 }, 
            { -50, -40, -30, -30, -30, -30, -40, -50 } };
        public static readonly int[,] whitePawnPositionWeights = new int[,] {
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 5, 10, 10, -20, -20, 10, 10, 5 },
            { 5, -5, -10, 0, 0, -10, -5, 5 },
            { 0, 0, 0, 20, 20, 0, 0, 0 },
            { 5, 5, 10, 25, 25, 10, 5, 5 },
            { 10, 10, 20, 30, 30, 20, 10, 10 },
            { 50, 50, 50, 50, 50, 50, 50, 50 },
            { 0, 0, 0, 0, 0, 0, 0, 0 }};
        public static readonly int[,] blackPawnPositionWeights = new int[,] { 
            { 0, 0, 0, 0, 0, 0, 0, 0 }, 
            { 50, 50, 50, 50, 50, 50, 50, 50 },
            { 10, 10, 20, 30, 30, 20, 10, 10 }, 
            { 5, 5, 10, 25, 25, 10, 5, 5 }, 
            { 0, 0, 0, 20, 20, 0, 0, 0 },
            { 5, -5, -10, 0, 0, -10, -5, 5 },
            { 5, 10, 10, -20, -20, 10, 10, 5 }, 
            { 0, 0, 0, 0, 0, 0, 0, 0 } };
        public static readonly int[,] nullPositionWeights = new int[,] {
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 } };
    }
}
