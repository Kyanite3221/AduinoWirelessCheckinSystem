#include <cstdio>

const int RFScannerPin = 1; //still to figure out

const int gemeentePin = 2;
const int BSOPin = 3;

const int gemeentePosition = 0;
const int BSOPosition = 1;

char boolString[5];

enum powerLevel{
    LOW,
    HIGH,
    MEDIUM,
    OFF
};

enum io{
    OUTPUT,
    INPUT
};

char *getServerData();

void pinMode(int pin, io state){

    printf("%i set to %o\n",pin,state);
}

void digitalWrite(int pin, powerLevel level){
    printf("%i set to %o\n",pin,level);
}

void setup() {


    pinMode(gemeentePin, OUTPUT);
    pinMode(BSOPin, OUTPUT);
    pinMode(RFScannerPin, INPUT);

    boolString[0] = 't';
    boolString[1] = 't';
    boolString[2] = 'f';
    boolString[3] = 'f';
    boolString[4] = 'f';
}

void loop() {
    //scan the scanner
    //communicate with the server
    char* serverdata = getServerData();
    printf("%s is %i long\n",serverdata,(serverdata[0]-'0'));
    if (serverdata != NULL) {
        int progress = 0;
        while (progress < (serverdata[0]-'0') && progress < (sizeof(boolString)/sizeof(*boolString))) {
            if (serverdata[progress+1] != boolString[progress]){
                printf("found a difference in %i, it was %c, not %c\n", progress, serverdata[progress+1], boolString[progress]);
                if (serverdata[progress+1]=='t'){
                    digitalWrite(progress+2,HIGH);
                } else {
                    digitalWrite(progress+2,LOW);
                }
                boolString[progress] = serverdata[progress+1];
            }
            progress++;
        }
    }
    printf("%s is %i long\n",boolString,(sizeof(boolString)/sizeof(*boolString)));
}

char *getServerData() {
    return (char*)"5ffftt";
}

void lightOn(int pin){
    digitalWrite(pin, HIGH);
}

void lightOut(int pin){
    digitalWrite(pin, LOW);
}

int main(){
    setup();
    loop();
}



