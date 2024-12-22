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
        public async Task AStarSearch(Nodes startNode, Nodes goalNode, string outputPath, Action<int, int, string> visualizeAction, int delay)
        {
            PriorityQueue<(Nodes Node, List<string> Path, int Cost, int Heuristic)> openSet = new PriorityQueue<(Nodes, List<string>, int, int)>();
            Dictionary<Nodes, int> visited = new Dictionary<Nodes, int>();
            List<(List<string> Path, int Cost)> foundPaths = new List<(List<string>, int)>();

            int CalculateHeuristic(Nodes node1, Nodes node2)
            {
                return Math.Abs(node1.Location.Row - node2.Location.Row) + Math.Abs(node1.Location.Col - node2.Location.Col);
            }

            openSet.Enqueue((startNode, new List<string>(), 0, CalculateHeuristic(startNode, goalNode)), 0);

            while (openSet.Any())
            {
                var (current, path, cost, heuristic) = openSet.Dequeue();

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
                            int newCost = cost + 1;
                            int newHeuristic = CalculateHeuristic(neighbor, goalNode);
                            openSet.Enqueue((neighbor, newPath, newCost, newHeuristic), newCost + newHeuristic);
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
            builder.AppendLine("Search Method: A* Search");
            builder.AppendLine("Format: Path - Cost");

            foreach (var (Path, Cost) in paths)
            {
                builder.AppendLine($"{string.Join(", ", Path)} - Cost: {Cost}");
            }

            File.WriteAllText(filePath, builder.ToString());
            Debug.WriteLine($"Paths saved to {filePath}");
        }

        private class PriorityQueue<T>
        {
            private List<(T Item, int Priority)> elements = new List<(T, int)>();

            public void Enqueue(T item, int priority)
            {
                elements.Add((item, priority));
                elements = elements.OrderBy(e => e.Priority).ToList();
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
