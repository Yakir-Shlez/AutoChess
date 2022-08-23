using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace Chess_Client
{
    class ChessAI
    {
        private int depth;
        ChessBoard currentGameBoard;
        private AutoChess GUI;
        private bool testing;
        private bool testingAIBrain;
        private string testingFile;
        private List<CheckedBoard> allreadySeenBoards;
        private PieceColor AIColor;
        private List<Move> PreviousMoves;
        private int moveCounter;
        private string xlpath;

        public ChessAI(ChessBoard currentGameBoard, int depth, PieceColor AIColor) :
            this(currentGameBoard, depth, AIColor, null, false, false, "")
        { }
        public ChessAI(ChessBoard currentGameBoard, int depth, PieceColor AIColor, AutoChess GUI, bool testing, bool testingAIBrain, string testingLogPath)
        {
            this.currentGameBoard = currentGameBoard;
            this.depth = depth;
            this.GUI = GUI;
            this.testing = testing;
            this.testingAIBrain = testingAIBrain;
            this.allreadySeenBoards = new List<CheckedBoard>();
            this.AIColor = AIColor;
            this.PreviousMoves = new List<Move>();
            this.moveCounter = 1;
            this.xlpath = testingLogPath + @"\Chess_Client_AI_Brain_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".xlsx";
            if (testing)
            {
                this.testingFile = testingLogPath + @"\Chess_Client_AI_Log_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".txt";
                File.Create(testingFile).Dispose();
                using (StreamWriter writer = new StreamWriter(testingFile, true))
                {
                    writer.WriteLine("depth: " + depth.ToString());
                }
            }
        }
        public Move PlayMove()
        {
            DateTime startTime = new DateTime(0);
            if (testing == true)
            {
                startTime = DateTime.Now;
                using (StreamWriter writer = new StreamWriter(testingFile, true))
                {
                    writer.WriteLine("play move start time: " + startTime.ToString());
                }
            }
            /*
            for(int i = 0;i < allreadySeenBoards.Count; i++)
            {
                if(allreadySeenBoards[i].numberOfPieces > currentGameBoard.numberOfPieces)
                {
                    allreadySeenBoards.Remove(allreadySeenBoards[i]);
                    i--;
                }
            }*/
            int bestScore;
            Move bestMove;
            Move forbiddenMove = new Move();
            if (PreviousMoves.Count == 2)
                forbiddenMove = PreviousMoves[0];
            //TBD start
            //if (PreviousMoves.Count == 2 && PreviousMoves[0].ToString() == "b4-c4" && PreviousMoves[1].ToString() == "c4-b4")
            //{
            //    bool found = true;
            //}
            //TBD end
            AINode AIBrain;
            bestMove = minimax(currentGameBoard.Copy(), 1, int.MinValue, int.MaxValue, AIColor, forbiddenMove, out bestScore, out AIBrain);

            PreviousMoves.Add(bestMove);
            if (PreviousMoves.Count == 3)
                PreviousMoves.RemoveAt(0);
            if (testing == true)
            {
                if (testingAIBrain == true)
                    PrintBrain(AIBrain);
                using (StreamWriter writer = new StreamWriter(testingFile, true))
                {
                    DateTime endTime = DateTime.Now;
                    writer.WriteLine("play move end time: " + endTime.ToString());
                    writer.WriteLine("play move delta time (seconds): " + (endTime - startTime).TotalSeconds.ToString());
                    writer.WriteLine("play move delta time (ticks): " + (endTime - startTime).Ticks.ToString());
                }
            }

            return bestMove;
        }
        private Move minimax(ChessBoard board, int depth, int alpha, int beta, PieceColor color, Move forbiddenMove, out int bestScore, out AINode currentNode)
        {

            //TBD debugging start
            ///*
            /*
            if (depth == 6 && board.board[0, 4].type == PieceType.King && board.board[0, 3].type == PieceType.Queen && board.board[0, 3].color == PieceColor.Black &&
                (board.board[0, 2].color == PieceColor.Black || board.board[0, 2].color == PieceColor.Null))
            {
                bool found = true;
            }
            //*/
            //TBD debugging end
            //TBD debugging start
            ///*
            /*
            int wkingRowIndex = -1;
            int wkingColIndex = -1;
            int bkingRowIndex = -1;
            int bkingColIndex = -1;

            for (int i = color == PieceColor.White ? 0 : 7
                ; color == PieceColor.White ? i < 8 : i > -1
                ; i += color == PieceColor.White ? 1 : -1)
                for (int j = 0; j < 8; j++)
                {
                    if (board.board[i, j].type == PieceType.King)
                    {
                        if (board.board[i, j].color == PieceColor.White)
                        {
                            wkingRowIndex = i;
                            wkingColIndex = j;
                        }
                        else
                        {
                            bkingRowIndex = i;
                            bkingColIndex = j;
                        }
                        break;
                    }
                }
            if (wkingRowIndex == -1 || bkingRowIndex == -1)
                throw new Exception();

            /*
            if (depth == 5 && copy.board[6, 5].type == PieceType.King && copy.board[4, 6].type == PieceType.Knight)
            {
                bool found = true;
            }
            //*/
            //TBD debugging end
            currentNode = new AINode(depth);
            List<Move> allPossibleMoves;
            /*
            CheckedBoard prevBoard = null;
            int hashCode = board.GetHashCode();

            foreach (CheckedBoard testBoard in allreadySeenBoards)
            {
                if (testBoard.hashCode == hashCode)
                {
                    prevBoard = testBoard;
                    break;
                }
            }
            */
            //if (prevBoard == null)
            //{

            bool checkChess = board.CheckChess(-1, -1, out int temp1, out int temp2, color);

            allPossibleMoves = new List<Move>();

            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    if (board.board[i, j].color == color)
                    {
                        board.GetAllPossibleMoves(i, j, checkChess, color, allPossibleMoves);
                    }


            //allreadySeenBoards.Add(new CheckedBoard(hashCode, allPossibleMoves, board.numberOfPieces, board.CopyForTesting()));
            //}
            //else
            //allPossibleMoves = prevBoard.allMoves;
            //if (prevBoard != null)
            //{
            //  bool found = true;
            //}

            //allPossibleMoves.
            //TBD testing start
            ///*
            if (testing == true && depth == 1)
            {
                using (StreamWriter writer = new StreamWriter(testingFile, true))
                {
                    writer.WriteLine("play total availabe moves: " + allPossibleMoves.Count.ToString());
                }
            }
            //*/
            //TBD testing end


            bool max;
            if (color == currentGameBoard.opColor)
            {
                max = false;
                bestScore = int.MaxValue;
                allPossibleMoves = allPossibleMoves.OrderBy(o => o.destWeight).ToList();
            }
            else
            {
                max = true;
                bestScore = int.MinValue;
                allPossibleMoves = allPossibleMoves.OrderByDescending(o => o.destWeight).ToList();
            }
            currentNode.max = max;
            currentNode.allPossibleMoves = allPossibleMoves;
            currentNode.allPossibleNodes = new List<AINode>();
            currentNode.allScores = new List<int>();

            int currentScore;
            Move bestMove = new Move();

            foreach (Move move in allPossibleMoves)
            {
                //TBD testing start
                ///*
                if (testing == true && depth == 1 && GUI != null)
                    GUI.UpdateAiProgress(allPossibleMoves.IndexOf(move).ToString() + "/" + allPossibleMoves.Count.ToString());
                //*/
                //TBD testing end

                //TBD debugging start
                /*
                if (move.ToString()[3] == 'd' && move.ToString()[4] == '3' && depth == 4 && board.board[move.sourceRowIndex, move.sourceColIndex].type == PieceType.King)
                {
                    bool found = true;
                }   
                if (depth == 6 && move.ToString() == "e1-c1" && board.board[0, 4].type == PieceType.King && allPossibleMoves.Count == 32 && bestScore == -1195 && bestMove.ToString() == "a4-c6")
                {
                    bool found = true;
                }
                if(move.ToString()[0] == 'a' && move.ToString()[0] == '1')
                {
                    bool found = true;
                }
                //*/
                //TBD debugging end
                if (checkChess == false && board.CheckChessHypothetical(move, color) == true)
                    continue;
                if (move.sourceRowIndex == forbiddenMove.sourceRowIndex &&
                    move.sourceColIndex == forbiddenMove.sourceColIndex &&
                    move.destRowIndex == forbiddenMove.destRowIndex &&
                    move.destColIndex == forbiddenMove.destColIndex)
                    continue;

                if (depth == this.depth)
                {
                    currentScore = board.GetExecuteGameMoveScore(move);
                    if ((color == PieceColor.White && move.destRowIndex == 7 &&
                        board.board[move.destRowIndex, move.destColIndex].type == PieceType.Pawn) ||
                        (color == PieceColor.Black && move.destRowIndex == 0 &&
                        board.board[move.destRowIndex, move.destColIndex].type == PieceType.Pawn))
                    {
                        currentScore = board.GetPromoteScore(move, currentScore, color, new Piece(PieceType.Queen, color));
                    }
                    currentNode.allScores.Add(currentScore);
                }
                else
                {
                    ChessBoard copy = board.Copy();
                    copy.ExecuteGameMove(move, true, true);
                    if ((color == PieceColor.White && move.destRowIndex == 7 &&
                        copy.board[move.destRowIndex, move.destColIndex].type == PieceType.Pawn) ||
                        (color == PieceColor.Black && move.destRowIndex == 0 &&
                        copy.board[move.destRowIndex, move.destColIndex].type == PieceType.Pawn))
                    {
                        copy.Promote(color, new Piece(PieceType.Queen, color), true);
                    }
                    AINode sonNode;
                    minimax(copy, depth + 1, alpha, beta, ChessBoard.GetOpColor(color), forbiddenMove, out currentScore, out sonNode);
                    currentNode.allScores.Add(currentScore);
                    currentNode.allPossibleNodes.Add(sonNode);
                }

                //TBD debugging start
                /*        
                if (depth == 6 && copy.board[0, 2].type == PieceType.King && copy.board[0, 3].type == PieceType.Queen && copy.board[0, 3].color == PieceColor.Black)
                {
                    bool found = true;
                }
                wkingRowIndex = -1;
                wkingColIndex = -1;
                bkingRowIndex = -1;
                bkingColIndex = -1;
                
                for (int i = color == PieceColor.White ? 0 : 7
                    ; color == PieceColor.White ? i < 8 : i > -1
                    ; i += color == PieceColor.White ? 1 : -1)
                    for (int j = 0; j < 8; j++)
                    {
                        if (copy.board[i, j].type == PieceType.King)
                        {
                            if (copy.board[i, j].color == PieceColor.White)
                            {
                                wkingRowIndex = i;
                                wkingColIndex = j;
                            }
                            else
                            {
                                bkingRowIndex = i;
                                bkingColIndex = j;
                            }
                            break;
                        }
                    }
                if (wkingRowIndex == -1 || bkingRowIndex == -1)
                    throw new Exception();

                if (depth == 5 && copy.board[6, 5].type == PieceType.King && copy.board[4, 6].type == PieceType.Knight)
                {
                    bool found = true;
                }
                //*/
                //TBD debugging end

                //TBD debugging start
                /*
                if (move.ToString() == "g2-g1")
                {
                    bool found = true;
                }
                if (move.ToString() == "g2-f1")
                {
                    bool found = true;
                }
                //*/
                //TBD debugging end
                if (max == true)
                {
                    if (currentScore > bestScore) //TBD
                    {
                        bestMove = move;
                        bestScore = currentScore;
                    }

                    if (currentScore > alpha)
                        alpha = currentScore;
                    if (beta <= alpha)
                        break;
                }
                else
                {
                    if (currentScore < bestScore) //TBD
                    {
                        bestMove = move;
                        bestScore = currentScore;
                    }

                    if (currentScore < beta)
                        beta = currentScore;
                    if (beta <= alpha)
                        break;
                }
            }
            currentNode.bestMove = bestMove;
            currentNode.bestScore = bestScore;
            return bestMove;
        }
        private void PrintBrain(AINode node)
        {
            Excel.Application xlApp;
            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;
            xlApp = new Microsoft.Office.Interop.Excel.Application();
            if (moveCounter == 1)
            {
                xlWorkBook = xlApp.Workbooks.Add();
                xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);
            }
            else
            {
                xlWorkBook = xlApp.Workbooks.Open(Directory.GetCurrentDirectory() + @"\" + xlpath);
                xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.Add();
            }
            List<Excel.Range> c11 = new List<Excel.Range>();
            List<Excel.Range> c21 = new List<Excel.Range>();
            List<Excel.Range> c12 = new List<Excel.Range>();
            List<Excel.Range> c22 = new List<Excel.Range>();
            object[,] allData = new object[calculateNumberOfRows(node) + 2, this.depth * 4];

            PrintNode(node, 1, allData, xlWorkSheet, c11, c12, c21, c22);



            bool max = node.max;
            for (int i = 1; i <= this.depth; i++)
            {
                allData[1, (i - 1) * 4 + 1] = i.ToString();
                allData[1, (i - 1) * 4 + 2] = max ? "maximizer" : "minimizer";
                max = !max;
            }

            Excel.Range c1 = (Excel.Range)xlWorkSheet.Cells[1, 1];
            Excel.Range c2 = (Excel.Range)xlWorkSheet.Cells[allData.GetLength(0), allData.GetLength(1)];
            Excel.Range range = xlWorkSheet.get_Range(c1, c2);
            range.Value = allData;

            for (int i = 0; i < c11.Count; i++)
            {
                range = (Excel.Range)xlWorkSheet.get_Range(c11[i], c21[i]);
                range.BorderAround(Excel.XlLineStyle.xlContinuous, Excel.XlBorderWeight.xlThick);
            }

            for (int i = 0; i < c12.Count; i++)
            {
                range = (Excel.Range)xlWorkSheet.get_Range(c12[i], c22[i]);
                range.Interior.Color = Excel.XlRgbColor.rgbYellow;
            }

            xlWorkSheet.Columns.AutoFit();
            if (moveCounter == 1)
                xlWorkBook.SaveAs(Directory.GetCurrentDirectory() + @"\" + xlpath);
            else
                xlWorkBook.Save();
            moveCounter++;
            xlWorkBook.Close();
            xlApp.Quit();

        }
        private int PrintNode(AINode node, int rowOffset, object[,] allData, Excel.Worksheet xlWorkSheet, List<Excel.Range> c11, List<Excel.Range> c12, List<Excel.Range> c21, List<Excel.Range> c22)
        {
            int rowCounter = 0;
            int rowtempCounter = 0;
            Excel.Range c1;
            if (node.depth != this.depth)
            {
                c1 = xlWorkSheet.Cells[rowOffset + 2, (node.depth - 1) * 4 + 1];
                for (int i = 0; i < node.allPossibleMoves.Count; i++)
                {
                    rowtempCounter = 0;
                    if (node.allPossibleNodes.Count > i)
                    {
                        rowtempCounter = PrintNode(node.allPossibleNodes[i], rowOffset + rowCounter, allData, xlWorkSheet, c11, c12, c21, c22);
                        allData[rowOffset + rowCounter + rowtempCounter / 2 + 1, (node.depth - 1) * 4 + 1] = node.allPossibleMoves[i].ToString();
                        allData[rowOffset + rowCounter + rowtempCounter / 2 + 1, (node.depth - 1) * 4 + 2] = node.allPossibleMoves[i].destWeight.ToString();
                        allData[rowOffset + rowCounter + rowtempCounter / 2 + 1, (node.depth - 1) * 4 + 3] = node.allScores[i].ToString();
                    }
                    else
                    {
                        allData[rowOffset + rowCounter + 1, (node.depth - 1) * 4 + 1] = node.allPossibleMoves[i].ToString();
                        allData[rowOffset + rowCounter + 1, (node.depth - 1) * 4 + 2] = node.allPossibleMoves[i].destWeight.ToString();
                        rowtempCounter = 1;
                    }
                    if (node.bestMove.ToString() == node.allPossibleMoves[i].ToString())
                    {
                        c12.Add(xlWorkSheet.Cells[rowOffset + rowCounter + rowtempCounter / 2 + 2, (node.depth - 1) * 4 + 1]);
                        c22.Add(xlWorkSheet.Cells[rowOffset + rowCounter + rowtempCounter / 2 + 2, (node.depth - 1) * 4 + 4]);
                    }
                    rowCounter += rowtempCounter;

                }
                c11.Add(c1);
                c21.Add(xlWorkSheet.Cells[rowOffset + rowCounter + 1, (node.depth - 1) * 4 + 4]);
            }
            else
            {
                for (int i = 0; i < node.allPossibleMoves.Count; i++)
                {
                    allData[rowOffset + i + 1, (node.depth - 1) * 4 + 1] = node.allPossibleMoves[i].ToString();
                    allData[rowOffset + i + 1, (node.depth - 1) * 4 + 2] = node.allPossibleMoves[i].destWeight.ToString();
                    if (node.allScores.Count > i)
                        allData[rowOffset + i + 1, (node.depth - 1) * 4 + 3] = node.allScores[i].ToString();

                    if (node.bestMove.ToString() == node.allPossibleMoves[i].ToString())
                    {
                        c12.Add(xlWorkSheet.Cells[rowOffset + i + 2, (node.depth - 1) * 4 + 1]);
                        c22.Add(xlWorkSheet.Cells[rowOffset + i + 2, (node.depth - 1) * 4 + 4]);
                    }
                }
                c11.Add(xlWorkSheet.Cells[rowOffset + 2, (node.depth - 1) * 4 + 1]);
                c21.Add(xlWorkSheet.Cells[rowOffset + node.allPossibleMoves.Count + 1, (node.depth - 1) * 4 + 4]);
                rowCounter = node.allPossibleMoves.Count;
            }
            return rowCounter;
        }
        private int calculateNumberOfRows(AINode node)
        {
            int rows = 0;
            if (node.depth == this.depth)
                return node.allPossibleMoves.Count;
            else
                for(int i = 0; i < node.allPossibleMoves.Count; i++)
                {
                    if (i < node.allPossibleNodes.Count)
                        rows += calculateNumberOfRows(node.allPossibleNodes[i]);
                    else
                        rows += 1;
                }
            return rows;
        }
    }
}
