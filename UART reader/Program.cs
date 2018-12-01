using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;
using UART_reader.DTW;
using System.IO;


namespace UART_reader
{
    public class PortChat
    {
        static bool _continue;
        static SerialPort _serialPort;
        static int[] minmax;
        static int avgZ;
        static List<Motion> motions = new List<Motion>();
        const int samples = 5;
        const int accelsamples = 25;


        
        

        public static void Main()
        {
            int[] x = new int[] { 142, 205, 147, 53, 8, -55, -87, -131, -105, -94 };
            int[] y = new int[] { -154, -128, -72, -14, 28, 50, 82, 100, 89, 61 };
            SimpleDTW dtw = new SimpleDTW(x, y);
            dtw.computeDTW();
            double[,] f = dtw.getFMatrix();
                
            
            Beolvas();
            string message;
            StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
            //Thread readThread = new Thread(Read);
            
            Task recogtask = new Task(Felismer);
            Task trainTask = new Task(Train);
            
            
            //Thread nyugalmithread = new Thread(Nyugalmi);
            
           

            
            _serialPort = new SerialPort();

            
            _serialPort.PortName = SetPortName(_serialPort.PortName);
            _serialPort.BaudRate = SetPortBaudRate(_serialPort.BaudRate);           

           
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;

            _serialPort.Open();

            _continue = true;

            Console.WriteLine("Kis türelmet, a nyugalmi helyzetet mérjük fel.");
            Nyugalmi();

            
            //readThread.Start();
            //nyugalmithread.Start();
                                 

            Console.WriteLine("Type QUIT to exit");
            Console.WriteLine("Type TRAIN to learn a motion ");
            Console.WriteLine("Type RECOG to start recognizing motions ");

            while (_continue)
            {
                message = Console.ReadLine();

                if (stringComparer.Equals("quit", message))
                {
                    _continue = false;
                }
                if (stringComparer.Equals("train", message))
                { 
                    if(trainTask.IsCompleted)
                    {
                        trainTask = new Task(Train);
                    }
                    trainTask.Start();
                    Task.WaitAll(trainTask);
                                                                             
                    
                }

                if (stringComparer.Equals("RECOG", message))
                {
                    Console.WriteLine("Felismerés folyamatban");
                    if (recogtask.IsCompleted)
                    {
                        recogtask = new Task(Felismer);
                    }                   
                    recogtask.Start();
                }
            }            
            
            _serialPort.Close();
        }

        //public static void Read()
        //{
        //    while (_continue)
        //    {
        //        try
        //        {
        //            string message = _serialPort.ReadLine();
        //            //Console.WriteLine(message);
        //        }
        //        catch (TimeoutException) { }
        //    }


        //}
       

        public static void Nyugalmi()
        {
            string[] acceldatas = new string[50];
            for (int i = 0; i < 50; i++)
            {
                try
                {
                    acceldatas[i] = _serialPort.ReadLine();
                }
                catch (TimeoutException) { }

            }

            NyugalmiHelyzet(acceldatas);
        }

        public static void Felismer()
        {
            _serialPort.DiscardInBuffer();
            int[] acc = new int[3];
            bool train = false;
          

            do
            {
                try
                {                   
                    acc = Feldolgoz(_serialPort.ReadLine());
                }
                catch (System.IO.IOException)
                {
                    train = true;
                }             
               
            } while (Math.Abs(acc[0]) - Math.Abs(minmax[0]) < 40 && Math.Abs(acc[0]) - Math.Abs(minmax[1]) < 40 && Math.Abs(acc[1]) - Math.Abs(minmax[2]) < 40 &&
                 Math.Abs(acc[1]) - Math.Abs(minmax[3]) < 40 && Math.Abs(acc[2]) - Math.Abs(minmax[4]) < 110 && Math.Abs(acc[2]) - Math.Abs(minmax[5]) < 110  && train==false);

            if (train == false)
            {
                string[] acceldatas = new string[25];
                acceldatas[0] = acc[0].ToString() + ";" + acc[1].ToString() + ";" + (acc[2]+avgZ).ToString();

                for (int j = 1; j < 25; j++)
                {
                    try
                    {
                        acceldatas[j] = _serialPort.ReadLine();
                    }
                    catch (TimeoutException) { }
                }

                Motion m = new Motion(acceldatas,avgZ);

                foreach (Motion item in motions)
                {
                    SimpleDTW x = new SimpleDTW(item.X, m.X);
                    SimpleDTW y = new SimpleDTW(item.Y, m.Y);
                    SimpleDTW z = new SimpleDTW(item.Z, m.Z);
                                      
                    x.computeDTW();
                    y.computeDTW();
                    z.computeDTW();
                    double[,] f = x.getFMatrix();

                    if ((x.getSum() + y.getSum() + z.getSum()) < 2000)
                    {

                        Console.WriteLine(item.name);
                    }

                }                
            }

            _serialPort.DiscardInBuffer();


        }

                 

        public static void Train()
        {
            Console.WriteLine("Add meg a mozgás nevét:");
            string motion_name = Console.ReadLine();

            Console.WriteLine("A Tanuláshoz ismételje meg +"+samples.ToString()+"x a mozgás fajtát.");
            List<Motion> minták = new List<Motion>();


            _serialPort.DiscardInBuffer();
            
            for (int i = 0; i < samples; i++)
            {
                
                
                int[] acc = new int[3];              

                do
                {                                    
                    acc = Feldolgoz(_serialPort.ReadLine());
                } while (Math.Abs(acc[0]) - Math.Abs(minmax[0]) < 40 && Math.Abs(acc[0]) - Math.Abs(minmax[1]) < 40 && Math.Abs(acc[1]) - Math.Abs(minmax[2]) < 40 &&
                 Math.Abs(acc[1]) - Math.Abs(minmax[3]) < 40 && Math.Abs(acc[2]) - Math.Abs(minmax[4]) < 40 && Math.Abs(acc[2]) - Math.Abs(minmax[5]) < 40);

                Console.WriteLine("x:" + acc[0].ToString() + "y:" + acc[1].ToString() + "z:" + acc[2].ToString() + "xmin:" + minmax[0].ToString() + "xmax:" + minmax[1].ToString() + "ymin:" + minmax[2].ToString() + "ymax:" + minmax[3].ToString() + "zmin:" + minmax[4].ToString() + "zmax:" + minmax[5].ToString());

                string[] acceldatas = new string[25];
                acceldatas[0] = acc[0].ToString() + ";" + acc[1].ToString() + ";" + (acc[2]+avgZ).ToString();

                for (int j = 1; j < 25; j++)
                {
                    try
                    {
                        acceldatas[j] = _serialPort.ReadLine();
                    }
                    catch (TimeoutException) { }
                }
                minták.Add(new Motion(acceldatas,avgZ));
                


                Console.WriteLine("Az " + i.ToString() + ". tanulás elkészült. Helyezze kezdőállapotba az eszközt majd entert a kezdéshez.");
                Console.ReadLine();
                _serialPort.DiscardInBuffer();



            }

            MotionAvarage(minták, motion_name);
            //motions.Add(new Motion(acceldatas,name));
            // Console.WriteLine("Tanulás vége.");
        }
             

       
        public static string SetPortName(string defaultPortName)
        {
            string portName;

            Console.WriteLine("Available Ports:");
            foreach (string s in SerialPort.GetPortNames())
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter COM port value (Default: {0}): ", defaultPortName);
            portName = Console.ReadLine();

            if (portName == "" || !(portName.ToLower()).StartsWith("com"))
            {
                portName = defaultPortName;
            }
            return portName;
        }
        
        public static int SetPortBaudRate(int defaultPortBaudRate)
        {
            string baudRate;

            Console.Write("Baud Rate(default:{0}): ", defaultPortBaudRate);
            baudRate = Console.ReadLine();

            if (baudRate == "")
            {
                baudRate = defaultPortBaudRate.ToString();
            }

            return int.Parse(baudRate);
        }

       

        public static int[] Feldolgoz(string message)
        {
            int[] accel = new int[3];
            string[] accel_datas = new string[3];

            accel_datas = message.Split(';');
            accel[0] = int.Parse(accel_datas[0]);
            accel[1] = int.Parse(accel_datas[1]);
            accel[2] = int.Parse(accel_datas[2])-avgZ;     

            return accel;


            //int[] accel = new int[3];
            //string[] accel_datas = new string[3];

            //int x = int.Parse(accel_datas[0]);
            //int y = int.Parse(accel_datas[1]);
            //int z = int.Parse(accel_datas[2]) - avgZ;

            //accel_datas = message.Split(';');
            //accel[0] = x <= 40 || x >= -40 ? 0 : x;
            //accel[1] = y <= 40 || y >= -40 ? 0 : y;
            //accel[2] = z <= 40 || z >= -40 ? 0 : z;

            //return accel;

        }

        public static void NyugalmiHelyzet(string[] accel_datas)
        {
            int[] all = new int[3];
            int[] x = new int[accel_datas.Length];
            int[] y = new int[accel_datas.Length];
            int[] z = new int[accel_datas.Length];
            minmax = new int[6];

            for (int i = 0; i < accel_datas.Length; i++)
            {
                all = Feldolgoz(accel_datas[i]);
                x[i] = all[0];
                y[i] = all[1];
                z[i] = all[2];
            }

            avgZ = (int)z.Average();

            minmax[0] = x.Min();
            minmax[1] = x.Max();
            minmax[2] = y.Min();
            minmax[3] = y.Max();
            minmax[4] = z.Min()-avgZ;
            minmax[5] = z.Max()-avgZ;

            Console.WriteLine("avgZ: "+ avgZ.ToString());
            Console.WriteLine("xmin:" + minmax[0].ToString() + " xmax:" + minmax[1].ToString() + " ymin:" + minmax[2].ToString() + " ymax:" + minmax[3].ToString() + " zmin:" + minmax[4].ToString() + " zmax:" + minmax[5].ToString());
        }


        public static void MotionAvarage(List<Motion> mozgások, string name)
        {
            int[] x = new int[accelsamples];
            int[] y = new int[accelsamples];
            int[] z = new int[accelsamples];
            foreach (Motion item in mozgások)
            {
                int[] temp = item.X;
                for (int i = 0; i < temp.Length; i++)
                {
                    x[i] += temp[i];
                }
                temp = item.Y;
                for (int i = 0; i < temp.Length; i++)
                {
                    y[i] += temp[i];
                }
                temp = item.Z;
                for (int i = 0; i < temp.Length; i++)
                {
                    z[i] += temp[i];
                }
            }

            for (int i = 0; i < accelsamples; i++)
            {
                x[i] = x[i] / samples;
                y[i] = y[i] / samples;
                z[i] = z[i] / samples;
            }

            Console.WriteLine("Adja meg, hogy ártalmas (YES) vagy ártalmatlan (NO) a mozgás. ");
            string type = Console.ReadLine();
            bool motion_type;
            
            StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
            if (stringComparer.Equals("yes", type)){

                motion_type = true;
            }
            if (stringComparer.Equals("no", type))
            {

                motion_type = false;
            }
            else
            {
                motion_type = false;
            }

            Motion newMotion = new Motion(x, y, z, motion_type,name);
            

            Console.WriteLine("Menti a mozgást?(YES,NO)");
            string save = Console.ReadLine();

            if (stringComparer.Equals("yes", save))
            {
                newMotion.Mentes();
            }
            else { }
            
            motions.Add(newMotion);
            
        }
        
        public static void MozgásokKilistázása()
        {
            foreach (Motion item in motions)
            {
                Console.WriteLine(item.name+"/r/n");
            }
        }  
        
        public static void Beolvas()
        {
            StreamReader sr;
            string[] files = Directory.GetFiles("./", "*txt");
            string[] lines = new string[accelsamples];
            foreach (string s in files)
            {
                sr = new StreamReader(s);
                int i = 0;
                while (!sr.EndOfStream)
                {
                    lines[i] = sr.ReadLine();
                    i++;
                }
                string name = s.Remove(s.Length - 4, 4);
                motions.Add(new Motion(lines, name.Remove(0, 2),avgZ));
            }
            
        }


    }


}

