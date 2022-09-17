//Client_A_Address
//E0:E2:E6:CE:0E:94

#include <esp_now.h>
#include <WiFi.h>
#include <Wire.h>

uint8_t serverAddress[] = {0x94, 0xB5, 0x55, 0x6C, 0x8F, 0xE4};
uint8_t groupAPins[] = {};


// Variable to store if sending data was successful
String success;

typedef struct struct_message {
    int groupA;
    int groupB;
} struct_message;

// Create a struct_message call9ed BME280Readings to hold sensor readings
struct_message hallSensorsData;

int serverRequest;
//0 = NOP
//1 = Send_All_Data

esp_now_peer_info_t peerInfo;

// Callback when data is sent
void OnDataSent(const uint8_t *mac_addr, esp_now_send_status_t status) {
  Serial.print("\r\nLast Packet Send Status:\t");
  Serial.println(status == ESP_NOW_SEND_SUCCESS ? "Delivery Success" : "Delivery Fail");
  if (status ==0){
    success = "Delivery Success :)";
  }
  else{
    success = "Delivery Fail :(";
  }
}

// Callback when data is received
void OnDataRecv(const uint8_t * mac, const uint8_t *incomingData, int len) {
  memcpy(&serverRequest, incomingData, sizeof(serverRequest));
  Serial.print("Bytes received: ");
  Serial.println(len);
  if(serverRequest == 0) //NOP
  {
    Serial.println("Receive: NOP");

    hallSensorsData.groupA = 0;
    hallSensorsData.groupB = 0;
  }
  else if(serverRequest == 1)
  {
    Serial.println("Receive: Send_All_Data");
    hallSensorsData.groupA = 
      digitalRead(15) + 
      digitalRead(2)*2 + 
      digitalRead(21)*4 + 
      digitalRead(4)*8 + 
      digitalRead(16)*16 + 
      digitalRead(17)*32 + 
      digitalRead(5)*64 + 
      digitalRead(18)*128;
    hallSensorsData.groupB = 
      digitalRead(32) + 
      digitalRead(33)*2 + 
      digitalRead(25)*4 + 
      digitalRead(26)*8 + 
      digitalRead(27)*16 + 
      digitalRead(14)*32 + 
      digitalRead(19)*64 + 
      digitalRead(13)*128;
  }
  
  esp_err_t result = esp_now_send(serverAddress, (uint8_t *) &hallSensorsData, sizeof(hallSensorsData));
  if (result == ESP_OK) {
    Serial.println("Sent with success");
  }
  else {
    Serial.println("Error sending the data");
  }
}

void setup() {
  // Init Serial Monitor
  Serial.begin(115200);

  // Set device as a Wi-Fi Station
  WiFi.mode(WIFI_STA);

  // Init ESP-NOW
  if (esp_now_init() != ESP_OK) {
    Serial.println("Error initializing ESP-NOW");
    return;
  }

  // Once ESPNow is successfully Init, we will register for Send CB to
  // get the status of Trasnmitted packet
  esp_now_register_send_cb(OnDataSent);
  
  // Register peer
  memcpy(peerInfo.peer_addr, serverAddress, 6);
  peerInfo.channel = 0;  
  peerInfo.encrypt = false;
  
  // Add peer        
  if (esp_now_add_peer(&peerInfo) != ESP_OK){
    Serial.println("Failed to add peer");
    return;
  }
  // Register for a callback function that will be called when data is received
  esp_now_register_recv_cb(OnDataRecv);

  PinsInit();

  Serial.println();
  Serial.print("ESP Board MAC Address:  ");
  Serial.println(WiFi.macAddress());
}
 
void loop() {
  delay(100);
}

void PinsInit()
{
  pinMode(15, INPUT_PULLUP);
  pinMode(2, INPUT_PULLUP);
  pinMode(21, INPUT_PULLUP);
  pinMode(4, INPUT_PULLUP);
  pinMode(16, INPUT_PULLUP);
  pinMode(17, INPUT_PULLUP);
  pinMode(5, INPUT_PULLUP);
  pinMode(18, INPUT_PULLUP);
  pinMode(32, INPUT_PULLUP);
  pinMode(33, INPUT_PULLUP);
  pinMode(25, INPUT_PULLUP);
  pinMode(26, INPUT_PULLUP);
  pinMode(27, INPUT_PULLUP);
  pinMode(14, INPUT_PULLUP);
  pinMode(19, INPUT_PULLUP);
  pinMode(13, INPUT_PULLUP);
}

void getReadings(){
  
}
