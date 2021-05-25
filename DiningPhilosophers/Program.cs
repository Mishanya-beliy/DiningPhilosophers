using System;
using System.Threading;
using System.Threading.Tasks;

namespace DiningPhilosophers
{
    class Program
    {
        private static Random rnd = new();

        private static int[] forkFlag = new int[5]; // 0 - free
        private static object[] fork = new object[5];

        private static void Philosoph(int id)
        {
            //Philosoph think
            Thread.Sleep(rnd.Next(5));

            var idLeftFork = id >= fork.Length - 1 ? 0 : id;
            var idRightFork = id <= 0 ? fork.Length - 1 : id - 1;

            //Check for free left fork
            while(Interlocked.CompareExchange(ref forkFlag[idLeftFork], 1, 0) != 0)
                Thread.Sleep(1);

            //Some action at first fork
            lock (fork[idLeftFork])
            {
                Thread.Sleep(1000);  //Wait
            }

            //Check right fork
            if (Interlocked.CompareExchange(ref forkFlag[idRightFork], 1, 0) != 0)
            {
                //Unlock left fork
                Interlocked.Decrement(ref forkFlag[idLeftFork]);
                
                //Wait for two fork
                while (true)
                    if(Interlocked.CompareExchange(ref forkFlag[idLeftFork], 1, 0) == 0)
                        if(Interlocked.CompareExchange(ref forkFlag[idRightFork], 1, 0) == 0)
                            break;
                        else
                            Interlocked.Decrement(ref forkFlag[idLeftFork]);

            }

            //Eating
            lock (fork[idLeftFork])
            {
                lock (fork[idRightFork])
                {
                    Console.WriteLine($" Philosoph[{id}] eating");
                    Thread.Sleep(1000);                     //Eat
                }
                Interlocked.Decrement(ref forkFlag[id]);
            }
            Interlocked.Decrement(ref forkFlag[idLeftFork]);
            Console.WriteLine($" Philosoph[{id}] stop eating");

        }

        //private static bool[] fork = new bool[5]; //false значит свободна
        //private static void Philosoph(int id)
        //{
        //    Thread.Sleep(rnd.Next(5));

        //    var idLeftFork = id  >= fork.Length - 1 ? 0 : id;
        //    var idRightFork = id <= 0 ? fork.Length - 1 : id - 1;

        //    while (fork[idLeftFork] || fork[idRightFork])
        //        Thread.Sleep(1);

        //    fork[idLeftFork] = true;
        //    fork[idRightFork] = true;

        //    Console.WriteLine($" Philosoph[{id}] eating");
        //    Thread.Sleep(100);

        //    fork[idRightFork] = false;
        //    fork[idLeftFork] = false;
        //    Console.WriteLine($" Philosoph[{id}] stop eating");
        //} 

        static void Main(string[] args)
        {
            for (int i = 0; i < 5; i++)
                fork[i] = new();

            Task[] philosophs = new Task[5];
            while(true)
            {
                for (int i = 0; i < 5; i++)
                {
                    var i1 = i;
                    philosophs[i] = Task.Run(() => Philosoph(i1));
                }
            }
        }
    }
}