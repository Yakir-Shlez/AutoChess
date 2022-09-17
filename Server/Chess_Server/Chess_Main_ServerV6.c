#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <errno.h>
#include <signal.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <netdb.h>
#include <poll.h>
#include <sys/un.h>
#include <pthread.h>
#include <math.h>
#include <time.h>
#include <mariadb/mysql.h>
//#include <Chess_Api.h>
#include "Chess_Api.h"

#define PORT "3333" // server port
#define MAXDATASIZE 1024 // max number of bytes we can get at once
#define SOCK_PATH "ChessDBUnixFile"
#define playersCountJump 20

typedef struct Player
{
	char nickname[30];
	char username[30];
	char password[30];
	char profile[6];
	char rating[5];
	char ratingDelta[5];
} Player;

typedef struct Lobby_Player
{
	Player opponent;
	int sockfd;
	int pwfd;
	int prfd;
	char state[3];
} Lobby_Player;

typedef struct Opponent_Player
{
	char nickname[30];
	char profile[6];
	char rating[5];
	int sockfd;
	int pwfd;
	int prfd;
	char state[3];
} Opponent_Player;

//lobby players
//Lobby_Player *allPlayersInLobby; //All players currently online
//int lobby_players_Count = 0; //count
//int lobby_players_Size = 0;

//all players
//Player* allPlayers;
//int all_players_Count = 0;

//Handlers
void *Player_Handler(void *ptr);

void *Stdin_Handler(void *ptr);

//game management functions
int Wait_For_Game(Lobby_Player* myPlayer, int friendlyGame);

int Start_Game(Lobby_Player* myPlayer ,Opponent_Player* friend, int friendlyGame);

int Manage_Game(int mfd, int ofd, int pwfd, int white);

void Update_Game(double score, Player *my_Player, int secOpRating);

//lobby & players management functions - only 1 thread at a time
Lobby_Player Add_Player_To_Lobby(Player *opponent, int sockfd);

void Remove_Player_From_Lobby(Player *opponent);

Opponent_Player Search_Player_In_Lobby_Game(Player* my_Player);

Opponent_Player Search_Player_In_Lobby_Nickname(char* nickname);

void View_Lobby();

int Update_All_Players_Offline();

int Update_Player_Nickname(Player* myPlayer, char* nickname);

int Update_Player_Username(Player* myPlayer, char* username);

void Update_Player_Password(Player* myPlayer, char* password);

void Update_Player_Profile(Player* myPlayer, char* profile);

void Update_Player_Rating(Player* myPlayer, char* rating);

void Update_Player_RatingDelta(Player* myPlayer, char* ratingDelta);

void Update_Lobby_Player_State(Lobby_Player* myPlayer, char* state);

void Update_Lobby_Player_State_Free(Lobby_Player* myPlayer);

void Update_Lobby_Player_State_Waiting(Lobby_Player* myPlayer);

void Update_Lobby_Player_State_Busy(Lobby_Player* myPlayer);

//int Get_Player(char *username, Player **player);

int Register_Player(Player *playerToRegister, char *out_buf);

//DB functions
int DB_Query_Delete (char *table, char *where);

int DB_Query_Insert (char *table, char *columns, char *values);

int DB_Query_Update (char *table, char *set, char *where);

MYSQL_RES *DB_Query_Select (char *table, char *columns, char *where);

int DB_Query (char *query);

Player Receive_Player_From_DB(char *username, char *password);

//Connection & general functions
//Send & Receive player functions
void Send_Player(Player* player_To_Send, int fd);

Player recv_Player(int fd);

//rec data
int Recv_Data(int fd, int size, char* buf);

int Send_Data(int fd, char* buf);

int Read_Pipe(int prfd, char* buf, int size);

int Write_Pipe(int pwfd, char *buf);

//Print player function
void Print_All_Players();

void Print_Player_Player(Player player);

void Print_Player_Stats(Player player);

void *get_in_addr(struct sockaddr *sa);

int get_listener_socket(char *port);

//cleanup
void Thread_Cleanup(Lobby_Player *player);

//signal handler
void sigchld_handler(int signo);

// Main
int main(int argc, char *argv[])
{
    int listener;     // Listening socket descriptor

    int newfd;        // Newly accept()ed socket descriptor

    struct sockaddr_storage remoteaddr; // Client address
    struct sockaddr_un remote;
    int len;
    socklen_t addrlen;
	struct pollfd pfds[2];
    char remoteIP[INET6_ADDRSTRLEN];

	Update_All_Players_Offline();

    // Set up and get a listening socket
    listener = get_listener_socket(PORT);

    if (listener == -1) {
        fprintf(stderr, "error getting listening socket\n");
        exit(1);
    }

	
	pfds[0].fd = 0;
    pfds[0].events = POLLIN;
    pfds[1].fd = listener;
    pfds[1].events = POLLIN;
    

    // Main loop
    for(;;) {
		
		int poll_count = poll(pfds, 2, -1);

        if (poll_count == -1) {
            perror("poll");
            exit(1);
        }
        
        if (pfds[0].revents & POLLIN) {
			//stdin
			pthread_t thread;
			pthread_create( &thread, NULL, Stdin_Handler, NULL);
		}
		else
		{
			//listener
			addrlen = sizeof remoteaddr;
			newfd = accept(listener,
				(struct sockaddr *)&remoteaddr,
				&addrlen);

			if (newfd == -1) {
				perror("accept");
			} else {
					
				printf("new connection from %s on socket %d\n",
					inet_ntop(remoteaddr.ss_family,
					get_in_addr((struct sockaddr*)&remoteaddr),
					remoteIP, INET6_ADDRSTRLEN),
					newfd);
				
				pthread_t thread;
				pthread_create( &thread, NULL, Player_Handler, (void*)(&newfd));
				pthread_detach(thread);
			}
		}
    } // END for(;;)--and you thought it would never end!
    
    return 0;
}


//Handlers
void *Player_Handler(void *ptr)
{
	Player myPlayer; 
	Lobby_Player myLobbyPlayer;
	Opponent_Player oppon;
	char buf[MAXDATASIZE];    // Buffer for client data
	char secbuf[MAXDATASIZE];    // Buffer for client data
	int sfd = *((int *)ptr);
	int flag = 0;
	int nfds = 1;
	int prfd;
	int invitePlayer = 0;
	int inviteMe = 0;
	int init = 0;
	
	struct pollfd pfds [2];
	pfds[0].fd = sfd;
    pfds[0].events = POLLIN;
    //pfds[1].fd = prfd;
    //pfds[1].events = POLLIN;    

	for(;;)
	{
		int poll_count = poll(pfds, nfds, 0);
		
		/*
		if(nfds == 1)
			printf("%d %d\n", sfd, nfds);
		else
			printf("%d %d %d\n", sfd, nfds, pfds[1].fd);
		*/
		
        if (poll_count == -1) {
            perror("poll");
            exit(1);
        }
        
        if (pfds[0].revents & POLLIN) {
			if(init == 1)
				Update_Lobby_Player_State_Busy(&myLobbyPlayer);
				
			//client
			int nbytes = Recv_Data(sfd, 10, buf);
			if(nbytes <= 0)
			{
				Thread_Cleanup(&myLobbyPlayer);
				return NULL;
			}

			// we got some data from a client
			
			if(invitePlayer == 1) {
				if(strcmp(buf,"cancel____") == 0)
				{
					Send_Data(sfd, "Ack__");
					Write_Pipe(oppon.pwfd, "cancel____");
					invitePlayer = 0;
				}
			}
			
			else if(inviteMe == 1)
			{
				Write_Pipe(oppon.pwfd, buf);
				if(strcmp(buf, "yes_______") == 0)
				{
					Send_Data(sfd, "game_");
					//initiate game - passive
					int result = Wait_For_Game(&myLobbyPlayer, 1);
					if(result == 3)
					{
						Thread_Cleanup(&myLobbyPlayer);
						return NULL;
					}
					inviteMe = 0;
				}
				else
				{
					Send_Data(sfd, "Ack__");
					inviteMe = 0;
				}
			}
			
			else if(strcmp(buf,"log_in____") == 0 && init == 0)	{
				nbytes = Recv_Data(sfd, MAXDATASIZE - 1, buf);
				if(nbytes <= 0)
				{
					printf("0\n");
					Thread_Cleanup(&myLobbyPlayer);
					return NULL;
				}
				char *user = strtok(buf,"/");
				char *pass = strtok(NULL,"/");
				
				myPlayer = Receive_Player_From_DB(user, pass);
				if(strcmp(user, myPlayer.username) != 0)
					Send_Data(sfd, "Denid");
				else
				{
					myLobbyPlayer = Add_Player_To_Lobby(&myPlayer, sfd);
					
					if(strcmp(myLobbyPlayer.state, "99") != 0)
					{
						//myPlayer = myLobbyPlayer -> opponent;
						Send_Data(sfd, "Ack__");
						Send_Player(&myPlayer, sfd);
						prfd = myLobbyPlayer.prfd;
						pfds[1].fd = prfd;
						pfds[1].events = POLLIN;
						nfds = 2;
						printf("%s logged in, client socket = %d, pipe read socket = %d, pipe write socket = %d\n",
							myLobbyPlayer.opponent.nickname, myLobbyPlayer.sockfd, myLobbyPlayer.prfd, myLobbyPlayer.pwfd);
						init = 1;
						//Print_Player_Player(myPlayer[0]);
					}
					else
					{
						Send_Data(sfd, "contd");
					}
				}
			} 
			
			else if(strcmp(buf,"register__") == 0 && init == 0)	{
				myPlayer = recv_Player(sfd);
				if(strcmp("0", myPlayer.rating) != 0)
				{
					Thread_Cleanup(&myLobbyPlayer);
					return NULL;
				}
				strcpy(myPlayer.rating,"1500");
				
				flag = Register_Player(&myPlayer, buf);
				
				if(flag == 0)
				{
					Send_Data(sfd, "Denid");
					Send_Data(sfd, buf);
				}
				else
				{
					myLobbyPlayer = Add_Player_To_Lobby(&myPlayer, sfd);
					Send_Data(sfd, "Ack__");
					prfd = myLobbyPlayer.prfd;
					pfds[1].fd = prfd;
					pfds[1].events = POLLIN;
					nfds = 2;
					printf("%s registered & logged in, client socket = %d, pipe read socket = %d, pipe write socket = %d\n",
						myLobbyPlayer.opponent.nickname, myLobbyPlayer.sockfd, myLobbyPlayer.prfd, myLobbyPlayer.pwfd);
					init = 1;
				}
			}
			
			else if(strcmp(buf, "update____") == 0 && init == 1)	{
				nbytes = Recv_Data(sfd, 10, buf);
				if(nbytes <= 0)
				{
					Thread_Cleanup(&myLobbyPlayer);
					return NULL;
				}
				
				if(strcmp(buf, "nickname__") == 0)
				{
					nbytes = Recv_Data(sfd, 29, secbuf);
					if(nbytes <= 0)
					{
						Thread_Cleanup(&myLobbyPlayer);
						return NULL;
					}
					if(Update_Player_Nickname(&myPlayer, secbuf) == 1)
						Send_Data(sfd, "Ack__");
					else
						Send_Data(sfd, "Denid");
				}
				/*else if(strcmp(buf, "username__") == 0)
				{
					nbytes = Recv_Data(sfd, 29, secbuf);
					if(nbytes <= 0)
					{
						Thread_Cleanup(&myLobbyPlayer);
						return NULL;
					}
					if(Update_Player_Username(&myPlayer, secbuf) == 1)
						Send_Data(sfd, "Ack__");
					else
						Send_Data(sfd, "Denid");
				}*/
				else if(strcmp(buf, "password__") == 0)
				{
					nbytes = Recv_Data(sfd, 29, secbuf);
					if(nbytes <= 0)
					{
						Thread_Cleanup(&myLobbyPlayer);
						return NULL;
					}
					Update_Player_Password(&myPlayer, secbuf);
					Send_Data(sfd, "Ack__");
				}
				else if(strcmp(buf, "profile___") == 0)
				{
					nbytes = Recv_Data(sfd, 5, secbuf);
					if(nbytes <= 0)
					{
						Thread_Cleanup(&myLobbyPlayer);
						return NULL;
					}
					Update_Player_Profile(&myPlayer, secbuf);
					Send_Data(sfd, "Ack__");
				}
				else if(strcmp(buf, "ratingdelt") == 0)
				{
					nbytes = Recv_Data(sfd, 4, secbuf);
					if(nbytes <= 0)
					{
						Thread_Cleanup(&myLobbyPlayer);
						return NULL;
					}
					Update_Player_RatingDelta(&myPlayer, secbuf);
					Send_Data(sfd, "Ack__");
				}
				
			}
			
			else if(strcmp(buf,"get_user__") == 0 && init == 1)	{
				Send_Player(&myPlayer, sfd);			
			}
			
			else if(strcmp(buf, "get_rate__") == 0 && init == 1)	{
				Send_Data(sfd, myPlayer.rating);
			}
			
			else if(strcmp(buf, "startgame_") == 0 && init == 1)	{ 
				printf("Start game %d\n", myLobbyPlayer.sockfd);
				int result = Start_Game(&myLobbyPlayer, NULL, 0);
				if(result == 3)
				{
					Thread_Cleanup(&myLobbyPlayer);
					return NULL;
				}
			}
			
			else if(strcmp(buf, "invite____") == 0 && init == 1)	{ 
				nbytes = Recv_Data(sfd, 29, secbuf);
				if(nbytes <= 0)
				{
					Thread_Cleanup(&myLobbyPlayer);
					return NULL;
				}
					
				oppon = Search_Player_In_Lobby_Nickname(secbuf);
				if(strcmp(oppon.state, "99") == 0)
					Send_Data(sfd, "nfond");
				else if(strcmp(oppon.state, "00") == 0)
				{
					Send_Data(sfd, "Ack__");
					invitePlayer = 1;
					Write_Pipe(oppon.pwfd, "invite____");
					Write_Pipe(oppon.pwfd, myPlayer.nickname);
				}
				else
					Send_Data(sfd, "busy_");
			}
			
			/*else if(strcmp(buf, "viewlobby_") == 0 && myLobbyPlayer != NULL)	{) 
			{
				View_Lobby(buf);
				send(sfd, buf, strlen(buf), 0);
			}
			*/
			if(inviteMe == 0 && invitePlayer == 0 && init == 1)
				Update_Lobby_Player_State_Free(&myLobbyPlayer);
		}
		else if (pfds[1].revents & POLLIN) {
			//pipe
			
			Read_Pipe(prfd, buf, 10);
			
			if(invitePlayer == 1) {
				if(strcmp(buf, "yes_______") == 0)
				{
					Send_Data(sfd, "game_");
					printf("game active %d\n", sfd); 
					//start game - initiate
					int result = Start_Game(&myLobbyPlayer, &oppon, 1);
					if(result == 3)
					{
						Thread_Cleanup(&myLobbyPlayer);
						return NULL;
					}
					invitePlayer = 0;
				}
				else
				{
					Send_Data(sfd, "Denid");
					invitePlayer = 0;
				}
			}
			
			else if(inviteMe == 1)
			{
				if(strcmp(buf,"cancel____") == 0)
				{
					Send_Data(sfd, "Cancl");
					inviteMe = 0;
				}
			}
			
			else if(strcmp(buf,"invite____") == 0 && init == 1)	{
				int pwfd;
				int nbytes = Read_Pipe(prfd, buf, 30);
				
				oppon = Search_Player_In_Lobby_Nickname(buf); 
				
				if(strcmp(oppon.state, "99") != 0)
				{
					pwfd = (oppon.pwfd);
					if(strcmp(myLobbyPlayer.state, "00") == 0)
					{
						Update_Lobby_Player_State_Busy(&myLobbyPlayer);
						Send_Data(sfd, "invit");		
						Send_Data(sfd, buf);
						inviteMe = 1;
					}
					else
						Write_Pipe(pwfd, "no________");
				}
			}
			
			if(inviteMe == 0 && invitePlayer == 0 && init == 1)
				Update_Lobby_Player_State_Free(&myLobbyPlayer);
		}
	}
	return NULL;
}

void *Stdin_Handler(void *ptr) //TBD
{
	char buf[MAXDATASIZE]; 
	int nbytes = read(0, buf, MAXDATASIZE - 1);
	buf[nbytes - 1] = 0;
	printf("%s\n", buf);
	printf("%d %d\n", strcmp("Display_All_Players", buf), strcmp("Display_All_Lobby", buf));
	//if(strcmp("Display_All_Players", buf) == 0)
	//	Print_All_Players();
	//else if(strcmp("Display_All_Lobby", buf) == 0)
	//	View_Lobby();
}

//game management functions
int Wait_For_Game(Lobby_Player* myPlayer, int friendlyGame)
{
	int sfd = myPlayer -> sockfd;
	printf("%d Wait for game\n",sfd);
	int result;
	int opRate;
	char buf[MAXDATASIZE];
	int prfd = myPlayer -> prfd;
	int inGame = 0;
	int endGame = 0;
	int respondCounter = 0;
	//return 3;
	struct pollfd pfds[2];
	pfds[0].fd = myPlayer->sockfd;
    pfds[0].events = POLLIN;
    pfds[1].fd = prfd;
    pfds[1].events = POLLIN;
	//return 3;
    Update_Lobby_Player_State_Waiting(myPlayer);
    
    for(;;) {
        int poll_count = poll(pfds, 2, -1);

        if (poll_count == -1) {
            perror("poll");
            exit(1);
        }
        
        if (pfds[0].revents & POLLIN) {
			//client
			int nbytes = Recv_Data(sfd, 10, buf);
			
			if(nbytes <= 0)
				return 3;
			// we got some data from a client
	
			if(strcmp(buf, "cancel____") == 0)
				return -1;
		}
		else if (pfds[1].revents & POLLIN) {
			//pipe
			Read_Pipe(prfd, buf, 10);
			printf("%s from socket pipe\n", buf);
			if(strcmp(buf, "wait______") == 0)
			{
				respondCounter = 1 ;
				for (int i = 0; i < respondCounter; i++)
				{
					Read_Pipe(prfd, buf, 10);
					printf("%s from socket pipe\n", buf);
					if(strcmp(buf, "continue__") == 0)
					{
						endGame = 1;
						i--;
						continue;
					}
					if(strcmp(buf, "wait______") == 0)
					{
						respondCounter++;
						i--;
					}
					else
					{
						int pwfd = atoi(buf);
						if(inGame == 0)
						{
							Update_Lobby_Player_State_Busy(myPlayer);
							pfds[0].fd = -1;
							Write_Pipe(pwfd, "Ack_______");
							inGame = 1;
						}
						else
							Write_Pipe(pwfd, "busy______");
					}
				}
			}
			if(strcmp(buf, "continue__") == 0)
				endGame = 1;
			if(endGame == 1)
			{
				Read_Pipe(prfd, buf, 1);
				result = atoi(buf);
					
				Read_Pipe(prfd, buf, 4);
				opRate = atoi(buf);
					
				if(friendlyGame == 0)
				{
					if(result == 0 || result == 1)
						Update_Game((double)result, &(myPlayer -> opponent), opRate);
					else if(result == 2)
						Update_Game(0.5, &(myPlayer -> opponent), opRate);
					else if(result == 3)
					{
						Update_Game(0, &(myPlayer -> opponent), opRate);
						Remove_Player_From_Lobby(&(myPlayer -> opponent));
					}
				}
				else if(result == 3)
					Remove_Player_From_Lobby(&(myPlayer -> opponent));
				return result;
			}
		}
	}
	return -1;
}

int Start_Game(Lobby_Player* myPlayer ,Opponent_Player* friend, int friendlyGame) 
{
	int result = -1;
	char buf[MAXDATASIZE];
	char whiteBuf[2];
	char rateBuf[5];
	char opUser[30];
	int sfd = myPlayer -> sockfd;
	int counter;
	int game = 0;
	int pwfd;
	int ofd;
	
	Opponent_Player oppon;
	do
	{
		if(friendlyGame == 0)
		{
			oppon = Search_Player_In_Lobby_Game(&(myPlayer -> opponent));
			printf("%s\n", oppon.state);
			if(strcmp(oppon.state, "99") == 0)		
				return Wait_For_Game(myPlayer, 0);
		}
		else
		{
			oppon = friend[0];
			game = 1;
		}
			
		pwfd = (oppon.pwfd);
		ofd = (oppon.sockfd);

		printf("Found %d\n",ofd);
		Write_Pipe(pwfd, "wait______");
		sprintf(buf, "%010d", myPlayer -> pwfd);
		Write_Pipe(pwfd, buf);
		Read_Pipe(myPlayer -> prfd, buf, 10);
		if(strcmp(buf, "Ack_______") == 0)
			game = 1;
	} while(game == 0);
	int white;
	
							//Write_Pipe(pwfd, "Ack_______");
							//Write_Pipe(pwfd, "busy______");
	
	double randomNum = ((double)rand() / (double)RAND_MAX);
	//double randomNum = 0;
	if(randomNum < 0.5)
		white = 1;
	else
		white = 0;
		
	sprintf(whiteBuf, "%d", white);
	Send_Data(sfd, whiteBuf);
	Send_Data(sfd, "/");
	Send_Data(sfd, oppon.nickname);
	Send_Data(sfd, "/");
	Send_Data(sfd, oppon.rating);
	Send_Data(sfd, "/");
	Send_Data(sfd, oppon.profile);
	Send_Data(sfd, "/");
	
	if(white == 1)
		white = 0;
	else
		white = 1;
	
	sprintf(whiteBuf, "%d", white);
	Send_Data(ofd, whiteBuf);
	Send_Data(ofd, "/");
	Send_Data(ofd, myPlayer -> opponent.nickname);
	Send_Data(ofd, "/");
	Send_Data(ofd, myPlayer -> opponent.rating);
	Send_Data(ofd, "/");
	Send_Data(ofd, myPlayer -> opponent.profile);
	Send_Data(ofd, "/");
	
	if(white == 1)
		white = 0;
	else
		white = 1;
	
	char white_Nickname[MAXDATASIZE];
	char white_Rating[MAXDATASIZE];
	char black_Nickname[MAXDATASIZE];
	char black_Rating[MAXDATASIZE];
	char winner_Nickname[MAXDATASIZE];
		
	if(white == 1)
	{
		strcpy(white_Nickname, myPlayer -> opponent.nickname);
		strcpy(white_Rating, myPlayer -> opponent.rating);
		strcpy(black_Nickname, oppon.nickname);
		strcpy(black_Rating, oppon.rating);
	}
	else
	{
		strcpy(white_Nickname, oppon.nickname);
		strcpy(white_Rating, oppon.rating);
		strcpy(black_Nickname, myPlayer -> opponent.nickname);
		strcpy(black_Rating, myPlayer -> opponent.rating);
	}
	
	sprintf(buf, "'%s', '%s', '%s', '%s'"
		, white_Nickname, white_Rating, black_Nickname, black_Rating);
		
	DB_Query_Insert("Ongoing_Games", "White_Nickname, White_Rating, Black_Nickname, Black_Rating", buf);
	
	result = Manage_Game(sfd, ofd, pwfd, white);

	sprintf(buf, "White_Nickname = '%s'"
		, white_Nickname);
	
	DB_Query_Delete("Ongoing_Games", buf);

	if(result == 0 || result == 3)
		sprintf(winner_Nickname,"'%s'", oppon.nickname);
	else if(result == 1)
		sprintf(winner_Nickname,"'%s'", myPlayer -> opponent.nickname);
	else
		strcpy(winner_Nickname, "NULL");

	sprintf(buf, "'%s', '%s', '%s', '%s', %s, '%d'" //no '' in winner_Nickname %s
		, white_Nickname, white_Rating, black_Nickname, black_Rating, winner_Nickname, friendlyGame);
		
	DB_Query_Insert("Game_History", "White_Nickname, White_Rating, Black_Nickname, Black_Rating, Winner_Nickname, Friendly_Game", buf);
	
	sprintf(rateBuf, "%04d", atoi(myPlayer -> opponent.rating));
	Write_Pipe(pwfd, rateBuf);
	
	int opRate = atoi(oppon.rating);
	
	if(friendlyGame == 0)
	{
		if(result == 0 || result == 1)
			Update_Game((double)result, &(myPlayer -> opponent), opRate);
		else if(result == 2)
			Update_Game(0.5, &(myPlayer -> opponent), opRate);
		else if(result == 3)
		{
			Update_Game(0, &(myPlayer -> opponent), opRate);
			Remove_Player_From_Lobby(&(myPlayer -> opponent));
		}
	}
	else if(result == 3)
		Remove_Player_From_Lobby(&(myPlayer -> opponent));
			
	return result;
}

int Manage_Game(int mfd, int ofd, int pwfd, int white) 
{
	printf("Game started: %d against %d\n", mfd, ofd);
	char buf[MAXDATASIZE];
	int nbytes;
	int drawReq = 0;
	int opMove = 1;
	double randomNum;
	int curMovfd;
	int opMovfd;
	int temp;
	int proCol;
	int pro = 0;
	Square_Color curMove;
	time_t mMovTime, oMovTime, lastCycleTime, maxTime;
	//maxTime = 30;
	mMovTime = 0;
	oMovTime = 0;
	maxTime = 30 * 60; //30 minutes 
	time_t* curMovTime; 
	struct pollfd pfds[2];
	
	Chess_Board board;
	board = Init_Board();
    //Print_Board(board);
    curMove = White;
	
	if(white == 1)
	{
		curMovTime = &mMovTime;
		curMovfd = mfd;
		opMovfd = ofd;
	}
	else
	{
		curMovTime = &oMovTime;
		curMovfd = ofd;
		opMovfd = mfd;
	}
	printf("%d is white\n", curMovfd);
	
	pfds[0].fd = curMovfd;
    pfds[0].events = POLLIN;
    pfds[1].fd = opMovfd;
    pfds[1].events = POLLIN;
    lastCycleTime = time(NULL);
    //printf("start for\n");
	for(;;)
	{
		//printf("1\n");
		//Print_Board(board);
		//printf("2\n");
		if(CheckAvailableMoves(board, curMove) == 0)
		{
			if(CheckChess(board, -1, -1, curMove) == 1)
			{
				Send_Data(opMovfd,"Win__");
				Send_Data(curMovfd,"Lose_");
				Write_Pipe(pwfd, "continue__");
				if(curMovfd == mfd)
				{
					Write_Pipe(pwfd, "1");
					return 0;
				}
				else
				{
					Write_Pipe(pwfd, "0");
					return 1;
				}
			}
			else
			{
				Send_Data(opMovfd,"stlmt");
				Send_Data(curMovfd,"stlmt");
				Write_Pipe(pwfd, "continue__");
				Write_Pipe(pwfd, "2");
				return 2;
			}
		}
		//printf("3\n");
		opMove = 0;
		do {
			curMovTime[0] += (time(NULL) - lastCycleTime);
			lastCycleTime = time(NULL);
			//printf("%d\n", (int)(curMovTime[0]));
			
			if(curMovTime[0] == maxTime) // timed up
			{
				Send_Data(curMovfd, "ytime");
				Send_Data(opMovfd, "otime");
				Write_Pipe(pwfd, "continue__");
				if(curMovfd == mfd)
				{
					Write_Pipe(pwfd, "1");
					return 0;
				}
				else
				{
					Write_Pipe(pwfd, "0");
					return 1;
				}
			}
			//printf("4\n");
			int poll_count = poll(pfds, 2, 0);
			//printf("111\n");
			if (poll_count == -1) {
				perror("poll");
				exit(1);
			}
			//printf("5\n");
			if (pfds[0].revents & POLLIN) //current turn player
			{
				//printf("6\n");
				opMove = 1;
				
				//printf("11\n");
				nbytes =  Recv_Data(curMovfd, 5, buf);
				//printf("21\n");
				if (nbytes <= 0) {
					Send_Data(opMovfd, "Resin");
					Write_Pipe(pwfd, "continue__");
					if(curMovfd == mfd)
					{
						Write_Pipe(pwfd, "1");
						return 3;
					}
					else
					{
						Write_Pipe(pwfd, "3");
						return 1;
					}
				}
				// we got some data from a client
				if(drawReq == 1)
				{
					Send_Data(opMovfd, buf);
					if(strcmp(buf, "Yes__") == 0)
					{
						Write_Pipe(pwfd, "continue__");
						Write_Pipe(pwfd, "2");
						return 2;
					}
					drawReq = 0;
				}
				else if(pro == 1)
				{
					if(strcmp(buf, "Queen") == 0)
					{
						if(curMove == White)
							PromotePawn(&board, curMove, proCol, WQueen);
						else
							PromotePawn(&board, curMove, proCol, BQueen);
					}
					else if(strcmp(buf, "Rook_") == 0)
					{
						if(curMove == White)
							PromotePawn(&board, curMove, proCol, WRook);
						else
							PromotePawn(&board, curMove, proCol, BRook);
					}
					else if(strcmp(buf, "Bisop") == 0)
					{
						if(curMove == White)
							PromotePawn(&board, curMove, proCol, WBishop);
						else
							PromotePawn(&board, curMove, proCol, BBishop);
					}
					else if(strcmp(buf, "Knigt") == 0)
					{
						if(curMove == White)
							PromotePawn(&board, curMove, proCol, WKnight);
						else
							PromotePawn(&board, curMove, proCol, BKnight);
					}
					else
					{
						Send_Data(curMovfd, "Denid");
						opMove = 0;
					}
					if(opMove == 1)
					{
						proCol = -1;
						pro = 0;
						Send_Data(curMovfd, "Ack__");
						Send_Data(opMovfd, buf);
						Send_Data(opMovfd, "End__");
					}
				}
				else if(strcmp(buf, "Resin") == 0)
				{
					Send_Data(opMovfd, buf);
					Write_Pipe(pwfd, "continue__");
					if(curMovfd == mfd)
					{
						Write_Pipe(pwfd, "1");
						return 0;
					}
					else
					{
						Write_Pipe(pwfd, "0");
						return 1;
					}
				}
				else if(strcmp(buf, "Draw_") == 0)
				{
					drawReq = 1;
					Send_Data(opMovfd, buf);
				}
				else
				{			
					if(nbytes != 5)
					{
						Send_Data(curMovfd, "Denid");
						opMove = 0;
					}
					else if(buf[0] < 97 || buf[0] > 104 ||
						buf[1] < 49 || buf[1] > 56 || 
						buf[2] != 45 ||
						buf[3] < 97 || buf[3] > 104 || 
						buf[4] < 49 || buf[4] > 56)
					{
						Send_Data(curMovfd, "Denid");
						opMove = 0;
					}
					else if(ExequteMove(buf, &board, curMove) == 0)
					{
						Send_Data(curMovfd, "Denid");
						opMove = 0;
					}
					else
					{
						proCol = CheckPromotion(board, curMove);
						if(proCol != -1)
						{
							Send_Data(curMovfd, "Promo");
							Send_Data(opMovfd, buf);
							Send_Data(opMovfd, "Promo");
							pro = 1;
							opMove = 0;
						}
						else
						{
							Send_Data(curMovfd, "Ack__");
							Send_Data(opMovfd, buf);
							Send_Data(opMovfd, "End__");
							opMove = 1;
						}
					}			
				}
			}
			else if (pfds[1].revents & POLLIN) { //waiting player
				//printf("7\n");
				nbytes = Recv_Data(opMovfd, 5, buf);
				if (nbytes <= 0) {
					Send_Data(curMovfd, "Resin");
					Write_Pipe(pwfd, "continue__");
					if(opMovfd == mfd)
					{
						Write_Pipe(pwfd, "1");
						return 3;
					}
					else
					{
						Write_Pipe(pwfd, "3");
						return 1;
					}
				}
				// we got some data from a client
				if(strcmp(buf, "Resin") == 0)
				{
					Send_Data(curMovfd, buf);
					Write_Pipe(pwfd, "continue__");
					if(opMovfd == mfd)
					{
						Write_Pipe(pwfd, "1");
						return 0;
					}
					else
					{
						Write_Pipe(pwfd, "0");
						return 1;
					}
				}
			}
		} while(opMove == 0);
		temp = curMovfd;
		curMovfd = opMovfd;
		opMovfd = temp;
		pfds[0].fd = curMovfd;
		pfds[1].fd = opMovfd;
		lastCycleTime = time(NULL);
		if(curMovTime == &mMovTime)
			curMovTime = &oMovTime;
		else
			curMovTime = &mMovTime;
		if(curMove == White)
			curMove = Black;
		else
			curMove = White;
	}
	return -1;
} 

void Update_Game(double score, Player *my_Player, int secOpRating)
{
	char buf[MAXDATASIZE];
	char rateBuf[5];
	int new_Rating = atoi(my_Player-> rating);
	double R1 = pow(10, ((double)new_Rating) / 400);
	double R2 = pow(10, ((double)secOpRating) / 400);
			
	double E1 = R1 / (R1 + R2);
				
	double K = 20;
				
	new_Rating += (int)round(K * (score - E1));
	
	sprintf(rateBuf,"%d",new_Rating);
	rateBuf[4] = 0;
	
	Update_Player_Rating(my_Player, rateBuf);
}


//lobby & players management functions - only 1 thread at a time
Lobby_Player Add_Player_To_Lobby(Player *opponent, int sockfd)
{
	char buf[MAXDATASIZE];
	int pfds[2];
		
	Lobby_Player new_Looby_Player;
	
	strcpy(new_Looby_Player.state, "00");
	new_Looby_Player.opponent = opponent[0];
	new_Looby_Player.sockfd = sockfd;
	
	if(pipe(pfds) == -1) {
		perror("pipe");
		exit(1);
	}
	new_Looby_Player.pwfd = pfds[1];
	new_Looby_Player.prfd = pfds[0];
	
	sprintf(buf, "'%s', '%d', '%d', '%d', '%s'"
		, opponent -> username, sockfd, pfds[1], pfds[0], new_Looby_Player.state);
		
	int ret = DB_Query_Insert("Online_Users", "Username, Socket, Pipe_Write, Pipe_Read, State", buf);
	
	if(ret == 0)
		strcpy(new_Looby_Player.state, "99");
	
	return new_Looby_Player;
}

void Remove_Player_From_Lobby(Player *opponent)
{
	char buf[MAXDATASIZE];
	
	sprintf(buf, "Username = '%s'"
		, opponent -> username);
	
	DB_Query_Delete("Online_Users", buf);
	
	return;
}

Opponent_Player Search_Player_In_Lobby_Game(Player* my_Player)
{
	int best_Delta = 4000;
	char buf[MAXDATASIZE];
	Opponent_Player oppon;
	
	sprintf(buf, "State = '01' AND ABS(Rating - %s) < %s AND ABS(Rating - %s) < Game_Rating_Delta"
		, my_Player -> rating, my_Player -> ratingDelta, my_Player -> rating);

	MYSQL_RES *results = DB_Query_Select("Online_Users_Server", "Socket, Pipe_Write, Pipe_Read, Nickname, Rating , Profile_Pic", buf);
		
	MYSQL_ROW row;
	MYSQL_ROW best_Row = NULL;
	best_Delta = 5000;
	
	while((row = mysql_fetch_row(results)))
	{
		printf(row[4]);
		printf(my_Player -> rating);
		printf("\n");
		if(abs(atoi(row[4]) - atoi(my_Player -> rating)) < best_Delta)
		{
			printf("best\n");
			best_Row = row;
		}
	}
	
	if(best_Row == NULL)
	{
		strcpy(oppon.state, "99");
		return oppon;
	}
	printf("return best\n");
	
	strcpy(oppon.nickname, best_Row[3]);
	strcpy(oppon.profile, best_Row[5]);
	strcpy(oppon.rating, best_Row[4]);
	strcpy(oppon.state, "01");
	
	oppon.sockfd = atoi(best_Row[0]);
	oppon.pwfd = atoi(best_Row[1]);
	oppon.prfd = atoi(best_Row[2]);
	
	mysql_free_result(results);
	
	return oppon;
}

Opponent_Player Search_Player_In_Lobby_Nickname(char* nickname)
{
	int best_Delta = 4000;
	char buf[MAXDATASIZE];
	Opponent_Player oppon;
	
	sprintf(buf, "Nickname = '%s'"
		, nickname);

	MYSQL_RES *results = DB_Query_Select("Online_Users_Server", "Socket, Pipe_Write, Pipe_Read, Nickname, Rating , Profile_Pic, State", buf);
		
	MYSQL_ROW row = mysql_fetch_row(results);
	
	if(row == NULL)
	{
		strcpy(oppon.state, "99");
		return oppon;
	}
	
	strcpy(oppon.nickname, row[3]);
	strcpy(oppon.profile, row[5]);
	strcpy(oppon.rating, row[4]);

	
	oppon.sockfd = atoi(row[0]);
	oppon.pwfd = atoi(row[1]);
	oppon.prfd = atoi(row[2]);
	strcpy(oppon.state, row[6]);
	
	mysql_free_result(results);
	
	return oppon;
}

void View_Lobby()
{
	/* TBD DB
	if(lobby_players_Count == 0)
		printf("Lobby is empty\n");
		
	for(int i = 0; i < lobby_players_Size; i++)
	{
		if(allPlayersInLobby[i].sockfd == -1)
			continue;
		
		printf("nickname: %s\n",allPlayersInLobby[i].opponent -> nickname);
		printf("username: %s\n",allPlayersInLobby[i].opponent -> username);
		printf("password: %s\n",allPlayersInLobby[i].opponent -> password);
		printf("profile: %s\n",allPlayersInLobby[i].opponent -> profile);
		printf("rating: %s\n",allPlayersInLobby[i].opponent -> rating);
		printf("rating delta: %s\n",allPlayersInLobby[i].opponent -> ratingDelta);
		printf("state: %s\n",allPlayersInLobby[i].opponent -> state);
		printf("sockfd: %d\n",allPlayersInLobby[i].sockfd);
		printf("pipe write: %d\n",allPlayersInLobby[i].pwfd);
		printf("pipe read: %d\n",allPlayersInLobby[i].prfd);
	}
	*/
}

int Update_All_Players_Offline()
{
	if(DB_Query("TRUNCATE TABLE Online_Users") == 0)
		return 0;
	if(DB_Query("TRUNCATE TABLE Ongoing_Games") == 0)
		return 0;
	return 1;
}

int Update_Player_Nickname(Player* myPlayer, char* nickname)
{
	char set_buf[MAXDATASIZE];
	char where_buf[MAXDATASIZE];
	int result;
	
	sprintf(set_buf, " Nickname = '%s'"
		, nickname);
		
	sprintf(where_buf, " Username = '%s'"
		, myPlayer -> username);
		
	result = DB_Query_Update("Users", set_buf, where_buf);
	
	if(result == 1)
	{
		strcpy(myPlayer -> nickname, nickname);
	}
}

int Update_Player_Username(Player* myPlayer, char* username)
{
	char set_buf[MAXDATASIZE];
	char where_buf[MAXDATASIZE];
	int result;
	
	sprintf(set_buf, " Username = '%s'"
		, username);
		
	sprintf(where_buf, " Nickname = '%s'"
		, myPlayer -> nickname);
		
	result = DB_Query_Update("Users", set_buf, where_buf);
	
	if(result == 1)
	{
		strcpy(myPlayer -> username, username);
	}
}

void Update_Player_Password(Player* myPlayer, char* password)
{	
	char set_buf[MAXDATASIZE];
	char where_buf[MAXDATASIZE];
	int result;
	
	sprintf(set_buf, " Password = '%s'"
		, password);
		
	sprintf(where_buf, " Username = '%s'"
		, myPlayer -> username);
		
	result = DB_Query_Update("Users", set_buf, where_buf);
	
	if(result == 1)
	{
		strcpy(myPlayer -> password, password);
	}
}

void Update_Player_Profile(Player* myPlayer, char* profile)
{
	char set_buf[MAXDATASIZE];
	char where_buf[MAXDATASIZE];
	int result;
	
	sprintf(set_buf, " Profile_Pic = '%s'"
		, profile);
		
	sprintf(where_buf, " Username = '%s'"
		, myPlayer -> username);
		
	result = DB_Query_Update("Users", set_buf, where_buf);
	
	if(result == 1)
	{
		strcpy(myPlayer -> profile, profile);
	}
}

void Update_Player_Rating(Player* myPlayer, char* rating)
{
	char set_buf[MAXDATASIZE];
	char where_buf[MAXDATASIZE];
	int result;
	
	sprintf(set_buf, " Rating = '%s'"
		, rating);
		
	sprintf(where_buf, " Username = '%s'"
		, myPlayer -> username);
		
	result = DB_Query_Update("Users", set_buf, where_buf);
	
	if(result == 1)
	{
		strcpy(myPlayer -> rating, rating);
	}
}

void Update_Player_RatingDelta(Player* myPlayer, char* ratingDelta)
{
	char set_buf[MAXDATASIZE];
	char where_buf[MAXDATASIZE];
	int result;
	
	sprintf(set_buf, " Game_Rating_Delta = '%s'"
		, ratingDelta);
		
	sprintf(where_buf, " Username = '%s'"
		, myPlayer -> username);
		
	result = DB_Query_Update("Users", set_buf, where_buf);
	
	if(result == 1)
	{
		strcpy(myPlayer -> ratingDelta, ratingDelta);
	}
}

void Update_Lobby_Player_State(Lobby_Player* myPlayer, char *state)
{
	char set_buf[MAXDATASIZE];
	char where_buf[MAXDATASIZE];
	int result;
	
	sprintf(set_buf, " State = '%s'"
		, state);
		
	sprintf(where_buf, " Username = '%s'"
		, myPlayer ->  opponent.username);
		
	result = DB_Query_Update("Online_Users", set_buf, where_buf);
	
	if(result == 1)
	{
		strcpy(myPlayer -> state, state);
	}
}

void Update_Lobby_Player_State_Free(Lobby_Player *myPlayer)
{
	Update_Lobby_Player_State(myPlayer, "00");
}

void Update_Lobby_Player_State_Waiting(Lobby_Player *myPlayer)
{
	Update_Lobby_Player_State(myPlayer, "01");
}

void Update_Lobby_Player_State_Busy(Lobby_Player *myPlayer)
{
	Update_Lobby_Player_State(myPlayer, "02");
}
/*
int Get_Player_From_DB (char *username, char *password, Player **player)
{
	pthread_mutex_lock( &mutexLobby);
	for( int i = 0; i < all_players_Count; i++)
	{
		if(strcmp(username, allPlayers[i].username) == 0)
		{
			player[0] = &(allPlayers[i]);
			pthread_mutex_unlock( &mutexLobby);
			return 1;
		}
	}
	
	pthread_mutex_unlock( &mutexLobby);
	
	return 0;
}
*/

int Register_Player(Player *playerToRegister, char *out_buf)
{	
	char buf[MAXDATASIZE];
	
	sprintf(out_buf, "Username = '%s'"
		, playerToRegister -> username);

	MYSQL_RES *results = DB_Query_Select("Users", "*", buf);
	
	if(mysql_fetch_row(results) != NULL)
	{
		strcpy(buf, "usern");
		return 0;
	}
	
	sprintf(out_buf, "Nickname = '%s'"
		, playerToRegister -> nickname);

	results = DB_Query_Select("Users", "*", buf);
	
	if(mysql_fetch_row(results) != NULL)
	{
		strcpy(buf, "nickn");
		return 0;
	}
	
	sprintf(buf, "'%s', '%s', '%s', '%s', '%s', '%s'"
		, playerToRegister -> nickname, playerToRegister -> username, playerToRegister -> password, playerToRegister -> profile, playerToRegister -> rating, playerToRegister -> ratingDelta);
		
	DB_Query_Insert("User", "Nickname, Username, Password, Profile_pic, Rating, Game_Rating_Delta", buf);
	
	return 1;
}

//DB functions

int DB_Query_Delete (char *db, char *where)
{
	char buf[MAXDATASIZE];
	
	MYSQL *con = mysql_init(NULL);
	
	if(con == NULL)
	{
		perror(mysql_error(con));
		return 0;
	}
	
	if(mysql_real_connect(con, "localhost", "user", "123456",
		"Chess_DB", 0, NULL, 0) == NULL)
	{
		perror(mysql_error(con));
		mysql_close(con);
		return 0;
	}
	
	sprintf(buf, "DELETE FROM %s WHERE %s"
		, db, where);
	
	printf("mysql_query: %s\n", buf);
	if(mysql_query(con, buf))
	{
		perror(mysql_error(con));
		mysql_close(con);
		return 0;
	}

	mysql_close(con);
	
	return 1;
}

int DB_Query_Insert (char *table, char *columns, char *values)
{
	char buf[MAXDATASIZE];
	
	MYSQL *con = mysql_init(NULL);
	
	if(con == NULL)
	{
		perror(mysql_error(con));
		return 0;
	}
	
	if(mysql_real_connect(con, "localhost", "user", "123456",
		"Chess_DB", 0, NULL, 0) == NULL)
	{
		perror(mysql_error(con));
		mysql_close(con);
		return 0;
	}
	
	sprintf(buf, "INSERT INTO %s (%s) VALUES (%s)"
		, table, columns, values);
		
	printf("mysql_query: %s\n", buf);
	if(mysql_query(con, buf))
	{
		perror(mysql_error(con));
		mysql_close(con);
		return 0;
	}

	mysql_close(con);
	
	return 1;
}

int DB_Query_Update (char *table, char *set, char *where)
{
	char buf[MAXDATASIZE];
	
	MYSQL *con = mysql_init(NULL);
	
	if(con == NULL)
	{
		perror(mysql_error(con));
		return 0;
	}
	
	if(mysql_real_connect(con, "localhost", "user", "123456",
		"Chess_DB", 0, NULL, 0) == NULL)
	{
		perror(mysql_error(con));
		mysql_close(con);
		return 0;
	}
	
	sprintf(buf, "UPDATE %s SET %s WHERE %s"
		, table, set, where);
	
	printf("mysql_query: %s\n", buf);
	if(mysql_query(con, buf))
	{
		perror(mysql_error(con));
		mysql_close(con);
		return 0;
	}

	mysql_close(con);
	
	return 1;
}

MYSQL_RES *DB_Query_Select (char *table, char *columns, char *where)
{
	MYSQL_RES *results = NULL;
	char buf[MAXDATASIZE];
	
	MYSQL *con = mysql_init(NULL);
	
	if(con == NULL)
	{
		perror(mysql_error(con));
		return NULL;
	}
	
	if(mysql_real_connect(con, "localhost", "user", "123456",
		"Chess_DB", 0, NULL, 0) == NULL)
	{
		perror(mysql_error(con));
		mysql_close(con);
		return NULL;
	}
	
	sprintf(buf, "SELECT %s FROM %s WHERE %s"
		, columns, table, where);
	
	printf("mysql_query: %s\n", buf);
	if(mysql_query(con, buf))
	{
		perror(mysql_error(con));
		mysql_close(con);
		return NULL;
	}

	results = mysql_store_result(con);

	if(results == NULL)
	{
		perror(mysql_error(con));
		mysql_close(con);
		return NULL;
	}

	mysql_close(con);
	return results;
}

int DB_Query (char *query)
{
	MYSQL *con = mysql_init(NULL);
	
	if(con == NULL)
	{
		perror(mysql_error(con));
		return 0;
	}
	
	if(mysql_real_connect(con, "localhost", "user", "123456",
		"Chess_DB", 0, NULL, 0) == NULL)
	{
		perror(mysql_error(con));
		mysql_close(con);
		return 0;
	}
		
	printf("mysql_query: %s\n", query);
	if(mysql_query(con, query))
	{
		perror(mysql_error(con));
		mysql_close(con);
		return 0;
	}

	mysql_close(con);
	
	return 1;
}

Player Receive_Player_From_DB(char *username, char *password)
{
	char buf[MAXDATASIZE];
	Player new_Player;
	
	sprintf(buf, "Username = '%s' AND Password = '%s'"
		, username, password);
	
	MYSQL_RES *results = DB_Query_Select("Users", "Nickname, Username, Password, Profile_pic, Rating, Game_Rating_Delta", buf);
	
	if(results == NULL)
		return new_Player;
	
	int num_fields = mysql_num_fields(results);
	
	if(num_fields == 0)
		return new_Player;
	
	MYSQL_ROW row = mysql_fetch_row(results);
	
	if(row == NULL)
		return new_Player;
	
	strcpy(new_Player.nickname, row[0]);
	strcpy(new_Player.username, row[1]);
	strcpy(new_Player.password, row[2]);
	strcpy(new_Player.profile, row[3]);
	strcpy(new_Player.rating, row[4]);
	strcpy(new_Player.ratingDelta, row[5]);

	mysql_free_result(results);
	
	return new_Player;
}

//Connection & general functions
//Send & Receive player functions
void Send_Player(Player* player_To_Send, int fd)
{
	Send_Data(fd, player_To_Send->nickname);
	Send_Data(fd, "/");
	Send_Data(fd, player_To_Send->username);
	Send_Data(fd, "/");
	Send_Data(fd, player_To_Send->password);
	Send_Data(fd, "/");
	Send_Data(fd, player_To_Send->profile);
	Send_Data(fd, "/");
	Send_Data(fd, player_To_Send->rating);
	Send_Data(fd, "/");
	Send_Data(fd, player_To_Send->ratingDelta);
	Send_Data(fd, "/");
}

Player recv_Player(int fd)
{
	Player thisPlayer;
	char buf[MAXDATASIZE];
	char *splitted;
	int nbytes = Recv_Data(fd, MAXDATASIZE - 1, buf);
	
	if(nbytes <= 0)
	{
		return thisPlayer;
	}
	
	splitted = strtok(buf, "/");
	strncpy(thisPlayer.nickname, splitted, 29);
	//thisPlayer -> nickname[29] = 0;
	
	splitted = strtok(NULL, "/");
	strncpy(thisPlayer.username,splitted, 29);
	//thisPlayer -> username[29] = 0;
	
	splitted = strtok(NULL, "/");
	strncpy(thisPlayer.password,splitted, 29);
	//thisPlayer -> password[29] = 0;
	
	splitted = strtok(NULL, "/");
	strncpy(thisPlayer.profile,splitted, 5);
	//thisPlayer -> profile[5] = 0;
	
	splitted = strtok(NULL, "/");
	strncpy(thisPlayer.ratingDelta,splitted, 4);
	//thisPlayer -> ratingDelta[4] = 0;
	strncpy(thisPlayer.rating,"0", 4);
	
	return thisPlayer;
}

//rec % send data
int Recv_Data(int fd, int size, char* buf)
{
	int nbytes = recv(fd, buf, size, 0);
	
	if (nbytes <= 0) {
		// Got error or connection closed by client
		// Connection closed
		printf("recv: tcp/ip socket %d hung up\n", fd);
		return nbytes;
	}
	buf[nbytes] = 0;
	printf("recv: %s from tcp/ip socket %d\n", buf, fd);
	return nbytes;
}

int Send_Data(int fd, char *buf)
{
	int nbytes = send(fd, buf, strlen(buf), 0);
	printf("send: %s to tcp/ip socket %d\n", buf, fd);
	return nbytes;
}

int Read_Pipe(int prfd, char *buf, int size)
{
	int nbytes = read(prfd, buf, size);
	buf[nbytes] = 0;
	printf("read: %s from pipe socket %d\n", buf, prfd);
	return nbytes;
}

int Write_Pipe(int pwfd, char *buf)
{
	int nbytes = write(pwfd, buf, strlen(buf));
	printf("write: %s to pipe socket %d\n", buf, pwfd);
	return nbytes;
}

void Print_Player_Player(Player player)
{
	printf("nickname: %s\n",player.nickname);
	printf("username: %s\n",player.username);
	printf("password: %s\n",player.password);
	printf("profile: %s\n",player.profile);
	printf("rating: %s\n",player.rating);
	printf("rating delta: %s\n",player.ratingDelta);
}

void Print_Player_Stats(Player player)
{
	printf("nickname: %s\n",player.nickname);
	printf("rating: %s\n",player.rating);
	printf("rating Delta: %s\n",player.ratingDelta);
}

void *get_in_addr(struct sockaddr *sa)
{
    if (sa->sa_family == AF_INET) {
        return &(((struct sockaddr_in*)sa)->sin_addr);
    }

    return &(((struct sockaddr_in6*)sa)->sin6_addr);
}

int get_listener_socket(char *port)
{
    int listener;     // Listening socket descriptor
    int yes=1;        // For setsockopt() SO_REUSEADDR, below
    int rv;

    struct addrinfo hints, *ai, *p;

    // Get us a socket and bind it
    memset(&hints, 0, sizeof hints);
    hints.ai_family = AF_UNSPEC;
    hints.ai_socktype = SOCK_STREAM;
    hints.ai_flags = AI_PASSIVE;
    if ((rv = getaddrinfo(NULL, port, &hints, &ai)) != 0) {
        fprintf(stderr, "selectserver: %s\n", gai_strerror(rv));
        exit(1);
    }
    
    for(p = ai; p != NULL; p = p->ai_next) {
        listener = socket(p->ai_family, p->ai_socktype, p->ai_protocol);
        if (listener < 0) { 
            continue;
        }
        
        // Lose the pesky "address already in use" error message
        setsockopt(listener, SOL_SOCKET, SO_REUSEADDR, &yes, sizeof(int));

        if (bind(listener, p->ai_addr, p->ai_addrlen) < 0) {
            close(listener);
            continue;
        }

        break;
    }

    freeaddrinfo(ai); // All done with this

    // If we got here, it means we didn't get bound
    if (p == NULL) {
        return -1;
    }

    // Listen
    if (listen(listener, 10) == -1) {
        return -1;
    }

    return listener;
}

void Thread_Cleanup(Lobby_Player *player)
{
	if(player == NULL)
	{
		return;
	}
	Remove_Player_From_Lobby(&(player -> opponent));
	
	//free(player -> opponent -> nickname);
	//free(player -> opponent -> username);
	//free(player -> opponent -> password);
	//free(player -> opponent -> profile);
	//free(player -> opponent -> rating);
	//free(player -> opponent -> ratingDelta);
	//free(player -> opponent);
	close(player -> sockfd);
	close(player -> pwfd);
	close(player -> prfd);
	//free(player);
}

void sigchld_handler(int signo)
{
    return;
}

