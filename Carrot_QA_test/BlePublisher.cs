using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Storage.Streams;

namespace Carrot_QA_test
{
    public class BlePublisher
    {
        enum State
        {
            None,
            Init,
            Stop,
            Update,
            Start,
        }

        private static BlePublisher _instance = null;
        private BluetoothLEAdvertisementPublisher _publisher = null;
        private Task _task = null;
        private BluetoothLEAdvertisementPublisherStatus bleStatus = BluetoothLEAdvertisementPublisherStatus.Created;
        private State _state = State.None;
        private object _lock = null;
        private UInt32 _imei = 0;

        public static BlePublisher Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new BlePublisher();
                }

                return _instance;
            }
        }

        private BlePublisher()
        {
            _lock = new object();
            _publisher = new BluetoothLEAdvertisementPublisher();
            _publisher.StatusChanged += _publisher_StatusChanged;
            _task = new Task(Run);
            _task.Start();
        }

        private void _publisher_StatusChanged(BluetoothLEAdvertisementPublisher sender, BluetoothLEAdvertisementPublisherStatusChangedEventArgs args)
        {
            bleStatus = args.Status;

            Console.WriteLine($"Error : {args.Error}, Status : {args.Status}");
            Console.WriteLine("");
        }

        

        public void SetImei(UInt32 imei = 1)
        {
            lock(_lock)
            {
                if(_state == State.None)
                {
                    _state = State.Update;
                }
                else
                {
                    _state = State.Stop;
                }
                
                _imei = imei;
            }
            _publisher.Stop();
        }

        private void Stop()
        {

        }

        private void Run()
        {
            while (true)
            {
                switch(_state)
                {
                    case State.None:
                        break;

                    case State.Init:
                        break;

                    case State.Stop:
                        Stoped();
                        break;

                    case State.Update:
                        Update();
                        break;

                    case State.Start:
                        Started();
                        break;
                }
            }
        }

        private void Stoped()
        {
            if (bleStatus == BluetoothLEAdvertisementPublisherStatus.Stopped)
            {
                lock (_lock)
                {
                    _state = State.Update;
                }
            }
        }

        private void Started()
        {
            if(bleStatus == BluetoothLEAdvertisementPublisherStatus.Created || bleStatus == BluetoothLEAdvertisementPublisherStatus.Stopped)
            {
                lock (_lock)
                {
                    _state = State.None;
                }
                _publisher.Start();
            }
        }

        private void Update()
        {
            try
            {
                var manufacturerData = new BluetoothLEManufacturerData();

                manufacturerData.CompanyId = 0x4151;

                var writer = new DataWriter();
                byte[] data = new byte[]
                {
                    //magic code
                    0x40,
                    
                    //action
                    0x60,

                    //IMEI
                    ((byte)(0xFF & (_imei >> 24))), ((byte)(0xFF & (_imei >> 16))), ((byte)(0xFF & (_imei >> 8))), ((byte)(0xFF & (_imei >> 0)))
                };
                
                writer.WriteBytes(data);

                //checkSum
                ushort crc = CRC16.ComputeChecksumInComp(manufacturerData.CompanyId, data, data.Length);
                ushort Rcrc = (ushort)((0xFF00 & (crc << 8)) |(0xFF & (crc >> 8)));

                //writer.WriteUInt16((ushort)Rcrc);
                writer.WriteUInt16((ushort)crc);

                manufacturerData.Data = writer.DetachBuffer();
                _publisher.Advertisement.ManufacturerData.Add(manufacturerData);
                lock (_lock)
                {
                    _state = State.Start;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }

        }

    }
}
