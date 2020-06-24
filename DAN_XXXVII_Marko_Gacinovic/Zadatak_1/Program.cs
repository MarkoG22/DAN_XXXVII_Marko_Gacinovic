using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Zadatak_1
{
    class Program
    {
        // object for locking threads
        static readonly object locker = new object(); 

        static Random rnd = new Random();

        // array for selected truck routes
        static int[] selectedRoutes = new int[10];

        // list of routes for reading from the file
        static List<int> routesList = new List<int>();

        // array of loading times for calculation unloading times
        static int[] loadingTimes = new int[10];

        // counter for giving routes to the trucks
        static int counter = 0;

        static void Main(string[] args)
        {
            // thread for writing and reading routes from the file
            Thread route = new Thread(() => Routes());

            // thread for route selection
            Thread manager = new Thread(() => RouteSelection());

            // starting and joining the threads
            route.Start();
            manager.Start();

            route.Join();
            manager.Join();

            // variable for getting the name in the for loop
            int a = 0;
            // variable for getting the loading time in the for loop
            int b = -1;

            // loop for creating the threads for loading
            for (int i = 0; i < 5; i++)
            {
                a++;
                b++;

                // random loading times for the threads
                int time1 = rnd.Next(500, 5000);
                int time2 = rnd.Next(500, 5000);

                // creating the threads for loading
                Thread t = new Thread(() => TruckLoad(time1));
                Thread t2 = new Thread(() => TruckLoad(time2));
                t.Name = "Truck_" + (a);
                t2.Name = "Truck_" + (++a);

                loadingTimes[b] = time1;
                b++;
                loadingTimes[b] = time2;
                
                // starting and joining the threads for loading
                t.Start();
                t2.Start();

                t.Join();
                t2.Join();                
            }            

            // loop for creating the threads for unloading
            for (int i = 0; i < 10; i++)
            {
                int unload = loadingTimes[i];

                // creating and starting the thread for truck's unloading
                Thread t3 = new Thread(() => TruckUnload(unload));
                t3.Name = "Truck_" + (i + 1);
                t3.Start();                
            }
            
            Console.ReadLine();
        }

        /// <summary>
        /// method for writing and reading routes from the file
        /// </summary>
        static void Routes()
        {
            try
            {
                // writing random routes to the file
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
                    // reading routes from the file
                    using (StreamReader sr = new StreamReader("../../Routes.txt"))
                    {
                        string line;

                        while ((line = sr.ReadLine()) != null)
                        {
                            routesList.Add(Convert.ToInt32(line));
                        }
                    }

                    // notifying that routes are in the list
                    Monitor.Pulse(locker);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        /// <summary>
        /// method for routes selection
        /// </summary>
        static void RouteSelection()
        {
            lock (locker)
            {
                while (routesList.Count < 1000)
                {
                    // waiting until routes are read from the file
                    Monitor.Wait(locker, rnd.Next(3000));
                }

                // deleting duplicate routes and sorting the routes list
                routesList = routesList.Distinct().ToList();
                routesList.Sort();

                int index = 0;

                // loop for taking the smallest numbers divisible with 3
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

                // loop for displayin selected routes
                for (int i = 0; i < selectedRoutes.Length; i++)
                {
                    Console.Write(selectedRoutes[i] + " ");
                }
                Console.WriteLine("\n");
            }
        }

        /// <summary>
        ///  method for loading trucks
        /// </summary>
        /// <param name="time"></param>
        static void TruckLoad(int time)
        {      
            Console.WriteLine("{0} is loading...", Thread.CurrentThread.Name);
            
            Thread.Sleep(time);            

            Console.WriteLine("{0} loaded for {1} ms", Thread.CurrentThread.Name, time);
        }

        /// <summary>
        ///  method for unloading trucks
        /// </summary>
        /// <param name="time"></param>
        static void TruckUnload(int time)
        {
            // lock for giving each route to the truck
            lock (locker)
            {
                Console.WriteLine("\n{0} route is: {1}", Thread.CurrentThread.Name, selectedRoutes[counter]);
                counter++;
            }

            int deliveryTime = rnd.Next(500, 5000);

            Console.WriteLine("{0} started, delivery expected for {1} ms", Thread.CurrentThread.Name, deliveryTime);
            Thread.Sleep(deliveryTime);

            // checking if the load is delivered
            if (deliveryTime > 3000)
            {
                Console.WriteLine("Delivery expired, {0} is returning to starting point for 3000 ms", Thread.CurrentThread.Name);                
            }
            else
            {
                // unloading time
                double unloading = time / 1.5;

                Console.WriteLine("{0} successfully delivered the load. Unloading time: {1:0.00} ms", Thread.CurrentThread.Name, unloading);

                Thread.Sleep((int)unloading);
            }
        }
    }
}
