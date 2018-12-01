using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace UART_reader
{
    public class Motion
    {
        int[] x;
        int[] y;
        int[] z;
        bool type;
        bool mentve;
        //public static int[] x { get; set; }
        //public static int[] y { get; set; }
        //public static int[] z { get; set; }       
        public string name { get; set; }
        public int[] X { get => x; set => x = value; }
        public int[] Y { get => y; set => y = value; }
        public int[] Z { get => z; set => z = value; }
        public bool Type { get => type; set => type = value; }

        public Motion(string[] acceldata,string name,int avgZ)
        {
            x = new int[acceldata.Length];
            y = new int[acceldata.Length];
            z = new int[acceldata.Length];
            this.name = name;
            AccelerationToIntBlock(acceldata,avgZ);           
            //AccelerationToFile(name+".txt",acceldata);
        }

        public Motion(string[] acceldata,int avgZ)
        {
            x = new int[acceldata.Length];
            y = new int[acceldata.Length];
            z = new int[acceldata.Length];
            AccelerationToIntBlock(acceldata,avgZ);           

        }

        public Motion(int[] x, int[] y, int[] z, bool type,string name)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.type = type;
            this.name = name;
            mentve = false;
        }



        public void AccelerationToIntBlock(string[] acceldata, int avgZ)
        {
            int[] all = new int[3];                     

            for (int i = 0; i < acceldata.Length; i++)
            {
                all = AccelerationStringToInt(acceldata[i]);
                x[i] = all[0];
                y[i] = all[1];
                z[i] = all[2]-avgZ;
            }        

        }

        static int[] AccelerationStringToInt(string message)
        {
            int[] accel = new int[3];
            string[] accel_datas = new string[3];

            accel_datas = message.Split(';');
            accel[0] = int.Parse(accel_datas[0]);
            accel[1] = int.Parse(accel_datas[1]);
            accel[2] = int.Parse(accel_datas[2]);

            return accel;

        }

        static void AccelerationToFile(string name,string[] acceldata)
        {
            StreamWriter sw = new StreamWriter(name,true);
            foreach (string s in acceldata)
            {
                sw.WriteLine(s);
            }

            sw.Close();
                   
        }

        public void Mentes()
        {
            if (mentve != true)
            {
                StreamWriter sw = new StreamWriter(name+".txt", true);
                for (int i = 0; i < x.Length; i++)
                {
                    sw.WriteLine(x[i].ToString() + ";" + y[i].ToString() + ";" + z[i].ToString());
                }
                sw.Close();
            }
        }
       
    }
}
