using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace IntroToAIAssignment1
{
    internal class UCS
    {
        string bestPathOutputPath = "best_path_ucs.txt";  // File path for the best path

        public async Task<List<(List<string> Path, int Cost)>>UniformCostSearch(Nodes startNode, Nodes goalNode, Action<int, int, string, int, bool, List<string>> uiCallback, int delay)
        {
            // Priority Queue for UCS: Nodes are prioritized by path cost
            PriorityQueue<(Nodes Node, List<string> Path, int Cost), int> priorityQueue = new PriorityQueue<(Nodes, List<string>, int), int>();

            // Dictionary to keep track of the minimum cost to reach each node
            Dictionary<Nodes, int> visited = new Dictionary<Nodes, int>();

            // List to store all valid paths to the goal
            List<(List<string> Path, int Cost)> foundPaths = new List<(List<string>, int)>();

            // Start the search with the initial node
            priorityQueue.Enqueue((startNode, new List<string>(), 0), 0);

            while (priorityQueue.Count > 0)
            {
                var (current, path, cost) = priorityQueue.Dequeue();

                // If this node has been visited with a lower cost before, skip processing
                if (visited.ContainsKey(current) && visited[current] <= cost)
                    continue;

                // Mark this node as visited with the current cost
                visited[current] = cost;

                // Visualization
                if (path.Any())
                {
                    // Correctly checking if the current node is the goal when calling the delegate
                    bool isGoalReached = current == goalNode;
                    uiCallback?.Invoke(current.Location.Row, current.Location.Col, path.LastOrDefault() ?? "", cost, isGoalReached, path);
                    await Task.Delay(delay);  // Simulate processing delay
                }


                // Check if the current node is the goal node
                if (current == goalNode)
                {
                    Debug.WriteLine($"Goal reached with path: {string.Join(", ", path)} and cost: {cost}");
                    foundPaths.Add((new List<string>(path), cost));
                    continue;
                }

                // Explore neighbors
                foreach (var (neighbor, direction) in new[]
                {
                    (current.Up, "Up"),
                    (current.Left, "Left"),
                    (current.Down, "Down"),
                    (current.Right, "Right")
                })
                {
                    if (neighbor != null && neighbor.IsPassable)
                    {
                        var newPath = new List<string>(path) { direction };
                        int newCost = cost + 1;  // Assuming uniform cost, adjust if different
                        if (!visited.ContainsKey(neighbor) || visited[neighbor] > newCost)
                        {
                            priorityQueue.Enqueue((neighbor, newPath, newCost), newCost);
                        }
                    }
                }
            }

            // Save results to files
         
            SaveBestPathToFile(foundPaths, bestPathOutputPath);
            return foundPaths;
        }


        private void SaveBestPathToFile(List<(List<string> Path, int Cost)> paths, string filePath)
        {
            if (paths.Any())
            {
                var bestPath = paths.OrderBy(p => p.Cost).First();
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("----------------------------------------------------------------------");
                builder.AppendLine($"Best UCS Route: {string.Join(", ", bestPath.Path)}");
                builder.AppendLine("----------------------------------------------------------------------");

                File.WriteAllText(filePath, builder.ToString());
                Debug.WriteLine($"Best path saved to {filePath}");
            }
        }
    }
}
