using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BestDelivery;

namespace case1
{
    class CalculateRoute
    {
        public List<int> GetMinOrdersPatch(Order[] orders)
        {
            int n = orders.Length;
            if (n == 0) return new List<int>();
            if (n == 1) return new List<int> { orders[0].ID, orders[0].ID };

            double[,] dist = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i == j)
                    {
                        dist[i, j] = 0;
                    }
                    else
                    {
                        double baseDist = BestDelivery.RoutingTestLogic.CalculateDistance(orders[i].Destination, orders[j].Destination);
                        double priorityFactor = (orders[i].Priority + orders[j].Priority) / 2.0;
                        dist[i, j] = baseDist / (1 + priorityFactor);
                    }
                }
            }

            var mstEdges = BuildMST(dist, n);
            var oddVertices = FindOddDegreeVertices(mstEdges, n);
            var matchingEdges = MinimumWeightPerfectMatching(oddVertices, dist);
            var multigraph = CombineGraphs(mstEdges, matchingEdges);
            var eulerianCircuit = FindEulerianCircuit(multigraph, n);
            var hamiltonianCycle = EulerToHamiltonian(eulerianCircuit);
            List<int> route = hamiltonianCycle.Select(i => orders[i].ID).ToList();
            route.Add(orders[hamiltonianCycle[0]].ID);
            return route;
        }
        private List<(int, int)> BuildMST(double[,] dist, int n)
        {
            var mstEdges = new List<(int, int)>();
            bool[] inMST = new bool[n];
            double[] key = new double[n];
            int[] parent = new int[n];
            for (int i = 0; i < n; i++)
            {
                key[i] = double.MaxValue;
                parent[i] = -1;
            }
            key[0] = 0;
            var pq = new PriorityQueue<int, double>();
            for (int i = 0; i < n; i++)
            {
                pq.Enqueue(i, key[i]);
            }

            while (pq.Count > 0)
            {
                int u = pq.Dequeue();
                inMST[u] = true;

                for (int v = 0; v < n; v++)
                {
                    if (!inMST[v] && dist[u, v] < key[v])
                    {
                        key[v] = dist[u, v];
                        parent[v] = u;
                        pq.Enqueue(v, key[v]);
                    }
                }
            }
            for (int v = 1; v < n; v++)
            {
                mstEdges.Add((parent[v], v));
            }
            return mstEdges;
        }
        private List<int> FindOddDegreeVertices(List<(int, int)> edges, int n)
        {
            int[] degree = new int[n];
            foreach (var (u, v) in edges)
            {
                degree[u]++;
                degree[v]++;
            }
            var oddVertices = new List<int>();
            for (int i = 0; i < n; i++)
                if (degree[i] % 2 == 1)
                    oddVertices.Add(i);
            return oddVertices;
        }
        private List<(int, int)> MinimumWeightPerfectMatching(List<int> oddVertices, double[,] dist)
        {
            var matchingEdges = new List<(int, int)>();
            var used = new HashSet<int>();
            oddVertices.Sort();

            foreach (int u in oddVertices)
            {
                if (used.Contains(u)) continue;
                double minDist = double.MaxValue;
                int minV = -1;
                foreach (int v in oddVertices)
                {
                    if (u != v && !used.Contains(v))
                    {
                        if (dist[u, v] < minDist)
                        {
                            minDist = dist[u, v];
                            minV = v;
                        }
                    }
                }
                if (minV != -1)
                {
                    matchingEdges.Add((u, minV));
                    used.Add(u);
                    used.Add(minV);
                }
            }
            return matchingEdges;
        }

        private Dictionary<int, List<int>> CombineGraphs(List<(int, int)> mstEdges, List<(int, int)> matchingEdges)
        {
            var graph = new Dictionary<int, List<int>>();
            void AddEdge(int u, int v)
            {
                if (!graph.ContainsKey(u)) graph[u] = new List<int>();
                if (!graph.ContainsKey(v)) graph[v] = new List<int>();
                graph[u].Add(v);
                graph[v].Add(u);
            }

            foreach (var (u, v) in mstEdges)
                AddEdge(u, v);
            foreach (var (u, v) in matchingEdges)
                AddEdge(u, v);

            return graph;
        }

        private List<int> FindEulerianCircuit(Dictionary<int, List<int>> graph, int n)
        {
            var circuit = new List<int>();
            var stack = new Stack<int>();
            var current = 0;
            stack.Push(current);

            var localGraph = new Dictionary<int, List<int>>();
            foreach (var kvp in graph)
                localGraph[kvp.Key] = new List<int>(kvp.Value);

            while (stack.Count > 0)
            {
                current = stack.Peek();
                if (localGraph.ContainsKey(current) && localGraph[current].Count > 0)
                {
                    int next = localGraph[current][0];
                    localGraph[current].RemoveAt(0);
                    localGraph[next].Remove(current);
                    stack.Push(next);
                }
                else
                {
                    circuit.Add(current);
                    stack.Pop();
                }
            }

            circuit.Reverse();
            return circuit;
        }

        private List<int> EulerToHamiltonian(List<int> eulerCircuit)
        {
            var visited = new HashSet<int>();
            var path = new List<int>();
            foreach (var v in eulerCircuit)
            {
                if (!visited.Contains(v))
                {
                    visited.Add(v);
                    path.Add(v);
                }
            }
            return path;
        }
    }     
}
