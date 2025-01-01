using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace IntroToAIAssignment1
{
    internal class HCS
    {
        string bestPathOutputPath = "best_path_hillclimbing.txt"; // Path for saving the best path

        public async Task<List<(List<string> Path, int Cost)>> HillClimbingSearch(Nodes startNode, Nodes goalNode, Action<int, int, string, int, bool, List<string>> uiCallback, int delay)
        {
            Nodes current = startNode;
            List<string> path = new List<string>();
            int cost = 0; // This needs to be calculated based on your specific cost conditions
            List<(List<string> Path, int Cost)> foundPaths = new List<(List<string>, int)>();

            while (current != goalNode)
            {
                Nodes next = null;
                double currentHeuristic = current.Heuristic(goalNode);
                double nextHeuristic = double.MaxValue; // Assuming lower heuristic values are better (closer to goal)

                // Explore neighbors to find the one with the lowest heuristic value
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
                        double heuristic = neighbor.Heuristic(goalNode);
                        if (heuristic < nextHeuristic)
                        {
                            nextHeuristic = heuristic;
                            next = neighbor;
                            if (path.Count == 0 || path.Last() != direction)
                                path.Add(direction);
                        }
                    }
                }
                if (path.Any())
                {
                    // Correctly checking if the current node is the goal when calling the delegate
                    bool isGoalReached = current == goalNode;
                    uiCallback?.Invoke(current.Location.Row, current.Location.Col, path.LastOrDefault() ?? "", cost, isGoalReached, path);
                    await Task.Delay(delay);  // Simulate processing delay
                }

                await Task.Delay(delay);

                if (next != null && nextHeuristic < currentHeuristic)
                {
                    current = next;
                    cost++; // Increment cost for moving to next node
                }
                else
                {
                    // No better neighbor, stuck at local maximum
                    break;
                }
            }

            if (current == goalNode)
            {
                foundPaths.Add((path, cost));
                Debug.WriteLine("Goal reached.");
            }

        
            SaveBestPathToFile(path, cost, bestPathOutputPath);

            return foundPaths;
        }

     

        private void SaveBestPathToFile(List<string> path, int cost, string filePath)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("----------------------------------------------------------------------");
            builder.AppendLine($"Best Hill Climbing Route: {string.Join(", ", path)} - Cost: {cost}");
            builder.AppendLine("----------------------------------------------------------------------");

            File.WriteAllText(filePath, builder.ToString());
            Debug.WriteLine($"Best path saved to {filePath}");
        }
    }
}
