using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;
class WorkerPool
{

    private ManualResetEvent[] doneEvents;
    private Worker[] workers;
    private int numWorker;
    private int numberOfAgents=0;
    internal IList<GameAgent> gameAgents;
    private static WorkerPool instance = null;
    public static WorkerPool getInstance() {
        if (instance == null)
        {
            instance = new WorkerPool();
        }
        return instance;
    }
    private WorkerPool()
    {
        
        int completionPorts;
        ThreadPool.GetMinThreads(out this.numWorker, out completionPorts);
        Debug.Log("Number of worker " + numWorker);
        workers = new Worker[numWorker];
        
        doneEvents = new ManualResetEvent[workers.Length];
       for (int block = 0; block < workers.Length; ++block)
            {
                doneEvents[block] = new ManualResetEvent(false);
                workers[block] = new Worker(0,0,doneEvents[block]);
            }
    }
 
  
    private class Worker
    {
        private ManualResetEvent doneEvent;
        private int end;
        private int start;
        private WaitCallback waitCallback;
        /**
         * <summary>Constructs and initializes a worker.</summary>
         *
         * <param name="start">Start.</param>
         * <param name="end">End.</param>
         * <param name="doneEvent">Done event.</param>
         */
        internal Worker(int start, int end, ManualResetEvent doneEvent)
        {
            this.start = start;
            this.end = end;
            this.doneEvent = doneEvent;
        }

        internal void config(int start, int end)
        {
            this.start = start;
            this.end = end;
        }

        internal void work(object obj) {

            
     
            for (int i = start; i < end; i++)
            {
                WorkerPool.getInstance().gameAgents[i].ReplanPath();
                
            }
           
     
            doneEvent.Set();
        }

    }

    internal void doStep(IList<GameAgent> listAgent)
    {
        if (this.gameAgents==null || listAgent.Count != this.gameAgents.Count)
        {
            this.gameAgents = listAgent;
        }
        if (workers == null)
        {
            workers = new Worker[numWorker];
            doneEvents = new ManualResetEvent[workers.Length];
            //workerAgentCount = getNumAgents();
            numberOfAgents = gameAgents.Count;
            for (int block = 0; block < workers.Length; ++block)
            {
                doneEvents[block] = new ManualResetEvent(false);
                workers[block] = new Worker(block * gameAgents.Count / workers.Length, (block + 1) * gameAgents.Count / workers.Length, doneEvents[block]);
            }
        }

        if (numberOfAgents != gameAgents.Count)
        {
            numberOfAgents = gameAgents.Count;
            for (int block = 0; block < workers.Length; ++block)
            {
                workers[block].config(block * gameAgents.Count / workers.Length, (block + 1) * gameAgents.Count / workers.Length);
            }
        }

        for (int i = 0; i < workers.Length; i++)
        {
            doneEvents[i].Reset();
            ThreadPool.QueueUserWorkItem(workers[i].work);
        }

        WaitHandle.WaitAll(doneEvents);
        
    }
}
