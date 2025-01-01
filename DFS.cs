using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using static IntroToAIAssignment1.SearchHandler;

namespace IntroToAIAssignment1
{
    internal class DFS
    {

        string bestPathOutputPath = "DFS_best_path.txt";
        public async Task<List<(List<string> Path, int Cost)>> DepthFirstSearch(Nodes startNode, Nodes goalNode, Action<int, int, string, int, bool, List<string>> uiCallback, int delay)
        {
            Stack<(Nodes Node, List<string> Path, int Cost)> stack = new Stack<(Nodes, List<string>, int)>();
            Dictionary<Nodes, int> visited = new Dictionary<Nodes, int>();
            List<(List<string> Path, int Cost)> foundPaths = new List<(List<string>, int)>();

            stack.Push((startNode, new List<string>(), 0));

            while (stack.Any())
            {
                var (current, path, cost) = stack.Pop();

                if (!visited.ContainsKey(current) || visited[current] > cost)
                {
                    visited[current] = cost;

                    if (path.Any())
                    {
                        // Correctly checking if the current node is the goal when calling the delegate
                        bool isGoalReached = current == goalNode;
                        uiCallback?.Invoke(current.Location.Row, current.Location.Col, path.LastOrDefault() ?? "", cost, isGoalReached, path);


                 
                        await Task.Delay(100);  // Simulate processing delay
                    }


                    if (current == goalNode)
                    {
                        foundPaths.Add((new List<string>(path), cost));
                        continue;
                    }

                    // Explore neighbors, the order is reversed in DFS vs BFS due to the use of LIFO structure instead of FIFO
                    foreach (var (neighbor, direction) in new[]
                    {
                       (current.Right, "Right"),
                       (current.Down, "Down"),
                       (current.Left, "Left"),
                       (current.Up, "Up")


                    })
                    {
                        if (neighbor != null && neighbor.IsPassable && (!visited.ContainsKey(neighbor) || visited[neighbor] > cost + 1))
                        {
                            var newPath = new List<string>(path) { direction };
                            stack.Push((neighbor, newPath, cost + 1));
                        }
                    }
                }
            }

            SavePathsToFile(foundPaths, bestPathOutputPath);
      
            return foundPaths;  // Return the list of found paths
        }

        private void SavePathsToFile(List<(List<string> Path, int Cost)> paths, string filePath)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Search Method: Depth-First Search (DFS)");
            builder.AppendLine("Format: Path - Cost");
            builder.AppendLine();

            if (paths.Any())
            {
                var firstPath = paths.FirstOrDefault(); // Get the first path found
                builder.AppendLine("--------------------------------------------------------");
                builder.AppendLine("Initial path (first found, compare to example results):");
                builder.AppendLine("--------------------------------------------------------");
                builder.AppendLine($"{string.Join(", ", firstPath.Path)} - Cost: {firstPath.Cost}");
                builder.AppendLine("--------------------------------------------------------");
                builder.AppendLine();
            }

            // List all paths found
            builder.AppendLine("All paths found:");
            foreach (var (path, cost) in paths)
            {
                builder.AppendLine($"{string.Join(", ", path)} - Cost: {cost}");
            }

            File.WriteAllText(filePath, builder.ToString());
            Debug.WriteLine($"Paths saved to {filePath}");
        }


    }
}
