using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace IntroToAIAssignment1
{
    internal class AStar
    {
        string bestPathOutputPath = "best_path_astar.txt";  // Path for saving the best route

        public async Task<List<(List<string> Path, int Cost)>>AStarSearch(Nodes startNode, Nodes goalNode, Action<int, int, string, int, bool, List<string>> uiCallback, int delay, bool StopAtFirstGoal = false)
        {
            PriorityQueue<(Nodes Node, List<string> Path, int Cost, int Heuristic)> openSet = new PriorityQueue<(Nodes, List<string>, int, int)>();
            Dictionary<Nodes, int> visited = new Dictionary<Nodes, int>();
            List<(List<string> Path, int Cost)> foundPaths = new List<(List<string>, int)>();

            int CalculateHeuristic(Nodes node1, Nodes node2)
            {
                // Simple Manhattan Distance for heuristic
                return Math.Abs(node1.Location.Row - node2.Location.Row) + Math.Abs(node1.Location.Col - node2.Location.Col);
            }
            // Enqueue the starting node into the priority queue. 
            openSet.Enqueue((startNode, new List<string>(), 0, CalculateHeuristic(startNode, goalNode)), 0);
            if (!StopAtFirstGoal) { delay = 100; }
            while (openSet.Any())
            {
                var (current, path, cost, heuristic) = openSet.Dequeue();

                // Only process this node if it's the best path to it found so far
                if (!visited.ContainsKey(current) || visited[current] > cost)
                {
                    visited[current] = cost;

                    // Visualize current step
                    if (path.Any())
                    {
                        // Correctly checking if the current node is the goal when calling the delegate
                        bool isGoalReached = current == goalNode;
                        uiCallback?.Invoke(current.Location.Row, current.Location.Col, path.LastOrDefault() ?? "", cost, isGoalReached, path);
                        await Task.Delay(delay);  // Simulate processing delay
                    }


                    // If goal is reached, add path to found paths
                    if (current == goalNode)
                    {
                        Debug.WriteLine($"Goal reached with path: {string.Join(", ", path)} and cost: {cost}");
                        foundPaths.Add((new List<string>(path), cost));
                        if (StopAtFirstGoal) { return foundPaths; }
                        continue;
                    }

                    // Process each potential move
                    foreach (var (neighbor, direction) in new[]
                    {
                        (current.Up, "Up"),
                        (current.Left, "Left"),
                        (current.Down, "Down"),
                        (current.Right, "Right")
                    })
                    {
                        if (neighbor != null && neighbor.IsPassable && (!visited.ContainsKey(neighbor) || visited[neighbor] > cost + 1))
                        {
                            var newPath = new List<string>(path) { direction };
                            int newCost = cost + 1;
                            int newHeuristic = CalculateHeuristic(neighbor, goalNode);
                            openSet.Enqueue((neighbor, newPath, newCost, newHeuristic), newCost + newHeuristic);
                        }
                    }
                }
            }

            // Save all paths and the best path
            SaveBestPathToFile(foundPaths, bestPathOutputPath);
            return foundPaths;
        }


        private void SaveBestPathToFile(List<(List<string> Path, int Cost)> paths, string filePath)
        {
            if (paths.Any())
            {
                // Take the first path found; assuming paths are stored as they are found
                var firstPath = paths.First();

                // Build the string to write to file
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("----------------------------------------------------------------------");
                builder.AppendLine("Initial A* Path (first found):");
                builder.AppendLine("----------------------------------------------------------------------");
                builder.AppendLine($"{string.Join(", ", firstPath.Path)} - Cost: {firstPath.Cost}");
                builder.AppendLine("----------------------------------------------------------------------");

                // Write the first path to the specified file
                File.WriteAllText(filePath, builder.ToString());
                Debug.WriteLine($"First path saved to {filePath}");
            }
        }


        private class PriorityQueue<T>
        {
            private List<(T Item, int Priority)> elements = new List<(T, int)>();

            public void Enqueue(T item, int priority)
            {
                elements.Add((item, priority));
                elements.Sort((x, y) => x.Priority.CompareTo(y.Priority));
            }

            public T Dequeue()
            {
                var item = elements.First().Item;
                elements.RemoveAt(0);
                return item;
            }

            public bool Any()
            {
                return elements.Count > 0;
            }
        }
    }
}
