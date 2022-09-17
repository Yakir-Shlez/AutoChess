#include <stdint.h>
#include <string.h>
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <ss_oled.h>
#include <signal.h>
#include <mariadb/mysql.h>

#include <arpa/inet.h>
#include <sys/socket.h>
#include <netdb.h>
#include <ifaddrs.h>
#include <linux/if_link.h>

#include <dirent.h>
#include <sys/types.h>

SSOLED ssoled; // data structure for OLED object
unsigned char ucBackBuf[1024];

int DB_Query_Online_Players_Games (int players);

void Get_ip (char *buf);

pid_t proc_find(const char* name);

int main(int argc, char **argv)
{
	char ip[24];
	char buf[24];
	char ip_buf[24];
	char *new_line = "                      ";
	int ret = -1;
	int iOLEDType = OLED_128x64; // Change this for your specific display
	int bFlip = 0, bInvert = 0;
	pid_t server_Online_buf;
	pid_t server_Online;
	int online_Players = -2;
	int online_Players_buf = -1;
	int cur_Games = -2;
	int cur_Games_buf = -1;
	
	ret = oledInit(&ssoled, iOLEDType, -1, bFlip, bInvert, 0, 3, 5, -1, 100000L);
	if (ret == OLED_NOT_FOUND)
	{
		printf("Unable to initialize I2C bus 0-2, please check your connections and verify the device address by typing 'i2cdetect -y <channel>\n");	
		return 0;
	}
	oledSetBackBuffer(&ssoled, ucBackBuf);
	oledFill(&ssoled, 0,1); // fill with black
	oledWriteString(&ssoled, 0,0,1,"port:3333",FONT_SMALL,0,1);
	while(1)
	{
		//online_Players = DB_Query_Online_Players ();
		//printf("online_Players %d\n", online_Players);
		
		Get_ip(ip_buf);
		//printf("%s\n", ip_buf);
		snprintf(buf, 23, "ip:%s", ip_buf);
		buf[23] = 0;
		//printf("%s\n", buf);
		if (strcmp(buf, ip) != 0)
		{
			strcpy(ip, buf);
			//printf("%s\n", ip);
			oledWriteString(&ssoled, 0,0,0,new_line,FONT_SMALL,0,1);
			oledWriteString(&ssoled, 0,0,0,ip,FONT_SMALL,0,1);
		}
		//oledWriteString(&ssoled, 0,0,0,"                      ",FONT_SMALL,0,1);
		//oledWriteString(&ssoled, 0,0,0,buf,FONT_SMALL,0,1);
		
		//TBD
		server_Online_buf = proc_find("./Chess_Main_Server");
		if (server_Online_buf != server_Online)
		{
			server_Online = server_Online_buf;
			
			oledWriteString(&ssoled, 0,0,2,new_line,FONT_SMALL,0,1);
			sprintf(buf, "server:%s", server_Online == -1 ? "Offline" : "Online");
			oledWriteString(&ssoled, 0,0,2,buf,FONT_SMALL,0,1);
		}
		
		online_Players_buf = DB_Query_Online_Players_Games(1);
		if (online_Players_buf != online_Players)
		{
			online_Players = online_Players_buf;
			
			oledWriteString(&ssoled, 0,0,3,new_line,FONT_SMALL,0,1);
			sprintf(buf, "DB:%s", online_Players == -1 ? "Offline" : "Online");
			oledWriteString(&ssoled, 0,0,3,buf,FONT_SMALL,0,1);
			
			oledWriteString(&ssoled, 0,0,4,new_line,FONT_SMALL,0,1);
			sprintf(buf, "Online Users:%d", online_Players);
			oledWriteString(&ssoled, 0,0,4,buf,FONT_SMALL,0,1);
		}

		cur_Games_buf = DB_Query_Online_Players_Games(0);
		if (cur_Games_buf != cur_Games)
		{
			cur_Games = cur_Games_buf;
			
			oledWriteString(&ssoled, 0,0,5,new_line,FONT_SMALL,0,1);
			sprintf(buf, "Online Games:%d", cur_Games);
			oledWriteString(&ssoled, 0,0,5,buf,FONT_SMALL,0,1);
		}
		
		//sprintf(buf, "Current games:%d", online_Players);
		//oledWriteString(&ssoled, 0,0,5,buf,FONT_SMALL,0,1);
		//printf("Press ENTER to quit\n");
		//getchar();
		//
		sleep(2);
	}
	oledPower(&ssoled, 0);
	return 0;
}

int DB_Query_Online_Players_Games (int players)
{
	MYSQL_RES *results = NULL;
	int onlien_Players = -1;
	int ret;
	
	MYSQL *con = mysql_init(NULL);
	
	if(con == NULL)
	{
		perror(mysql_error(con));
		return -1;
	}
	
	if(mysql_real_connect(con, "localhost", "user", "123456",
		"Chess_DB", 0, NULL, 0) == NULL)
	{
		perror(mysql_error(con));
		mysql_close(con);
		return -1;
	}
	
	//if(players == 1)
	//	ret = mysql_query(con, "SELECT COUNT(1) FROM Online_Users");
	//else
	//	ret = mysql_query(con, "SELECT COUNT(1) FROM Ongoing_Games");
		
	if((players == 0 && mysql_query(con, "SELECT COUNT(1) FROM Ongoing_Games")) ||
		(players == 1 && mysql_query(con, "SELECT COUNT(1) FROM Online_Users")))
	{
		perror(mysql_error(con));
		mysql_close(con);
		return -1;
	}

	results = mysql_store_result(con);

	if(results == NULL)
	{
		perror(mysql_error(con));
		mysql_close(con);
		return -1;
	}
	
	MYSQL_ROW row = mysql_fetch_row(results);
	
	if(row == NULL)
	{
		perror(mysql_error(con));
		mysql_close(con);
		return -1;
	}
	
	onlien_Players = atoi(row[0]);

	mysql_close(con);
	mysql_free_result(results);
	return onlien_Players;
}

void Get_ip (char *buf)
{
	struct ifaddrs *addrs;
	struct ifaddrs *tmp;
    //int family, s;
	getifaddrs(&addrs);
	tmp = addrs;

	while (tmp) 
	{
		if (tmp->ifa_addr && tmp->ifa_addr->sa_family == AF_INET && strcmp(tmp->ifa_name, "wlan0") == 0)
		{
			struct sockaddr_in *pAddr = (struct sockaddr_in *)tmp->ifa_addr;
			strncpy(buf, inet_ntoa(pAddr->sin_addr), 23);
			buf[23] = 0;
			//printf("%s\n", inet_ntoa(pAddr->sin_addr));
			//printf("%s\n", buf);
		}

		tmp = tmp->ifa_next;
	}

	freeifaddrs(addrs);
	
	return;
}

pid_t proc_find(const char* name) 
{
    DIR* dir;
    struct dirent* ent;
    char* endptr;
    char buf[512];
    int len;

    if (!(dir = opendir("/proc"))) {
        perror("can't open /proc");
        return -1;
    }

    while((ent = readdir(dir)) != NULL) {
        /* if endptr is not a null character, the directory is not
         * entirely numeric, so ignore it */
        long lpid = strtol(ent->d_name, &endptr, 10);
        if (*endptr != '\0') {
            continue;
        }

        /* try to open the cmdline file */
        snprintf(buf, sizeof(buf), "/proc/%ld/cmdline", lpid);
        FILE* fp = fopen(buf, "r");

        if (fp) {
            if (fgets(buf, sizeof(buf), fp) != NULL) {
                /* check the first token in the file, the program name */
                char* first = strtok(buf, " ");
                len = strlen(first);
                if(len >= 3)
					first[len-2] = '\0';
                //printf(first);
                //printf("\n");
                if (!strcmp(first, name)) {
                    fclose(fp);
                    closedir(dir);
                    return (pid_t)lpid;
                }
            }
            fclose(fp);
        }

    }

    closedir(dir);
    return -1;
}
