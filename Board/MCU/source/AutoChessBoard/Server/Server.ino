#include "BluetoothSerial.h"
#include <Arduino.h>
#include <esp_now.h>
#include <WiFi.h>
#include <Wire.h>

#define DEBUG

#define EN_PIN_A         5 // Enable
#define DIR_PIN_A          25 // Direction
#define STEP_PIN_A         4 // Step

#define EN_PIN_B           21 // Enable
#define DIR_PIN_B          18 // Direction
#define STEP_PIN_B         19 // Step

#define MS1_PIN            17
#define MS2_PIN            16

#define MAGNET_PIN         14

#define RGB_B_PIN          33
#define RGB_G_PIN          15
#define RGB_R_PIN          32

#define X_END_STOP_PIN      23
#define Y_END_STOP_PIN      22

#if !defined(CONFIG_BT_ENABLED) || !defined(CONFIG_BLUEDROID_ENABLED)
#error Bluetooth is not enabled! Please run `make menuconfig` to and enable it
#endif

typedef struct Move
{
  int sourceRowIndex;
  int sourceColIndex;
  int destRowIndex;
  int destColIndex;
} Move;

typedef struct Location
{
  int rowIndex;
  int colIndex;
} Location;

typedef struct struct_message {
    int groupA;
    int groupB;
} struct_message;

BluetoothSerial SerialBT;

String buf;

const int MICRO_STEPS = 16;

const float SQUARE_TO_MM = 56.5;
const float BUF_FIRST_LINE_Y = 48;
const float BUF_SECOND_LINE_Y = 32;
const float BUF_SECOND_LINE_X = 20;
const int STEPS_PER_MM = 5 * MICRO_STEPS;
const float MAX_X_POSITION_MM = 453;
const float MAX_Y_POSITION_MM = 571; 
const float BOARD_SIZE_Y = 505;
const float BOARD_SIZE_X = 507;

//SPEED
const int SPEED_STEPS_PER_SEC = 250 * MICRO_STEPS;

// ACC/DEC
const int SPEED_START = 50 * MICRO_STEPS;
const int SPEED_END = SPEED_STEPS_PER_SEC;
const int SPEED_STEP = SPEED_START;
const int SPEED_STEP_DELAY = 20 * MICRO_STEPS;

Location curHeadLocation;
bool connected = false;

bool board[8][8];
bool curBoard[8][8];

float boardSquareLocXRow[8][8] =
  {{-427.5882 ,-427 ,-428.9048 ,-427 ,-430.5 ,-429.5 ,-430.4091 ,-430.5714},
  {-377 ,-373.8421 ,-372 ,-372 ,-372 ,-372.4546 ,-376.2105 ,-375.75},
  {-315 ,-314.8261 ,-317 ,-312.5 ,-317 ,-317.4546 ,-318.8421 ,-317.5882},
  {-258.7647 ,-259.5 ,-258.1111 ,-259.5 ,-259.5 ,-257.3333 ,-260.9474 ,-260.9131},
  {-195.4615 ,-200.9474 ,-202.5882 ,-201.5455 ,-200.9474 ,-202 ,-202 ,-202.25},
  {-142 ,-143.6667 ,-143.1111 ,-142.7895 ,-142.8696 ,-143.1111 ,-143.1111 ,-148.7647},
  {-83.66666 ,-86.41177 ,-85 ,-88.11111 ,-85.1579 ,-85.88889 ,-87.45454 ,-90.1579},
  {-29.5 ,-28.25 ,-28.84211 ,-28.25 ,-27.58824 ,-27 ,-28.11111 ,-33.76471}};
  
float boardSquareLocYCol[8][8] =
  {{483.0588 ,423.5 ,369.8095 ,311 ,256.5 ,198.5 ,136.6818 ,83.5},
  {481.7692 ,424.9474 ,366 ,311 ,256 ,200.5455 ,140.2105 ,84.125},
  {483 ,425.1304 ,368.5 ,311.25 ,256 ,200.5455 ,142.0526 ,83.05882},
  {483.3529 ,427.4286 ,368.5 ,313.5 ,256 ,199 ,137.8421 ,85.56522},
  {486 ,426 ,368.9412 ,311.4546 ,256 ,201 ,142.6667 ,85.5},
  {483.5 ,427.3333 ,368.5 ,311.7895 ,256 ,198.5 ,143.5 ,86},
  {484.6667 ,428.0588 ,369 ,313.5 ,257.0526 ,198.5 ,141.4545 ,84.94736},
  {484.5714 ,427.25 ,372.0526 ,314.75 ,258.0588 ,201 ,143.5 ,83.64706}};


bool reportChanges = false;

unsigned long magnetOnTime;
int magnetTimeout = 60 * 1000;

unsigned long motorsOnTime;
int motorsTimeout = 40 * 60 * 1000;

// REPLACE WITH THE MAC Address of your receiver 
uint8_t client_A_Address[] = {0x08, 0x3A, 0xF2, 0x94, 0x42, 0x08};
uint8_t client_B_Address[] = {0xEC, 0x62, 0x60, 0x93, 0x6F, 0x48};
uint8_t client_C_Address[] = {0xE0, 0xE2, 0xE6, 0xCE, 0x0E, 0x94};
uint8_t client_D_Address[] = {0xEC, 0x62, 0x60, 0x93, 0x8D, 0xD0};
uint8_t *clients[] = {client_A_Address, client_B_Address, client_C_Address, client_D_Address};
// Variable to store if sending data was successful
String success;
// struct_message to hold incoming sensor readings
struct_message incomingReadingsClients[4];
//peers
esp_now_peer_info_t peerInfoA;
esp_now_peer_info_t peerInfoB;
esp_now_peer_info_t peerInfoC;
esp_now_peer_info_t peerInfoD;
//clients management
bool clientsReceive[] = {false, false, false, false};
unsigned long clientRequestTime = 0;
int clientRequestTimeout = 1 * 1000; //10 seconds timeout
int clientRequestCounter = 0;
int clientRequestMax = 10;

void setup() {
  
  InitHW();
  InitSerial();
  InitClients();

  
  HomeX();
  HomeY();
  HomeToSquareZero();
  SetRGBStandby();
  
  #if defined(DEBUG)
    Serial.printf("setup done\n");
  #endif
}

void loop() {
  if (SerialBT.available()) 
  {
    ReceiveClientCmd();
  }

  if(reportChanges == true)
  {
    UpdateCurBoard();
    CompareBoard();
    CopyCurBoardToBoard();
  }

  if(magnetOnTime != 0 && millis() > magnetOnTime + magnetTimeout)
  {
    EnableDisableMagnet(false);
  }

  if(motorsOnTime != 0 && millis() > motorsOnTime + motorsTimeout)
  {
    EnableDisableMotors(false);
  }
  //delay(20);
}

void InitHW()
{
  pinMode(RGB_B_PIN, OUTPUT);
  pinMode(RGB_G_PIN, OUTPUT);
  pinMode(RGB_R_PIN, OUTPUT);

  SetRGBInit_A();
  
  pinMode(EN_PIN_A, OUTPUT);
  pinMode(STEP_PIN_A, OUTPUT);
  pinMode(DIR_PIN_A, OUTPUT);

  pinMode(EN_PIN_B, OUTPUT);
  pinMode(STEP_PIN_B, OUTPUT);
  pinMode(DIR_PIN_B, OUTPUT);

  pinMode(MS1_PIN, OUTPUT);
  pinMode(MS2_PIN, OUTPUT);

  pinMode(X_END_STOP_PIN, INPUT_PULLUP);
  pinMode(Y_END_STOP_PIN, INPUT_PULLUP);
  
  pinMode(MAGNET_PIN, OUTPUT);

  digitalWrite(EN_PIN_A, HIGH);
  digitalWrite(EN_PIN_B, HIGH);
  digitalWrite(MAGNET_PIN, LOW);
  
  if(MICRO_STEPS == 8)
  {
    digitalWrite(MS1_PIN, LOW); 
    digitalWrite(MS2_PIN, LOW); 
  }
  else if(MICRO_STEPS == 16)
  {
    digitalWrite(MS1_PIN, HIGH); 
    digitalWrite(MS2_PIN, HIGH); 
  }
  else if(MICRO_STEPS == 32)
  {
    digitalWrite(MS1_PIN, HIGH); 
    digitalWrite(MS2_PIN, LOW); 
  }
  else if(MICRO_STEPS == 64)
  {
    digitalWrite(MS1_PIN, LOW); 
    digitalWrite(MS2_PIN, HIGH); 
  }
}

void InitSerial()
{
  SetRGBInit_B();
  Serial.begin(115200);
  SetRGBInit_C();
  SerialBT.begin("AutoChessBoard"); //Bluetooth device name
}

void InitClients()
{
  SetRGBInit_D();
  // Set device as a Wi-Fi Station
  WiFi.mode(WIFI_STA);

  // Init ESP-NOW
  if (esp_now_init() != ESP_OK) {
    Serial.println("Error initializing ESP-NOW");
    return;
  }
  
  // Register peerss
  memcpy(peerInfoA.peer_addr, client_A_Address, 6);
  peerInfoA.channel = 0;  
  peerInfoA.encrypt = false;

  memcpy(peerInfoB.peer_addr, client_B_Address, 6);
  peerInfoB.channel = 0;  
  peerInfoB.encrypt = false;

  memcpy(peerInfoC.peer_addr, client_C_Address, 6);
  peerInfoC.channel = 0;  
  peerInfoC.encrypt = false;

  memcpy(peerInfoD.peer_addr, client_D_Address, 6);
  peerInfoD.channel = 0;  
  peerInfoD.encrypt = false;
  
  // Add peer A    
  if (esp_now_add_peer(&peerInfoA) != ESP_OK){
    Serial.println("Failed to add peer");
    return;
  }

  // Add peer B   
  if (esp_now_add_peer(&peerInfoB) != ESP_OK){
    Serial.println("Failed to add peer");
    return;
  }
  
  // Add peer C    
  if (esp_now_add_peer(&peerInfoC) != ESP_OK){
    Serial.println("Failed to add peer");
    return;
  }

  // Add peer D    
  if (esp_now_add_peer(&peerInfoD) != ESP_OK){
    Serial.println("Failed to add peer");
    return;
  }

  // Register for a callback function that will be called when data is received
  esp_now_register_recv_cb(EspNowOnDataRecv);
}

void ReceiveClientCmd()
{
  Serial.printf("1\n");
  buf = SerialBT.readStringUntil('\n');
  buf.remove(buf.length()-1, 1);
  
  Serial.printf("Client cmd: |%s|\n", buf.c_str());
  
  if(buf == "report")
  {
    Serial.printf("report\n");
    Cmd_report();
  }
  else if(buf == "online")
  {
    SetRGBOnline();
    Serial.printf("online\n");
    Cmd_online();
  }
  else if(buf == "stoponline")
  {
    SetRGBStandby();
    Serial.printf("stoponline\n");
    Cmd_stoponline();
  }
  else if(buf == "startmove")
  {
    Serial.printf("startmove\n");
    Cmd_startmove();
  }
  else if(buf == "stopmove")
  {
    Serial.printf("stopmove\n");
    Cmd_stopmove();
  }
  else if(buf == "startgame")
  {
    Serial.printf("startgame\n");
    Cmd_startgame();
  }
  else if(buf == "endgame")
  {
    Serial.printf("stopgame\n");
    Cmd_stopgame();
  }
  else if(buf == "magneton")
  {
    Serial.printf("magneton\n");
    EnableDisableMagnet(true);
  }
  else if(buf == "magnetoff")
  {
    Serial.printf("magnetoff\n");
    EnableDisableMagnet(false);
  }
  else if(buf == "checkdrift")
  {
    Serial.printf("checkdrift\n");
    Cmd_checkdrift();
  }
  else
  {
    Serial.printf("move\n");
    Cmd_move(buf);
  }
}

void Cmd_report()
{
  UpdateCurBoard();
  for(int i = 0; i < 8; i++)
  {
    for(int j = 0; j < 8; j++)
    {
      if(curBoard[i][j] == 1)
      {
        SerialBT.printf("%c%d\r\n",j + 'a', i + 1);
        Serial.printf("%c%d\r\n",j + 'a', i + 1);
      }
    }
  }
  SerialBT.println("end");
  Serial.println("end");
}

void Cmd_online()
{
  UpdateCurBoard();
  CopyCurBoardToBoard();
  reportChanges = true;
}

void Cmd_stoponline()
{
  reportChanges = false;
}

void Cmd_move(String cmdMove)
{
  Move commandedMove;
  bool bufferMove = false;
  
  if(cmdMove.charAt(0) >= 'a' && cmdMove.charAt(0) <= 'h')
  {
    commandedMove.sourceColIndex = cmdMove.charAt(0) - 'a';
    Serial.printf("commandedMove.sourceColIndex: %d\n", commandedMove.sourceColIndex);
  }
  else
    return;
    
  if(cmdMove.charAt(1) >= '1' && cmdMove.charAt(1) <= '8')
  {
      commandedMove.sourceRowIndex = cmdMove.charAt(1) - '1';
      Serial.printf("commandedMove.sourceRowIndex: %d\n", commandedMove.sourceRowIndex);
  }
  else
    return;
    
  if(cmdMove.charAt(3) >= 'a' && cmdMove.charAt(3) <= 'h')
  {
    commandedMove.destColIndex = cmdMove.charAt(3) - 'a';
    Serial.printf("commandedMove.destColIndex: %d\n", commandedMove.destColIndex);
  }
  else if(cmdMove.charAt(3) == 'u') //user buffer
  {
    bufferMove = true;
    commandedMove.destRowIndex = commandedMove.sourceRowIndex;
    if(cmdMove.charAt(4) == '1' || cmdMove.charAt(4) == '2')
      commandedMove.destColIndex = 8 + (cmdMove.charAt(4) - '1');
    else
      return;
  }
  else if(cmdMove.charAt(3) == 'o') //opponent buffer
  {
    bufferMove = true;
    commandedMove.destRowIndex = commandedMove.sourceRowIndex;
    if(cmdMove.charAt(4) == '1' || cmdMove.charAt(4) == '2')
      commandedMove.destColIndex = -1 - (cmdMove.charAt(4) - '1');
    else
      return;
  }
  else
    return;

  if(bufferMove == false)
  {
    if(cmdMove.charAt(4) >= '1' && cmdMove.charAt(4) <= '8')
    {
        commandedMove.destRowIndex = cmdMove.charAt(4) - '1';
        Serial.printf("commandedMove.destRowIndex: %d\n", commandedMove.destRowIndex);
    }
    else
      return;
  }

  int success = 0;
  while(success != 1)
  {
    success = 0;
  	PerformCommandedMove(commandedMove);
    if(commandedMove.destColIndex >= 0 && commandedMove.destColIndex <= 7) // not a buffer move
    {
      delay(50);
    	UpdateCurBoard();
    	if(curBoard[commandedMove.destRowIndex][commandedMove.destColIndex] == 1)
    	{
    	  Serial.println("Move success");
    	  success = 1;
    	}
    	else
    	 Serial.println("Move fail");
    }
    else
      success = 1;
  }
  
  SerialBT.println("Ack__");
  Serial.println("Ack__");
}

void Cmd_startmove()
{
  SetRGBMove();
}

void Cmd_stopmove()
{
  SetRGBStandby();
}

void Cmd_startgame()
{
  EnableDisableMotors(1);
}

void Cmd_stopgame()
{
  EnableDisableMotors(0);
}

void Cmd_checkdrift()
{
  int steps = 0;
  steps = MoveXSteps(MAX_X_POSITION_MM * STEPS_PER_MM);
  Serial.printf("rowIndex: %d colIndex: %d\n", curHeadLocation.rowIndex, curHeadLocation.colIndex);
  Serial.printf("boardSquareLocXRow: %f steps: %d x: %f\n", boardSquareLocXRow[curHeadLocation.rowIndex][curHeadLocation.colIndex], steps, ((float)steps / (float)STEPS_PER_MM));
  SerialBT.printf("%f\n", ((float)steps / (float)STEPS_PER_MM) + boardSquareLocXRow[curHeadLocation.rowIndex][curHeadLocation.colIndex]);
  steps = 0;
  steps = MoveYSteps(-1 * MAX_Y_POSITION_MM * STEPS_PER_MM);
  Serial.printf("rowIndex: %d colIndex: %d\n", curHeadLocation.rowIndex, curHeadLocation.colIndex);
  Serial.printf("boardSquareLocYCol: %f steps: %d y: %f\n", boardSquareLocYCol[curHeadLocation.rowIndex][curHeadLocation.colIndex], steps, ((float)steps / (float)STEPS_PER_MM));
  SerialBT.printf("%f\n", ((float)steps / (float)STEPS_PER_MM) - boardSquareLocYCol[curHeadLocation.rowIndex][curHeadLocation.colIndex]);
  HomeToSquareZero();
}

void UpdateCurBoard()
{
  bool fail = false;
  int clientRequest = 1;
  uint8_t currentData;
  do
  {
    fail = false;
    for(int i = 0; i < 4; i++)
    {
      // Send message via ESP-NOW
      clientsReceive[i] = false;
      esp_err_t result = esp_now_send(clients[i], (uint8_t *) &clientRequest, sizeof(clientRequest));
       
      if (result == ESP_OK) {
        Serial.printf("Sent with success client %d\n",clients[i][5]);
      }
      else {
        Serial.printf("Error sending the data client %d\n",clients[i][5]);
      }
    }
    clientRequestTime = millis();
    clientRequestCounter = 0;
    while(clientsReceive[0] == false || clientsReceive[1] == false || clientsReceive[2] == false || clientsReceive[3] == false)
    {
      if(millis() > clientRequestTime + clientRequestTimeout)
      {
        fail = true;
        clientRequestCounter++;
        Serial.printf("Clients time out %d,%d %d,%d %d,%d %d,%d\n",clients[0][5], clientsReceive[0],
          clients[1][5], clientsReceive[1],
          clients[2][5], clientsReceive[2],
          clients[3][5], clientsReceive[3]);
        if(clientRequestCounter >= clientRequestMax)
        {
          SerialBT.printf("Error Clients time out %d,%d %d,%d %d,%d %d,%d\n",clients[0][5], clientsReceive[0],
            clients[1][5], clientsReceive[1],
            clients[2][5], clientsReceive[2],
            clients[3][5], clientsReceive[3]);
          clientRequestCounter = 0;
        }
        break;
      }
    }
  } while(fail == true);
  
  for(int i = 0; i < 4; i++)
  {
    currentData = incomingReadingsClients[i].groupA;
    for(int j = 0; j < 8; j++)
    {
      curBoard[i * 2][j] = !(currentData % 2);
      currentData /= 2;
    }
    currentData = incomingReadingsClients[i].groupB;
    for(int j = 0; j < 8; j++)
    {
      curBoard[i * 2 + 1][j] = !(currentData % 2);
      currentData /= 2;
    }
  }
  #if defined(DEBUG)
    PrintCurBoardScreen();
  #endif
}

// Callback when data is received
void EspNowOnDataRecv(const uint8_t * mac, const uint8_t *incomingData, int len) {
  Serial.print("Bytes received: ");
  Serial.println(len);
  for(int i = 0; i < 4; i++)
  {
    if(mac[5] == clients[i][5])
    {
      memcpy(&(incomingReadingsClients[i]), incomingData, sizeof(incomingReadingsClients[i]));
      Serial.printf("Client: %d\n", mac[5]);
      Serial.printf("GroupA: %d\n", incomingReadingsClients[i].groupA);
      Serial.printf("GroupB: %d\n", incomingReadingsClients[i].groupB);
      clientsReceive[i] = true;
      return;
    }
  }
}

void CompareBoard()
{
  for(int i = 0; i < 8; i++)
  {
    for(int j = 0; j < 8; j++)
    {
      if(curBoard[i][j] != board[i][j])
      {
        if(curBoard[i][j] == 1)
          SerialBT.printf("apr%c%d\r\n",j + 'a', i + 1);
        else
          SerialBT.printf("dis%c%d\r\n",j + 'a', i + 1);
      }
    }
  }
}

void CopyCurBoardToBoard()
{
  for(int i = 0; i < 8; i++)
  {
    for(int j = 0; j < 8; j++)
    {
      board[i][j] = curBoard[i][j];
    }
  }
}

void PrintCurBoardScreen()
{
  //tft.fillScreen(ST77XX_BLACK);
  //tft.setCursor(0, 30);
  Serial.printf("    a  b  c  d  e  f  g  h\n");
  for(int i = 0; i < 8; i++)
  {
    Serial.printf("%d||", i + 1);
    for(int j = 0; j < 8; j++)
      Serial.printf(" %d|",curBoard[i][j]);
    Serial.printf("\n");
  }
}

void SetRGBInit_A()
{
  SetRGB(HIGH, LOW, LOW);
}

void SetRGBInit_B()
{
  SetRGB(LOW, HIGH, HIGH);
}

void SetRGBInit_C()
{
  SetRGB(HIGH, HIGH, LOW);
}

void SetRGBInit_D()
{
  SetRGB(HIGH, HIGH, HIGH);
}

void SetRGBStandby()
{
  SetRGB(HIGH, LOW, HIGH);
}

void SetRGBOnline()
{
  SetRGB(LOW, HIGH, LOW);
}

void SetRGBMove()
{
  SetRGB(LOW, LOW, HIGH);
}

void SetRGB(int BVal, int GVal, int RVal)
{
  digitalWrite(RGB_B_PIN, BVal);
  digitalWrite(RGB_G_PIN, GVal);
  digitalWrite(RGB_R_PIN, RVal);
}

void PerformCommandedMove(Move commandedMove)
{
  if(commandedMove.sourceRowIndex != curHeadLocation.rowIndex)
  {
    EnableDisableMagnet(false);
    Serial.printf("Moving to source row \n");
    MoveXSquares(commandedMove.sourceRowIndex, curHeadLocation.rowIndex, curHeadLocation.colIndex);
    curHeadLocation.rowIndex = commandedMove.sourceRowIndex;
  }
  
  if(commandedMove.sourceColIndex != curHeadLocation.colIndex)
  {
    EnableDisableMagnet(false);
    Serial.printf("Moving to source col \n");
    MoveYSquares(commandedMove.sourceColIndex, curHeadLocation.colIndex, curHeadLocation.rowIndex);
    curHeadLocation.colIndex = commandedMove.sourceColIndex;
  }
  
  if(commandedMove.destRowIndex != curHeadLocation.rowIndex)
  {
    EnableDisableMagnet(true);
    Serial.printf("Moving to destination row\n");
    MoveXSquares(commandedMove.destRowIndex, curHeadLocation.rowIndex, curHeadLocation.colIndex);
	  EnableDisableMagnet(false);
    curHeadLocation.rowIndex = commandedMove.destRowIndex;
  }
  
  if(commandedMove.destColIndex != curHeadLocation.colIndex)
  {
    EnableDisableMagnet(true);
    Serial.printf("Moving to destination col\n");
    if(commandedMove.destColIndex >= 0 && commandedMove.destColIndex <= 7)
    {
      MoveYSquares(commandedMove.destColIndex, curHeadLocation.colIndex, curHeadLocation.rowIndex);
	    EnableDisableMagnet(false);
      curHeadLocation.colIndex = commandedMove.destColIndex;
    }
    else //buf move
    {
      if(commandedMove.destColIndex == -1)
      {
		    EnableDisableMagnet(true);
        MoveYmm(BUF_FIRST_LINE_Y);
        EnableDisableMagnet(false);
        MoveYmm(0 - BUF_FIRST_LINE_Y);
      }
      else if(commandedMove.destColIndex == -2)
      {
		    EnableDisableMagnet(true);
        MoveYmm(BUF_FIRST_LINE_Y);
        MoveXmm(BUF_SECOND_LINE_X);
        MoveYmm(BUF_SECOND_LINE_Y);
        EnableDisableMagnet(false);
        MoveYmm(0 - BUF_FIRST_LINE_Y - BUF_SECOND_LINE_Y);
        MoveXmm(0 - BUF_SECOND_LINE_X);
      }
      if(commandedMove.destColIndex == 8)
      {
        EnableDisableMagnet(true);
		    MoveYmm(0 - BUF_FIRST_LINE_Y);
        EnableDisableMagnet(false);
        MoveYmm(BUF_FIRST_LINE_Y);
      }
      else if(commandedMove.destColIndex == 9)
      {
        EnableDisableMagnet(true);
		    MoveYmm(0 - BUF_FIRST_LINE_Y);
        MoveXmm(BUF_SECOND_LINE_X);
        MoveYmm(0 - BUF_SECOND_LINE_Y);
        EnableDisableMagnet(false);
        MoveYmm(BUF_FIRST_LINE_Y + BUF_SECOND_LINE_Y);
        MoveXmm(0 - BUF_SECOND_LINE_X);
      }
    }
  }
}

void EnableDisableMotors(bool enableDisable)
{
  if(enableDisable)
  {
    Serial.printf("Enable Motors\n");
    digitalWrite(EN_PIN_A, LOW);
    digitalWrite(EN_PIN_B, LOW);
    motorsOnTime = millis();
  }
  else
  {
    Serial.printf("Disable Motors\n");
    digitalWrite(EN_PIN_A, HIGH);
    digitalWrite(EN_PIN_B, HIGH);
    motorsOnTime = 0;
  }
}

void EnableDisableMagnet(bool enableDisable)
{
  if(enableDisable)
  {
    Serial.printf("Enable Magnet\n");
    digitalWrite(MAGNET_PIN, HIGH);
    magnetOnTime = millis();
  }
  else
  {
    Serial.printf("Disable Magnet\n");
    digitalWrite(MAGNET_PIN, LOW);
    magnetOnTime = 0;
  }    
}

void HomeX()
{
  Serial.printf("HomeX\n");
  MoveXmm(1000);
  Serial.printf("HomeX Complete\n");
}

void HomeY()
{
  Serial.printf("HomeY\n");
  MoveYmm(-1200);
  Serial.printf("HomeY Complete\n");
}

void HomeToSquareZero()
{
  MoveXmm(boardSquareLocXRow[7][7]);
  MoveYmm(boardSquareLocYCol[7][7]);
  curHeadLocation.rowIndex = 7;
  curHeadLocation.colIndex = 7;
}

void MoveYSquares(int dstSquareCol, int srcSquareCol, int sqaureRow)
{
  Serial.printf("MoveYSquares dstSquareCol:%d srcSquareCol:%d sqaureRow:%d\n", dstSquareCol, srcSquareCol, sqaureRow);
  float yDel = boardSquareLocYCol[sqaureRow][dstSquareCol] - boardSquareLocYCol[sqaureRow][srcSquareCol];
  float xDel = boardSquareLocXRow[sqaureRow][dstSquareCol] - boardSquareLocXRow[sqaureRow][srcSquareCol];
  MoveYmm(yDel);
  MoveXmm(xDel);
}

void MoveXSquares(int dstSquareRow, int srcSquareRow, int sqaureCol)
{
  Serial.printf("MoveXSquares dstSquareRow:%d srcSquareRow:%d sqaureCol:%d\n", dstSquareRow, srcSquareRow, sqaureCol);
  float yDel = boardSquareLocYCol[dstSquareRow][sqaureCol] - boardSquareLocYCol[srcSquareRow][sqaureCol];
  float xDel = boardSquareLocXRow[dstSquareRow][sqaureCol] - boardSquareLocXRow[srcSquareRow][sqaureCol];
  MoveYmm(yDel);
  MoveXmm(xDel);
}

void MoveYmm(float mm)
{
  Serial.printf("MoveYmm %f\n", mm);
  MoveYSteps((int)(mm * STEPS_PER_MM));
}

void MoveXmm(float mm)
{
  Serial.printf("MoveXmm %f\n", mm);
  MoveXSteps((int)(mm * STEPS_PER_MM));
}

int MoveYSteps(int steps)
{
  Serial.printf("MoveYSteps %d\n", steps);
  if(steps > 0)
  {
    digitalWrite(DIR_PIN_A, LOW);
    digitalWrite(DIR_PIN_B, HIGH);
    return MoveBothMotors(steps, 3);
  }
  else
  {
    digitalWrite(DIR_PIN_A, HIGH);
    digitalWrite(DIR_PIN_B, LOW);
    return MoveBothMotors(steps * -1, 1);
  }
}

int MoveXSteps(int steps)
{
  Serial.printf("MoveXSteps %d\n", steps);
  if(steps > 0)
  {
    digitalWrite(DIR_PIN_A, HIGH);
    digitalWrite(DIR_PIN_B, HIGH);
    return MoveBothMotors(steps, 4);
  }
  else
  {
    digitalWrite(DIR_PIN_A, LOW);
    digitalWrite(DIR_PIN_B, LOW);
    return MoveBothMotors(steps * -1, 2);
  }
}

int MoveBothMotors(int steps, int dir) //dir 1 == -y, 2 == -x, 3 == +y, 4 == +x 
{
  Serial.printf("MoveBothMotors steps=%d dir=%d\n", steps, dir);
  EnableDisableMotors(1);
  int DECC_START;
  int CUR_SPEED = SPEED_START;
  int DELAY_MICRO_SEC = (1000000 / (CUR_SPEED * 2));
  if(steps > ((SPEED_STEP_DELAY * ((SPEED_END - SPEED_START) / SPEED_STEP)) * 2))
    DECC_START = steps - (SPEED_STEP_DELAY * ((SPEED_END - SPEED_START) / SPEED_STEP));
  else
    DECC_START = steps / 2;
  for(int i = 0; i < steps; i++)
  {
    //Serial.printf("i = %ld \n", i);
    //Serial.printf("Delay = %ld \n", DELAY_MICRO_SEC);
    //Serial.printf("ACC_STEP = %ld \n",ACC_STEP);
    //Serial.printf("i modulo ACC_STEP = %ld \n", (i % ACC_STEP));
    if(i != 0 && i < DECC_START && (i % SPEED_STEP_DELAY == 0) && (CUR_SPEED < SPEED_END))
    {
      CUR_SPEED += SPEED_STEP;
      if(CUR_SPEED > SPEED_END)
        CUR_SPEED = SPEED_END;
      DELAY_MICRO_SEC = (1000000 / (CUR_SPEED * 2));
      //Serial.printf("CUR_SPEED = %ld \n", CUR_SPEED);
      //Serial.printf("Delay = %ld \n", DELAY_MICRO_SEC);
    }
    if(i % SPEED_STEP_DELAY == 0 && i >= DECC_START && CUR_SPEED > SPEED_START)
    {
      CUR_SPEED -= SPEED_STEP;
      if(CUR_SPEED < SPEED_START)
        CUR_SPEED = SPEED_START;
      DELAY_MICRO_SEC = (1000000 / (CUR_SPEED * 2));
      //Serial.printf("CUR_SPEED = %ld \n", CUR_SPEED);
      //Serial.printf("Delay = %ld \n", DELAY_MICRO_SEC);
    }
    digitalWrite(STEP_PIN_A, HIGH);
    digitalWrite(STEP_PIN_B, HIGH);
    delayMicroseconds(DELAY_MICRO_SEC);
    digitalWrite(STEP_PIN_A, LOW);
    digitalWrite(STEP_PIN_B, LOW);
    delayMicroseconds(DELAY_MICRO_SEC);
    if((dir == 4 && digitalRead(X_END_STOP_PIN) == 0) || (dir == 1 && digitalRead(Y_END_STOP_PIN) == 0))
    {
      Serial.printf("MoveBothMotors Endstop\n");
      return i;
    }
  }
  Serial.printf("MoveBothMotors complete\n");
  return steps;
}
