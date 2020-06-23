using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Zadatak_1
{
    class Program
    {
        static readonly object locker = new object();

        static Random rnd = new Random();

        static List<int> routesList = new List<int>();

        static void Main(string[] args)
        {
            Thread route = new Thread(() => Routes());
            Thread manager = new Thread(() => RouteSelection());

            route.Start();
            manager.Start();

            Console.ReadLine();
        }

        static void Routes()
        {
            try
            {
                using (StreamWriter sw = new StreamWriter("../../Routes.txt"))
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        sw.WriteLine(rnd.Next(1, 5000));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            lock (locker)
            {
                try
                {
                    using (StreamReader sr = new StreamReader("../../Routes.txt"))
                    {
                        string line;

                        while ((line = sr.ReadLine()) != null)
                        {
                            routesList.Add(Convert.ToInt32(line));
                        }
                    }

                    Monitor.Pulse(locker);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        static void RouteSelection()
        {
            lock (locker)
            {
                while (routesList.Count < 1000)
                {
                    Monitor.Wait(locker);
                }

                routesList.Sort();

                int[] selectedRoutes = new int[10];
                int index = 0;

                for (int i = 0; i < routesList.Count; i++)
                {
                    if (routesList[i] % 3 == 0)
                    {
                        selectedRoutes[index] = routesList[i];
                        index++;

                        if (index == 10)
                        {
                            break;
                        }
                    }
                }

                Console.WriteLine("Selected routes are: ");

                for (int i = 0; i < selectedRoutes.Length; i++)
                {
                    Console.Write(selectedRoutes[i] + " ");
                }
            }            
        }
    }
}
