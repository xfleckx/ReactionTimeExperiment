#include <SerialCommand.h>
#include <SoftwareSerial.h>

const byte interruptPin = 2;
volatile byte await = LOW;

unsigned long timeAtAwait;
unsigned long timeAtResponse;

SerialCommand sCmd;

void setup() {
  // put your setup code here, to run once

  pinMode(interruptPin, INPUT_PULLUP);
  attachInterrupt(digitalPinToInterrupt(interruptPin), OnButtonPress, CHANGE);
  
  Serial.begin(9600);
  while(!Serial);
  
  sCmd.addCommand("Await", AwaitHandler);
  sCmd.addCommand("Reset", ResetHandler);
}

void loop() {
  // put your main code here, to run repeatedly:
  if (Serial.available() > 0)
      sCmd.readSerial();
}

void AwaitHandler(){
  await = HIGH;
  timeAtAwait = millis();
}

void ResetHandler(){
  await = LOW;
}

void OnButtonPress(){

  if(!await == HIGH)
    return;
  
  await = LOW;
  timeAtResponse = millis();
  unsigned long response = timeAtResponse - timeAtAwait;
  Serial.println(response);
}

