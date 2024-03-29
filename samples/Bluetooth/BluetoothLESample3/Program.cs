using System;
using System.Diagnostics;
using System.Threading;
using nanoFramework.Device.Bluetooth;
using nanoFramework.Device.Bluetooth.GenericAttributeProfile;
using nanoFramework.Device.Bluetooth.Services;
using nanoFramework.Runtime.Native;

namespace BluetoothLESample3
{
        
    public class Program
        {
            public static void Main()
            {
            Console.WriteLine("Bonjour depuis Bluetooth Sample 3");

            // Création et initialisation de l'instance BluetoothLEServer pour le server (server = coté voiture, client = manette)                 
            BluetoothLEServer server = BluetoothLEServer.Instance;
            server.DeviceName = "Sample3";
            
            Guid serviceUuid = new("A7EEDF2C-DA87-4CB5-A9C5-5151C78B0057");
            Guid readStaticCharUuid = new("A7EEDF2C-DA89-4CB5-A9C5-5151C78B0057");

            // Initialize the VehicleControlService
            VehicleControlService vehicleControlService = new VehicleControlService();


            GattServiceProviderResult result = GattServiceProvider.Create(serviceUuid);
            if (result.Error != BluetoothError.Success)
            {
                Console.WriteLine($"Erreur lors de la création du service principal : {result.Error}");
                return;
            }

            GattServiceProvider serviceProvider = result.ServiceProvider;
            GattLocalService service = serviceProvider.Service;

            DataWriter sw = new DataWriter();
                sw.WriteString("Ceci est un exemple Bluetooth 3");

            GattLocalCharacteristicResult characteristicResult = service.CreateCharacteristic(readStaticCharUuid,
                new GattLocalCharacteristicParameters()
                {
                    CharacteristicProperties = GattCharacteristicProperties.Read,
                    UserDescription = "Caractéristique statique",
                    StaticValue = sw.DetachBuffer()
                });

            if (characteristicResult.Error != BluetoothError.Success)
            {
                Console.WriteLine($"Erreur lors de la création de la caractéristique statique : {characteristicResult.Error}");
                return;
            }

            DeviceInformationServiceService DifService = new("MyGreatCompany", "Model-1", null, "v1.0", SystemInfo.Version.ToString(), "");
                BatteryService BatService = new() { BatteryLevel = 94 };
                CurrentTimeService CtService = new(true);
                EnvironmentalSensorService EnvService = new();

                int iTempOut = EnvService.AddSensor(EnvironmentalSensorService.SensorType.Temperature, "Température extérieure");
                EnvService.UpdateValue(iTempOut, 23.4F);
                // ... autres capteurs ...

           

            // Start advertising the service with the GattServiceProvider instance
            serviceProvider.StartAdvertising(new GattServiceProviderAdvertisingParameters
            {
                IsConnectable = true,
                IsDiscoverable = true
            });

            Console.WriteLine("Publicité démarrée.");

                // Simulation de la mise à jour des capteurs
                SimulateSensorUpdates(EnvService);
            }

            private static void SimulateSensorUpdates(EnvironmentalSensorService EnvService)
            {
                // Mettre à jour les valeurs de capteurs pour simuler des changements
                float t1 = 23.4F;
                float t3 = 7.5F;
                int iTempOut = 0; // L'index du capteur à mettre à jour

                while (true)
                {
                    t1 += 1.3F;
                    t3 += 2.1F;

                    EnvService.UpdateValue(iTempOut, t1);
                    Console.WriteLine("Données mis a jour");
                    Thread.Sleep(5000);
                }
            }
        }
    }


