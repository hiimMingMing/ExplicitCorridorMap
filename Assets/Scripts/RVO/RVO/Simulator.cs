/*
 * Simulator.cs
 * RVO2 Library C#
 *
 * Copyright 2008 University of North Carolina at Chapel Hill
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 * Please send all bug reports to <geom@cs.unc.edu>.
 *
 * The authors may be contacted via:
 *
 * Jur van den Berg, Stephen J. Guy, Jamie Snape, Ming C. Lin, Dinesh Manocha
 * Dept. of Computer Science
 * 201 S. Columbia St.
 * Frederick P. Brooks, Jr. Computer Science Bldg.
 * Chapel Hill, N.C. 27599-3175
 * United States of America
 *
 * <http://gamma.cs.unc.edu/RVO2/>
 */

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
namespace RVO
{
    /**
     * <summary>Defines the simulation.</summary>
     */
    public class Simulator
    {
        /**
         * <summary>Defines a worker.</summary>
         */
        private class Worker
        {
            private ManualResetEvent doneEvent;
            private int end;
            private int start;

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

            /**
             * <summary>Performs a simulation step.</summary>
             *
             * <param name="obj">Unused.</param>
             */
            internal void step(object obj)
            {
                for (int index = start; index < end; ++index)
                {
                    Simulator.Instance.agents[index].computeNeighbors();
                    Simulator.Instance.agents[index].computeNewVelocity();
                }
                doneEvent.Set();
            }

            /**
             * <summary>updates the two-dimensional position and
             * two-dimensional velocity of each agent.</summary>
             *
             * <param name="obj">Unused.</param>
             */
            internal void update(object obj)
            {
                for (int index = start; index < end; ++index)
                {
                    Simulator.Instance.agents[index].update();
                }

                doneEvent.Set();
            }
        }
        
        internal IDictionary<int, int> agentNo2indexDict;
     
        internal IList<Agent> agents;
        internal Dictionary<int,Obstacle> obstacles;
        internal int CountObstacle;
        internal KdTree kdTree;
        internal float timeStep;

        private static Simulator instance = new Simulator();

        private Agent defaultAgent;
    
        private ManualResetEvent[] doneEvents;
        private Worker[] workers;
        private int numWorkers;
        private int workerAgentCount;
       

        public static Simulator Instance
        {
            get
            {
                return instance;
            }
        }


        internal  IList<Agent> GetListAgents() {
            return agents;
        }
        public void delAgent(int agentNo)
        {
            agents[agentNo2indexDict[agentNo]].needDelete = true;
        }

        void updateDeleteAgent()
        {
            bool isDelete = false;
            for (int i = agents.Count - 1; i >= 0; i--)
            {
                if (agents[i].needDelete)
                {
                    agents.RemoveAt(i);
                    isDelete = true;
                }
            }
            if (isDelete)
                onDelAgent();
        }

        static int s_totalID = 0;
        /**
         * <summary>Adds a new agent with default properties to the simulation.
         * </summary>
         *
         * <returns>The number of the agent, or -1 when the agent defaults have
         * not been set.</returns>
         *
         * <param name="position">The two-dimensional starting position of this
         * agent.</param>
         */
        public int addAgent(Vector2 position)
        {
            if (defaultAgent == null)
            {
                return -1;
            }

            Agent agent = new Agent();
            agent.id = s_totalID;
            s_totalID++;
            agent.maxNeighbors = defaultAgent.maxNeighbors;
            agent.maxSpeed = defaultAgent.maxSpeed;
            agent.neighborDist = defaultAgent.neighborDist;
            agent.position = position;
            agent.radius = defaultAgent.radius;
            agent.timeHorizonAgent = defaultAgent.timeHorizonAgent;
            agent.timeHorizonObst = defaultAgent.timeHorizonObst;
            agent.velocity = defaultAgent.velocity;
            agents.Add(agent);
            onAddAgent();
            return agent.id;
        }

        void onDelAgent()
        {
            agentNo2indexDict.Clear();
        

            for (int i = 0; i < agents.Count; i++)
            {
                int agentNo = agents[i].id;
                agentNo2indexDict.Add(agentNo, i);
            
            }
        }

        void onAddAgent()
        {
            if (agents.Count == 0)
                return;

            int index = agents.Count - 1;
            int agentNo = agents[index].id;
            agentNo2indexDict.Add(agentNo, index);
         
        }

        /**
         * <summary>Adds a new agent to the simulation.</summary>
         *
         * <returns>The number of the agent.</returns>
         *
         * <param name="position">The two-dimensional starting position of this
         * agent.</param>
         * <param name="neighborDist">The maximum distance (center point to
         * center point) to other agents this agent takes into account in the
         * navigation. The larger this number, the longer the running time of
         * the simulation. If the number is too low, the simulation will not be
         * safe. Must be non-negative.</param>
         * <param name="maxNeighbors">The maximum number of other agents this
         * agent takes into account in the navigation. The larger this number,
         * the longer the running time of the simulation. If the number is too
         * low, the simulation will not be safe.</param>
         * <param name="timeHorizon">The minimal amount of time for which this
         * agent's velocities that are computed by the simulation are safe with
         * respect to other agents. The larger this number, the sooner this
         * agent will respond to the presence of other agents, but the less
         * freedom this agent has in choosing its velocities. Must be positive.
         * </param>
         * <param name="timeHorizonObst">The minimal amount of time for which
         * this agent's velocities that are computed by the simulation are safe
         * with respect to obstacles. The larger this number, the sooner this
         * agent will respond to the presence of obstacles, but the less freedom
         * this agent has in choosing its velocities. Must be positive.</param>
         * <param name="radius">The radius of this agent. Must be non-negative.
         * </param>
         * <param name="maxSpeed">The maximum speed of this agent. Must be
         * non-negative.</param>
         * <param name="velocity">The initial two-dimensional linear velocity of
         * this agent.</param>
         */
        public int addAgent(Vector2 position, float radius, float maxSpeed, float neighborDist, int maxNeighbors, float timeHorizon, float timeHorizonObst,  Vector2 velocity)
        {
            Agent agent = new Agent();
            agent.id = s_totalID;
            s_totalID++;
            agent.maxNeighbors = maxNeighbors;
            agent.maxSpeed = maxSpeed;
            agent.neighborDist = neighborDist;
            agent.position = position;
            agent.radius = radius;
            agent.timeHorizonAgent = timeHorizon;
            agent.timeHorizonObst = timeHorizonObst;
            agent.velocity = velocity;
            agents.Add(agent);
            onAddAgent();
            return agent.id;
        }



        public int addAgent(Vector2 position, float radius, float maxSpeed, float priority)
        {
            Agent agent = new Agent();
            agent.id = s_totalID;
            s_totalID++;
            agent.maxNeighbors = defaultAgent.maxNeighbors;
            agent.maxSpeed =maxSpeed;
            agent.neighborDist = defaultAgent.neighborDist;
            agent.position = position;
            agent.radius =radius;
            agent.timeHorizonAgent = defaultAgent.timeHorizonAgent;
            agent.timeHorizonObst = defaultAgent.timeHorizonObst;
            agent.velocity = defaultAgent.velocity;
            agent.priority = priority;
            agents.Add(agent);
            onAddAgent();
            return agent.id;
        }

        /**
         * <summary>Adds a new obstacle to the simulation.</summary>
         *
         * <returns>The number of the first vertex of the obstacle, or -1 when
         * the number of vertices is less than two.</returns>
         *
         * <param name="vertices">List of the vertices of the polygonal obstacle
         * in counterclockwise order.</param>
         *
         * <remarks>To add a "negative" obstacle, e.g. a bounding polygon around
         * the environment, the vertices should be listed in clockwise order.
         * </remarks>
         */
        public void addObstacle(ExplicitCorridorMap.Obstacle obs)
        {
            var rvoID = addObstacle(obs.ToList());
            obs.RvoID = rvoID;
        }
        private int addObstacle(IList<Vector2> vertices)
        {
            if (vertices.Count < 2)
            {
                return -1;
            }

            int obstacleNo = CountObstacle;

            for (int i = 0; i < vertices.Count; ++i)
            {
                Obstacle obstacle = new Obstacle();
                obstacle.point_ = vertices[i];

                if (i != 0)
                {
                    obstacle.previous_ = obstacles[CountObstacle - 1];
                    obstacle.previous_.next_ = obstacle;
                }

                if (i == vertices.Count - 1)
                {
                    obstacle.next_ = obstacles[obstacleNo];
                    obstacle.next_.previous_ = obstacle;
                }

                obstacle.direction_ = (vertices[(i == vertices.Count - 1 ? 0 : i + 1)] - vertices[i]).normalized;

                if (vertices.Count == 2)
                {
                    obstacle.convex_ = true;
                }
                else
                {
                    obstacle.convex_ = (RVOMath.leftOf(vertices[(i == 0 ? vertices.Count - 1 : i - 1)], vertices[i], vertices[(i == vertices.Count - 1 ? 0 : i + 1)]) >= 0.0f);
                }

                obstacle.id_ = CountObstacle;
                obstacles.Add(CountObstacle,obstacle);
                CountObstacle++;
            }

            return obstacleNo;
        }

        public void deleteObstacle(int id)
        {
            int obsId = id;
            while (obstacles.ContainsKey(obsId))
            {
                var deleteID = obsId;
                obsId = obstacles[obsId].next_.id_;
                obstacles.Remove(deleteID);
            }
        }
        /**
         * <summary>Clears the simulation.</summary>
         */
        public void Clear()
        {
            agents = new List<Agent>();
            agentNo2indexDict = new Dictionary<int, int>();
        
            defaultAgent = null;
            kdTree = new KdTree();
            obstacles = new Dictionary<int, Obstacle>();
          
            timeStep = 0.1f;
            CountObstacle = 0;
            SetNumWorkers(0);
        }

        /**
         * <summary>Performs a simulation step and updates the two-dimensional
         * position and two-dimensional velocity of each agent.</summary>
         *
         * <returns>The global time after the simulation step.</returns>
         */
        public void doStep()
        {
            
            updateDeleteAgent();

            if (workers == null)
            {
                workers = new Worker[numWorkers];
                doneEvents = new ManualResetEvent[workers.Length];
                workerAgentCount = getNumAgents();

                for (int block = 0; block < workers.Length; ++block)
                {
                    doneEvents[block] = new ManualResetEvent(false);
                    workers[block] = new Worker(block * getNumAgents() / workers.Length, (block + 1) * getNumAgents() / workers.Length, doneEvents[block]);
                }
            }

            if (workerAgentCount != getNumAgents())
            {
                workerAgentCount = getNumAgents();
                for (int block = 0; block < workers.Length; ++block)
                {
                    workers[block].config(block * getNumAgents() / workers.Length, (block + 1) * getNumAgents() / workers.Length);
                }
            }

            kdTree.buildAgentTree();
           
            for (int block = 0; block < workers.Length; ++block)
            {
                doneEvents[block].Reset();
                ThreadPool.QueueUserWorkItem(workers[block].step);
            }

            WaitHandle.WaitAll(doneEvents);

            for (int block = 0; block < workers.Length; ++block)
            {
                doneEvents[block].Reset();
                ThreadPool.QueueUserWorkItem(workers[block].update);
            }

            WaitHandle.WaitAll(doneEvents);
       
          
           
          

            return;
        }

        /**
         * <summary>Returns the specified agent neighbor of the specified agent.
         * </summary>
         *
         * <returns>The number of the neighboring agent.</returns>
         *
         * <param name="agentNo">The number of the agent whose agent neighbor is
         * to be retrieved.</param>
         * <param name="neighborNo">The number of the agent neighbor to be
         * retrieved.</param>
         */
        public int getAgentAgentNeighbor(int agentNo, int neighborNo)
        {
            return agents[agentNo2indexDict[agentNo]].agentNeighbors[neighborNo].Value.id;
        }

        /**
         * <summary>Returns the maximum neighbor count of a specified agent.
         * </summary>
         *
         * <returns>The present maximum neighbor count of the agent.</returns>
         *
         * <param name="agentNo">The number of the agent whose maximum neighbor
         * count is to be retrieved.</param>
         */
        public int getAgentMaxNeighbors(int agentNo)
        {
            return agents[agentNo2indexDict[agentNo]].maxNeighbors;
        }

        /**
         * <summary>Returns the maximum speed of a specified agent.</summary>
         *
         * <returns>The present maximum speed of the agent.</returns>
         *
         * <param name="agentNo">The number of the agent whose maximum speed is
         * to be retrieved.</param>
         */
        public float getAgentMaxSpeed(int agentNo)
        {
            return agents[agentNo2indexDict[agentNo]].maxSpeed;
        }

        /**
         * <summary>Returns the maximum neighbor distance of a specified agent.
         * </summary>
         *
         * <returns>The present maximum neighbor distance of the agent.
         * </returns>
         *
         * <param name="agentNo">The number of the agent whose maximum neighbor
         * distance is to be retrieved.</param>
         */
        public float getAgentNeighborDist(int agentNo)
        {
            return agents[agentNo2indexDict[agentNo]].neighborDist;
        }

        /**
         * <summary>Returns the count of agent neighbors taken into account to
         * compute the current velocity for the specified agent.</summary>
         *
         * <returns>The count of agent neighbors taken into account to compute
         * the current velocity for the specified agent.</returns>
         *
         * <param name="agentNo">The number of the agent whose count of agent
         * neighbors is to be retrieved.</param>
         */
        public int getAgentNumAgentNeighbors(int agentNo)
        {
            return agents[agentNo2indexDict[agentNo]].agentNeighbors.Count;
        }

        /**
         * <summary>Returns the count of obstacle neighbors taken into account
         * to compute the current velocity for the specified agent.</summary>
         *
         * <returns>The count of obstacle neighbors taken into account to
         * compute the current velocity for the specified agent.</returns>
         *
         * <param name="agentNo">The number of the agent whose count of obstacle
         * neighbors is to be retrieved.</param>
         */
        public int getAgentNumObstacleNeighbors(int agentNo)
        {
            return agents[agentNo2indexDict[agentNo]].obstacleNeighbors.Count;
        }

        /**
         * <summary>Returns the specified obstacle neighbor of the specified
         * agent.</summary>
         *
         * <returns>The number of the first vertex of the neighboring obstacle
         * edge.</returns>
         *
         * <param name="agentNo">The number of the agent whose obstacle neighbor
         * is to be retrieved.</param>
         * <param name="neighborNo">The number of the obstacle neighbor to be
         * retrieved.</param>
         */
        public int getAgentObstacleNeighbor(int agentNo, int neighborNo)
        {
            return agents[agentNo2indexDict[agentNo]].obstacleNeighbors[neighborNo].Value.id_;
        }

        /**
         * <summary>Returns the ORCA constraints of the specified agent.
         * </summary>
         *
         * <returns>A list of lines representing the ORCA constraints.</returns>
         *
         * <param name="agentNo">The number of the agent whose ORCA constraints
         * are to be retrieved.</param>
         *
         * <remarks>The halfplane to the left of each line is the region of
         * permissible velocities with respect to that ORCA constraint.
         * </remarks>
         */
        public IList<Line> getAgentOrcaLines(int agentNo)
        {
            return agents[agentNo2indexDict[agentNo]].orcaLines_;
        }

        /**
         * <summary>Returns the two-dimensional position of a specified agent.
         * </summary>
         *
         * <returns>The present two-dimensional position of the (center of the)
         * agent.</returns>
         *
         * <param name="agentNo">The number of the agent whose two-dimensional
         * position is to be retrieved.</param>
         */
        public Vector2 getAgentPosition(int agentNo)
        {
            return agents[agentNo2indexDict[agentNo]].position;
        }

        /**
         * <summary>Returns the two-dimensional preferred velocity of a
         * specified agent.</summary>
         *
         * <returns>The present two-dimensional preferred velocity of the agent.
         * </returns>
         *
         * <param name="agentNo">The number of the agent whose two-dimensional
         * preferred velocity is to be retrieved.</param>
         */
        public Vector2 getAgentPrefVelocity(int agentNo)
        {
            return agents[agentNo2indexDict[agentNo]].prefVelocity;
        }

        /**
         * <summary>Returns the radius of a specified agent.</summary>
         *
         * <returns>The present radius of the agent.</returns>
         *
         * <param name="agentNo">The number of the agent whose radius is to be
         * retrieved.</param>
         */
        public float getAgentRadius(int agentNo)
        {
            return agents[agentNo2indexDict[agentNo]].radius;
        }

        /**
         * <summary>Returns the time horizon of a specified agent.</summary>
         *
         * <returns>The present time horizon of the agent.</returns>
         *
         * <param name="agentNo">The number of the agent whose time horizon is
         * to be retrieved.</param>
         */
        public float getAgentTimeHorizon(int agentNo)
        {
            return agents[agentNo2indexDict[agentNo]].timeHorizonAgent;
        }

        /**
         * <summary>Returns the time horizon with respect to obstacles of a
         * specified agent.</summary>
         *
         * <returns>The present time horizon with respect to obstacles of the
         * agent.</returns>
         *
         * <param name="agentNo">The number of the agent whose time horizon with
         * respect to obstacles is to be retrieved.</param>
         */
        public float getAgentTimeHorizonObst(int agentNo)
        {
            return agents[agentNo2indexDict[agentNo]].timeHorizonObst;
        }

        /**
         * <summary>Returns the two-dimensional linear velocity of a specified
         * agent.</summary>
         *
         * <returns>The present two-dimensional linear velocity of the agent.
         * </returns>
         *
         * <param name="agentNo">The number of the agent whose two-dimensional
         * linear velocity is to be retrieved.</param>
         */
        public Vector2 getAgentVelocity(int agentNo)
        {
            return agents[agentNo2indexDict[agentNo]].velocity;
        }

     
        /**
         * <summary>Returns the count of agents in the simulation.</summary>
         *
         * <returns>The count of agents in the simulation.</returns>
         */
        public int getNumAgents()
        {
            return agents.Count;
        }

        /**
         * <summary>Returns the count of obstacle vertices in the simulation.
         * </summary>
         *
         * <returns>The count of obstacle vertices in the simulation.</returns>
         */
        public int getNumObstacleVertices()
        {
            return obstacles.Count;
        }

        /**
         * <summary>Returns the count of workers.</summary>
         *
         * <returns>The count of workers.</returns>
         */
        public int GetNumWorkers()
        {
            return numWorkers;
        }

        /**
         * <summary>Returns the two-dimensional position of a specified obstacle
         * vertex.</summary>
         *
         * <returns>The two-dimensional position of the specified obstacle
         * vertex.</returns>
         *
         * <param name="vertexNo">The number of the obstacle vertex to be
         * retrieved.</param>
         */
        public Vector2 getObstacleVertex(int vertexNo)
        {
            return obstacles[vertexNo].point_;
        }

        /**
         * <summary>Returns the number of the obstacle vertex succeeding the
         * specified obstacle vertex in its polygon.</summary>
         *
         * <returns>The number of the obstacle vertex succeeding the specified
         * obstacle vertex in its polygon.</returns>
         *
         * <param name="vertexNo">The number of the obstacle vertex whose
         * successor is to be retrieved.</param>
         */
        public int getNextObstacleVertexNo(int vertexNo)
        {
            return obstacles[vertexNo].next_.id_;
        }

        /**
         * <summary>Returns the number of the obstacle vertex preceding the
         * specified obstacle vertex in its polygon.</summary>
         *
         * <returns>The number of the obstacle vertex preceding the specified
         * obstacle vertex in its polygon.</returns>
         *
         * <param name="vertexNo">The number of the obstacle vertex whose
         * predecessor is to be retrieved.</param>
         */
        public int getPrevObstacleVertexNo(int vertexNo)
        {
            return obstacles[vertexNo].previous_.id_;
        }

        /**
         * <summary>Returns the time step of the simulation.</summary>
         *
         * <returns>The present time step of the simulation.</returns>
         */
        public float getTimeStep()
        {
            return timeStep;
        }

        /**
         * <summary>Processes the obstacles that have been added so that they
         * are accounted for in the simulation.</summary>
         *
         * <remarks>Obstacles added to the simulation after this function has
         * been called are not accounted for in the simulation.</remarks>
         */
        public void processObstacles()
        {
            kdTree.buildObstacleTree();
        }

        /**
         * <summary>Performs a visibility query between the two specified points
         * with respect to the obstacles.</summary>
         *
         * <returns>A boolean specifying whether the two points are mutually
         * visible. Returns true when the obstacles have not been processed.
         * </returns>
         *
         * <param name="point1">The first point of the query.</param>
         * <param name="point2">The second point of the query.</param>
         * <param name="radius">The minimal distance between the line connecting
         * the two points and the obstacles in order for the points to be
         * mutually visible (optional). Must be non-negative.</param>
         */
        public bool queryVisibility(Vector2 point1, Vector2 point2, float radius)
        {
            return kdTree.queryVisibility(point1, point2, radius);
        }

        public int queryNearAgent(Vector2 point, float radius)
        {
            if (getNumAgents() == 0)
                return -1;
            return kdTree.queryNearAgent(point, radius);
        }

        /**
         * <summary>Sets the default properties for any new agent that is added.
         * </summary>
         *
         * <param name="neighborDist">The default maximum distance (center point
         * to center point) to other agents a new agent takes into account in
         * the navigation. The larger this number, the longer he running time of
         * the simulation. If the number is too low, the simulation will not be
         * safe. Must be non-negative.</param>
         * <param name="maxNeighbors">The default maximum number of other agents
         * a new agent takes into account in the navigation. The larger this
         * number, the longer the running time of the simulation. If the number
         * is too low, the simulation will not be safe.</param>
         * <param name="timeHorizon">The default minimal amount of time for
         * which a new agent's velocities that are computed by the simulation
         * are safe with respect to other agents. The larger this number, the
         * sooner an agent will respond to the presence of other agents, but the
         * less freedom the agent has in choosing its velocities. Must be
         * positive.</param>
         * <param name="timeHorizonObst">The default minimal amount of time for
         * which a new agent's velocities that are computed by the simulation
         * are safe with respect to obstacles. The larger this number, the
         * sooner an agent will respond to the presence of obstacles, but the
         * less freedom the agent has in choosing its velocities. Must be
         * positive.</param>
         * <param name="radius">The default radius of a new agent. Must be
         * non-negative.</param>
         * <param name="maxSpeed">The default maximum speed of a new agent. Must
         * be non-negative.</param>
         * <param name="velocity">The default initial two-dimensional linear
         * velocity of a new agent.</param>
         */
        public void setAgentDefaults(float neighborDist, int maxNeighbors, float timeHorizon, float timeHorizonObst, float radius, float maxSpeed, Vector2 velocity)
        {
            if (defaultAgent == null)
            {
                defaultAgent = new Agent();
            }

            defaultAgent.maxNeighbors = maxNeighbors;
            defaultAgent.maxSpeed = maxSpeed;
            defaultAgent.neighborDist = neighborDist;
            defaultAgent.radius = radius;
            defaultAgent.timeHorizonAgent = timeHorizon;
            defaultAgent.timeHorizonObst = timeHorizonObst;
            defaultAgent.velocity = velocity;
        }

        /**
         * <summary>Sets the maximum neighbor count of a specified agent.
         * </summary>
         *
         * <param name="agentNo">The number of the agent whose maximum neighbor
         * count is to be modified.</param>
         * <param name="maxNeighbors">The replacement maximum neighbor count.
         * </param>
         */
        public void setAgentMaxNeighbors(int agentNo, int maxNeighbors)
        {
            agents[agentNo2indexDict[agentNo]].maxNeighbors = maxNeighbors;
        }

        /**
         * <summary>Sets the maximum speed of a specified agent.</summary>
         *
         * <param name="agentNo">The number of the agent whose maximum speed is
         * to be modified.</param>
         * <param name="maxSpeed">The replacement maximum speed. Must be
         * non-negative.</param>
         */
        public void setAgentMaxSpeed(int agentNo, float maxSpeed)
        {
            agents[agentNo2indexDict[agentNo]].maxSpeed = maxSpeed;
        }

        /**
         * <summary>Sets the maximum neighbor distance of a specified agent.
         * </summary>
         *
         * <param name="agentNo">The number of the agent whose maximum neighbor
         * distance is to be modified.</param>
         * <param name="neighborDist">The replacement maximum neighbor distance.
         * Must be non-negative.</param>
         */
        public void setAgentNeighborDist(int agentNo, float neighborDist)
        {
            agents[agentNo2indexDict[agentNo]].neighborDist = neighborDist;
        }

        /**
         * <summary>Sets the two-dimensional position of a specified agent.
         * </summary>
         *
         * <param name="agentNo">The number of the agent whose two-dimensional
         * position is to be modified.</param>
         * <param name="position">The replacement of the two-dimensional
         * position.</param>
         */
        public void setAgentPosition(int agentNo, Vector2 position)
        {
            agents[agentNo2indexDict[agentNo]].position = position;
        }

        /**
         * <summary>Sets the two-dimensional preferred velocity of a specified
         * agent.</summary>
         *
         * <param name="agentNo">The number of the agent whose two-dimensional
         * preferred velocity is to be modified.</param>
         * <param name="prefVelocity">The replacement of the two-dimensional
         * preferred velocity.</param>
         */
        public void setAgentPrefVelocity(int agentNo, Vector2 prefVelocity)
        {
            agents[agentNo2indexDict[agentNo]].prefVelocity = prefVelocity;
        }

        /**
         * <summary>Sets the radius of a specified agent.</summary>
         *
         * <param name="agentNo">The number of the agent whose radius is to be
         * modified.</param>
         * <param name="radius">The replacement radius. Must be non-negative.
         * </param>
         */
        public void setAgentRadius(int agentNo, float radius)
        {
            agents[agentNo2indexDict[agentNo]].radius = radius;
        }

        /**
         * <summary>Sets the time horizon of a specified agent with respect to
         * other agents.</summary>
         *
         * <param name="agentNo">The number of the agent whose time horizon is
         * to be modified.</param>
         * <param name="timeHorizon">The replacement time horizon with respect
         * to other agents. Must be positive.</param>
         */
        public void setAgentTimeHorizon(int agentNo, float timeHorizon)
        {
            agents[agentNo2indexDict[agentNo]].timeHorizonAgent = timeHorizon;
        }

        /**
         * <summary>Sets the time horizon of a specified agent with respect to
         * obstacles.</summary>
         *
         * <param name="agentNo">The number of the agent whose time horizon with
         * respect to obstacles is to be modified.</param>
         * <param name="timeHorizonObst">The replacement time horizon with
         * respect to obstacles. Must be positive.</param>
         */
        public void setAgentTimeHorizonObst(int agentNo, float timeHorizonObst)
        {
            agents[agentNo2indexDict[agentNo]].timeHorizonObst = timeHorizonObst;
        }

        /**
         * <summary>Sets the two-dimensional linear velocity of a specified
         * agent.</summary>
         *
         * <param name="agentNo">The number of the agent whose two-dimensional
         * linear velocity is to be modified.</param>
         * <param name="velocity">The replacement two-dimensional linear
         * velocity.</param>
         */
        public void setAgentVelocity(int agentNo, Vector2 velocity)
        {
            agents[agentNo2indexDict[agentNo]].velocity = velocity;
        }

     
        /**
         * <summary>Sets the number of workers.</summary>
         *
         * <param name="numWorkers">The number of workers.</param>
         */
        public void SetNumWorkers(int numWorkers)
        {
            this.numWorkers = numWorkers;

            if (this.numWorkers <= 0)
            {
                int completionPorts;
                ThreadPool.GetMinThreads(out this.numWorkers, out completionPorts);
                
            }
            workers = null;
            workerAgentCount = 0;
            
        }

        /**
         * <summary>Sets the time step of the simulation.</summary>
         *
         * <param name="timeStep">The time step of the simulation. Must be
         * positive.</param>
         */
        public void setTimeStep(float timeStep)
        {
            this.timeStep = timeStep;
        }

        /**
         * <summary>Constructs and initializes a simulation.</summary>
         */
        private Simulator()
        {
            Clear();
        }
    }
}
