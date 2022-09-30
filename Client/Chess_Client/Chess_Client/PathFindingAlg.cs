using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Client
{
    static class PathFindingAlg
    {
        public static int V = 64;

        static int minDistance(int[] dist,
                        bool[] sptSet)
        {
            // Initialize min value
            int min = int.MaxValue, min_index = -1;

            for (int v = 0; v < V; v++)
                if (sptSet[v] == false && dist[v] <= min)
                {
                    min = dist[v];
                    min_index = v;
                }

            return min_index;
        }
        static void dijkstra(int[,] graph, int src, out int[] parent, out int[] dist)
        {
            dist = new int[V]; // The output array. dist[i]
                               // will hold the shortest
                               // distance from src to i

            // sptSet[i] will true if vertex
            // i is included in shortest path
            // tree or shortest distance from
            // src to i is finalized
            bool[] sptSet = new bool[V];

            parent = new int[V];

            // Initialize all distances as
            // INFINITE and stpSet[] as false
            for (int i = 0; i < V; i++)
            {
                dist[i] = int.MaxValue;
                sptSet[i] = false;
            }

            // Distance of source vertex
            // from itself is always 0
            dist[src] = 0;

            // Find shortest path for all vertices
            for (int count = 0; count < V - 1; count++)
            {
                // Pick the minimum distance vertex
                // from the set of vertices not yet
                // processed. u is always equal to
                // src in first iteration.
                int u = minDistance(dist, sptSet);

                // Mark the picked vertex as processed
                sptSet[u] = true;

                // Update dist value of the adjacent
                // vertices of the picked vertex.
                for (int v = 0; v < V; v++)

                    // Update dist[v] only if is not in
                    // sptSet, there is an edge from u
                    // to v, and total weight of path
                    // from src to v through u is smaller
                    // than current value of dist[v]
                    if (!sptSet[v] && graph[u, v] != 0 && dist[u] != int.MaxValue && dist[u] + graph[u, v] < dist[v])
                    {
                        parent[v] = u;
                        dist[v] = dist[u] + graph[u, v];
                    }
            }

        }
        static int[,] BoardToGraph(ChessBoard board, Move avoidMove)
        {

            int avoidSrcRow = avoidMove == null ? -1 : avoidMove.sourceRowIndex;
            int avoidSrcCol = avoidMove == null ? -1 : avoidMove.sourceColIndex;
            int avoidDestRow = avoidMove == null ? -1 : avoidMove.destRowIndex;
            int avoidDestCol = avoidMove == null ? -1 : avoidMove.destColIndex;
            int[,] graph = new int[64, 64];
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    for (int i = 0; i < 64; i++)
                    {
                        if ((row > 0 && i == ((row - 1) * 8 + col)) ||
                            (row < 7 && i == (((row + 1) * 8 + col))) ||
                            (col > 0 && i == (row * 8 + col - 1)) ||
                            (col < 7 && i == (row * 8 + col + 1)))
                        {
                            if ((i / 8 == avoidSrcRow && i % 8 == avoidSrcCol) || (row == avoidDestRow && col == avoidDestCol))
                                graph[row * 8 + col, i] = 1000;
                            else
                                graph[row * 8 + col, i] = board.board[i / 8, i % 8].type == PieceType.Null ? 1 : 10;
                        }
                        else
                            graph[row * 8 + col, i] = 0;
                    }
                }

            }
            return graph;
        }
        static public List<List<Move>> GetShortestPath(ChessBoard board, PieceToMove piece, List<List<Move>> avoidPieces, Move avoidSourceMove, out int score)
        {
            return GetShortestPath(board, new Move(piece.rowIndex, piece.colIndex, -1, -1), avoidPieces, avoidSourceMove, out score);
        }

        static public List<List<Move>> GetShortestPath(ChessBoard board, Move move, List<List<Move>> avoidPieces, Move avoidSourceMove, out int score)
        {
			if (move.destColIndex == move.sourceColIndex && move.destRowIndex == move.sourceRowIndex)
            {
                score = 0;
                return new List<List<Move>>() { new List<Move>() { new Move(move.sourceRowIndex, move.sourceColIndex, move.destRowIndex, move.destColIndex) } };
            }
            score = int.MaxValue;
            if (avoidPieces == null)
                avoidPieces = new List<List<Move>>();

            int[,] graph = BoardToGraph(board, avoidSourceMove);

            int src = move.sourceRowIndex * 8 + move.sourceColIndex;
            int dst = move.destRowIndex * 8 + move.destColIndex;

            int[] parent;
            int[] dist;
            dijkstra(graph, src, out parent, out dist);

            List<Move> allMoves = new List<Move>();
            if (move.destRowIndex != -1 && move.destColIndex != -1)
            {
                score = dist[dst];
                while (dst != src)
                {
                    allMoves.Add(new Move(parent[dst] / 8, parent[dst] % 8, dst / 8, dst % 8));
                    dst = parent[dst];
                }
            }
            else
            {
                bool legal = true;
                int minDist = int.MaxValue;
                int minDistIndex = -1;
                //find a place to move to
                for (int i = 0; i < dist.Length; i++)
                {
                    legal = true;
                    if (board.board[i / 8, i % 8].type != PieceType.Null)
                        continue;
                    foreach (List<Move> listToAvoid in avoidPieces)
                    {
                        foreach (Move moveToAvoid in listToAvoid)
                        {
                            if ((moveToAvoid.sourceRowIndex * 8 + moveToAvoid.sourceColIndex) == i ||
                                (moveToAvoid.destRowIndex * 8 + moveToAvoid.destColIndex) == i)
                            {
                                legal = false;
                                break;
                            }
                        }
                    }
                    if (legal == true && dist[i] < minDist)
                    {
                        minDist = dist[i];
                        minDistIndex = i;
                    }
                }

                score = minDist;
                dst = minDistIndex;
                while (dst != src)
                {
                    allMoves.Add(new Move(parent[dst] / 8, parent[dst] % 8, dst / 8, dst % 8));
                    dst = parent[dst];
                }
            }
            List<PieceToMove> evilPieces = new List<PieceToMove>();
            for (int i = 0; i < allMoves.Count; i++)
            {
                if (board.board[allMoves[i].destRowIndex, allMoves[i].destColIndex].type != PieceType.Null)
                {
                    evilPieces.Add(new PieceToMove(allMoves[i].destRowIndex, allMoves[i].destColIndex));
                }
            }

            List<Move> allMovesCombined = CombineShortPath(allMoves);

            List<List<Move>> allMovesList = new List<List<Move>>();


            if (avoidSourceMove == null)
                avoidSourceMove = new Move(allMovesCombined[0].sourceRowIndex,
                    allMovesCombined[0].sourceColIndex,
                    allMovesCombined[allMovesCombined.Count - 1].destRowIndex,
                    allMovesCombined[allMovesCombined.Count - 1].destColIndex);

            if (evilPieces.Count != 0)
            {
                avoidPieces.Add(allMoves);
                //need to move some pieces
                foreach (PieceToMove piece in evilPieces)
                {
                    if (board.board[piece.rowIndex, piece.colIndex].type == PieceType.Null)
                        continue;
                    List<List<Move>> tmpMoves = GetShortestPath(board, piece, new List<List<Move>>(avoidPieces), avoidSourceMove, out int tempScore);
                    allMovesList.AddRange(tmpMoves);
                }
            }

            board.board[allMovesCombined[allMovesCombined.Count - 1].destRowIndex, allMovesCombined[allMovesCombined.Count - 1].destColIndex] =
                board.board[allMovesCombined[0].sourceRowIndex, allMovesCombined[0].sourceColIndex];
            board.board[allMovesCombined[0].sourceRowIndex, allMovesCombined[0].sourceColIndex] = new Piece();

            allMovesList.Add(allMovesCombined);

            return allMovesList;
        }

        static private List<Move> CombineShortPath(List<Move> longShortPath)
        {
            int lastDir = -10;
            int curDir = -10;
            List<Move> shortShortPath = new List<Move>();
            for (int i = longShortPath.Count - 1; i >= 0; i--)
            {
                Move curMove = longShortPath[i];
                curDir = curMove.destColIndex - curMove.sourceColIndex + 8 * (curMove.destRowIndex - curMove.sourceRowIndex);
                if (curDir != lastDir)
                    shortShortPath.Add(new Move(curMove.sourceRowIndex, curMove.sourceColIndex, curMove.destRowIndex, curMove.destColIndex));
                else
                {
                    shortShortPath[shortShortPath.Count - 1].destRowIndex = curMove.destRowIndex;
                    shortShortPath[shortShortPath.Count - 1].destColIndex = curMove.destColIndex;
                }
                lastDir = curDir;
            }
            return shortShortPath;
        }
    }
}
