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

        static readonly Semaphore s = new Semaphore(2, 2);

        static Random rnd = new Random();

        static int[] selectedRoutes = new int[10];
        static List<int> routesList = new List<int>();
        static Thread[] trucks = new Thread[10];

        static int counter = 0;
        static int count = 0;

        static void Main(string[] args)
        {
            Thread route = new Thread(() => Routes());
            Thread manager = new Thread(() => RouteSelection());

            route.Start();
            manager.Start();

            route.Join();
            manager.Join();

            for (int i = 0; i < selectedRoutes.Length; i++)
            {
                Thread t = new Thread(() => TruckLoad());
                t.Name = "Truck_" + (i + 1);
                trucks[i] = t;
                t.Start();                
            }

            
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
                    Monitor.Wait(locker, rnd.Next(3000));
                }

                routesList = routesList.Distinct().ToList();
                routesList.Sort();

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
                Console.WriteLine("\n");
            }
        }

        static void TruckLoad()
        {
            int loadingTime = 0;

            while (count < 10)
            {                
                s.WaitOne();                

                Console.WriteLine("{0} is loading...", Thread.CurrentThread.Name);

                count++;

                loadingTime = rnd.Next(500, 5000);

                Thread.Sleep(loadingTime);
                
                s.Release();

                Console.WriteLine("{0} loaded for {1} ms", Thread.CurrentThread.Name, loadingTime);
            }            

            Console.WriteLine("{0} route is: {1}", Thread.CurrentThread.Name, selectedRoutes[counter]);
            counter++;

            int deliveryTime = rnd.Next(500, 5000);

            Console.WriteLine("\n{0} started, delivery expected for {1} ms", Thread.CurrentThread.Name, deliveryTime);

            if (deliveryTime > 3000)
            {
                Console.WriteLine("Delivery expired, {0} is returning for 3000 ms", Thread.CurrentThread.Name);
                Thread.Sleep(3000);
                int unloading = loadingTime / (3/2);

                Console.WriteLine("{0} unloaded for {1} ms", Thread.CurrentThread.Name, unloading);

                Thread.Sleep(unloading);
            }
            else
            {
                Console.WriteLine("{0} successfully delivered the load.", Thread.CurrentThread.Name);
            }
        }
    }
}
