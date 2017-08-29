#define nullptr 0

#define RFIDSendCID 1
#define InfoReqCID 2
#define GroupRevCID 3
#define PingCID 4
#define PongCID 5

#define InfoReqInterval 10000

void setup() {
  // put your setup code here, to run once:
  //set Digital Pin 2-8 to output mode
  DDRD = DDRD | B11111100;

  //startup communication with wifi module
  Serial.begin(115200);

  //reset wifi module
//  digitalWrite(4, LOW);
//  delay(300);
//  digitalWrite(4, HIGH);
}

bool checkDataEqualString(const char * string, const char * data){
  int index = 0;
  while(string[index] != '\0'){
    if(string[index] != data[index]){
      return false;
    }
    index += 1;
  }

  return true;
}

char buffer[200];
int bufferPos;

bool defaultCommandPart(){
  Serial.write("\r\n");

  bufferPos = 0;

  for(;;){
    if(Serial.available()){
      buffer[bufferPos] = Serial.read();

      if(buffer[bufferPos] == '\n'){
        int result = checkBufferReturn(bufferPos);
        if(result > 0){
          return result == 1;
        }
      }

      bufferPos += 1;
      if(bufferPos > 199){
        return false; //avoid array overflow, stop action
      }
    }
  }
}

bool sendCommandString(const char * command){
  Serial.write(command);
  return defaultCommandPart();
}

int checkBufferReturn(const int bufferPos){
  if(bufferPos >= 3){
    if(checkDataEqualString("OK", buffer + bufferPos - 3)){
      return 1;
    }
  }
  if(bufferPos >= 6){
    if(checkDataEqualString("ERROR", buffer + bufferPos - 6)){
      return 2;
    }
  }
  return 0;
}

void clearBuffer(){
  bufferPos = 0;
}

bool checkServerRecieve(){
  while(Serial.available()){
    buffer[bufferPos] = Serial.read();

    if(bufferPos > 3){
      if(checkDataEqualString("+IPD,", buffer + bufferPos - 4)){
        recieveMessage();
        return true;
      }
    }

    bufferPos += 1;
    if(bufferPos > 199){
      return false; //avoid array overflow, stop action
    }
  }
  
  return false;
}

void recieveMessage(){
  int messageLength = 0;
  
  for(;;){
    char data = Serial.read();
    if(data == ':'){
      break;
    }else if(data >= '0' && data <= '9'){
      messageLength = messageLength * 10 + (data - '0'); //string to int conversion

      if(messageLength > 199){
        return; //avoid array overflow, stop action
      }
    }else if(data == ','){
      messageLength = 0;
    }
  }

  for(bufferPos = 0; bufferPos < messageLength; bufferPos++){
    if(Serial.available()){
      buffer[bufferPos] = Serial.read();
    }else{
      return;
    }
  }
}

bool sendCommandToServer(uint8_t commandID, const char * data = nullptr, uint32_t dataLength = 0){
  Serial.write("AT+CIPSEND=");
  Serial.print(dataLength + 1);
  if(!defaultCommandPart()){
    return false;
  }
  Serial.write(commandID);
  Serial.write(data, dataLength);
  return defaultCommandPart();
}



void loop() {
  // put your main code here, to run repeatedly:
  digitalWrite(13, LOW);

  delay(1000);

  //TEST IF WIFI MODULE IS ONLINE
  if(!sendCommandString("AT")){
    return;
  }

  //MODE TO CONNECT WITH WIFI ACCESSPOINT
  if(!sendCommandString("AT+CWMODE=1")){
    return;
  }

  //CONNECT WITH WIFI
  if(!sendCommandString("AT+CWJAP=\"SSID\",\"PASSWORD\"")){
    return;
  }

  //SET MULTI CONNECTION MODE
  if(!sendCommandString("AT+CIPMUX=1")){
    return;
  }

  //CONNECT WITH UDP SERVER (SERVER COULD ALSO RESPOND ON THIS SOCCET)
  sendCommandString("AT+CIPSTART=\"UDP\",\"192.168.0.6\",12345");     //NO CHECK TO NOT CAPTURE POSSIBLE ALREADY ASSIGNED ERROR

  sendCommandToServer(PingCID);

  clearBuffer();

  digitalWrite(13, HIGH); //SHOW THAT I HAVE CONNECTION WITH WIFI

  for(;;){
    if(checkServerRecieve()){
      switch(buffer[0]){
        case GroupRevCID:
          PORTD |= (buffer[1] << 2);
          PORTD &= ~(buffer[1] << 2);
          break;
        case PingCID:
          sendCommandToServer(PongCID);
      }

      clearBuffer();
    }
    delay(2000);
  }
}
