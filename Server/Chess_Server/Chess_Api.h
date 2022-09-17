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

//chess piece
typedef enum Piece {
	WPawn,
	WBishop,
	WKnight,
	WRook,
	WQueen,
	WKing,
	BPawn,
	BBishop,
	BKnight,
	BRook,
	BQueen,
	BKing,
	Empty} Piece;
	
typedef enum Square_Color {
	Black,
	White} Square_Color;
	
//chess board
typedef struct Chess_Board
{
	Piece board[64];
	int numberOfWhitePlyaers;
	int numberOfBlackPlayers;
	int bShortCastlingFlag;
	int bLongCastlingFlag;
	int wShortCastlingFlag;
	int wLongCastlingFlag;
	int enPassantCol;
} Chess_Board;

Chess_Board Init_Board();

void Print_Board(Chess_Board board);

char* Piece_To_Short_String(Piece my_piece);

void SquareToIndex(char* square, int* rowIndex, int* colIndex);

int ExequteMove(char* move, Chess_Board* board, Square_Color turnColor);

Square_Color PieceColor(Piece piece);

int CheckChessHypothetical(Chess_Board* board, int sourceRowIndex, int sourceColIndex, int destRowIndex, int destColIndex
	, Square_Color turnColor, int returnIfAccept);
	
int CheckChess(Chess_Board board, int kingRowIndex, int kingColIndex, Square_Color turnColor);

int CheckAvailableMoves(Chess_Board board, Square_Color turnColor);

int CheckPromotion(Chess_Board board, Square_Color turnColor);

int PromotePawn(Chess_Board* board, Square_Color turnColor, int col, Piece piece);
