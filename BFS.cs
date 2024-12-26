﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace IntroToAIAssignment1
{
    internal class BFS
    {
        string bestPathOutputPath = "best_path.txt";
        public async Task<List<(List<string> Path, int Cost)>>BreadthFirstSearch(Nodes startNode, Nodes goalNode, string outputPath, SearchVisualizeAction visualizeAction, int delay)
        {
            Queue<(Nodes Node, List<string> Path, HashSet<Nodes> Visited, int Cost)> queue = new Queue<(Nodes, List<string>, HashSet<Nodes>, int)>();
            List<(List<string> Path, int Cost)> foundPaths = new List<(List<string>, int)>();

            HashSet<Nodes> initialVisited = new HashSet<Nodes> { startNode };
            queue.Enqueue((startNode, new List<string>(), initialVisited, 0));

            while (queue.Any())
            {
                var (current, path, visited, cost) = queue.Dequeue();

                // Visualization
                if (path.Any())
                {
                    // Correctly checking if the current node is the goal when calling the delegate
                    bool isGoalReached = current == goalNode;
                    visualizeAction(current.Location.Row, current.Location.Col, path.Last(), cost, isGoalReached, path);
                    await Task.Delay(delay);  // Simulate processing delay
                }

                // Check if this is the goal node
                if (current == goalNode)
                {
                    Debug.WriteLine($"Goal reached with path: {string.Join(", ", path)} and cost: {cost}");
                    foundPaths.Add((new List<string>(path), cost)); // Save this path
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
                    if (neighbor != null && neighbor.IsPassable && !visited.Contains(neighbor))
                    {
                        var newPath = new List<string>(path) { direction };
                        var newVisited = new HashSet<Nodes>(visited) { neighbor };
                        queue.Enqueue((neighbor, newPath, newVisited, cost + 1)); // Add neighbor to the queue
                    }
                }
            }

            // Save results to file
            SavePathsToFile(foundPaths, outputPath);
            SaveBestPathToFile(foundPaths, bestPathOutputPath);
            return foundPaths;
        }

        private void SavePathsToFile(List<(List<string> Path, int Cost)> paths, string filePath)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Search Method: Breadth-First Search (BFS)");
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
                // Order paths by cost and take the first one as the best path
                var bestPath = paths.OrderBy(p => p.Cost).First();

                // Build the string to write to file
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("----------------------------------------------------------------------");
                builder.AppendLine($"Best BFS Route: {string.Join(", ", bestPath.Path)}");
                builder.AppendLine("----------------------------------------------------------------------");

                // Write the best path to the specified file
                File.WriteAllText(filePath, builder.ToString());
                Debug.WriteLine($"Best path saved to {filePath}");
            }
        }

    }
}
