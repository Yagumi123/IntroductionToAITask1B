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

        public async Task<List<(List<string> Path, int Cost)>> HillClimbingSearch(Nodes startNode, Nodes goalNode, Action<int, int, string, int, bool, List<string>> uiCallback, int delay, bool StopAtFirstGoal = false)
        {
            Nodes current = startNode;
            List<string> path = new List<string>();
            int cost = 0;
            List<(List<string> Path, int Cost)> foundPaths = new List<(List<string>, int)>();

            if (!StopAtFirstGoal) { delay = 100; }

            // Manhattan Distance Heuristic 
            int CalculateHeuristic(Nodes node1, Nodes node2)
            {
                return Math.Abs(node1.Location.Row - node2.Location.Row) + Math.Abs(node1.Location.Col - node2.Location.Col);
            }

            while (current != goalNode)
            {
                Nodes next = null;
                double currentHeuristic = CalculateHeuristic(current, goalNode);
                double nextHeuristic = double.MaxValue;

                // Explore neighbors, ensuring none are out-of-bounds
                var possibleMoves = new List<(Nodes neighbor, string direction)>
                {
                    (current.Up, "Up"),
                    (current.Left, "Left"),
                    (current.Down, "Down"),
                    (current.Right, "Right")
                };

                foreach (var (neighbor, direction) in possibleMoves)
                {
                    if (neighbor != null && neighbor.IsPassable)
                    {
                        double heuristic = CalculateHeuristic(neighbor, goalNode);
                        if (heuristic < nextHeuristic)
                        {
                            nextHeuristic = heuristic;
                            next = neighbor;
                            if (!path.Any() || path.Last() != direction)
                                path.Add(direction);
                        }
                    }
                }

                if (path.Any())
                {
                    bool isGoalReached = current == goalNode;
                   // uiCallback?.Invoke(current.Location.Row, current.Location.Col, path.LastOrDefault() ?? "", cost, isGoalReached, path);
                    await Task.Delay(delay);
                }

                if (next != null && nextHeuristic < currentHeuristic)
                {
                    current = next;
                    cost++;
                }
                else
                {
                    break; // Break if no better neighbor is found
                }
            }

            if (current == goalNode)
            {
                foundPaths.Add((path, cost));
                if (StopAtFirstGoal) { return foundPaths; }
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
