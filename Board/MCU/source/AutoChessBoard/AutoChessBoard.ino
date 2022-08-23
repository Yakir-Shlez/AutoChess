#include <ESP_FlexyStepper.h>

#define EN_PIN_1           13 // Enable
#define DIR_PIN_1          14 // Direction
#define STEP_PIN_1         15 // Step

#define EN_PIN_2           22 // Enable
#define DIR_PIN_2          23 // Direction
#define STEP_PIN_2         25 // Step

#define MS1_PIN            26
#define MS2_PIN            27

#define TEST_PIN         21 // Step

#define MICRO_STEPS 16
#define SPEED_AND_ACC 1000

// Speed settings
const int DISTANCE_TO_TRAVEL_IN_STEPS = 5 * 200 * MICRO_STEPS;
const int SPEED_IN_STEPS_PER_SECOND = SPEED_AND_ACC * MICRO_STEPS;
const int ACCELERATION_IN_STEPS_PER_SECOND = SPEED_AND_ACC * MICRO_STEPS;
const int DECELERATION_IN_STEPS_PER_SECOND = SPEED_AND_ACC * MICRO_STEPS;

// create the stepper motor object
ESP_FlexyStepper stepper_1;
//ESP_FlexyStepper stepper_2;
void setup() {
  Serial.begin(115200);
  pinMode(TEST_PIN, OUTPUT);
  digitalWrite(TEST_PIN, LOW);
  digitalWrite(TEST_PIN, HIGH); 
  
  pinMode(EN_PIN_1, OUTPUT);
  pinMode(STEP_PIN_1, OUTPUT);
  pinMode(DIR_PIN_1, OUTPUT);

  pinMode(EN_PIN_2, OUTPUT);
  pinMode(STEP_PIN_2, OUTPUT);
  pinMode(DIR_PIN_2, OUTPUT);

  pinMode(MS1_PIN, OUTPUT);
  pinMode(MS2_PIN, OUTPUT);
  
  digitalWrite(EN_PIN_1, LOW);      // Enable driver in hardware
  digitalWrite(EN_PIN_2, LOW);      // Enable driver in hardware

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

  stepper_1.connectToPins(STEP_PIN_1, DIR_PIN_1);
  //stepper_2.connectToPins(STEP_PIN_2, DIR_PIN_2);

  stepper_1.setSpeedInStepsPerSecond(SPEED_IN_STEPS_PER_SECOND);
  stepper_1.setAccelerationInStepsPerSecondPerSecond(ACCELERATION_IN_STEPS_PER_SECOND);
  stepper_1.setDecelerationInStepsPerSecondPerSecond(DECELERATION_IN_STEPS_PER_SECOND);

  //stepper_2.setSpeedInStepsPerSecond(SPEED_IN_STEPS_PER_SECOND);
  //stepper_2.setAccelerationInStepsPerSecondPerSecond(ACCELERATION_IN_STEPS_PER_SECOND);
  //stepper_2.setDecelerationInStepsPerSecondPerSecond(DECELERATION_IN_STEPS_PER_SECOND);

  stepper_1.startAsService(1);
  //stepper_2.startAsService(1);
  
}

int previousDirection_1 = -1;
int previousDirection_2 = -1;
bool shaft = true;

void loop() {
  if(shaft)
    digitalWrite(TEST_PIN, HIGH);
  else
    digitalWrite(TEST_PIN, LOW);

  if (stepper_1.getDistanceToTargetSigned() == 0)
  {
    //delay(5000);
    previousDirection_1 *= -1;
    long relativeTargetPosition = DISTANCE_TO_TRAVEL_IN_STEPS * previousDirection_1;
    Serial.printf("Moving stepper_1 by %ld steps\n", relativeTargetPosition);
    stepper_1.setTargetPositionRelativeInSteps(relativeTargetPosition);
  }
  //if (stepper_2.getDistanceToTargetSigned() == 0)
  //{
    //delay(5000);
    //previousDirection_2 *= -1;
    //long relativeTargetPosition = DISTANCE_TO_TRAVEL_IN_STEPS * previousDirection_2;
    //Serial.printf("Moving stepper_2 by %ld steps\n", relativeTargetPosition);
    //stepper_2.setTargetPositionRelativeInSteps(relativeTargetPosition);
  //}
  shaft = !shaft;
}
