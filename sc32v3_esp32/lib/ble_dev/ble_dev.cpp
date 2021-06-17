#include "ble_dev.h"
#include "Platform.h"

// The remote service we wish to connect to.
static BLEUUID serviceUUID("00001812-0000-1000-8000-00805f9b34fb");
// The characteristic of the remote service we are interested in.
static BLEUUID    charUUID("00002a4d-0000-1000-8000-00805f9b34fb");

static boolean connected = false;
static boolean doConnect = false;
static boolean doScan = false;
static BLERemoteCharacteristic* inputs[2];
static BLERemoteCharacteristic* output;
static BLEAdvertisedDevice* myDevice;
static ble_data_callback dataCallback;
static uint32_t last_ble_run = 0;

class MyClientCallback : public BLEClientCallbacks {
    void onConnect(BLEClient* pClient) {
        Log.println("Connected");
        /** After connection we should change the parameters if we don't need fast response times.
         *  These settings are 150ms interval, 0 latency, 450ms timout.
         *  Timeout should be a multiple of the interval, minimum is 100ms.
         *  I find a multiple of 3-5 * the interval works best for quick response/reconnect.
         *  Min interval: 120 * 1.25ms = 150, Max interval: 120 * 1.25ms = 150, 0 latency, 60 * 10ms = 600ms timeout
         */
        pClient->updateConnParams(120,120,0,60);
        doScan = false;
    }

    void onDisconnect(BLEClient* pClient) {
        Log.print(pClient->getPeerAddress().toString().c_str());
        Log.println(" Disconnected - Starting scan");
        connected = false;
        doScan = true;
    }
};

class MyAdvertisedDeviceCallbacks: public BLEAdvertisedDeviceCallbacks {
    /**
     * Called for each advertising BLE server.
     */
    void onResult(BLEAdvertisedDevice *advertisedDevice) {
        Log.print("Advertised Device found: ");
        Log.println(advertisedDevice->toString().c_str());
        // We have found a device, let us now see if it contains the service we are looking for.
        if (advertisedDevice->haveServiceUUID() && advertisedDevice->isAdvertisingService(serviceUUID)) {
            Log.println("Found Our Service");
            BLEDevice::getScan()->stop();
            myDevice = advertisedDevice;
            doConnect = true;
        } // Found our server
    } // onResult
}; // MyAdvertisedDeviceCallbacks

static void notifyCallback_1(
    BLERemoteCharacteristic* pBLERemoteCharacteristic,
    uint8_t* pData,
    size_t length,
    bool isNotify) {
    dataCallback(pBLERemoteCharacteristic->getUUID().toString(), 0x1, pData, length);
}

static void notifyCallback_2(
    BLERemoteCharacteristic* pBLERemoteCharacteristic,
    uint8_t* pData,
    size_t length,
    bool isNotify) {
    dataCallback(pBLERemoteCharacteristic->getUUID().toString(), 0x2, pData, length);
}

bool connectToServer() {
    BLEClient* pClient = nullptr;

    /** Check if we have a client we should reuse first **/
    if(NimBLEDevice::getClientListSize()) {
        /** Special case when we already know this device, we send false as the
         *  second argument in connect() to prevent refreshing the service database.
         *  This saves considerable time and power.
         */
        pClient = NimBLEDevice::getClientByPeerAddress(myDevice->getAddress());
        if(pClient){
            if(!pClient->connect(myDevice, false)) {
                Log.println("Reconnect failed");
                return false;
            }
            Log.println("Reconnected client");
        }
        /** We don't already have a client that knows this device,
         *  we will check for a client that is disconnected that we can use.
         */
        else {
            pClient = NimBLEDevice::getDisconnectedClient();
        }
    }

    if(!pClient) {
        if(NimBLEDevice::getClientListSize() >= NIMBLE_MAX_CONNECTIONS) {
            Log.println("Max clients reached - no more connections available");
            return false;
        }
    
        pClient  = BLEDevice::createClient();
        Log.println("New client created");

        pClient->setClientCallbacks(new MyClientCallback(), false);

        /** Set initial connection parameters: These settings are 15ms interval, 0 latency, 120ms timout.
         *  These settings are safe for 3 clients to connect reliably, can go faster if you have less
         *  connections. Timeout should be a multiple of the interval, minimum is 100ms.
         *  Min interval: 12 * 1.25ms = 15, Max interval: 12 * 1.25ms = 15, 0 latency, 51 * 10ms = 510ms timeout
         */
        pClient->setConnectionParams(12,12,0,51);
        /** Set how long we are willing to wait for the connection to complete (seconds), default is 30. */
        pClient->setConnectTimeout(5);
        if (!pClient->connect(myDevice)) {
            /** Created a client but failed to connect, don't need to keep it as it has no data */
            NimBLEDevice::deleteClient(pClient);
            Log.println("Failed to connect, deleted client");
            return false;
        }
    }

    if(!pClient->isConnected()) {
        if (!pClient->connect(myDevice)) {
            Log.println("Failed to connect");
            return false;
        }
    }

    Log.print("Connected to: ");
    Log.println(pClient->getPeerAddress().toString().c_str());
    Log.print("RSSI: ");
    Log.println(pClient->getRssi());

    // Obtain a reference to the service we are after in the remote BLE server.
    BLERemoteService* pRemoteService = pClient->getService(serviceUUID);
    if (pRemoteService == nullptr) {
        Log.print("BLE: Failed to find our service UUID: ");
        Log.println(serviceUUID.toString().c_str());
        pClient->disconnect();
        return false;
    }

    // Obtain a reference to the in/out characteristics in the service of the remote BLE server.
    auto charsMap = pRemoteService->getCharacteristics(true);
    Log.printf("Characteristics count=%d begin=%x end=%x\n\r",charsMap->capacity(), charsMap->begin(), charsMap->end());
    int inputs_cnt = 0;
    for (auto it = charsMap->begin(); it != charsMap->end(); ++it)
    {
        auto ch = *it;
        if (ch->getUUID().equals(charUUID))
        {
            if (ch->canWrite())
            {
                output = ch;
            }
            else {
                inputs[inputs_cnt++] = ch;
            }
            Log.printf("Found %s h: %02X r:%d, w:%d, n:%d", ch->getUUID().toString().c_str(), ch->getHandle(), ch->canRead(), ch->canWrite(), ch->canNotify());
            Log.println();
        }
    }

    if (inputs[0] == NULL || inputs[1] == NULL || output == NULL ||
        !inputs[0]->canRead() || !inputs[1]->canRead() || !output->canWrite()) {
        Log.println("BLE: No input or output characteristics detected");
        return false;
    }

    if(inputs[0]->canNotify())
        inputs[0]->subscribe(true, notifyCallback_1);
    if(inputs[1]->canNotify())
        inputs[1]->subscribe(true, notifyCallback_2);

    connected = true;
    return true;
}

void ble_begin(ble_data_callback callback) {
    BLEDevice::init("");

    NimBLEDevice::setSecurityAuth(/*BLE_SM_PAIR_AUTHREQ_BOND | BLE_SM_PAIR_AUTHREQ_MITM |*/ BLE_SM_PAIR_AUTHREQ_MITM);

    BLEDevice::setMTU(33);
    BLEScan* pBLEScan = BLEDevice::getScan();
    pBLEScan->setAdvertisedDeviceCallbacks(new MyAdvertisedDeviceCallbacks());
    /** Set scan interval (how often) and window (how long) in milliseconds */
    pBLEScan->setInterval(45);
    pBLEScan->setWindow(15);
    pBLEScan->setActiveScan(true);
    pBLEScan->start(5, false);
    dataCallback = callback;
    inputs[0] = inputs[1] = output = NULL;
}

void ble_run() {
    uint32_t time = millis();
    if ((time-last_ble_run)>500)
    {
        if (doConnect == true) {
            if (connectToServer()) {
                Log.println("BLE: We are now connected to the BLE Server.");
                Log.printf("\033c");
            } else {
                Log.println("BLE: We have failed to connect to the server; there is nothin more we will do.");
            }
            doConnect = false;
        }

        if(doScan){
            BLEDevice::getScan()->start(5);  
            // this is just example to start scan after disconnect, most likely there is better way to do it in arduino
        }
        last_ble_run = time;
    }
}