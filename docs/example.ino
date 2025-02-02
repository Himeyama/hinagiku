#define INTERVAL 10
unsigned long startTime = 0;
unsigned long time = 0;
unsigned long old = 0;

void setup() {
  Serial.begin(9600);
  startTime = millis();
}

void loop() {
  time = (millis() - startTime);
  if(time / INTERVAL != old){
    float sinValue = sin(TWO_PI * time / 1000.0);
    int scaledValue = (sinValue + 1) * 511;
    Serial.println(scaledValue);
  }
  old = time / INTERVAL;
}