using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;

namespace Chess_Client
{
    internal class AutoChessBoard
    {
        private Piece[,] userBuf;
        private Piece[,] opBuf;
        System.Net.Sockets.NetworkStream stream = null;
        StreamReader sr = null;
        public BluetoothClient boardClient;
        public bool waitForUserTurn = false;
        public Config config_File;
        public string testingFile;
        public bool Connected
        {
            get { return boardClient != null && boardClient.Connected; }
        }
        public AutoChessBoard(Config config_File, string testingFile)
        {
            this.waitForUserTurn = false;
            this.boardClient = null;
            this.config_File = config_File;
            this.testingFile = testingFile;
            boardClient = new BluetoothClient();
            BoardConnection boardConn = new BoardConnection(boardClient);
            boardConn.ShowDialog();
        }
        #region Board Functions
        public void SendBoardMove(Move move, ChessBoard currentGameBoard)
        {
            SendBoardMove(move, currentGameBoard, null);
        }
        public void SendBoardMove(Move move, ChessBoard currentGameBoard, Move bufferMove)
        {
            /*
            destination (captured piece) to relevant buffer entrence with shortest path
            destination (captured piece) to buffer
            destination (captured piece) return shortest path moved pieces
            source to destination (now empty) with shortest path
            source to destination (now empty) return shortest path moved pieces
             */
            if (boardClient == null || boardClient.Connected == false)
                return;

            SendToBoard("startmove", false);

            if (bufferMove == null && currentGameBoard.board[move.destRowIndex, move.destColIndex].type != PieceType.Null)
            {
                //capture
                Move tempBufferMove = GetNextCaptureMove(move, currentGameBoard);
                Move preBufferMove = new Move(move.destRowIndex, move.destColIndex, tempBufferMove.sourceRowIndex, tempBufferMove.sourceColIndex);
                SendBoardMove(preBufferMove, currentGameBoard, tempBufferMove);
                currentGameBoard.board[move.destRowIndex, move.destColIndex] = new Piece();
            }

            List<List<Move>> allMovesToSend = PathFindingAlg.GetShortestPath(currentGameBoard.Copy(), move, null, null);


            for (int i = 0; i < allMovesToSend.Count; i++)
            {
                for (int j = 0; j < allMovesToSend[i].Count; j++)
                {
                    Move flippedMove = FlipMove(allMovesToSend[i][j], currentGameBoard);
                    if (SendToBoard(flippedMove.ToString(), true) == false)
                    {
                        MessageBox.Show("Board rejected move");
                    }
                }
            }
            if (bufferMove != null)
            {
                Move flippedMove = new Move(FlipRowIndex(bufferMove.sourceRowIndex, currentGameBoard),
                    FlipColIndex(bufferMove.sourceColIndex, currentGameBoard),
                    bufferMove.destRowIndex,
                    bufferMove.destColIndex);
                if (SendToBoard(flippedMove.ToString(), true) == false)
                {
                    MessageBox.Show("Board rejected move");
                }
            }

            for (int i = allMovesToSend.Count - 2; i >= 0; i--)
            {
                for (int j = allMovesToSend[i].Count - 1; j >= 0; j--)
                {
                    Move flippedMove = FlipMove(allMovesToSend[i][j].ReverseMove(), currentGameBoard);
                    if (SendToBoard(flippedMove.ToString(), true) == false)
                    {
                        MessageBox.Show("Board rejected move");
                    }
                }
            }

            if(currentGameBoard.board[move.sourceRowIndex, move.sourceColIndex].type == PieceType.King && 
                (move.sourceColIndex - move.destColIndex > 1 || move.sourceColIndex - move.destColIndex < -1))
            {
                //castling
                currentGameBoard.board[move.destRowIndex, move.destColIndex] = currentGameBoard.board[move.sourceRowIndex, move.sourceColIndex];
                currentGameBoard.board[move.sourceRowIndex, move.sourceColIndex] = new Piece();
                Move castlingMove = null;
                if (move.sourceColIndex - move.destColIndex > 0)
                {
                    castlingMove = new Move(move.sourceRowIndex, 0, move.destRowIndex, move.destColIndex + 1);
                }  
                else
                {
                    castlingMove = new Move(move.sourceRowIndex, 7, move.destRowIndex, move.destColIndex - 1);
                }
                SendBoardMove(castlingMove, currentGameBoard, null);
                return;
            }
            if (bufferMove == null)
                SendToBoard("stopmove", false);
        }

        public void SendBoardPromo(Piece piece, Move move, ChessBoard currentGameBoard)
        {
            if (boardClient == null || boardClient.Connected == false)
                return;

            if(move == null) //infer move
            {
                int rowIndex = -1;
                if (currentGameBoard.opColor == PieceColor.White)
                    rowIndex = 7;
                else
                    rowIndex = 0;
                for (int i = 0; i < 8; i++)
                {
                    if (currentGameBoard.board[rowIndex, i].type == PieceType.Pawn)
                    {
                        move = new Move(-1, -1, rowIndex, i);
                        break;
                    }
                }
            }

            string line;
            bool fail = true;
            do
            {
                fail = true;
                MessageBox.Show("Promote Pawn on square " + FlipPieceToMove(new PieceToMove(move), currentGameBoard).ToString() + " to " + piece.ToString());

                SendToBoard("report", false);
                line = GetMsgFromBoard();
                while (line != "end")
                {
                    PieceToMove sqaure = FlipPieceToMove(new PieceToMove(line), currentGameBoard);
                    if (sqaure == new PieceToMove(move))
                        fail = false;
                    line = GetMsgFromBoard();
                }
            } while (fail == true);
        }
        
        public void StartUserTurn()
        {
            SendToBoard("online", false);
            this.waitForUserTurn = true;
        }
        public void StopUserTurn()
        {
            SendToBoard("stoponline", false);
            this.waitForUserTurn = false;
        }
        #endregion
        #region Game Start Functions
        public bool GameInit(ChessBoard currentGameBoard)
        {
            bool fail = false;
            string line = "";
            InitBuffer();
            do
            {
                ChessBoard tempBoard = currentGameBoard.Copy();
                SendToBoard("report", false);
                fail = false;
                line = GetMsgFromBoard();
                while (line != "end")
                {
                    if (fail == true)
                    {
                        line = GetMsgFromBoard();
                        continue; //just to clear the socket
                    }
                    PieceToMove sqaure = FlipPieceToMove(new PieceToMove(line), tempBoard);
                    if (tempBoard.board[sqaure.rowIndex, sqaure.colIndex].type == PieceType.Null)
                    {
                        MessageBox.Show(line + " sqaure should be empty, there is a piece there!");
                        fail = true;
                    }
                    tempBoard.board[sqaure.rowIndex, sqaure.colIndex] = new Piece();
                    line = GetMsgFromBoard();
                }
                for(int i = 0; i < 8; i++)
                {
                    for(int j = 0; j < 8; j++)
                    {
                        if(tempBoard.board[i,j].type != PieceType.Null)
                        {
                            MessageBox.Show(FlipPieceToMove(new PieceToMove(i, j), currentGameBoard).ToString() + " sqaure is empty, there should be a " +
                                tempBoard.board[i, j].ToString() + " there!");
                            fail = true;
                        }
                    }
                }    
            } while (fail == true);
            return true;
        }
        #endregion
        #region Buffer Functions
        public void InitBuffer()
        {
            userBuf = new Piece[8, 2];
            opBuf = new Piece[8, 2];
            Piece nullPiece = new Piece();
            for(int i = 0; i < 8; i++)
            {
                userBuf[i, 0] = nullPiece;
                userBuf[i, 1] = nullPiece;
                opBuf[i, 0] = nullPiece;
                opBuf[i, 1] = nullPiece;
            }
        }
        public Move GetNextCaptureMove(Move move, ChessBoard currentGameBoard)
        {
            Move bufMove = new Move();
            bool found = true;
            bufMove.sourceColIndex = currentGameBoard.currentTurn == GameState.OpTurn ? 
                (currentGameBoard.myColor == PieceColor.White ? 7 : 0) :
                (currentGameBoard.myColor == PieceColor.White ? 0 : 7);
            found = false;
            if (currentGameBoard.currentTurn == GameState.OpTurn)
            {
                for (int j = 0; j == 0 || (j == 1 && found == true); j++)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        if (userBuf[i, 1].type == PieceType.Null)
                        {
                            found = true;
                            if (currentGameBoard.board[i, bufMove.sourceColIndex].type != PieceType.Null && j == 0)
                                continue;
                            bufMove.sourceRowIndex = i;
                            bufMove.destRowIndex = 1;
                            bufMove.destColIndex = (int)('u') - (int)('a');
                            userBuf[i, 1] = currentGameBoard.board[move.destRowIndex, move.destColIndex];
                            return bufMove;
                        }
                    }
                }
                found = false;
                for (int j = 0; j == 0 || (j == 1 && found == true); j++)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        if (userBuf[i, 0].type == PieceType.Null)
                        {
                            found = true;
                            if (currentGameBoard.board[i, bufMove.sourceColIndex].type != PieceType.Null && j == 0)
                                continue;
                            bufMove.sourceRowIndex = i;
                            bufMove.destRowIndex = 0;
                            bufMove.destColIndex = (int)('u') - (int)('a');
                            userBuf[i, 0] = currentGameBoard.board[move.destRowIndex, move.destColIndex];
                            return bufMove;
                        }
                    }
                }
            }
            else
            {
                for (int j = 0; j == 0 || (j == 1 && found == true); j++)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        if (opBuf[i, 1].type == PieceType.Null)
                        {
                            found = true;
                            if (currentGameBoard.board[i, bufMove.sourceColIndex].type != PieceType.Null && j == 0)
                                continue;
                            bufMove.sourceRowIndex = i;
                            bufMove.destRowIndex = 1;
                            bufMove.destColIndex = (int)('o') - (int)('a');
                            opBuf[i, 1] = currentGameBoard.board[move.destRowIndex, move.destColIndex];
                            return bufMove;
                        }
                    }
                }
                found = false;
                for (int j = 0; j == 0 || (j == 1 && found == true); j++)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        if (opBuf[i, 0].type == PieceType.Null)
                        {
                            found = true;
                            if (currentGameBoard.board[i, bufMove.sourceColIndex].type != PieceType.Null && j == 0)
                                continue;
                            bufMove.sourceRowIndex = i;
                            bufMove.destRowIndex = 0;
                            bufMove.destColIndex = (int)('o') - (int)('a');
                            opBuf[i, 0] = currentGameBoard.board[move.destRowIndex, move.destColIndex];
                            return bufMove;
                        }
                    }
                }
            }    
            return bufMove;
        }
        #endregion
        #region helper functions
        public Move FlipMove(Move move, ChessBoard currentGameBoard)
        {
            return new Move(FlipRowIndex(move.sourceRowIndex, currentGameBoard),
                        FlipColIndex(move.sourceColIndex, currentGameBoard),
                        FlipRowIndex(move.destRowIndex, currentGameBoard),
                        FlipColIndex(move.destColIndex, currentGameBoard));
        }
        public PieceToMove FlipPieceToMove(PieceToMove square, ChessBoard currentGameBoard)
        {
            return new PieceToMove(FlipRowIndex(square.rowIndex, currentGameBoard),
                        FlipColIndex(square.colIndex, currentGameBoard));
        }
        public int FlipRowIndex(int rowIndex, ChessBoard currentGameBoard)
        {
            bool flipVertical;

            if (currentGameBoard.myColor == PieceColor.Black)
                flipVertical = true;
            else
                flipVertical = false;

            return flipVertical ? 7 - rowIndex : rowIndex;
        }
        public int FlipColIndex(int colIndex, ChessBoard currentGameBoard)
        {
            bool flipHorizontal;

            if (currentGameBoard.myColor == PieceColor.Black)
                flipHorizontal = true;
            else
                flipHorizontal = false;

            return flipHorizontal ? 7 - colIndex : colIndex;
        }
        #endregion
        #region CloseBoard function
        public void CloseBoard()
        {
            if (this.Connected)
                this.boardClient.Close();
            this.boardClient = null;
        }
        #endregion
        #region board communication functions
        public bool SendToBoard(string line, bool ack)
        {
            if (this.Connected == false)
                return false;
            if (stream == null)
                stream = boardClient.GetStream();
            StreamWriter sw = new StreamWriter(stream, System.Text.Encoding.ASCII);
            sw.WriteLine(line);
            sw.Flush();
            if (config_File.Testing == true)
                using (StreamWriter writer = new StreamWriter(testingFile, true))
                {
                    writer.WriteLine(DateTime.Now.ToString("HH:mm:ss ") + "board send: " + line);
                }
            if (ack == true)
                return GetBoardAck();
            return true;
        }
        public bool CheckMsgFromBoard()
        {
            if (stream == null)
                stream = boardClient.GetStream();
            return stream.DataAvailable;
        }
        public string GetMsgFromBoard()
        {
            bool fail;
            string line;
            do
            {
                fail = false;
                if (this.Connected == false)
                    return null;
                if (stream == null)
                    stream = boardClient.GetStream();
                if (sr == null)
                    sr = new StreamReader(stream, System.Text.Encoding.ASCII);
                line = sr.ReadLine();
                //line = sr.ReadLine();
                //line = sr.ReadLine();
                //line = sr.ReadLine();
                line.TrimEnd('\r');
                if (config_File.Testing == true)
                    using (StreamWriter writer = new StreamWriter(testingFile, true))
                    {
                        writer.WriteLine(DateTime.Now.ToString("HH:mm:ss ") + "board recv: " + line);
                    }
                if (line.StartsWith("Error") == true)
                {
                    MessageBox.Show(line);
                    fail = true;
                }
            } while (fail == true);
            return line;
        }
        public bool GetBoardAck()
        {
            if (this.Connected == false)
                return false;
            string line = GetMsgFromBoard();
            return line == "Ack__";
        }
        public void CleanMsgsFromBoard()
        {
            if (this.Connected == false)
                return;
            while(CheckMsgFromBoard())
            {
                GetMsgFromBoard();
            }
        }
        #endregion
    }
}
