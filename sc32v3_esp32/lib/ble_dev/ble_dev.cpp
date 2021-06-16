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
static uint8_t inputs_cnt = 0;
static BLERemoteCharacteristic* output;
static BLEAdvertisedDevice* myDevice;
static ble_data_callback dataCallback;
static uint32_t last_ble_run = 0;

class MyClientCallback : public BLEClientCallbacks {
    void onConnect(BLEClient* pclient) {
        doScan = false;
    }

    void onDisconnect(BLEClient* pclient) {
        connected = false;
        doScan = true;
    }
};

class MyAdvertisedDeviceCallbacks: public BLEAdvertisedDeviceCallbacks {
    /**
     * Called for each advertising BLE server.
     */
    void onResult(BLEAdvertisedDevice advertisedDevice) {
        // Serial.print("BLE Advertised Device found: ");
        // Serial.println(advertisedDevice.toString().c_str());

        // We have found a device, let us now see if it contains the service we are looking for.
        if (advertisedDevice.haveServiceUUID() && advertisedDevice.isAdvertisingService(serviceUUID)) {

            BLEDevice::getScan()->stop();
            myDevice = new BLEAdvertisedDevice(advertisedDevice);
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
    // Log.print("Forming a connection to ");
    Log.println(myDevice->getAddress().toString().c_str());
    
    BLEClient*  pClient  = BLEDevice::createClient();
    //Log.println(" - Created client");

    pClient->setClientCallbacks(new MyClientCallback());

    // Connect to the remove BLE Server.
    pClient->connect(myDevice);  // if you pass BLEAdvertisedDevice instead of address, it will be recognized type of peer device address (public or private)
    //Log.println(" - Connected to server");

    // Obtain a reference to the service we are after in the remote BLE server.
    BLERemoteService* pRemoteService = pClient->getService(serviceUUID);
    if (pRemoteService == nullptr) {
        Log.print("BLE: Failed to find our service UUID: ");
        Log.println(serviceUUID.toString().c_str());
        pClient->disconnect();
        return false;
    }
    //Serial.println(" - Found our service");

    // Obtain a reference to the in/out characteristics in the service of the remote BLE server.
    auto charsMap = pRemoteService->getCharacteristicsByHandle();
    for (auto it = charsMap->begin(); it != charsMap->end(); ++it)
    {
        if (it->second->getUUID().equals(charUUID))
        {
            if (it->second->canWrite())
            {
                output = it->second;
            }
            else {
                inputs[inputs_cnt++] = it->second;
            }
            Log.printf("Found %s h: %02X r:%d, w:%d, n:%d", it->second->getUUID().toString().c_str(), it->first, it->second->canRead(), it->second->canWrite(), it->second->canNotify());
            Log.println();
        }
    }

    if (!inputs[0]->canRead() || !inputs[1]->canRead() || !output->canWrite()) {
        Log.println("BLE: No input or output characteristics detected");
        return false;
    }

    inputs[0]->setAuth(esp_gatt_auth_req_t::ESP_GATT_AUTH_REQ_NO_MITM);
    inputs[1]->setAuth(esp_gatt_auth_req_t::ESP_GATT_AUTH_REQ_NO_MITM);
    output->setAuth(esp_gatt_auth_req_t::ESP_GATT_AUTH_REQ_NO_MITM);

    if(inputs[0]->canNotify())
        inputs[0]->registerForNotify(notifyCallback_1);
    if(inputs[1]->canNotify())
        inputs[1]->registerForNotify(notifyCallback_2);

    connected = true;
    return true;
}

void ble_begin(ble_data_callback callback) {
    BLEDevice::init("");
    BLEDevice::setMTU(33);
    BLEScan* pBLEScan = BLEDevice::getScan();
    pBLEScan->setAdvertisedDeviceCallbacks(new MyAdvertisedDeviceCallbacks());
    pBLEScan->setInterval(1349);
    pBLEScan->setWindow(449);
    pBLEScan->setActiveScan(true);
    pBLEScan->start(5, false);
    dataCallback = callback;
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

        // if(doScan){
        //     BLEDevice::getScan()->start(2);  
        //     // this is just example to start scan after disconnect, most likely there is better way to do it in arduino
        // }
        last_ble_run = time;
    }
}