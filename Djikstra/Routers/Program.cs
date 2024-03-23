using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Routers
{
    class Program
    {
        public static void Main(string[] args)
        {
            List<Switch> switches = new List<Switch>();
            List<Computer> computers = new List<Computer>();

            Console.WriteLine("Construction du réseau...");
            switches.Add(new Switch(new IpAddress("192.168.1.1")));
            switches.Add(new Switch(new IpAddress("192.168.4.1")));
            switches.Add(new Switch(new IpAddress("192.168.12.1")));
            switches.Add(new Switch(new IpAddress("192.168.11.1")));
            switches.Add(new Switch(new IpAddress("192.168.6.1")));
            switches.Add(new Switch(new IpAddress("240.27.71.1")));
            switches.Add(new Switch(new IpAddress("11.44.22.1")));
            switches.Add(new Switch(new IpAddress("1.1.1.1")));
            switches.Add(new Switch(new IpAddress("18.16.69.1")));
            switches.Add(new Switch(new IpAddress("194.200.73.1")));
            switches.Add(new Switch(new IpAddress("12.12.6.1")));
            //Maillage
            int[] maillage =
            {
                0, 1, 24,
                1, 2, 31,
                1, 5, 17,
                2, 5, 8,
                2, 8, 51,
                3, 5, 28,
                3, 6, 14,
                3, 7, 30,
                5, 6, 12,
                5, 10, 17,
                6, 7, 10,
                7, 8, 11,
                7, 10, 4,
                8, 9, 30,
                8, 10, 21,
            };

            int curIndex = 0;
            while (curIndex < maillage.Length)
            {
                switches[maillage[curIndex]].ConnectSwitch(switches[maillage[curIndex + 1]], maillage[curIndex + 2]);
                curIndex += 3;
            }
            //PC
            for (int i = 0; i < 50; ++i)
            {
                Computer computer = new Computer();
                computers.Add(computer);
                switches[i % switches.Count].ConnectPC(computer);
            }

            Console.WriteLine("Création des tables d'addressage...");
            foreach (Switch sw in switches)
            {
                //Fonction manquante....
                sw.FillRouteTable(switches);
            }

            Console.WriteLine("Envoi de messages...");
            Random random = new Random();
            for (int i = 0; i < 25; ++i)
            {
                try
                {
                    int source = random.Next(computers.Count);
                    int dest = random.Next(computers.Count);
                    Console.WriteLine("Message de {0} à {1}", computers[source].GetIpAddress().GetStringAddress(), computers[dest].GetIpAddress().GetStringAddress());
                    computers[source].SendMessage(computers[dest], "coucou " + i);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            Console.ReadKey();
        }

    }
}
