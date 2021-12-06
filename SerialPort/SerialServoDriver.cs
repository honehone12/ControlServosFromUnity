using System;
using System.Threading;
using System.IO.Ports;
using UnityEngine;

namespace UnitySerialPort
{
    public class SerialServoDriver : MonoBehaviour
    {
        [SerializeField]
        private string portName = "COM@";
        [SerializeField]
        private uint baudRate = 115200;
        [SerializeField]
        private byte maxFalseCount = 255;

        private SerialPort serialPort;
        // need 2 threads ??
        private Thread portThreadRead;
        private Thread portThreadWrite;
        private readonly byte[] bytesBuffer = new byte[8];
        private readonly float[] recievedFeedback = new float[2];
        private readonly byte[] writingCommand = new byte[2];

        private const int BYTE_TYPE = 0x00;
        private const int FLOAT_TYPE = 0x01;
        private const int MINIMUM_COMMUNICATION_LEN = 20;
        private static readonly byte[] Header =
        {
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff
        };
        private static readonly byte[] DataDescription = 
        {
                0x02, 0x01, 0x02, BYTE_TYPE
        };

        public bool IsInitialized { get; private set; }
        public bool RunningThreadFLG { get; private set; }
        public bool WriteFLG { get; private set; }
        public byte[] RequestWritingBuffer => writingCommand;
        public float[] RequestFeedbackData => recievedFeedback;

        public void Write()
        {
            WriteFLG = true;
        }

        public void PrepareExit()
        {
            if (serialPort == null)
            {
                return;
            }

            writingCommand[0] = 90;
            writingCommand[1] = 90;
            try
            {
                serialPort.Write(
                    Header,
                    0,
                    8
                );

                serialPort.Write(
                    DataDescription,
                    0,
                    4
                );

                serialPort.Write(
                    writingCommand,
                    0,
                    2
                );

                WriteFLG = false;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        private void UniPortThreadRead()
        {
            while (RunningThreadFLG)
            {
                if(serialPort.ReadBufferSize >= MINIMUM_COMMUNICATION_LEN)
                {
                    try
                    {
                        byte headerCount = 0;
                        byte falseCount = 0;
                        while (headerCount < 8)
                        {
                            if (serialPort.ReadByte() == 0xff)
                            {
                                headerCount++;
                                //Debug.Log("Recieved header.");
                            }
                            else
                            {
                                falseCount++;
                                headerCount = 0;

                                if (falseCount >= maxFalseCount)
                                {
                                    throw new Exception("Failed to fined Header. Exeeded max false count.");
                                }
                            }
                        }

                        int byteLen = serialPort.ReadByte();
                        int dataSize = serialPort.ReadByte();
                        int dataLen = serialPort.ReadByte();
                        int dataType = serialPort.ReadByte();

                        if (headerCount == 8 && dataType == FLOAT_TYPE)
                        {
                            //Debug.LogFormat("Recieved data description. {0}, {1}, {2}, {3},", byteLen, dataSize, dataLen, dataType);
                            serialPort.Read(
                                bytesBuffer,
                                0,
                                byteLen
                            );

                            for (byte i = 0; i < dataLen; i++)
                            {
                                float read = BitConverter.ToSingle(
                                    bytesBuffer,
                                    i * dataSize
                                );
                                if(!float.IsNaN(read) && !float.IsInfinity(read))
                                {
                                    recievedFeedback[i] = read;
                                }
                            }

                            //Debug.LogFormat("Recieved {0}, {1}", recievedFeedback[0], recievedFeedback[1]);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                    }
                }
            }
        }

        private void UniPortThreadWrite()
        {
            while(RunningThreadFLG)
            {
                if(WriteFLG)
                {
                    try
                    {
                        serialPort.Write(
                            Header,
                            0,
                            8
                        );

                        serialPort.Write(
                            DataDescription,
                            0,
                            4
                        );

                        serialPort.Write(
                            writingCommand,
                            0,
                            2
                        );

                        //Debug.LogFormat("Port written {0} {1}", writingCommand[0], writingCommand[1]);

                        WriteFLG = false;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                    }
                }
            }
        }

        private void Start()
        {
            if(!IsInitialized)
            {                
                serialPort = new SerialPort(
                    portName,
                    (int)baudRate,
                    Parity.None
                );
                serialPort.Open();

                RunningThreadFLG = true;
                portThreadRead = new Thread(new ThreadStart(UniPortThreadRead));
                portThreadRead.Start();
                portThreadWrite = new Thread(new ThreadStart(UniPortThreadWrite));
                portThreadWrite.Start();

                IsInitialized = true;
            }
        }

        private void OnDisable()
        {
            PrepareExit();
        }

        private void OnDestroy()
        {
            RunningThreadFLG = false;
            if(IsInitialized && portThreadRead.IsAlive)
            {
                portThreadRead?.Join();
            }

            if(IsInitialized && portThreadRead.IsAlive)
            {
                portThreadWrite?.Join();
            }

            if(IsInitialized && serialPort.IsOpen)
            {
                serialPort?.Close();
                serialPort?.Dispose();
            }
        }
    }
}
