using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Chess_Client
{
    public class Piece : IEquatable<Piece>
    {
        public readonly PieceType type;
        public readonly PieceColor color;
        public readonly int[] rowPattern;
        public readonly int[] colPattern;
        public readonly int continuesPattern;
        public readonly int pieceWeight;
        public readonly int[,] pieceSquaresWeight;
        public readonly Bitmap pieceGUI;
        
        public Piece() : this(PieceType.Null, PieceColor.Null)
        { }
        public Piece(Bitmap pieceGUI) : this(PieceGUIToType(pieceGUI), PieceGUIToColor(pieceGUI))
        { }
        public Piece(PieceType type, PieceColor color)
        {
            this.type = type;
            this.color = color;
            switch (type)
            {
                case PieceType.King:
                    rowPattern = new int[] { 0, 0, -1, 1, 1, 1, -1, -1 };
                    colPattern = new int[] { -1, 1, 0, 0, -1, 1, -1, 1 };
                    continuesPattern = 0;
                    pieceWeight = 20000;
                    if (color == PieceColor.White)
                    {
                        pieceGUI = global::Chess_Client.Properties.Resources.White_King_Piece;
                        pieceSquaresWeight = ChessDB.whiteKingPositionWeights;
                    }
                    else
                    {
                        pieceGUI = global::Chess_Client.Properties.Resources.Black_King_Piece;
                        pieceSquaresWeight = ChessDB.blackKingPositionWeights;
                    }
                    break;
                case PieceType.Queen:
                    rowPattern = new int[] { 0, 0, -1, 1, 1, 1, -1, -1 };
                    colPattern = new int[] { -1, 1, 0, 0, -1, 1, -1, 1 };
                    continuesPattern = 1;
                    pieceWeight = 900;
                    if (color == PieceColor.White)
                    {
                        pieceGUI = global::Chess_Client.Properties.Resources.White_Queen_Piece;
                        pieceSquaresWeight = ChessDB.whiteQueenPositionWeights;
                    }
                    else
                    {
                        pieceGUI = global::Chess_Client.Properties.Resources.Black_Queen_Piece;
                        pieceSquaresWeight = ChessDB.blackQueenPositionWeights;
                    }
                    break;
                case PieceType.Bishop:
                    rowPattern = new int[] { 1, 1, -1, -1 };
                    colPattern = new int[] { -1, 1, -1, 1 };
                    continuesPattern = 1;
                    pieceWeight = 330;
                    if (color == PieceColor.White)
                    {
                        pieceGUI = global::Chess_Client.Properties.Resources.White_Bishop_Piece;
                        pieceSquaresWeight = ChessDB.whiteBishopPositionWeights;
                    }
                    else
                    {
                        pieceGUI = global::Chess_Client.Properties.Resources.Black_Bishop_Piece;
                        pieceSquaresWeight = ChessDB.blackBishopPositionWeights;
                    }
                    break;
                case PieceType.Knight:
                    rowPattern = new int[] { 2, 2, -2, -2, 1, 1, -1, -1 };
                    colPattern = new int[] { 1, -1, 1, -1, 2, -2, 2, -2 };
                    continuesPattern = 0;
                    pieceWeight = 320;
                    if (color == PieceColor.White)
                    {
                        pieceGUI = global::Chess_Client.Properties.Resources.White_Knight_Piece;
                        pieceSquaresWeight = ChessDB.whiteKnightPositionWeights;
                    }
                    else
                    {
                        pieceGUI = global::Chess_Client.Properties.Resources.Black_Knight_Piece;
                        pieceSquaresWeight = ChessDB.blackKnightPositionWeights;
                    }
                    break;
                case PieceType.Rook:
                    rowPattern = new int[] { 0, 0, -1, 1 };
                    colPattern = new int[] { -1, 1, 0, 0 };
                    continuesPattern = 1;
                    pieceWeight = 500;
                    if (color == PieceColor.White)
                    {
                        pieceGUI = global::Chess_Client.Properties.Resources.White_Rook_Piece;
                        pieceSquaresWeight = ChessDB.whiteRookPositionWeights;
                    }
                    else
                    {
                        pieceGUI = global::Chess_Client.Properties.Resources.Black_Rook_Piece;
                        pieceSquaresWeight = ChessDB.blackRookPositionWeights;
                    }
                    break;
                case PieceType.Pawn:
                    pieceWeight = 100;
                    if (color == PieceColor.White)
                    {
                        pieceGUI = global::Chess_Client.Properties.Resources.White_Pawn_Piece;
                        pieceSquaresWeight = ChessDB.whitePawnPositionWeights;
                    }
                    else
                    {
                        pieceGUI = global::Chess_Client.Properties.Resources.Black_Pawn_Piece;
                        pieceSquaresWeight = ChessDB.blackPawnPositionWeights;
                    }
                    break;
                case PieceType.Null:
                    pieceWeight = 0;
                    pieceGUI = null;
                    pieceSquaresWeight = ChessDB.nullPositionWeights;
                    break;
            }
        }

        public int calculateTotalScore(int rowLocation, int colLocation, PieceColor myColor)
        {
            return ((pieceWeight + pieceSquaresWeight[rowLocation, colLocation]) * (myColor == color ? 1 : -1));
        }

        static public PieceType PieceGUIToType(Bitmap piece)
        {
            if(piece == null)
                return PieceType.Null;
            if (piece == Properties.Resources.Black_King_Piece || piece == Properties.Resources.White_King_Piece)
                return PieceType.King;
            else if (piece == Properties.Resources.Black_Queen_Piece || piece == Properties.Resources.White_Queen_Piece)
                return PieceType.Queen;
            else if (piece == Properties.Resources.Black_Rook_Piece || piece == Properties.Resources.White_Rook_Piece)
                return PieceType.Rook;
            else if (piece == Properties.Resources.Black_Bishop_Piece || piece == Properties.Resources.White_Bishop_Piece)
                return PieceType.Bishop;
            else if (piece == Properties.Resources.Black_Knight_Piece || piece == Properties.Resources.White_Knight_Piece)
                return PieceType.Knight;
            else
                return PieceType.Pawn;
            
        }
        static private PieceColor PieceGUIToColor(Bitmap piece)
        {
            if(piece == null)
                return PieceColor.Null;
            else if (piece == Properties.Resources.Black_King_Piece || piece == Properties.Resources.Black_Queen_Piece || piece == Properties.Resources.Black_Rook_Piece ||
                piece == Properties.Resources.Black_Bishop_Piece || piece == Properties.Resources.Black_Knight_Piece || piece == Properties.Resources.Black_Pawn_Piece)
                return PieceColor.Black;
            else
                return PieceColor.White;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Piece);
        }

        public bool Equals(Piece other)
        {
            return other != null &&
                   type == other.type &&
                   color == other.color;
        }

        public override int GetHashCode()
        {
            var hashCode = 1066637485;
            hashCode = hashCode * -1521134295 + type.GetHashCode();
            hashCode = hashCode * -1521134295 + color.GetHashCode();
            return hashCode;
        }
    }
}
