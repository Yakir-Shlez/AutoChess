#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <netdb.h>
#include <poll.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <sys/mman.h>
#include <pthread.h>
#include <sys/un.h>
#include <math.h>
#include "Chess_Api.h"

/*
Init board, board indexes:
*          White
*    a  b  c  d  e  f  g  h
*  ___________________________
* 1||00|01|02|03|04|05|06|07||1
* 2||08|09|10|11|12|13|14|15||2
* 3||16|17|18|19|20|21|22|23||3
* 4||24|25|26|27|28|29|30|31||4
* 5||32|33|34|35|36|37|38|39||5
* 6||40|41|42|43|44|45|46|47||6
* 7||48|49|50|51|52|53|54|55||7
* 8||56|57|58|59|60|61|62|63||8
*  ___________________________
*    a  b  c  d  e  f  g  h        
*          Black
*/
Chess_Board Init_Board()
{
	//printf("Init_Board\n");
	//printf("1\n");
	Chess_Board myboard;
	//printf("%d\n", myboard->board);
	//myboard->board = (Piece*) malloc(8 * 8 * sizeof(Piece));
	//printf("%d\n", myboard->board);
	//printf("2\n");
	//myboard->board[0] = myboard->board[7] = WRook;
	myboard.board[0] = WRook;
	myboard.board[7] = WRook;
	//printf("%d\n", (int)myboard.board[0]);
	myboard.board[1] = myboard.board[6] = WKnight;
	myboard.board[2] = myboard.board[5] = WBishop;
	myboard.board[3] = WQueen;
	myboard.board[4] = WKing;
	myboard.board[56] = myboard.board[63] = BRook;
	myboard.board[57] = myboard.board[62] = BKnight;
	myboard.board[58] = myboard.board[61] = BBishop;
	myboard.board[59] = BQueen;
	myboard.board[60] = BKing;
	//printf("3\n");
	for(int i = 8; i <= 15; i++)
		myboard.board[i] = WPawn;
		//printf("4\n");
	for(int i = 48; i <= 55; i++)
		myboard.board[i] = BPawn;
		//printf("5\n");
	for(int i = 16; i <= 47; i++)
		myboard.board[i] = Empty;
		//printf("6\n");
	myboard.numberOfBlackPlayers = 16;
	myboard.numberOfWhitePlyaers = 16;
	myboard.bShortCastlingFlag = 1;
	myboard.bLongCastlingFlag = 1;
	myboard.wShortCastlingFlag = 1;
	myboard.wLongCastlingFlag = 1;
	myboard.enPassantCol = -1;
	//printf("7\n");
	return myboard;
}

void Print_Board(Chess_Board board)
{
	printf("                 White               \n");
	printf("_____________________________________\n");
	printf("     a   b   c   d   e   f   g   h   \n");
	printf("     0   1   2   3   4   5   6   7   \n");
	for(int i = 0; i < 8; i++)
	{
		printf("%d|%d|", i + 1, i);
		for(int j = 0; j < 8; j++)
			printf("%s|",Piece_To_Short_String(board.board[i * 8 + j]));
		//printf("%d|",(int)(myboard->board[i * 8 + j]));
			
		printf("\n");
	}
	printf("_____________________________________\n");
	printf("                 Black               \n");
}

char* Piece_To_Short_String(Piece my_piece)
{
	switch(my_piece)
	{
		case WPawn:
			return "WPn";
		case WBishop:
			return "WBs";
		case WKnight:
			return "WKn";
		case WRook:
			return "WRo";
		case WQueen:
			return "WQe";
		case WKing:
			return "WKi";
		case BPawn:
			return "BPn";
		case BBishop:
			return "BBs";		
		case BKnight:
			return "BKn";
		case BRook:
			return "BRo";
		case BQueen:
			return "BQe";
		case BKing:
			return "BKi";
		case Empty:
			return "Ety";
	}
}

void SquareToIndex(char* square, int* rowIndex, int* colIndex)
{
	colIndex[0] = square[0] - 97;
	rowIndex[0] = square[1] - 49;
}

int ExequteMove(char* moveToExec, Chess_Board* board, Square_Color turnColor)
{
	//printf("ExequteMove\n"); //TBD
	char move[6];
	strncpy(move, moveToExec, 5);
	move[5] = 0;
	char* source = strtok(move, "-");
	char* dest = strtok(NULL, "-");
	
	int sourceRowIndex;
	int sourceColIndex;
	int destRowIndex;
	int destColIndex;
	int rowIndexOffset;
	int colIndexOffset;
	int flag = 0;
	
	//flags
	int captureFlag = 0;
	int enPassant = 0;
	int leftRookMoved = 0;
	int rightRookMoved = 0;
	
	SquareToIndex(source, &sourceRowIndex, &sourceColIndex);
	SquareToIndex(dest, &destRowIndex, &destColIndex);
	//printf("sourceRowIndex: %d sourceColIndex: %d destRowIndex: %d destColIndex:%d\n",
		//sourceRowIndex,sourceColIndex,destRowIndex,destColIndex); //TBD
	if(sourceRowIndex > 7 || sourceRowIndex < 0 || sourceColIndex > 7 || sourceColIndex < 0 ||
		destRowIndex > 7 || destRowIndex < 0 || destColIndex > 7 || destColIndex < 0)
		return 0;
		
	if(sourceRowIndex == destRowIndex && sourceColIndex == destColIndex)
		return 0;
	
	Piece sourcePiece = board->board [8 * sourceRowIndex + sourceColIndex];
	
	Square_Color sourcePieceColor = PieceColor(sourcePiece);
	if(sourcePiece == Empty || sourcePieceColor != turnColor) //wrong piece color or no piece at all
		return 0;
		
	if(board->board [8 * destRowIndex + destColIndex] != Empty)
	{
		Square_Color destPieceColor = PieceColor(board->board [8 * destRowIndex + destColIndex]);
		if((sourcePieceColor == Black && destPieceColor == White) || (sourcePieceColor == White && destPieceColor == Black))
			captureFlag = 1;
		else //capturing your own piece
			return 0;
	}
	
	// check - piece pattern -> chess condition
	//king
	if(sourcePiece == WKing || sourcePiece == BKing)
	{
		//printf("King move\n"); //TBD
		//castling
		if(destColIndex - sourceColIndex > 1 || destColIndex - sourceColIndex < -1)
		{
			//printf("Castling\n");
			if(sourceColIndex != 4)
				return 0;
			if(!(sourceRowIndex == 0 && turnColor == White) && !(sourceRowIndex == 7 && turnColor == Black))
				return 0;
				
			
			if(destColIndex == 6 && ((turnColor == White && board -> wShortCastlingFlag) || (turnColor == Black && board -> bShortCastlingFlag)))
			{
				//Short
				//printf("Short\n");
				if(board -> board[8 * sourceRowIndex + sourceColIndex +1] != Empty || board -> board[8 * sourceRowIndex + sourceColIndex +2] != Empty)
					return 0;
				if(CheckChessHypothetical(board, sourceRowIndex, sourceColIndex, sourceRowIndex, sourceColIndex, turnColor, 1) == 1 ||
					CheckChessHypothetical(board, sourceRowIndex, sourceColIndex, sourceRowIndex, sourceColIndex + 1, turnColor, 1) == 1 ||
					CheckChessHypothetical(board, sourceRowIndex, sourceColIndex, sourceRowIndex, sourceColIndex + 2, turnColor, 0) == 1)
					return 0;
				board -> board[8 * destRowIndex + 5] = board -> board[8 * destRowIndex + 7];
				board -> board[8 * destRowIndex + 7] = Empty;
			}
			else if(destColIndex == 2 &&( (turnColor == White && board -> wLongCastlingFlag) || (turnColor == Black && board -> bLongCastlingFlag)))
			{
				//Long
				//printf("Long\n");
				if(board -> board[8 * sourceRowIndex + sourceColIndex - 1] != Empty || board -> board[8 * sourceRowIndex + sourceColIndex - 2] != Empty 
					|| board -> board[8 * sourceRowIndex + sourceColIndex - 3] != Empty)
					return 0;
				if(CheckChessHypothetical(board, sourceRowIndex, sourceColIndex, sourceRowIndex, sourceColIndex, turnColor, 1) == 1 ||
					CheckChessHypothetical(board, sourceRowIndex, sourceColIndex, sourceRowIndex, sourceColIndex - 1, turnColor, 1) == 1 ||
					CheckChessHypothetical(board, sourceRowIndex, sourceColIndex, sourceRowIndex, sourceColIndex - 2, turnColor, 0) == 1)
					return 0;
				board -> board[8 * destRowIndex + 3] = board -> board[8 * destRowIndex + 0];
				board -> board[8 * destRowIndex + 0] = Empty;
			}
			else
				return 0;
		}
		else
		{
			//king move
			rowIndexOffset = destRowIndex - sourceRowIndex;
			colIndexOffset = destColIndex - sourceColIndex;
			int rowPattern[8] = { 0, 0, -1, 1, 1, 1, -1, -1};
			int colPattern[8] = { -1, 1, 0, 0, 1, -1, 1, -1};
			for(int j = 0; j< 8; j++)
			{
				if(rowIndexOffset == rowPattern[j] && colIndexOffset == colPattern[j])
				{
					if(CheckChessHypothetical(board, sourceRowIndex, sourceColIndex, destRowIndex, destColIndex, turnColor, 0))
						return 0;
					break;
				}
			}
		}
		if(turnColor == White)
		{
			board -> wShortCastlingFlag = 0;
			board -> wLongCastlingFlag = 0;
		}
		else
		{
			board -> bShortCastlingFlag = 0;
			board -> bLongCastlingFlag = 0;
		}
	}
	//Queen
	else if(sourcePiece == WQueen || sourcePiece == BQueen)
	{
		//printf("Queen move\n"); //TBD
		rowIndexOffset = destRowIndex - sourceRowIndex;
		colIndexOffset = destColIndex - sourceColIndex;
		int rowPattern[8] = { 0, 0, -1, 1, 1, 1, -1, -1};
		int colPattern[8] = { -1, 1, 0, 0, 1, -1, 1, -1};
		for(int j = 0; j< 8; j++)
		{
			if(rowIndexOffset == (abs(rowIndexOffset) * rowPattern[j]) && colIndexOffset == (abs(colIndexOffset) * colPattern[j]))
			{
				int lineSize;
				if(j == 0 || j == 1)
					lineSize = abs(colIndexOffset);
				else
					lineSize = abs(rowIndexOffset);
				for(int i = 1; i < lineSize; i++)
				{
					if(board -> board[(sourceRowIndex + (i * rowPattern[j])) * 8 + (sourceColIndex + (i * colPattern[j]))] != Empty)
						return 0;
				}
				if(CheckChessHypothetical(board, sourceRowIndex, sourceColIndex, destRowIndex, destColIndex, turnColor, 0))
					return 0;
				break;
			}
		}
	}
	//Rook
	else if(sourcePiece == WRook || sourcePiece == BRook)
	{
		//printf("Rook move\n"); //TBD
		rowIndexOffset = destRowIndex - sourceRowIndex;
		colIndexOffset = destColIndex - sourceColIndex;
		int rowPattern[4] = { 0, 0, -1, 1};
		int colPattern[4] = { -1, 1, 0, 0};
		for(int j = 0; j< 4; j++)
		{
			if(rowIndexOffset == (abs(rowIndexOffset) * rowPattern[j]) && colIndexOffset == (abs(colIndexOffset) * colPattern[j]))
			{
				int lineSize;
				if(j == 0 || j == 1)
					lineSize = abs(colIndexOffset);
				else
					lineSize = abs(rowIndexOffset);
				for(int i = 1; i < lineSize; i++)
				{
					if(board -> board[(sourceRowIndex + (i * rowPattern[j])) * 8 + (sourceColIndex + (i * colPattern[j]))] != Empty)
						return 0;
				}
				if(CheckChessHypothetical(board, sourceRowIndex, sourceColIndex, destRowIndex, destColIndex, turnColor, 0))
					return 0;
				break;
			}
		}
	}
	//Bishop
	else if(sourcePiece == WBishop || sourcePiece == BBishop)
	{
		//printf("Bishop move\n"); //TBD
		rowIndexOffset = destRowIndex - sourceRowIndex;
		colIndexOffset = destColIndex - sourceColIndex;
		int rowPattern[4] = { 1, 1, -1, -1};
		int colPattern[4] = { 1, -1, 1, -1};
		for(int j = 0; j< 4; j++)
		{
			if(rowIndexOffset == (abs(rowIndexOffset) * rowPattern[j]) && colIndexOffset == (abs(colIndexOffset) * colPattern[j]))
			{
				int lineSize = abs(colIndexOffset);
				for(int i = 1; i < lineSize; i++)
				{
					if(board -> board[(sourceRowIndex + (i * rowPattern[j])) * 8 + (sourceColIndex + (i * colPattern[j]))] != Empty)
						return 0;
				}
				if(CheckChessHypothetical(board, sourceRowIndex, sourceColIndex, destRowIndex, destColIndex, turnColor, 0))
					return 0;
				break;
			}
		}
	}
	//Knight
	else if(sourcePiece == WKnight || sourcePiece == BKnight)
	{
		//printf("Knight move\n"); //TBD
		rowIndexOffset = destRowIndex - sourceRowIndex;
		colIndexOffset = destColIndex - sourceColIndex;
		int rowPattern[8] = { 2, 2, -2, -2, 1, 1, -1, -1 };
		int colPattern[8] = { 1, -1, 1, -1, 2, -2, 2, -2 };
		for(int j = 0; j< 8; j++)
		{
			if(rowIndexOffset == rowPattern[j] && colIndexOffset == colPattern[j])
			{
				if(CheckChessHypothetical(board, sourceRowIndex, sourceColIndex, destRowIndex, destColIndex, turnColor, 0))
					return 0;
				break;
			}
		}
	}
	//Pawn
	else if(sourcePiece == WPawn || sourcePiece == BPawn)
	{
		//printf("Pawn move\n"); //TBD
		//Advancing forward - no capture
		if(destColIndex == sourceColIndex)
		{
			//printf("0\n");
			//printf("Pawn move forward\n"); //TBD
			if(captureFlag)
				return 0;
			rowIndexOffset = destRowIndex - sourceRowIndex;
			if(rowIndexOffset > 2 || rowIndexOffset < -2)
				return 0;
			if(turnColor == White)
			{
				if(rowIndexOffset < 0)
					return 0;
				if(rowIndexOffset == 2)
				{
					if(sourceRowIndex == 1)
						enPassant = 1; 
					else
						return 0;
				}
			}
			else
			{
				if(rowIndexOffset > 0)
					return 0;
				if(rowIndexOffset == -2)
				{
					if(sourceRowIndex == 6)
						enPassant = 1; 
					else
						return 0;
				}
			}
			//printf("Check chess\n"); //TBD
			if(CheckChessHypothetical(board, sourceRowIndex, sourceColIndex, destRowIndex, destColIndex, turnColor, 0))
			{
				//printf("In chess\n"); //TBD
				return 0;
			}
		}
		//Advancing sideways - capture
		else
		{
			if(!captureFlag && !(destColIndex == board -> enPassantCol))
				return 0;
			rowIndexOffset = destRowIndex - sourceRowIndex;
			colIndexOffset = destColIndex - sourceColIndex;
			
			if(rowIndexOffset != 1 && rowIndexOffset != -1)
				return 0;
			if(colIndexOffset != 1 && colIndexOffset != -1)
				return 0;
			
			if(turnColor == White && rowIndexOffset == -1)
				return 0;
			else if(turnColor == Black && rowIndexOffset == 1)
				return 0;
			
			if(destColIndex == board -> enPassantCol && (
				(sourceRowIndex == 3 && destRowIndex == 2 && sourcePiece == BPawn) ||
				(sourceRowIndex == 4 && destRowIndex == 5 && sourcePiece == WPawn)))
			{
				//En passant
				board -> board [sourceRowIndex * 8 + destColIndex] = Empty;
			}
			if(CheckChessHypothetical(board, sourceRowIndex, sourceColIndex, destRowIndex, destColIndex, turnColor, 0))
			{
				if(destColIndex == board -> enPassantCol)
				{
					if(destRowIndex == 2)
						board -> board [sourceRowIndex * 8 + destColIndex] = WPawn;
					else
						board -> board [sourceRowIndex * 8 + destColIndex] = BPawn;
				}
				return 0;
			}
		}
	}
	else //Error
		return 0;
	
	if(captureFlag)
	{
		if(turnColor == White)
			board -> numberOfBlackPlayers --;
		else
			board -> numberOfWhitePlyaers --;
	}
	
	if(enPassant)
		board -> enPassantCol = sourceColIndex;
	else
		board -> enPassantCol = -1;
		
	return 1;
}

Square_Color PieceColor(Piece piece)
{
	if(piece == WPawn ||
		piece == WBishop ||
		piece == WKnight ||
		piece == WRook ||
		piece == WQueen ||
		piece == WKing)
		return White;
	
	if(piece == BPawn ||
		piece == BBishop ||
		piece == BKnight ||
		piece == BRook ||
		piece == BQueen ||
		piece == BKing)
		return Black;
		
	return White;
}

int CheckChessHypothetical(Chess_Board* board, int sourceRowIndex, int sourceColIndex, int destRowIndex, int destColIndex
	, Square_Color turnColor, int returnIfAccept)
{
	Piece piece = board -> board[8 * sourceRowIndex + sourceColIndex];
    Piece secPiece = Empty;
    int returnFlag = 0;
	int kingRowIndex = -1;
	int kingColIndex = -1;
	
    if ((8 * sourceRowIndex + sourceColIndex) != (8 * destRowIndex + destColIndex))
    {
		board -> board[8 * sourceRowIndex + sourceColIndex] = Empty;
		secPiece = board -> board[8 * destRowIndex + destColIndex];
		board -> board[8 * destRowIndex + destColIndex] = piece;
	}

    if(piece == WKing || piece == BKing)
    {
		kingRowIndex = destRowIndex;
		kingColIndex = destColIndex;
	}
	//TBD
	//if(sourceRowIndex == 7 && sourceColIndex == 3 && destRowIndex == 6 && destColIndex == 3)
		//printf("Try %s\n", Piece_To_Short_String(piece));
    returnFlag = CheckChess(*board, kingRowIndex, kingColIndex, turnColor);

	if(returnFlag == 1 || returnIfAccept == 1)
	{
		board -> board[8 * sourceRowIndex + sourceColIndex] = piece;
		if ((8 * sourceRowIndex + sourceColIndex) != (8 * destRowIndex + destColIndex))
			board -> board[8 * destRowIndex + destColIndex] = secPiece;
	}
	return returnFlag;
}
	
int CheckChess(Chess_Board board, int kingRowIndex, int kingColIndex, Square_Color turnColor)
{
	Piece king;
	Piece opPiece;
	Piece opPieces[6];
	if (turnColor == Black)
	{
		king = BKing;
		opPieces[0] = WKing;
		opPieces[1] = WQueen;
        opPieces[2] = WRook;
        opPieces[3] = WBishop;
        opPieces[4] = WKnight;
        opPieces[5] = WPawn;
    }
    else
    {
		king = WKing;
		opPieces[0] = BKing;
        opPieces[1] = BQueen;
        opPieces[2] = BRook;
        opPieces[3] = BBishop;
        opPieces[4] = BKnight;
        opPieces[5] = BPawn;
    }
    
    if (kingRowIndex == -1 || kingColIndex == -1)
    {
		for (int i = 0; i < 8; i++)
			for (int j = 0; j < 8; j++)
				if (board.board[i * 8 + j] == king)
				{
					kingRowIndex = i;
					kingColIndex = j;
					break;
                }
    }
	
	int rowIndex;
	int colIndex;
	
	//printf("Check chess Up, Down, Right, Left\n");
	//Up, Down, Right, Left
	int rowPattern[4] = { 0, 0, -1, 1};
    int colPattern[4] = { -1, 1, 0, 0};
    for(int j = 0; j< 4; j++)
    {
		for (int i = 1; i < 9; i++)
		{
			rowIndex = kingRowIndex + i * rowPattern[j];
			colIndex = kingColIndex + i * colPattern[j];
			if(rowIndex > 7 || rowIndex < 0 || colIndex > 7 || colIndex < 0)
				break;
			opPiece = board.board[rowIndex * 8 + colIndex];
			if (opPiece != Empty)
			{
				if (opPiece == opPieces[1] || opPiece == opPieces[2] || (i == 1 && opPiece == opPieces[0]))
					return 1;
				break;
			}
		}
	}
	
	//printf("Check chess Up left, Up right, Down left, Down right\n");
	//Up left, Up right, Down left, Down right
	rowPattern[0] = 1;
	rowPattern[1] = 1;
	rowPattern[2] = -1;
	rowPattern[3] = -1;
	colPattern[0] = -1;
	colPattern[1] = 1;
	colPattern[2] = -1;
	colPattern[3] = 1;
	//printf("king %d %d\n", kingRowIndex, kingColIndex);
    for(int j = 0; j< 4; j++)
    {
		for (int i = 1; i < 9; i++)
		{
			rowIndex = kingRowIndex + i * rowPattern[j];
			colIndex = kingColIndex + i * colPattern[j];
			//printf("%d %d\n", rowIndex, colIndex);
			if(rowIndex > 7 || rowIndex < 0 || colIndex > 7 || colIndex < 0)
				break;
			opPiece = board.board[rowIndex * 8 + colIndex];
			if (opPiece != Empty)
			{
				if (opPiece == opPieces[1] || opPiece == opPieces[3] || (i == 1 && opPiece == opPieces[0]))
					return 1;
				break;
			}
		}
	}
	
	//printf("Check chess Knight\n");
    //Knight
     int knightRowPattern[8] = { 2, 2, -2, -2, 1, 1, -1, -1 };
     int knightColPattern[8] = { 1, -1, 1, -1, 2, -2, 2, -2 };
     for (int j = 0; j < 8; j++)
     {
		rowIndex = kingRowIndex + knightRowPattern[j];
		colIndex = kingColIndex + knightColPattern[j];
		if(rowIndex > 7 || rowIndex < 0 || colIndex > 7 || colIndex < 0)
			continue;
		opPiece = board.board[rowIndex * 8 + colIndex];
		if (opPiece == opPieces[4])
			return 1;
	}
	
	//printf("Check chess Pawn\n");
	//Pawn
	if(turnColor == White)
		rowIndex = kingRowIndex + 1;
	else
		rowIndex = kingRowIndex - 1;
		
	colIndex = kingColIndex + 1;
	if(!(rowIndex > 7 || rowIndex < 0 || colIndex > 7 || colIndex < 0))
	{
		opPiece = board.board[rowIndex * 8 + colIndex];
		if (opPiece == opPieces[5])
			return 1;
	}
	colIndex = kingColIndex - 1;
	if(!(rowIndex > 7 || rowIndex < 0 || colIndex > 7 || colIndex < 0))
	{
		opPiece = board.board[rowIndex * 8 + colIndex];
		if (opPiece == opPieces[5])
			return 1;
	}
	
	//printf("Check done\n");
	return 0;
}

int CheckAvailableMoves(Chess_Board board, Square_Color turnColor)
{
	Piece sourcePiece;
	int sourceRowIndex;
	int sourceColIndex;
	int destRowIndex;
	int destColIndex;
	int startRowIndex;
	int rowIndexDirection;
	int pieceCounter;
	int stop = 0;
	if(turnColor == White)
	{
		startRowIndex = 0;
		rowIndexDirection = 1;
	}
	else
	{
		startRowIndex = 7;
		rowIndexDirection = -1;
	}
	for(int i = startRowIndex; (i <= 7 && i >= 0); i += rowIndexDirection)
	{
		for(int j = 0; j <= 7; j++)
		{
			stop = 0;
			sourcePiece = board.board[i * 8 + j];
			if(sourcePiece == Empty || PieceColor(sourcePiece) != turnColor)
				continue;
			//printf("CheckAvailableMoves %d, %d\n", i, j);
			pieceCounter ++;
			
			sourceRowIndex = i;
			sourceColIndex = j;
			//king
			if(sourcePiece == WKing || sourcePiece == BKing)
			{
				int rowPattern[8] = { 0, 0, -1, 1, 1, 1, -1, -1};
				int colPattern[8] = { -1, 1, 0, 0, 1, -1, 1, -1};
				for(int n = 0; n< 8; n++)
				{
					destRowIndex = sourceRowIndex + rowPattern[n];
					destColIndex = sourceColIndex + colPattern[n];
					if(destRowIndex < 0 || destRowIndex > 7 || destColIndex < 0 || destColIndex > 7)
						continue;
					if(board.board[destRowIndex * 8 + destColIndex] != Empty && 
						PieceColor(board.board[destRowIndex * 8 + destColIndex]) == turnColor)
						continue;
					if(!CheckChessHypothetical(&board, sourceRowIndex, sourceColIndex, destRowIndex, destColIndex, turnColor, 1))
						return 1;
				}
			}
			//Queen 
			else if(sourcePiece == WQueen || sourcePiece == BQueen)
			{
				int rowPattern[8] = { 0, 0, -1, 1, 1, 1, -1, -1};
				int colPattern[8] = { -1, 1, 0, 0, 1, -1, 1, -1};
				for(int n = 0; n< 8; n++)
				{
					for(int m = 1; m < 8; m++)
					{
						stop = 0;
						destRowIndex = sourceRowIndex + rowPattern[n] * m;
						destColIndex = sourceColIndex + colPattern[n] * m;
						if(destRowIndex < 0 || destRowIndex > 7 || destColIndex < 0 || destColIndex > 7)
							break;
						if(board.board[destRowIndex * 8 + destColIndex] != Empty)
						{
							if(PieceColor(board.board[destRowIndex * 8 + destColIndex]) == turnColor)
								break;
							else
								stop = 1;
						}
						//TBD
						//printf("Check chess queen %d, %d, %d, %d\n", sourceRowIndex, sourceColIndex, destRowIndex, destColIndex);
						if(!CheckChessHypothetical(&board, sourceRowIndex, sourceColIndex, destRowIndex, destColIndex, turnColor, 1))
							return 1;
						if(stop)
							break;
					}
				}
			}
			//Rook
			else if(sourcePiece == WRook || sourcePiece == BRook)
			{
				int rowPattern[4] = { 0, 0, -1, 1};
				int colPattern[4] = { -1, 1, 0, 0};
				for(int n = 0; n< 4; n++)
				{
					for(int m = 1; m < 8; m++)
					{
						stop = 0;
						destRowIndex = sourceRowIndex + rowPattern[n] * m;
						destColIndex = sourceColIndex + colPattern[n] * m;
						if(destRowIndex < 0 || destRowIndex > 7 || destColIndex < 0 || destColIndex > 7)
							break;
						if(board.board[destRowIndex * 8 + destColIndex] != Empty)
						{
							if(PieceColor(board.board[destRowIndex * 8 + destColIndex]) == turnColor)
								break;
							else
								stop = 1;
						}
						if(!CheckChessHypothetical(&board, sourceRowIndex, sourceColIndex, destRowIndex, destColIndex, turnColor, 1))
							return 1;
						if(stop)
							break;
					}
				}
			}
			//Bishop
			else if(sourcePiece == WBishop || sourcePiece == BBishop)
			{
				int rowPattern[4] = { 1, 1, -1, -1};
				int colPattern[4] = { -1, 1, -1, 1};
				for(int n = 0; n< 4; n++)
				{
					for(int m = 1; m < 8; m++)
					{
						stop = 0;
						destRowIndex = sourceRowIndex + rowPattern[n] * m;
						destColIndex = sourceColIndex + colPattern[n] * m;
						if(destRowIndex < 0 || destRowIndex > 7 || destColIndex < 0 || destColIndex > 7)
							break;
						//printf("turnColor %d\n", turnColor);
						//printf("piece %d\n", board.board[destRowIndex * 8 + destColIndex]);
						//printf("PieceColor %d\n", PieceColor(board.board[destRowIndex * 8 + destColIndex]));
						if(board.board[destRowIndex * 8 + destColIndex] != Empty)
						{
							if(PieceColor(board.board[destRowIndex * 8 + destColIndex]) == turnColor)
								break;
							else
								stop = 1;
						}
						//printf("CheckChessHypothetical %d %d\n", destRowIndex, destColIndex);
						if(!CheckChessHypothetical(&board, sourceRowIndex, sourceColIndex, destRowIndex, destColIndex, turnColor, 1))
							return 1;
						if(stop)
							break;
					}
				}
			}
			//Knight
			if(sourcePiece == WKnight || sourcePiece == BKnight)
			{
				int rowPattern[8] = { 2, 2, -2, -2, 1, 1, -1, -1 };
				int colPattern[8] = { 1, -1, 1, -1, 2, -2, 2, -2 };
				for(int n = 0; n< 8; n++)
				{
					destRowIndex = sourceRowIndex + rowPattern[n];
					destColIndex = sourceColIndex + colPattern[n];
					if(destRowIndex < 0 || destRowIndex > 7 || destColIndex < 0 || destColIndex > 7)
						continue;
					if(board.board[destRowIndex * 8 + destColIndex] != Empty && 
						PieceColor(board.board[destRowIndex * 8 + destColIndex]) == turnColor)
						continue;
					if(!CheckChessHypothetical(&board, sourceRowIndex, sourceColIndex, destRowIndex, destColIndex, turnColor, 1))
						return 1;
				}
			}
			//Pawn
			else if(sourcePiece == WPawn || sourcePiece == BPawn)
			{
				//non capturing
				destRowIndex = sourceRowIndex + rowIndexDirection;
				destColIndex = sourceColIndex;
				if(!(destRowIndex < 0 || destRowIndex > 7 || destColIndex < 0 || destColIndex > 7))
				{
					if(board.board[destRowIndex * 8 + destColIndex] == Empty)
					{
						if(!CheckChessHypothetical(&board, sourceRowIndex, sourceColIndex, destRowIndex, destColIndex, turnColor, 1))
							return 1;
					}
				}
				//non capturing double move
				if(sourceRowIndex == (startRowIndex + rowIndexDirection))
				{
					destRowIndex = sourceRowIndex + 2 * rowIndexDirection;
					destColIndex = sourceColIndex;
					if(!(destRowIndex < 0 || destRowIndex > 7 || destColIndex < 0 || destColIndex > 7))
					{
						if(board.board[(startRowIndex + 2 * rowIndexDirection) * 8 + destColIndex] == Empty && 
							board.board[destRowIndex * 8 + destColIndex] == Empty)
						{
							if(!CheckChessHypothetical(&board, sourceRowIndex, sourceColIndex, destRowIndex, destColIndex, turnColor, 1))
								return 1;
						}
					}
				}
				//capturing move
				destRowIndex = sourceRowIndex + rowIndexDirection;
				destColIndex = sourceColIndex + 1;
				if(!(destRowIndex < 0 || destRowIndex > 7 || destColIndex < 0 || destColIndex > 7))
				{
					if(board.board[destRowIndex * 8 + destColIndex] != Empty && 
						PieceColor(board.board[destRowIndex * 8 + destColIndex]) != turnColor)
					{
						if(!CheckChessHypothetical(&board, sourceRowIndex, sourceColIndex, destRowIndex, destColIndex, turnColor, 1))
							return 1;
					}
				}
				destColIndex = sourceColIndex - 1;
				if(!(destRowIndex < 0 || destRowIndex > 7 || destColIndex < 0 || destColIndex > 7))
				{
					if(board.board[destRowIndex * 8 + destColIndex] != Empty && 
						PieceColor(board.board[destRowIndex * 8 + destColIndex]) != turnColor)
					{
						if(!CheckChessHypothetical(&board, sourceRowIndex, sourceColIndex, destRowIndex, destColIndex, turnColor, 1))
							return 1;
					}
				}
				//en passant
				if(board.enPassantCol != -1 && sourceRowIndex == (startRowIndex + 3 * rowIndexDirection) &&
					(sourceColIndex == (board.enPassantCol + 1) || sourceColIndex == (board.enPassantCol - 1) ))
				{
					destRowIndex = sourceRowIndex + rowIndexDirection;
					destColIndex = board.enPassantCol;
					board.board [sourceRowIndex * 8 + board.enPassantCol] = Empty;
					if(!CheckChessHypothetical(&board, sourceRowIndex, sourceColIndex, destRowIndex, destColIndex, turnColor, 1))
					{
						return 1;
					}
					if(destRowIndex == 2)
						board.board[sourceRowIndex * 8 + board.enPassantCol] = WPawn;
					else
						board.board[sourceRowIndex * 8 + board.enPassantCol] = BPawn;
				}
			}

			
			if(turnColor == White && pieceCounter == board.numberOfWhitePlyaers)
				return 0;
			else if(turnColor == Black && pieceCounter == board.numberOfBlackPlayers)
				return 0;
		}
	}
	return 0;
}

int CheckPromotion(Chess_Board board, Square_Color turnColor)
{
	int rowIndex;
	Piece pawnPiece;
	if(turnColor == White)
	{
		rowIndex = 7;
		pawnPiece = WPawn;
	}
	else
	{
		rowIndex = 0;
		pawnPiece = BPawn;
	}
	for(int i = 0; i < 8; i++)
		if(board.board[rowIndex * 8 + i] == pawnPiece)
			return i;
	return -1;
}
int PromotePawn(Chess_Board* board, Square_Color turnColor, int col, Piece piece)
{
	int rowIndex;
	if(turnColor == White)
		rowIndex = 7;
	else
		rowIndex = 0;
	board -> board[rowIndex * 8 + col] = piece;
	return 1;
}
