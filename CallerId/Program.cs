using System;
using System.IO.Ports;
using System.Threading;

namespace CallerId
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("The following serial ports were found:");

            var retries = 100000;
            var portSelected = -1;
            while (true)
            {
                retries--;
                if (retries <= 0) break;

                // Get a list of serial port names.
                string[] ports = SerialPort.GetPortNames();
                string portName = null;

                // Display each port name to the console.
                var portCount = 0;
                foreach (string port in ports)
                {
                    portSelected++;
                    if (portSelected == ports.Length) portSelected = 0;

                    if (portCount == portSelected)
                    {
                        portName = port;
                        Console.WriteLine(port);
                        break;
                    }
                    portCount++;
                }

                if (portName == null) continue;

                try
                {
                    using (var serialPort = new SerialPort(portName)
                    {
                        BaudRate = 19200,
                        Parity = Parity.None,
                        DataBits = 8,
                        StopBits = StopBits.One,
                        Handshake = Handshake.RequestToSend,
                        WriteBufferSize = 1024,
                        ReadBufferSize = 1024,
                        ReadTimeout = 1000
                    })
                    {
                        serialPort.Open();

                        var ready = false;
                        var initialized = false;

                        if (serialPort.DsrHolding)
                        {
                            Console.WriteLine("DSR");

                            if (serialPort.CtsHolding)
                            {
                                Console.WriteLine("CTS");
                                ready = true;
                            }
                        }

                        if (ready)
                        {
                            serialPort.DtrEnable = true;

                            while (serialPort.DsrHolding)
                            {
                                if (!initialized && serialPort.CtsHolding)
                                {
                                    serialPort.Write("ATZ\r");
                                    serialPort.Write("ATE1I0+VCID=1\r");
                                    initialized = true;
                                }

                                try
                                {
                                    var data = serialPort.ReadLine();
                                    Console.WriteLine(data);
                                }
                                catch (TimeoutException)
                                {
                                    // ignore timeout
                                }
                            }

                            Console.WriteLine("--DSR--");
                        }
                    }
                }
                catch (Exception)
                {
                    // IOException -- unable to open port
                }
            }

            Console.ReadLine();
        }
    }
}
