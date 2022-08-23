using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace Chess_Client
{
    public class ChessBoard : IEquatable<ChessBoard>
    {
        public Panel[,] allSquaresPanels;
        public PictureBox[,] allSquaresPictureBoxes;
        public Piece[,] board;
        public PieceColor myColor;
        public PieceColor opColor;
        public DateTime lastMoveTime;
        public TimeSpan myTimer;
        public TimeSpan opTimer;
        public GameState currentTurn;
        public Piece nullPiece;
        public int enPassantCol = -1;
        public bool whiteShortCastleFlag = true;
        public bool whiteLongCastleFlag = true;
        public bool blackShortCastleFlag = true;
        public bool blackLongCastleFlag = true;
        public Label[] LLabels;
        public Label[] RLabels;
        public Label[] ULabels;
        public Label[] DLabels;
        public Label scoreLabel;
        public int totalBoardScore;
        public int numberOfPieces;

        private ChessBoard()
        { }

        public ChessBoard(PieceColor myColor, TimeSpan gameTime, Panel[,] allSquaresPanels, PictureBox[,] allSquaresPictureBoxes, Label[] LLabels, Label[] RLabels, Label[] ULabels, Label[] DLabels, Label scoreLabel)
        {
            lastMoveTime = DateTime.Now;
            this.numberOfPieces = 32;
            this.myColor = myColor;
            this.scoreLabel = scoreLabel;
            if (myColor == PieceColor.White)
            {
                opColor = PieceColor.Black;
                currentTurn = GameState.MyTurn;
            }
            else if (myColor == PieceColor.Black)
            {
                opColor = PieceColor.White;
                currentTurn = GameState.OpTurn;
            }
            else
                throw new ApplicationException("Wrong color input");

            Piece[]  myPieces = new Piece[] { new Piece(PieceType.King, myColor), new Piece(PieceType.Queen, myColor)
                    , new Piece(PieceType.Rook, myColor) , new Piece(PieceType.Bishop, myColor)
                    , new Piece(PieceType.Knight, myColor), new Piece(PieceType.Pawn, myColor)};

            Piece[]  opPieces = new Piece[] { new Piece(PieceType.King, opColor), new Piece(PieceType.Queen, opColor)
                    , new Piece(PieceType.Rook, opColor), new Piece(PieceType.Bishop, opColor)
                    , new Piece(PieceType.Knight, opColor), new Piece(PieceType.Pawn, opColor)};

            Piece[] whitePieces;
            Piece[] blackPieces; // TBD matybe takw it outside
            if (myColor == PieceColor.White)
            {
                whitePieces = myPieces;
                blackPieces = opPieces;
            }
            else
            {
                whitePieces = opPieces;
                blackPieces = myPieces;
            }

            this.allSquaresPanels = allSquaresPanels;
            this.allSquaresPictureBoxes = allSquaresPictureBoxes;
            this.LLabels = LLabels;
            this.RLabels = RLabels;
            this.ULabels = ULabels;
            this.DLabels = DLabels;

            board = new Piece[8, 8];
            nullPiece = new Piece();
            //TBD weights
            board[0, 0] = whitePieces[(int)PieceType.Rook];
            board[0, 7] = whitePieces[(int)PieceType.Rook];// TBD maybe seperate to 2 diffrent pieces? (relevant for all the followig pieces too)
            board[0, 1] = whitePieces[(int)PieceType.Knight];
            board[0, 6] = whitePieces[(int)PieceType.Knight];
            board[0, 2] = whitePieces[(int)PieceType.Bishop];
            board[0, 5] = whitePieces[(int)PieceType.Bishop];
            board[0, 3] = whitePieces[(int)PieceType.Queen];
            board[0, 4] = whitePieces[(int)PieceType.King];
            for (int i = 0; i < 8; i++)
                board[1, i] = whitePieces[(int)PieceType.Pawn];

            board[7, 0] = blackPieces[(int)PieceType.Rook]; // TBD maybe seperate to 2 diffrent pieces? (relevant for all the followig pieces too)
            board[7, 7] = blackPieces[(int)PieceType.Rook];
            board[7, 1] = blackPieces[(int)PieceType.Knight];
            board[7, 6] = blackPieces[(int)PieceType.Knight];
            board[7, 2] = blackPieces[(int)PieceType.Bishop];
            board[7, 5] = blackPieces[(int)PieceType.Bishop];
            board[7, 3] = blackPieces[(int)PieceType.Queen];
            board[7, 4] = blackPieces[(int)PieceType.King];
            for (int i = 0; i < 8; i++)
                board[6, i] = blackPieces[(int)PieceType.Pawn];

            for (int i = 2; i < 6; i++)
                for (int j = 0; j < 8; j++)
                    board[i, j] = nullPiece;

            myTimer = gameTime;
            opTimer = gameTime;

            CalculateScores();
        }

        #region Game manage functions
        public void SwitchTurn()
        {
            if (currentTurn == GameState.MyTurn)
                currentTurn = GameState.OpTurn;
            else
                currentTurn = GameState.MyTurn;
        }
        #endregion

        #region Game Functions
        public void ExecuteGameMove(Move move)
        {
            ExecuteGameMove(move, false, false);
        }
        public void ExecuteGameMove(Move move, bool testMove, bool switchTurn)
        {
            int sourceIndexRow = move.sourceRowIndex;
            int sourceIndexCol = move.sourceColIndex;
            int destIndexRow = move.destRowIndex;
            int destIndexCol = move.destColIndex;

            //castling
            if (board[sourceIndexRow, sourceIndexCol].type == PieceType.King)
            {
                if (board[sourceIndexRow, sourceIndexCol].color == PieceColor.White)
                {
                    whiteLongCastleFlag = false;
                    whiteShortCastleFlag = false;
                }
                else
                {
                    blackLongCastleFlag = false;
                    blackShortCastleFlag = false;
                }
                if (sourceIndexCol - destIndexCol > 1 || sourceIndexCol - destIndexCol < -1)
                {
                    if (sourceIndexCol - destIndexCol > 0)
                    {
                        board[destIndexRow, destIndexCol + 1] = board[sourceIndexRow, 0];
                        totalBoardScore += board[destIndexRow, destIndexCol + 1].calculateTotalScore(destIndexRow, destIndexCol + 1, myColor);
                        totalBoardScore -= board[sourceIndexRow, 0].calculateTotalScore(sourceIndexRow, 0, myColor);
                        board[sourceIndexRow, 0] = nullPiece;

                    }
                    else
                    {

                        board[destIndexRow, destIndexCol - 1] = board[sourceIndexRow, 7];
                        totalBoardScore += board[destIndexRow, destIndexCol - 1].calculateTotalScore(destIndexRow, destIndexCol - 1, myColor);
                        totalBoardScore -= board[sourceIndexRow, 7].calculateTotalScore(sourceIndexRow, 7, myColor);
                        board[sourceIndexRow, 7] = nullPiece;

                    }
                }
            }

            enPassantCol = -1;
            //En passant
            if (board[sourceIndexRow, sourceIndexCol].type == PieceType.Pawn)//pawn
            {
                if ((sourceIndexCol != destIndexCol) && //pawn takes
                    board[destIndexRow, destIndexCol].type == PieceType.Null) //empty sqaure
                {
                    numberOfPieces--;
                    totalBoardScore -= board[sourceIndexRow, destIndexCol].calculateTotalScore(sourceIndexRow, destIndexCol, myColor);
                    board[sourceIndexRow, destIndexCol] = nullPiece;
                }

                if (sourceIndexRow == 1 && destIndexRow == 3 && board[sourceIndexRow, sourceIndexCol].color == PieceColor.White)
                    enPassantCol = destIndexCol;
                if (sourceIndexRow == 6 && destIndexRow == 4 && board[sourceIndexRow, sourceIndexCol].color == PieceColor.Black)
                    enPassantCol = destIndexCol;
            }

            //rook move (maybe)
            if ((sourceIndexCol == 0 && sourceIndexRow == 0) || (destIndexCol == 0 && destIndexRow == 0))
                whiteLongCastleFlag = false;
            else if ((sourceIndexCol == 7 && sourceIndexRow == 0) || (destIndexCol == 7 && destIndexRow == 0))
                whiteShortCastleFlag = false;
            else if ((sourceIndexCol == 0 && sourceIndexRow == 7) || (destIndexCol == 0 && destIndexRow == 7))
                blackLongCastleFlag = false;
            else if ((sourceIndexCol == 7 && sourceIndexRow == 7) || (destIndexCol == 7 && destIndexRow == 7))
                blackShortCastleFlag = false;

            if (board[destIndexRow, destIndexCol].type != PieceType.Null) numberOfPieces--;

            totalBoardScore -= board[destIndexRow, destIndexCol].calculateTotalScore(destIndexRow, destIndexCol, myColor);
            board[destIndexRow, destIndexCol] = board[sourceIndexRow, sourceIndexCol];
            totalBoardScore += board[destIndexRow, destIndexCol].calculateTotalScore(destIndexRow, destIndexCol, myColor);

            totalBoardScore -= board[sourceIndexRow, sourceIndexCol].calculateTotalScore(sourceIndexRow, sourceIndexCol, myColor);
            board[sourceIndexRow, sourceIndexCol] = nullPiece;

            if (switchTurn == true)
                SwitchTurn();

            if (testMove == false)
            {
                PrintBoard(move);
                if (currentTurn == GameState.MyTurn)
                    myTimer -= (DateTime.Now - lastMoveTime);
                else
                    opTimer -= (DateTime.Now - lastMoveTime);
                lastMoveTime = DateTime.Now;
            }
        }
        public int GetExecuteGameMoveScore(Move move)
        {
            int sourceIndexRow = move.sourceRowIndex;
            int sourceIndexCol = move.sourceColIndex;
            int destIndexRow = move.destRowIndex;
            int destIndexCol = move.destColIndex;
            int totalBoardScore = this.totalBoardScore;

            //castling
            if (board[sourceIndexRow, sourceIndexCol].type == PieceType.King)
            {
                if (sourceIndexCol - destIndexCol > 1 || sourceIndexCol - destIndexCol < -1)
                {
                    if (sourceIndexCol - destIndexCol > 0)
                    {
                        totalBoardScore += board[sourceIndexRow, 0].calculateTotalScore(destIndexRow, destIndexCol + 1, myColor);
                        totalBoardScore -= board[sourceIndexRow, 0].calculateTotalScore(sourceIndexRow, 0, myColor);
                    }
                    else
                    {

                        totalBoardScore += board[sourceIndexRow, 7].calculateTotalScore(destIndexRow, destIndexCol - 1, myColor);
                        totalBoardScore -= board[sourceIndexRow, 7].calculateTotalScore(sourceIndexRow, 7, myColor);
                    }
                }
            }

            enPassantCol = -1;
            //En passant
            if (board[sourceIndexRow, sourceIndexCol].type == PieceType.Pawn &&//pawn
                sourceIndexCol != destIndexCol && //pawn takes
                board[destIndexRow, destIndexCol].type == PieceType.Null) //empty sqaure
                    totalBoardScore -= board[sourceIndexRow, destIndexCol].calculateTotalScore(sourceIndexRow, destIndexCol, myColor);

            totalBoardScore -= board[destIndexRow, destIndexCol].calculateTotalScore(destIndexRow, destIndexCol, myColor);
            totalBoardScore += board[sourceIndexRow, sourceIndexCol].calculateTotalScore(destIndexRow, destIndexCol, myColor);
            totalBoardScore -= board[sourceIndexRow, sourceIndexCol].calculateTotalScore(sourceIndexRow, sourceIndexCol, myColor);

            return totalBoardScore;
        }
        public void GetAllPossibleMoves(int sourceRow, int sourceCol, bool checkChess, List<Move> allPossibleMoves)
        {
            GetAllPossibleMoves(sourceRow, sourceCol, checkChess, myColor, allPossibleMoves);
        }
        public void GetAllPossibleMoves(int sourceRow, int sourceCol, bool checkChess, PieceColor color, List<Move> allPossibleMoves)
        {
            //List<Move> allPossibleMoves = new List<Move>();
            Move move;
            Piece piece = board[sourceRow, sourceCol];

            bool capture;
            int rowOffset;
            int colOffset;
            if (piece.color != color) return;
            if (piece.type == PieceType.Pawn)
            {
                int rowOffsetMax;
                int enPassantPossibleRow;
                if (piece.color == PieceColor.White)
                {
                    rowOffset = 1;
                    if (sourceRow == 1)
                        rowOffsetMax = 2;
                    else
                        rowOffsetMax = 1;
                    enPassantPossibleRow = 4;
                }
                else
                {
                    rowOffset = -1;
                    if (sourceRow == 6)
                        rowOffsetMax = -2;
                    else
                        rowOffsetMax = -1;
                    enPassantPossibleRow = 3;
                }
                int i = rowOffset;
                do
                {
                    if (board[sourceRow + i, sourceCol].type == PieceType.Null)
                    {
                        move = new Move(sourceRow, sourceCol, sourceRow + i, sourceCol);
                        move.destWeight = board[move.destRowIndex, move.destColIndex].calculateTotalScore(move.destRowIndex, move.destColIndex, color);
                        if (checkChess)
                        {
                            if (CheckChessHypothetical(move, color) == false)
                                allPossibleMoves.Add(move);
                        }
                        else
                            allPossibleMoves.Add(move);
                    }
                    else
                        break;
                    i += i;
                } while (i == rowOffsetMax);

                if (sourceCol != 7 && board[sourceRow + rowOffset, sourceCol + 1].type != PieceType.Null &&
                    board[sourceRow + rowOffset, sourceCol + 1].color != piece.color)
                {
                    //capture right
                    move = new Move(sourceRow, sourceCol, sourceRow + rowOffset, sourceCol + 1);
                    move.destWeight = board[move.destRowIndex, move.destColIndex].calculateTotalScore(move.destRowIndex, move.destColIndex, color);
                    if (checkChess)
                    {
                        if (CheckChessHypothetical(move, color) == false)
                            allPossibleMoves.Add(move);
                    }
                    else
                        allPossibleMoves.Add(move);
                }

                if (sourceCol != 0 && board[sourceRow + rowOffset, sourceCol - 1].type != PieceType.Null &&
                    board[sourceRow + rowOffset, sourceCol - 1].color != piece.color)
                {
                    //capture left
                    move = new Move(sourceRow, sourceCol, sourceRow + rowOffset, sourceCol - 1);
                    move.destWeight = board[move.destRowIndex, move.destColIndex].calculateTotalScore(move.destRowIndex, move.destColIndex, color);
                    if (checkChess)
                    {
                        if (CheckChessHypothetical(move, color) == false)
                            allPossibleMoves.Add(move);
                    }
                    else
                        allPossibleMoves.Add(move);
                }


                if (sourceRow == enPassantPossibleRow && enPassantCol != -1)
                {
                    //en passant
                    int colDelta = enPassantCol - sourceCol;
                    if (colDelta == -1 || colDelta == 1)
                    {
                        move = new Move(sourceRow, sourceCol, sourceRow + rowOffset, enPassantCol);
                        move.destWeight = board[move.destRowIndex, move.destColIndex].calculateTotalScore(move.destRowIndex, move.destColIndex, color);
                        Piece opPiece = board[sourceRow, enPassantCol];
                        board[sourceRow, enPassantCol] = new Piece();
                        if (checkChess)
                        {
                            if (CheckChessHypothetical(move, color) == false)
                                allPossibleMoves.Add( move);
                        }
                        else
                            allPossibleMoves.Add( move);
                        board[sourceRow, enPassantCol] = opPiece;
                    }
                }
                return;
            }
            if (piece.type == PieceType.King)
            {
                //castling
                bool shortCastleFlag;
                bool longCastleFlag;
                if (piece.color == PieceColor.Black)
                {
                    shortCastleFlag = blackShortCastleFlag;
                    longCastleFlag = blackLongCastleFlag;
                }
                else
                {
                    shortCastleFlag = whiteShortCastleFlag;
                    longCastleFlag = whiteLongCastleFlag;
                }
                if (shortCastleFlag == true)
                {
                    if (board[sourceRow, sourceCol + 1].type == PieceType.Null && board[sourceRow, sourceCol + 2].type == PieceType.Null)
                    {
                        move = new Move(sourceRow, sourceCol, sourceRow, sourceCol + 2);
                        move.destWeight = board[move.destRowIndex, move.destColIndex].calculateTotalScore(move.destRowIndex, move.destColIndex, color);
                        if (checkChess)
                        {
                            if (CheckChess(sourceRow, sourceCol, color) == false &&
                                CheckChessHypothetical(sourceRow, sourceCol + 1, new Move(sourceRow, sourceCol, sourceRow, sourceCol + 1), color) == false &&
                                CheckChessHypothetical(sourceRow, sourceCol + 2, move, color) == false)
                                allPossibleMoves.Add( move);
                        }
                        else
                            allPossibleMoves.Add( move);
                    }
                }
                if (longCastleFlag == true)
                {
                    if (board[sourceRow, sourceCol - 1].type == PieceType.Null && board[sourceRow, sourceCol - 2].type == PieceType.Null &&
                        board[sourceRow, sourceCol - 3].type == PieceType.Null)
                    {
                        move = new Move(sourceRow, sourceCol, sourceRow, sourceCol - 2);
                        move.destWeight = board[move.destRowIndex, move.destColIndex].calculateTotalScore(move.destRowIndex, move.destColIndex, color);
                        if (checkChess)
                        {
                            if (CheckChess(sourceRow, sourceCol, color) == false &&
                                CheckChessHypothetical(sourceRow, sourceCol - 1, new Move(sourceRow, sourceCol, sourceRow, sourceCol - 1), color) == false &&
                                CheckChessHypothetical(sourceRow, sourceCol - 2, move, color) == false)
                                allPossibleMoves.Add( move);
                        }
                        else
                            allPossibleMoves.Add( move);
                    }
                }
            }

            for (int j = 0; j < piece.rowPattern.Length; j++)
            {
                for (int i = 1; i <= 6 * piece.continuesPattern + 1; i++)
                {
                    rowOffset = piece.rowPattern[j] * i;
                    colOffset = piece.colPattern[j] * i;
                    if (sourceRow + rowOffset < 0 || sourceRow + rowOffset > 7 ||
                        sourceCol + colOffset < 0 || sourceCol + colOffset > 7) break; // TBD move up
                    move = new Move(sourceRow, sourceCol, sourceRow + rowOffset, sourceCol + colOffset);
                    move.destWeight = board[move.destRowIndex, move.destColIndex].calculateTotalScore(move.destRowIndex, move.destColIndex, color);
                    capture = false;

                    Piece opPiece = board[sourceRow + rowOffset, sourceCol + colOffset];
                    if (opPiece.type != PieceType.Null)
                    {
                        if (opPiece.color == piece.color)
                            break;
                        capture = true;
                    }
                    //return true;

                    if (checkChess)
                    {
                        if (CheckChessHypothetical(move, color) == false)
                            allPossibleMoves.Add(move);
                    }
                    else
                        allPossibleMoves.Add( move);
                    if (capture) break;
                }
            }

            return;
        }
        private bool CheckChessHypothetical(Move move)
        {
            return CheckChessHypothetical(-1, -1, move, myColor);
        }
        public bool CheckChessHypothetical(Move move, PieceColor color)
        {
            return CheckChessHypothetical(-1, -1, move, color);
        }
        private bool CheckChessHypothetical(int kingRowIndex, int kingColIndex, Move move)
        {
            return CheckChessHypothetical(kingRowIndex, kingColIndex, move, myColor);
        }
        private bool CheckChessHypothetical(int kingRowIndex, int kingColIndex, Move move, PieceColor color)
        {
            int sourceRowIndex = move.sourceRowIndex;
            int sourceColIndex = move.sourceColIndex;
            int destRowIndex = move.destRowIndex;
            int destColIndex = move.destColIndex;
            Piece firstPiece;
            Piece secPiece;
            bool returnFlag = false;
            //int kingRowIndex = -1;
            //int kingColIndex = -1;

            /*TBD
            if (destRowIndex == -1 || destColIndex == -1)
            {*/
            firstPiece = board[sourceRowIndex, sourceColIndex];
            board[sourceRowIndex, sourceColIndex] = nullPiece;
            secPiece = board[destRowIndex, destColIndex];
            board[destRowIndex, destColIndex] = firstPiece;
            //}

            returnFlag = CheckChess(kingRowIndex, kingColIndex, color);

            board[sourceRowIndex, sourceColIndex] = firstPiece;
            /*TBD
            if (!(destRowOffset == 0 && destColOffset == 0))*/
            board[destRowIndex, destColIndex] = secPiece;
            return returnFlag;
        }
        private bool CheckChess()
        {
            return CheckChess(-1, -1, out int kingIndexRow, out int kingIndexCol, myColor);
        }
        private bool CheckChess(int kingRowIndex, int kingColIndex)
        {
            return CheckChess(kingRowIndex, kingColIndex, out int kingIndexRow, out int kingIndexCol, myColor);
        }
        private bool CheckChess(int kingRowIndex, int kingColIndex, PieceColor color)
        {
            return CheckChess(kingRowIndex, kingColIndex, out int kingIndexRow, out int kingIndexCol, color);
        }
        public bool CheckChess(int kingRowIndex, int kingColIndex, out int outKingRowIndex, out int outKingColIndex, PieceColor color)
        {
            //blackPieces = new string[] { bKing, bQueen, bRook, bBishop, bKnight, bPawn };

            Piece opPiece;

            if (kingRowIndex == -1 || kingColIndex == -1)
            {
                for (int i = color == PieceColor.White ? 0 : 7
                    ; color == PieceColor.White ? i < 8 : i > -1
                    ; i += color == PieceColor.White ? 1 : -1)
                    for (int j = 0; j < 8; j++)
                        if (board[i, j].type == PieceType.King && board[i, j].color == color)
                        {
                            kingRowIndex = i;
                            kingColIndex = j;
                            break;
                        }
            }

            outKingRowIndex = kingRowIndex;
            outKingColIndex = kingColIndex;

            //Left
            for (int i = 1; i < 8; i++)
            {
                if (kingColIndex - i < 0) break;
                if (board[kingRowIndex, kingColIndex - i].color == color) break;
                if (board[kingRowIndex, kingColIndex - i].color == PieceColor.Null) continue;
                opPiece = board[kingRowIndex, kingColIndex - i];
                if (opPiece.type == PieceType.Queen || opPiece.type == PieceType.Rook
                    || (i == 1 && opPiece.type == PieceType.King))
                    return true;
                break;
            }
            //Right
            for (int i = 1; i < 8; i++)
            {
                if (kingColIndex + i > 7) break;
                if (board[kingRowIndex, kingColIndex + i].color == color) break;
                if (board[kingRowIndex, kingColIndex + i].color == PieceColor.Null) continue;
                opPiece = board[kingRowIndex, kingColIndex + i];
                if (opPiece.type == PieceType.Queen || opPiece.type == PieceType.Rook
                    || (i == 1 && opPiece.type == PieceType.King))
                    return true;
                break;
            }
            //Up
            for (int i = 1; i < 8; i++)
            {
                if (kingRowIndex - i < 0) break;
                if (board[kingRowIndex - i, kingColIndex].color == color) break;
                if (board[kingRowIndex - i, kingColIndex].color == PieceColor.Null) continue;
                opPiece = board[kingRowIndex - i, kingColIndex];
                if (opPiece.type == PieceType.Queen || opPiece.type == PieceType.Rook
                    || (i == 1 && opPiece.type == PieceType.King))
                    return true;
                break;
            }
            //Down
            for (int i = 1; i < 8; i++)
            {
                if (kingRowIndex + i > 7) break;
                if (board[kingRowIndex + i, kingColIndex].color == color) break;
                if (board[kingRowIndex + i, kingColIndex].color == PieceColor.Null) continue;
                opPiece = board[kingRowIndex + i, kingColIndex];
                if (opPiece.type == PieceType.Queen || opPiece.type == PieceType.Rook
                    || (i == 1 && opPiece.type == PieceType.King))
                    return true;
                break;
            }

            //Up Left
            for (int i = 1; i < 8; i++)
            {
                if (kingRowIndex - i < 0 || kingColIndex - i < 0) break;
                if (board[kingRowIndex - i, kingColIndex - i].color == color) break;
                if (board[kingRowIndex - i, kingColIndex - i].color == PieceColor.Null) continue;
                opPiece = board[kingRowIndex - i, kingColIndex - i];
                if (opPiece.type == PieceType.Queen || opPiece.type == PieceType.Bishop
                    || (i == 1 && (opPiece.type == PieceType.King || (opPiece.type == PieceType.Pawn && color == PieceColor.Black))))
                    return true;
                break;
            }
            //Up Right
            for (int i = 1; i < 8; i++)
            {
                if (kingRowIndex - i < 0 || kingColIndex + i > 7) break;
                if (board[kingRowIndex - i, kingColIndex + i].color == color) break;
                if (board[kingRowIndex - i, kingColIndex + i].color == PieceColor.Null) continue;
                opPiece = board[kingRowIndex - i, kingColIndex + i];
                if (opPiece.type == PieceType.Queen || opPiece.type == PieceType.Bishop
                    || (i == 1 && (opPiece.type == PieceType.King || (opPiece.type == PieceType.Pawn && color == PieceColor.Black))))
                    return true;
                break;
            }
            //Down Left
            for (int i = 1; i < 8; i++)
            {
                if (kingRowIndex + i > 7 || kingColIndex - i < 0) break;
                if (board[kingRowIndex + i, kingColIndex - i].color == color) break;
                if (board[kingRowIndex + i, kingColIndex - i].color == PieceColor.Null) continue;
                opPiece = board[kingRowIndex + i, kingColIndex - i];
                if (opPiece.type == PieceType.Queen || opPiece.type == PieceType.Bishop
                    || (i == 1 && (opPiece.type == PieceType.King || (opPiece.type == PieceType.Pawn && color == PieceColor.White))))
                    return true;
                break;
            }
            //Down Right
            for (int i = 1; i < 8; i++)
            {
                if (kingRowIndex + i > 7 || kingColIndex + i > 7) break;
                if (board[kingRowIndex + i, kingColIndex + i].color == color) break;
                if (board[kingRowIndex + i, kingColIndex + i].color == PieceColor.Null) continue;
                opPiece = board[kingRowIndex + i, kingColIndex + i];
                if (opPiece.type == PieceType.Queen || opPiece.type == PieceType.Bishop
                    || (i == 1 && (opPiece.type == PieceType.King || (opPiece.type == PieceType.Pawn && color == PieceColor.White))))
                    return true;
                break;
            }

            //Knight
            if (kingRowIndex - 2 > -1 && kingColIndex + 1 < 8 &&
                board[kingRowIndex - 2, kingColIndex + 1].color != color &&
                board[kingRowIndex - 2, kingColIndex + 1].type == PieceType.Knight)
                return true;
            if (kingRowIndex - 1 > -1 && kingColIndex + 2 < 8 &&
                board[kingRowIndex - 1, kingColIndex + 2].color != color &&
                board[kingRowIndex - 1, kingColIndex + 2].type == PieceType.Knight)
                return true;
            if (kingRowIndex + 1 < 8 && kingColIndex + 2 < 8 &&
                board[kingRowIndex + 1, kingColIndex + 2].color != color &&
                board[kingRowIndex + 1, kingColIndex + 2].type == PieceType.Knight)
                return true;
            if (kingRowIndex + 2 < 8 && kingColIndex + 1 < 8 &&
                board[kingRowIndex + 2, kingColIndex + 1].color != color &&
                board[kingRowIndex + 2, kingColIndex + 1].type == PieceType.Knight) //TBD
                return true;
            if (kingRowIndex + 2 < 8 && kingColIndex - 1 > -1 &&
                board[kingRowIndex + 2, kingColIndex - 1].color != color &&
                board[kingRowIndex + 2, kingColIndex - 1].type == PieceType.Knight)
                return true;
            if (kingRowIndex + 1 < 8 && kingColIndex - 2 > -1 &&
                board[kingRowIndex + 1, kingColIndex - 2].color != color &&
                board[kingRowIndex + 1, kingColIndex - 2].type == PieceType.Knight)
                return true;
            if (kingRowIndex - 1 > -1 && kingColIndex - 2 > -1 &&
                board[kingRowIndex - 1, kingColIndex - 2].color != color &&
                board[kingRowIndex - 1, kingColIndex - 2].type == PieceType.Knight)
                return true;
            if (kingRowIndex - 2 > -1 && kingColIndex - 1 > -1 &&
                board[kingRowIndex - 2, kingColIndex - 1].color != color &&
                board[kingRowIndex - 2, kingColIndex - 1].type == PieceType.Knight)
                return true;
            return false;
        }
        public void PromoteMe(Piece piece)
        {
            Promote(myColor, piece, false);
        }
        public void PromoteOp(Piece piece)
        {
            Promote(opColor, piece, false);
        }
        public void Promote(PieceColor color, Piece piece, bool testMove)
        {
            int rowIndex = -1;
            int colIndex = -1;
            if (color == PieceColor.White)
                rowIndex = 7;
            else
                rowIndex = 0;
            for (int i = 0; i < 8; i++)
            {
                if (board[rowIndex, i].type == PieceType.Pawn)
                {
                    colIndex = i;
                    totalBoardScore -= board[rowIndex, colIndex].calculateTotalScore(rowIndex, colIndex, myColor);
                    board[rowIndex, colIndex] = piece;
                    totalBoardScore += board[rowIndex, colIndex].calculateTotalScore(rowIndex, colIndex, myColor);

                    break;
                }
            }

            if (testMove == false)
            {
                Move move = new Move();
                move.destRowIndex = rowIndex;
                move.destColIndex = colIndex;
                PrintBoard(move);
                if (currentTurn == GameState.MyTurn)
                    myTimer -= (DateTime.Now - lastMoveTime);
                else
                    opTimer -= (DateTime.Now - lastMoveTime);
                lastMoveTime = DateTime.Now;
            }
        }
        public int GetPromoteScore(Move move,int previousScore, PieceColor color, Piece piece)
        {
            int totalBoardScore = previousScore;
            int rowIndex = -1;
            int colIndex = move.destColIndex;
            if (color == PieceColor.White)
                rowIndex = 7;
            else
                rowIndex = 0;
            if(move.destRowIndex == rowIndex)
            {
                totalBoardScore -= board[move.sourceRowIndex, colIndex].calculateTotalScore(rowIndex, colIndex, myColor);
                totalBoardScore += piece.calculateTotalScore(rowIndex, colIndex, myColor);
            }
            return totalBoardScore;
        }
        #endregion

        #region GUI Functions
        public void HighlightPossibleMoves(int rowIndex, int colIndex)
        {
            List<Move> allPossibleMoves = new List<Move>();
            GetAllPossibleMoves(rowIndex, colIndex, true, allPossibleMoves);

            foreach (Move move in allPossibleMoves)
                allSquaresPanels[FlipRowIndex(move.destRowIndex), FlipColIndex(move.destColIndex)].BackColor = Color.PaleGreen;
        }
        public void PrintBoard(Move move)
        {
            PrintBoardPattern(move);

            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    allSquaresPictureBoxes[FlipRowIndex(i), FlipColIndex(j)].Image = board[i, j].pieceGUI;

            for (int i = 0; i < 8; i++)
            {
                LLabels[i].Text = (FlipRowIndex(i) + 1).ToString();
                RLabels[i].Text = (FlipRowIndex(i) + 1).ToString();
                ULabels[i].Text = ((char)(FlipColIndex(i) + (int)'A')).ToString();
                DLabels[i].Text = ((char)(FlipColIndex(i) + (int)'A')).ToString();
            }

            if (scoreLabel != null) scoreLabel.Text = totalBoardScore.ToString();
        }
        public void PrintBoardPattern(Move move)
        {
            Color blackSquare = Color.FromArgb(192, 64, 0);
            Color whiteSquare = Color.FromArgb(255, 224, 192);
            Color[] allColors = new Color[2];
            allColors[0] = whiteSquare;
            allColors[1] = blackSquare;
            int counter = 0;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    allSquaresPanels[i, j].BackColor = allColors[counter % 2];
                    counter++;
                }
                counter++;
            }
            int kingRowIndex;
            int kingColIndex;
            if (CheckChess(-1, -1, out kingRowIndex, out kingColIndex, myColor))
                allSquaresPanels[FlipRowIndex(kingRowIndex), FlipColIndex(kingColIndex)].BackColor = Color.Salmon;
            if(move.destRowIndex != -1 && move.destColIndex != -1 && board[move.destRowIndex, move.destColIndex].color == opColor)
                allSquaresPanels[FlipRowIndex(move.destRowIndex), FlipColIndex(move.destColIndex)].BackColor = Color.GhostWhite;
        }
        #endregion

        #region helpers functions
        public int FlipRowIndex(int rowIndex)
        {
            bool flipVertical;

            if (myColor == PieceColor.White)
                flipVertical = true;
            else
                flipVertical = false;

            return flipVertical ? 7 - rowIndex : rowIndex;
        }
        public int FlipColIndex(int colIndex)
        {
            bool flipHorizontal;

            if (myColor == PieceColor.White)
                flipHorizontal = false;
            else
                flipHorizontal = true;

            return flipHorizontal ? 7 - colIndex : colIndex;
        }
        public int CalculateScores()
        {
            int totalBoardScore = 0;
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                {
                    if (board[i, j].type == PieceType.Null) continue;
                    totalBoardScore += board[i, j].calculateTotalScore(i, j, myColor);
                }

            return totalBoardScore;
        }
        public ChessBoard Copy()
        {
            ChessBoard copy = new ChessBoard();
            copy.board = new Piece[8, 8];
            Array.Copy(board, 0, copy.board, 0, board.Length);
            copy.myColor = myColor;
            copy.opColor = opColor;
            copy.nullPiece = new Piece();
            copy.enPassantCol = enPassantCol;
            copy.whiteShortCastleFlag = whiteShortCastleFlag;
            copy.whiteLongCastleFlag = whiteLongCastleFlag;
            copy.blackShortCastleFlag = blackShortCastleFlag;
            copy.blackLongCastleFlag = blackLongCastleFlag;
            copy.totalBoardScore = totalBoardScore;
            return copy;
        }
        public static PieceColor GetOpColor(PieceColor color)
        {
            if (color == PieceColor.White)
                return PieceColor.Black;
            else
                return PieceColor.White;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ChessBoard);
        }

        public bool Equals(ChessBoard other)
        {
            return other != null &&
                   EqualityComparer<Piece[,]>.Default.Equals(board, other.board) &&
                   currentTurn == other.currentTurn &&
                   enPassantCol == other.enPassantCol &&
                   whiteShortCastleFlag == other.whiteShortCastleFlag &&
                   whiteLongCastleFlag == other.whiteLongCastleFlag &&
                   blackShortCastleFlag == other.blackShortCastleFlag &&
                   blackLongCastleFlag == other.blackLongCastleFlag;
        }

        public override int GetHashCode()
        {
            var hashCode = -1594356302;

            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    hashCode = hashCode * -1521134295 + EqualityComparer<Piece>.Default.GetHashCode(board[i,j]);
            //hashCode = hashCode * -1521134295 + EqualityComparer<Piece[,]>.Default.GetHashCode(board);
            hashCode = hashCode * -1521134295 + currentTurn.GetHashCode();
            hashCode = hashCode * -1521134295 + enPassantCol.GetHashCode();
            hashCode = hashCode * -1521134295 + whiteShortCastleFlag.GetHashCode();
            hashCode = hashCode * -1521134295 + whiteLongCastleFlag.GetHashCode();
            hashCode = hashCode * -1521134295 + blackShortCastleFlag.GetHashCode();
            hashCode = hashCode * -1521134295 + blackLongCastleFlag.GetHashCode();
            return hashCode;
        }
        #endregion
    }
}
