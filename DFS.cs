﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Diagnostics;
using System.IO;

namespace IntroToAIAssignment1
{
    internal class DFS
    {
        public async Task DepthFirstSearch(Nodes startNode, Nodes goalNode, string outputPath, Action<int, int, string> visualizeAction, int delay)
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
                        visualizeAction(current.Location.Row, current.Location.Col, path.Last());
                        await Task.Delay(delay);  
                    }

                    if (current == goalNode)
                    {
                        foundPaths.Add((new List<string>(path), cost));  
                        continue;
                    }

                    // Explore neighbors
                    foreach (var (neighbor, direction) in new[]
                    {
                        (current.Left, "Left"),
                        (current.Right, "Right"),
                        (current.Up, "Up"),
                        (current.Down, "Down")
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

            if (foundPaths.Any())
            {
                SavePathsToFile(foundPaths.OrderBy(p => p.Cost).ToList(), outputPath);  

            }
            else
            {
                Debug.WriteLine("No path found.");
            }
        }

        private void SavePathsToFile(List<(List<string> Path, int Cost)> paths, string filePath)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Search Method: Depth-First Search (DFS)"); 
            builder.AppendLine("Format: Path - Cost");

            foreach (var (Path, Cost) in paths)
            {
                builder.AppendLine($"{string.Join(", ", Path)} - Cost: {Cost}");
            }

            File.WriteAllText(filePath, builder.ToString());
            Debug.WriteLine($"Paths saved to {filePath}");
        }
    }
}