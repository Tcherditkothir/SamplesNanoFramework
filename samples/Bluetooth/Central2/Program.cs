//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.Threading;
using System.Collections;

using nanoFramework.Device.Bluetooth;
using nanoFramework.Device.Bluetooth.Advertisement;
using nanoFramework.Device.Bluetooth.GenericAttributeProfile;

namespace Central2
{
    /// <summary>
    /// Sample to collect temperature values from a collection of
    /// Environmental Sensor devices. Designed to work with 
    /// the Bluetooth Sample 3.
    /// 
    /// It will first watch for advertisements from Sensor devices 
    /// for 15 seconds after finding first device.
    /// Then stop watcher and connect and collect temperature
    /// changes from the found devices.
    /// 
    /// Note: You can not run watcher and connect to devices at same time.
    /// </summary>
    public static class Program
    {
        // Devices found by watcher
        private readonly static Hashtable s_foundDevices = new();

        // Devices to collect from. Added when connected
        private readonly static Hashtable s_dataDevices = new();

        public static void Main()
        {
            Console.WriteLine("Client d'échantillon/Central 2 : Collecte des données depuis des capteurs environnementaux");
            Console.WriteLine("Recherche des capteurs environnementaux");

            // Créer un observateur d'annonces
            BluetoothLEAdvertisementWatcher watcher = new BluetoothLEAdvertisementWatcher();
            watcher.ScanningMode = BluetoothLEScanningMode.Active;
            watcher.Received += Watcher_Received;

            // Le dispositif auquel nous allons nous connecter et communiquer.
            BluetoothLEDevice connectedDevice = null;

            while (true)
            {
                Console.WriteLine("Démarrage du BluetoothLEAdvertisementWatcher");
                watcher.Start();

                // Attendre jusqu'à ce que nous trouvions des dispositifs à connecter
                while (s_foundDevices.Count == 0)
                {
                    Thread.Sleep(10000);
                }
                Console.WriteLine("Arrêt du BluetoothLEAdvertisementWatcher");
                watcher.Stop(); // Nous ne pouvons pas nous connecter si l'observateur est en cours d'exécution, donc l'arrêter.

                Console.WriteLine($"Dispositifs trouvés {s_foundDevices.Count}");
                Console.WriteLine("Connexion et lecture des capteurs");

                // Itérer sur les dispositifs trouvés et se connecter à eux
                foreach (DictionaryEntry entry in s_foundDevices)
                {
                    BluetoothLEDevice device = entry.Value as BluetoothLEDevice;

                    // Essayer de se connecter et s'inscrire pour les notifications
                    if (ConnectAndRegister(device))
                    {
                        if (s_dataDevices.Contains(device.BluetoothAddress))
                        {
                            s_dataDevices.Remove(device.BluetoothAddress);
                        }
                        s_dataDevices.Add(device.BluetoothAddress, device);

                        // Nous gérons une seule connexion dans cet exemple, donc sauvegarder ce dispositif
                        connectedDevice = device;

                        // Envoyer les instructions initiales de vitesse et de direction
                        WriteSpeedAndDirection(device, 25, 90);
                        Console.WriteLine("Instructions initiales envoyées.");

                        // Sortir de la boucle car nous voulons gérer une seule connexion
                        break;
                    }
                }

                // Effacer la liste des dispositifs trouvés pour la prochaine période de publicité
                s_foundDevices.Clear();

                // Vérifier si nous nous sommes connectés avec succès à un dispositif
                if (connectedDevice != null)
                {
                    // Tant que le dispositif est connecté, envoyer les instructions de vitesse et de direction
                    while (connectedDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
                    {
                        WriteSpeedAndDirection(connectedDevice, 25, 90);
                        Console.WriteLine("Instructions de vitesse et de direction envoyées.");

                        // Attendre une période avant de renvoyer les instructions
                        Thread.Sleep(1000);
                    }

                    // Si la connexion est perdue, l'enregistrer et nettoyer
                    Console.WriteLine("Déconnecté du dispositif. Nettoyage en cours.");
                    connectedDevice.Dispose();
                    connectedDevice = null;
                }
            }
        }

        /// <summary>
        /// Check for device with correct Service UUID in advert and not already found
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static bool IsValidDevice(BluetoothLEAdvertisementReceivedEventArgs args)
        {
            if (args.Advertisement.ServiceUuids.Length > 0 &&
                args.Advertisement.ServiceUuids[0].Equals(new Guid("A7EEDF2C-DA87-4CB5-A9C5-5151C78B0057")))
            {
                if (!s_foundDevices.Contains(args.BluetoothAddress))
                {
                    return true;
                }
            }

            return false;
        }

        private static void Watcher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            // Print information about received advertisement
            // You don't receive all information in 1 event and it can be split across 2 events
            // AdvertisementTypes 0 and 4
            Console.WriteLine($"Received advertisement address:{args.BluetoothAddress:X}/{args.BluetoothAddressType} Name:{args.Advertisement.LocalName}  Advert type:{args.AdvertisementType}  Services:{args.Advertisement.ServiceUuids.Length}");

            if (args.Advertisement.ServiceUuids.Length > 0)
            {
                Console.WriteLine($"Advert Service UUID {args.Advertisement.ServiceUuids[0]}");
            }

            // Look for advert with our primary service UUID from Bluetooth Sample 3
            if (IsValidDevice(args))
            {
                Console.WriteLine($"Found an Environmental test sensor :{args.BluetoothAddress:X}");

                // Add it to list as a BluetoothLEDevice
                s_foundDevices.Add(args.BluetoothAddress, BluetoothLEDevice.FromBluetoothAddress(args.BluetoothAddress, args.BluetoothAddressType));
            }
        }

        /// <summary>
        /// Connect and set-up Temperature Characteristics for value 
        /// changed notifications.
        /// </summary>
        /// <param name="device">Bluetooth device</param>
        /// <returns>True if device connected</returns>
        private static bool ConnectAndRegister(BluetoothLEDevice device)
        {
            bool result = false;

            GattDeviceServicesResult sr = device.GetGattServicesForUuid(GattServiceUuids.EnvironmentalSensing);
            if (sr.Status == GattCommunicationStatus.Success)
            {
                // Connected and services read
                result = true;

                // Pick up all temperature characteristics
                foreach (GattDeviceService service in sr.Services)
                {
                    Console.WriteLine($"Service UUID {service.Uuid}");

                    GattCharacteristicsResult cr = service.GetCharacteristicsForUuid(GattCharacteristicUuids.Temperature);
                    if (cr.Status == GattCommunicationStatus.Success)
                    {
                        //Temperature characteristics found now read value and 
                        //set up notify for value changed
                        foreach (GattCharacteristic gc in cr.Characteristics)
                        {
                            // Read current temperature
                            GattReadResult rr = gc.ReadValue();
                            if (rr.Status == GattCommunicationStatus.Success)
                            {
                                // Read current value and output
                                OutputTemp(gc, ReadTempValue(rr.Value));

                                // Set up a notify value changed event
                                gc.ValueChanged += TempValueChanged;
                                // and configure CCCD for Notify
                                gc.WriteClientCharacteristicConfigurationDescriptorWithResult(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                            }
                        }
                    }
                }
            }
            return result;
        }

        private static void WriteSpeedAndDirection(BluetoothLEDevice device, byte speed, byte direction)
        {
            // Obtention du service VehicleControlService par UUID
            GattDeviceServicesResult servicesResult = device.GetGattServicesForUuid(new Guid("245463B4-38E9-43BD-90A7-6CE397CA9BCE"));
            if (servicesResult.Status == GattCommunicationStatus.Success && servicesResult.Services.Length > 0)
            {
                GattDeviceService vehicleService = servicesResult.Services[0];

                // Écriture sur la caractéristique de vitesse
                WriteCharacteristic(vehicleService, new Guid("27BFA565-7FAE-4CFF-BE56-7AC3082B0E8D"), speed);

                // Écriture sur la caractéristique de direction
                WriteCharacteristic(vehicleService, new Guid("2F6343CE-E5BF-49F2-B189-07AB6EB6F168"), direction);
            }
        }

        private static void WriteCharacteristic(GattDeviceService service, Guid characteristicUuid, byte value)
        {
            GattCharacteristicsResult charResult = service.GetCharacteristicsForUuid(characteristicUuid);
            if (charResult.Status == GattCommunicationStatus.Success && charResult.Characteristics.Length > 0)
            {
                GattCharacteristic characteristic = charResult.Characteristics[0];
                DataWriter writer = new DataWriter();
                writer.WriteByte(value);
                characteristic.WriteValueWithResult(writer.DetachBuffer());
            }
        }


        private static void Device_ConnectionStatusChanged(object sender, EventArgs e)
        {
            BluetoothLEDevice dev = (BluetoothLEDevice)sender;
            if (dev.ConnectionStatus == BluetoothConnectionStatus.Disconnected)
            {
                Console.WriteLine($"Device {dev.BluetoothAddress:X} disconnected");

                // Remove device. We get picked up again once advert seen.
                s_dataDevices.Remove(dev.BluetoothAddress);
                dev.Dispose();
            }
        }

        private static float ReadTempValue(Buffer value)
        {
            DataReader rdr = DataReader.FromBuffer(value);
            return (float)rdr.ReadInt16() / 100;
        }

        private static void OutputTemp(GattCharacteristic gc, float value)
        {
            Console.WriteLine($"New value => Device:{gc.Service.Device.BluetoothAddress:X} Sensor:{gc.UserDescription,-20}  Current temp:{value}");
        }

        private static void TempValueChanged(GattCharacteristic sender, GattValueChangedEventArgs valueChangedEventArgs)
        {
            OutputTemp(sender,
                ReadTempValue(valueChangedEventArgs.CharacteristicValue));
        }
    }
}
