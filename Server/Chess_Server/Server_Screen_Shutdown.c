#include <stdint.h>
#include <string.h>
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <ss_oled.h>

SSOLED ssoled; // data structure for OLED object
unsigned char ucBackBuf[1024];

int main(int argc, char **argv)
{
	int ret = -1;
	int iOLEDType = OLED_128x64; // Change this for your specific display
	int bFlip = 1, bInvert = 0;
	ret = oledInit(&ssoled, iOLEDType, -1, bFlip, bInvert, 0, 3, 5, -1, 100000L);
	if (ret == OLED_NOT_FOUND)
	{
		printf("Unable to initialize I2C bus 0-2, please check your connections and verify the device address by typing 'i2cdetect -y <channel>\n");	
		return 0;
	}
	oledPower(&ssoled, 0);
	return 0;
}
