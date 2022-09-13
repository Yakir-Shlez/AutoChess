#include "BluetoothSerial.h"
#include <Arduino.h>
#include <Adafruit_GFX.h>    // Core graphics library
#include <Adafruit_I2CDevice.h>
#include <Adafruit_ST7789.h> // Hardware-specific library for ST7789
#include <SPI.h>             // Arduino SPI library

#include <Adafruit_MCP23X17.h>
#include <Wire.h>

#define DEBUG

#define I2C_SDA 21  //23
#define I2C_SCL 22

#define EN_PIN_A         13 // Enable
#define DIR_PIN_A          14 // Direction
#define STEP_PIN_A         15 // Step

#define EN_PIN_B           12 // Enable
#define DIR_PIN_B          17 // Direction TBD
#define STEP_PIN_B         25 // Step

#define MS1_PIN            26
#define MS2_PIN            27

#define MAGNET_PIN         32

#define RGB_B_PIN          16
#define RGB_G_PIN          33
#define RGB_R_PIN          0

#define X_END_STOP_PIN      35 //TBD  //34 33
#define Y_END_STOP_PIN      34 //TBD  //39

#define TFT_MOSI 23  // SDA Pin on ESP32  //18
#define TFT_SCLK 18  // SCL Pin on ESP32  //5
#define TFT_CS   5  // Chip select control pin  //33
#define TFT_DC    2  // Data Command control pin  //16
#define TFT_RST   4  // Reset pin (could connect to RST pin)

//19 33 16

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

BluetoothSerial SerialBT;

String buf;

const int MICRO_STEPS = 16;

const float SQUARE_TO_MM = 56.5;
const float BUF_FIRST_LINE_Y = 48;
const float BUF_SECOND_LINE_Y = 38;
const float BUF_SECOND_LINE_X = 30;
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

// Initialize Adafruit ST7789 TFT library
Adafruit_ST7789 tft = Adafruit_ST7789(TFT_CS, TFT_DC, TFT_RST);
Adafruit_MCP23X17 mcpArr[4];

bool reportChanges = false;

void setup() {
  Serial.begin(115200);
  SerialBT.begin("AutoChessBoard"); //Bluetooth device name

  InitHW();
  InitMcp();
  InitScreen();
  HomeX();
  HomeY();
  HomeToSquareZero();
  SetRGBStandby();
  
  #if defined(DEBUG)
    tft.printf("setup done\n");
  #endif
}

void loop() {
  
  //if (!SerialBT.available()) 
  //{
  //  delay(20);
  //}
  
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
  //delay(20);
}

void InitMcp()
{
  for(int i = 0; i < 4; i++)
  {
    if (!mcpArr[i].begin_I2C(0x20 + i)) {
        #if defined(DEBUG)
          tft.printf("MCP 0x%02x Init fail", 0x20 + i);
        #endif
        while(true);
    }
    Serial.printf("MCP %d successful.\n", 0x20 + i);
    
    mcpArr[i].pinMode(0, INPUT_PULLUP);
    mcpArr[i].pinMode(1, INPUT_PULLUP);
    mcpArr[i].pinMode(2, INPUT_PULLUP);
    mcpArr[i].pinMode(3, INPUT_PULLUP);
    mcpArr[i].pinMode(4, INPUT_PULLUP);
    mcpArr[i].pinMode(5, INPUT_PULLUP);
    mcpArr[i].pinMode(6, INPUT_PULLUP);
    mcpArr[i].pinMode(7, INPUT_PULLUP);
    mcpArr[i].pinMode(8, INPUT_PULLUP);
    mcpArr[i].pinMode(9, INPUT_PULLUP);
    mcpArr[i].pinMode(10, INPUT_PULLUP);
    mcpArr[i].pinMode(11, INPUT_PULLUP);
    mcpArr[i].pinMode(12, INPUT_PULLUP);
    mcpArr[i].pinMode(13, INPUT_PULLUP);
    mcpArr[i].pinMode(14, INPUT_PULLUP);
    mcpArr[i].pinMode(15, INPUT_PULLUP);
  }
}

void InitScreen()
{
  tft.init(240, 240, SPI_MODE2);
  tft.setRotation(1);
  tft.fillScreen(ST77XX_BLACK);
  tft.setTextWrap(false);
  tft.setTextColor(ST77XX_WHITE);
  tft.setTextSize(1);
}

void InitHW()
{
  pinMode(EN_PIN_A, OUTPUT);
  pinMode(STEP_PIN_A, OUTPUT);
  pinMode(DIR_PIN_A, OUTPUT);

  pinMode(EN_PIN_B, OUTPUT);
  pinMode(STEP_PIN_B, OUTPUT);
  pinMode(DIR_PIN_B, OUTPUT);

  pinMode(MS1_PIN, OUTPUT);
  pinMode(MS2_PIN, OUTPUT);

  pinMode(X_END_STOP_PIN, INPUT);
  pinMode(Y_END_STOP_PIN, INPUT);
  
  pinMode(MAGNET_PIN, OUTPUT);

  pinMode(RGB_B_PIN, OUTPUT);
  pinMode(RGB_G_PIN, OUTPUT);
  pinMode(RGB_R_PIN, OUTPUT);

  SetRGBInit();
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

void ReceiveClientCmd()
{
  tft.printf("1\n");
  buf = SerialBT.readStringUntil('\n');
  buf.remove(buf.length()-1, 1);
  
  tft.printf("Client cmd: |%s|\n", buf.c_str()); //TBD
  
  if(buf == "report")
  {
    tft.printf("report\n");
    Cmd_report();
  }
  else if(buf == "online")
  {
    SetRGBOnline();
    tft.printf("online\n");
    Cmd_online();
  }
  else if(buf == "stoponline")
  {
    SetRGBStandby();
    tft.printf("stoponline\n");
    Cmd_stoponline();
  }
  else if(buf == "startmove")
  {
    Cmd_startmove();
  }
  else if(buf == "stopmove")
  {
    Cmd_stopmove();
  }
  else
  {
    tft.printf("move\n");
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
        tft.printf("%c%d\r\n",j + 'a', i + 1);
      }
    }
  }
  SerialBT.println("end");
  tft.println("end");
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

  PerformCommandedMove(commandedMove);

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

void UpdateCurBoard()
{
  uint8_t currentData;
  for(int i = 0; i < 4; i++)
  {
    currentData = mcpArr[i].readGPIOA();
    //TBD - check if this algorithem is too slow
    for(int j = 0; j < 8; j++)
    {
      curBoard[i * 2][j] = !(currentData % 2);
      currentData /= 2;
    }
    currentData = mcpArr[i].readGPIOB();
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
  tft.fillScreen(ST77XX_BLACK);
  tft.setCursor(0, 30);
  tft.printf("    a  b  c  d  e  f  g  h\n");
  for(int i = 0; i < 8; i++)
  {
    tft.printf("%d||", i + 1);
    for(int j = 0; j < 8; j++)
      tft.printf(" %d|",curBoard[i][j]);
    tft.printf("\n");
  }
}

void SetRGBInit()
{
  SetRGB(HIGH, LOW, LOW);
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
    Serial.printf("Moving to source row \n");
    MoveXSquares(commandedMove.sourceRowIndex - curHeadLocation.rowIndex);
  }
  if(commandedMove.sourceColIndex != curHeadLocation.colIndex)
  {
    Serial.printf("Moving to source col \n");
    MoveYSquares(commandedMove.sourceColIndex - curHeadLocation.colIndex);
  }

  curHeadLocation.rowIndex = commandedMove.sourceRowIndex;
  curHeadLocation.colIndex = commandedMove.sourceColIndex;
  
  if(commandedMove.destRowIndex != curHeadLocation.rowIndex)
  {
    EnableDisableMagnet(true);
    Serial.printf("Moving to destination row\n");
    MoveXSquares(commandedMove.destRowIndex - curHeadLocation.rowIndex);
    EnableDisableMagnet(false);
    curHeadLocation.rowIndex = commandedMove.destRowIndex;
  }
  
  if(commandedMove.destColIndex != curHeadLocation.colIndex)
  {
    EnableDisableMagnet(true);
    Serial.printf("Moving to destination col\n");
    if(commandedMove.destColIndex >= 0 && commandedMove.destColIndex <= 7)
    {
      MoveYSquares(commandedMove.destColIndex - curHeadLocation.colIndex);
      curHeadLocation.colIndex = commandedMove.destColIndex;
      EnableDisableMagnet(false);
    }
    else //buf move
    {
      if(commandedMove.destColIndex == -1)
      {
        MoveYmm(BUF_FIRST_LINE_Y);
        EnableDisableMagnet(false);
        MoveYmm(0 - BUF_FIRST_LINE_Y);
      }
      else if(commandedMove.destColIndex == -2)
      {
        MoveYmm(BUF_FIRST_LINE_Y);
        MoveXmm(BUF_SECOND_LINE_X);
        MoveYmm(BUF_SECOND_LINE_Y);
        EnableDisableMagnet(false);
        MoveYmm(0 - BUF_FIRST_LINE_Y - BUF_SECOND_LINE_Y);
        MoveXmm(0 - BUF_SECOND_LINE_X);
      }
      if(commandedMove.destColIndex == 8)
      {
        MoveYmm(0 - BUF_FIRST_LINE_Y);
        EnableDisableMagnet(false);
        MoveYmm(BUF_FIRST_LINE_Y);
      }
      else if(commandedMove.destColIndex == 9)
      {
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
  }
  else
  {
    Serial.printf("Disable Motors\n");
    digitalWrite(EN_PIN_A, HIGH);
    digitalWrite(EN_PIN_B, HIGH);
  }
}

void EnableDisableMagnet(bool enableDisable)
{
  if(enableDisable)
  {
    Serial.printf("Enable Magnet\n");
    digitalWrite(MAGNET_PIN, HIGH);
  }
  else
  {
    Serial.printf("Disable Magnet\n");
    digitalWrite(MAGNET_PIN, LOW);
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
  MoveXmm(-30.5);
  MoveYmm(89.5);
  curHeadLocation.rowIndex = 7;
  curHeadLocation.colIndex = 7;
}

void MoveYSquares(float squares)
{
  Serial.printf("MoveYSquares %f\n", squares);
  MoveYmm(-1 * squares * SQUARE_TO_MM);
}

void MoveXSquares(float squares)
{
  Serial.printf("MoveXSquares %f\n", squares);
  MoveXmm(squares * SQUARE_TO_MM);
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

void MoveYSteps(int steps)
{
  Serial.printf("MoveYSteps %d\n", steps);
  if(steps > 0)
  {
    digitalWrite(DIR_PIN_A, LOW);
    digitalWrite(DIR_PIN_B, HIGH);
    MoveBothMotors(steps, 3);
  }
  else
  {
    digitalWrite(DIR_PIN_A, HIGH);
    digitalWrite(DIR_PIN_B, LOW);
    MoveBothMotors(steps * -1, 1);
  }
}

void MoveXSteps(int steps)
{
  Serial.printf("MoveXSteps %d\n", steps);
  if(steps > 0)
  {
    digitalWrite(DIR_PIN_A, HIGH);
    digitalWrite(DIR_PIN_B, HIGH);
    MoveBothMotors(steps, 4);
  }
  else
  {
    digitalWrite(DIR_PIN_A, LOW);
    digitalWrite(DIR_PIN_B, LOW);
    MoveBothMotors(steps * -1, 2);
  }
}

void MoveBothMotors(int steps, int dir) //dir 1 == -y, 2 == -x, 3 == +y, 4 == +x 
{
  EnableDisableMotors(1);
  Serial.printf("MoveBothMotors steps=%d dir=%d\n", steps, dir);
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
      EnableDisableMotors(0);
      break;
    }
  }
  Serial.printf("MoveBothMotors complete\n");
  EnableDisableMotors(0);
}
