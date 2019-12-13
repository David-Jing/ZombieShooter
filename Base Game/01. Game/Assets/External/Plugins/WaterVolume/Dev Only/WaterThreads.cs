using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;


namespace WaterVolume
{

    public class WaterThreads
    {
        private Thread thread;
        private bool running = true;
       
        public WaterVolume water;

        int threadCount = -1;
        int bucketIndex = 0;
        public List<bool> triggers = new List<bool>();

        private List<List<ItemInWater>> items = new List<List<ItemInWater>>();
        private List<Thread> threads = new List<Thread>();

        Dictionary<int, ItemInWater>.Enumerator itemsEnumerator;
        ManualResetEvent[] threadCompleteEvents;

        public void Add(ItemInWater item)
        {
            if (bucketIndex >= items.Count)
            {
                bucketIndex = 0;
            }
            items[bucketIndex].Add(item);
            bucketIndex++;
        }

        public void Clear()
        {
            for (int i = 0; i < items.Count; i++)
            {
                items[i].Clear();
            }
        }

        public void Process()
        {
            for (int i = 0; i < items.Count; i++)
            {
                triggers[i] = true;
            }
        }

       
        void InitializeThreads()
        {
            threadCount = water.threadCount;
            items.Clear();
            triggers.Clear();

            foreach (Thread t in threads)
            {
                if (t.IsAlive)
                {
                    t.Abort();
                }
            }
            threads.Clear();
            for (int i = 0; i < threadCount; i++)
            {
                items.Add(new List<ItemInWater>());
                triggers.Add(false);
                Thread thread = new Thread(new ParameterizedThreadStart(Run));
                thread.Start((object)i);
                threads.Add(thread);
            }

            if (threadCompleteEvents == null || threadCompleteEvents.Length != threadCount)
            {
                threadCompleteEvents = new ManualResetEvent[threadCount];
                for (int i = 0; i < threadCount; i++)
                {
                    threadCompleteEvents[i] = new ManualResetEvent(false);
                }
            }

        }

        public void DoFixedUpdate()
        {

            if (threadCount != water.threadCount)
            {
                InitializeThreads();
            }
           
            for (int i = 0; i < threadCount; i++)
            {
                threadCompleteEvents[i].Reset();
            }


            itemsEnumerator = water.itemsInVolume.GetEnumerator();


            Clear();
            while (itemsEnumerator.MoveNext())
            {
                itemsEnumerator.Current.Value.PrepareForThreadsafeFixedUpdate();
                Add(itemsEnumerator.Current.Value);
            }
            Process();
            WaitHandle.WaitAll(threadCompleteEvents);

            water.ForceLoop();
        }

        private void Run(object data)
        {
            int bucket = System.Convert.ToInt32(data);
            while (running)
            {

                if (triggers[bucket])
                {
                    lock (items[bucket])
                    {
                        for (int i = 0; i < items[bucket].Count; i++)
                        {
                            if (items[bucket][i] != null)
                            {
                                items[bucket][i].FixedUpdate_TS();
                            }
                        }
                    }
                    triggers[bucket] = false;
                    lock (threadCompleteEvents)
                    {
                        threadCompleteEvents[bucket].Set();
                    }

                }
                Thread.Sleep(1);
                
            }
        }

        public void OnDisable()
        {
            running = false;
        }

    }
}
