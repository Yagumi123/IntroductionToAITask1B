using IntroToAIAssignment1;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Linq;


internal class BFS
{
    public async Task BreadthFirstSearch(Nodes startNode, Nodes goalNode, string outputPath, Action<int, int, string> updateVisual, int delay = 500)
    {
        Queue<(Nodes, List<string>, int)> queue = new Queue<(Nodes, List<string>, int)>();
        Dictionary<Nodes, int> visited = new Dictionary<Nodes, int>();
        List<(List<string> Path, int Cost)> foundPaths = new List<(List<string> Path, int Cost)>();

        queue.Enqueue((startNode, new List<string>(), 0));

        while (queue.Count > 0)
        {
            var (current, path, cost) = queue.Dequeue();

            if (!visited.ContainsKey(current) || visited[current] > cost)
            {
                visited[current] = cost;

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
                        queue.Enqueue((neighbor, new List<string>(path) { direction }, cost + 1));
                        updateVisual(neighbor.Location.Row, neighbor.Location.Col, direction);
                        await Task.Delay(delay);
                    }
                }
            }
        }

        if (foundPaths.Any())
        {
            AppendPathsToFile(foundPaths.OrderBy(p => p.Cost).ToList(), outputPath);
        }
        else
        {
            Debug.WriteLine("No path found.");
        }
    }

    private void AppendPathsToFile(List<(List<string> Path, int Cost)> paths, string filePath)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("\nSearch Method: Breadth-First Search (BFS)");
        builder.AppendLine("Format: Path - Cost");

        foreach (var (Path, Cost) in paths)
        {
            builder.AppendLine($"{string.Join(", ", Path)} - Cost: {Cost}");
        }


        File.AppendAllText(filePath, builder.ToString());
        Debug.WriteLine($"Additional BFS paths appended to {filePath}");
    }
}
