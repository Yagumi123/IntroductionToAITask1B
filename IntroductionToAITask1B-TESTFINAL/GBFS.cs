using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace IntroToAIAssignment1
{
    internal class GBFS
    {
        string bestPathOutputPath = "best_path_gbfs.txt";  // File path for the best path

        public async Task<List<(List<string> Path, int Cost)>> GreedyBestFirstSearch(Nodes startNode, Nodes goalNode, Action<int, int, string, int, bool, List<string>> uiCallback, int delay, bool StopAtFirstGoal = false)
        {
            // PriorityQueue for GBFS: (Node, Path, Cost) with priority based on heuristic
            PriorityQueue<(Nodes Node, List<string> Path, int Cost), int> priorityQueue = new PriorityQueue<(Nodes, List<string>, int), int>();

            // Dictionary to track visited nodes with their minimum heuristic values
            Dictionary<Nodes, int> visited = new Dictionary<Nodes, int>();

            // List to store all valid paths to the goal
            List<(List<string> Path, int Cost)> foundPaths = new List<(List<string>, int)>();

            // Start the search with the initial node
            priorityQueue.Enqueue((startNode, new List<string>(), 0), startNode.Heuristic(goalNode));
            if (!StopAtFirstGoal) { delay = 100; }
            while (priorityQueue.Count > 0)
            {
                var (current, path, cost) = priorityQueue.Dequeue();

                // Skip this node if a better path (lower heuristic) to it already exists
                if (visited.ContainsKey(current) && visited[current] <= cost)
                    continue;

                // Mark this node as visited with the current heuristic
                visited[current] = cost;

                // Visualization
                if (path.Any())
                {
                    // Correctly checking if the current node is the goal when calling the delegate
                    bool isGoalReached = current == goalNode;
                    uiCallback?.Invoke(current.Location.Row, current.Location.Col, path.LastOrDefault() ?? "", cost, isGoalReached, path);
                    await Task.Delay(delay);  // Simulate processing delay
                }

                // Check if this is the goal node
                if (current == goalNode)
                {
                    Debug.WriteLine($"Goal reached with path: {string.Join(", ", path)} and cost: {cost}");
                    foundPaths.Add((new List<string>(path), cost)); // Save this path
                    if (StopAtFirstGoal) { return foundPaths; }
                    continue; // Continue exploring for other potential paths
                }

                // Explore all neighbors
                foreach (var (neighbor, direction) in new[]
                {
                        (current.Up, "Up"),
                        (current.Left, "Left"),
                        (current.Down, "Down"),
                        (current.Right, "Right")
                })
                {
                    // Add neighbors to the queue if they are passable
                    if (neighbor != null && neighbor.IsPassable && (!visited.ContainsKey(neighbor) || visited[neighbor] > cost + 1))
                    {
                        var newPath = new List<string>(path) { direction };
                        int heuristic = neighbor.Heuristic(goalNode);
                        priorityQueue.Enqueue((neighbor, newPath, cost + 1), heuristic);
                    }
                }
            }

            // Save all found paths to the output file
            SaveBestPathToFile(foundPaths, bestPathOutputPath);
            return foundPaths;
        }

        private void SavePathsToFile(List<(List<string> Path, int Cost)> paths, string filePath)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Search Method: Greedy Best-First Search (GBFS)");
            builder.AppendLine("Format: Path - Cost");

            foreach (var (Path, Cost) in paths.OrderBy(p => p.Cost))
            {
                builder.AppendLine($"{string.Join(", ", Path)} - Cost: {Cost}");
            }

            File.WriteAllText(filePath, builder.ToString());
            Debug.WriteLine($"Paths saved to {filePath}");
        }
        private void SaveBestPathToFile(List<(List<string> Path, int Cost)> paths, string filePath)
        {
            if (paths.Any())
            {
                // Take the first path found since BFS explores uniformly, it should be the shortest path in an unweighted graph
                var firstPath = paths.First(); 

                // Build the string to write to file
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("----------------------------------------------------------------------");
                builder.AppendLine("Initial path (first found, compare to example results):");
                builder.AppendLine("----------------------------------------------------------------------");
                builder.AppendLine($"{string.Join(", ", firstPath.Path)} - Cost: {firstPath.Cost}");
                builder.AppendLine("----------------------------------------------------------------------");

                // Write the first path to the specified file
                File.WriteAllText(filePath, builder.ToString());
                Debug.WriteLine($"First path saved to {filePath}");
            }
        }

    }
}
