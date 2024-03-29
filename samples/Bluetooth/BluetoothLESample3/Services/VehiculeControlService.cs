using System;
using System.Diagnostics;
using nanoFramework.Device.Bluetooth.GenericAttributeProfile;

namespace nanoFramework.Device.Bluetooth.Services
{
    public class VehicleControlService
    {
        private readonly GattLocalService _vehicleControlService;
        private GattLocalCharacteristic _speedCharacteristic;
        private GattLocalCharacteristic _directionCharacteristic;

        // UUIDs personnalisés pour le service et les caractéristiques 
        private Guid vehicleControlServiceUuid = new Guid("245463B4-38E9-43BD-90A7-6CE397CA9BCE");
        private Guid speedCharacteristicUuid = new Guid("27BFA565-7FAE-4CFF-BE56-7AC3082B0E8D");
        private Guid directionCharacteristicUuid = new Guid("2F6343CE-E5BF-49F2-B189-07AB6EB6F168");

        public VehicleControlService()
        {
            // Création du service VehicleControlService
            GattServiceProviderResult serviceProviderResult = GattServiceProvider.Create(vehicleControlServiceUuid);
            if (serviceProviderResult.Error != BluetoothError.Success)
            {
                throw new Exception("Impossible de créer le service de contrôle du véhicule");
            }
            _vehicleControlService = serviceProviderResult.ServiceProvider.Service;

            // Ajout de la caractéristique de vitesse au service
            GattLocalCharacteristicResult speedCharacteristicResult = _vehicleControlService.CreateCharacteristic(speedCharacteristicUuid,
                new GattLocalCharacteristicParameters
                {
                    CharacteristicProperties = GattCharacteristicProperties.Write,
                    UserDescription = "Speed"
                });

            if (speedCharacteristicResult.Error != BluetoothError.Success)
            {
                throw new Exception("Impossible de créer la caractéristique de vitesse");
            }
            _speedCharacteristic = speedCharacteristicResult.Characteristic;
            _speedCharacteristic.WriteRequested += SpeedCharacteristic_WriteRequested;

            // Ajout de la caractéristique de direction au service
            GattLocalCharacteristicResult directionCharacteristicResult = _vehicleControlService.CreateCharacteristic(directionCharacteristicUuid,
                new GattLocalCharacteristicParameters
                {
                    CharacteristicProperties = GattCharacteristicProperties.Write,
                    UserDescription = "Direction"
                });

            if (directionCharacteristicResult.Error != BluetoothError.Success)
            {
                throw new Exception("Impossible de créer la caractéristique de direction");
            }
            _directionCharacteristic = directionCharacteristicResult.Characteristic;
            _directionCharacteristic.WriteRequested += DirectionCharacteristic_WriteRequested;



            // Create GattServiceProvider for VehicleControlService
            GattServiceProviderResult result = GattServiceProvider.Create(vehicleControlServiceUuid);
            if (result.Error != BluetoothError.Success)
            {
                Debug.WriteLine($"Unable to create GattServiceProvider: {result.Error}");
                return;
            }

            // Get GattServiceProvider from result
            GattServiceProvider serviceProvider = result.ServiceProvider;
            // Commencez à faire de la publicité pour le service
            serviceProvider.StartAdvertising(new GattServiceProviderAdvertisingParameters
            {
                IsConnectable = true,
                IsDiscoverable = true
            });
        }

        private void SpeedCharacteristic_WriteRequested(GattLocalCharacteristic sender, GattWriteRequestedEventArgs args)
        {
            Console.WriteLine("Nouvelle vitesse recu");
            // Traitement d'une requête d'écriture pour la caractéristique de vitesse
            var request = args.GetRequest();
            var reader = DataReader.FromBuffer(request.Value);
            byte speedValue = reader.ReadByte();

            // Logique de traitement de la vitesse ici
            Console.WriteLine($"Nouvelle vitesse demandée : {speedValue}");

            request.Respond();
        }

        private void DirectionCharacteristic_WriteRequested(GattLocalCharacteristic sender, GattWriteRequestedEventArgs args)
        {
            // Traitement d'une requête d'écriture pour la caractéristique de direction
            var request = args.GetRequest();
            var reader = DataReader.FromBuffer(request.Value);
            byte directionValue = reader.ReadByte();

            // Logique de traitement de la direction ici
            Console.WriteLine($"Nouvelle direction demandée : {directionValue}");

            request.Respond();
        }
    }
}
