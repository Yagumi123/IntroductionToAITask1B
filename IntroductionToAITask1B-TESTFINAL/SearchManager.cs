using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using static IntroToAIAssignment1.MainWindow;

namespace IntroToAIAssignment1
{
    public class SearchHandler
    {
        public delegate void UpdateUICallback(int row, int col, string direction, int cost, bool isGoal, List<string> fullPath);
        // handles the search functionality for both batch and GUI searches 
        public async Task<List<PerformanceMonitoring.PerformanceData>> PerformSearches(string Methods, Nodes startNode, Nodes[,] nodes, GridLayout layout, UpdateUICallback uiCallback = null)
        {
            List < PerformanceMonitoring.PerformanceData> performanceResults = new List<PerformanceMonitoring.PerformanceData > ();
            List<string> allPaths = new List<string>();
            var methods = Methods.ToLower().Split(',');
            // determines if the monotoring is needed or not (currently not functional)
            if (methods.Contains("all"))
            {
                // Run all methods with monitoring
                methods = new string[] { "dfs", "bfs", "astar", "ucs", "gbfs", "hcs" };
                foreach (var method in methods)
                {
                    var result = await ExecuteSearchWithoutMonitoring(method, startNode, nodes, layout, uiCallback);
                    allPaths.AddRange(result.Paths);
                    performanceResults.Add(result);
                }
            }
            else
            {
                // Run only the specified methods without monitoring
                foreach (var method in methods)
                {
                    var result = await ExecuteSearchWithoutMonitoring(method, startNode, nodes, layout, uiCallback);
                    allPaths.AddRange(result.Paths);
                    performanceResults.Add(result);
                }
            }

            return performanceResults;
        }

        private async Task<PerformanceMonitoring.PerformanceData> ExecuteSearchWithMonitoring(string method, Nodes startNode, Nodes[,] nodes, GridLayout layout, UpdateUICallback  uiCallback = null)
        {
            // Wrapper to include performance monitoring for each search method as it isnt functional this will run the same as the normal execution
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            PerformanceMonitoring.PerformanceData result = await ExecuteSearchWithoutMonitoring(method, startNode, nodes, layout, uiCallback);
            stopwatch.Stop();

            Process currentProcess = Process.GetCurrentProcess();
            currentProcess.Refresh();
            long finalMemory = currentProcess.WorkingSet64;
            TimeSpan finalCpuTime = currentProcess.TotalProcessorTime;

            // Update the performance data with actual monitoring info
            return new PerformanceMonitoring.PerformanceData(
                result.Method,
                result.Paths,
                result.Cost,
                stopwatch.Elapsed.TotalMilliseconds,
                finalCpuTime.TotalMilliseconds,
                finalMemory / (1024.0 * 1024.0)  // Convert bytes to MB
            );
        }

        private async Task<PerformanceMonitoring.PerformanceData> ExecuteSearchWithoutMonitoring(string method, Nodes startNode, Nodes[,] nodes, GridLayout layout, UpdateUICallback uiCallback = null)
        {
            List<string> paths = new List<string>();
            int totalCost = 0;

            // Iterate through each goal location and do the selected search method with all gui functions
            foreach (var goalLocation in layout.GoalLocations)
            {
                switch (method)
                {
                    case "dfs":
                        var dfs = new DFS();
                        var dfsResults = await dfs.DepthFirstSearch(startNode, nodes[goalLocation[1], goalLocation[0]],
                            (row, col, direction, cost, isGoal, fullPath) => {
                                uiCallback?.Invoke(row, col, direction, cost, isGoal, fullPath);
                            }, 0);
                        paths.AddRange(dfsResults.Select(result => string.Join(", ", result.Path)));
                        totalCost += dfsResults.Sum(result => result.Cost);
                        break;
                    case "bfs":
                        var bfs = new BFS();
                        var bfsResults = await bfs.BreadthFirstSearch(startNode, nodes[goalLocation[1], goalLocation[0]],
                            (row, col, direction, cost, isGoal, fullPath) => {
                                uiCallback?.Invoke(row, col, direction, cost, isGoal, fullPath);
                            }, 0);
                        paths.AddRange(bfsResults.Select(result => string.Join(", ", result.Path)));
                        totalCost += bfsResults.Sum(result => result.Cost);
                        break;

                    case "astar":
                        var astar = new AStar();
                        var astarResults = await astar.AStarSearch(startNode, nodes[goalLocation[1], goalLocation[0]],
                            (row, col, direction, cost, isGoal, fullPath) => {
                                uiCallback?.Invoke(row, col, direction, cost, isGoal, fullPath);
                            }, 0);
                        paths.AddRange(astarResults.Select(result => string.Join(", ", result.Path)));
                        totalCost += astarResults.Sum(result => result.Cost);
                        break;

                    case "ucs":
                        var ucs = new UCS();
                        var ucsResults = await ucs.UniformCostSearch(startNode, nodes[goalLocation[1], goalLocation[0]],
                            (row, col, direction, cost, isGoal, fullPath) => {
                                uiCallback?.Invoke(row, col, direction, cost, isGoal, fullPath);
                            }, 0);
                        paths.AddRange(ucsResults.Select(result => string.Join(", ", result.Path)));
                        totalCost += ucsResults.Sum(result => result.Cost);
                        break;

                    case "gbfs":
                        var gbfs = new GBFS();
                        var gbfsResults = await gbfs.GreedyBestFirstSearch(startNode, nodes[goalLocation[1], goalLocation[0]],
                            (row, col, direction, cost, isGoal, fullPath) => {
                                uiCallback?.Invoke(row, col, direction, cost, isGoal, fullPath);
                            }, 0);
                        paths.AddRange(gbfsResults.Select(result => string.Join(", ", result.Path)));
                        totalCost += gbfsResults.Sum(result => result.Cost);
                        break;

                    case "hcs":
                        var hcs = new HCS();
                        var hcsResults = await hcs.HillClimbingSearch(startNode, nodes[goalLocation[1], goalLocation[0]],
                            (row, col, direction, cost, isGoal, fullPath) => {
                                uiCallback?.Invoke(row, col, direction, cost, isGoal, fullPath);
                            }, 0);
                        paths.AddRange(hcsResults.Select(result => string.Join(", ", result.Path)));
                        totalCost += hcsResults.Sum(result => result.Cost);
                        break;

                    default:
                        throw new InvalidOperationException("Unsupported search method: " + method);
                }
            }


            // Return results accumulated across all goal locations
            return new PerformanceMonitoring.PerformanceData(
                method,
                paths,
                totalCost,
                0,  // Duration is not monitored in this mode
                0,  // CPU usage is not monitored in this mode
                0   // Memory usage is not monitored in this mode
            );
        }
        public async Task<(List<string> Path, int Cost)?> PerformSingleSearch(
            string method,
            Nodes startNode,
            Nodes[,] nodes,
            GridLayout layout,
            Action<int, int, string, int, bool, List<string>> uiCallback = null)
        {

            switch (method)
            {
                case "dfs":
                    var dfs = new DFS();
                    var dfsResults = await dfs.DepthFirstSearch(startNode, nodes[layout.GoalLocations[0][1], layout.GoalLocations[0][0]], uiCallback, 0, true);
                    return dfsResults.FirstOrDefault();

                case "bfs":
                    var bfs = new BFS();
                    var bfsResults = await bfs.BreadthFirstSearch(startNode, nodes[layout.GoalLocations[0][1], layout.GoalLocations[0][0]], uiCallback, 0, true);
                    return bfsResults.FirstOrDefault();

                case "astar":
                    var astar = new AStar();
                    var astarResults = await astar.AStarSearch(startNode, nodes[layout.GoalLocations[0][1], layout.GoalLocations[0][0]], uiCallback, 0, true);
                    return astarResults.FirstOrDefault();

                case "ucs":
                    var ucs = new UCS();
                    var ucsResults = await ucs.UniformCostSearch(startNode, nodes[layout.GoalLocations[0][1], layout.GoalLocations[0][0]], uiCallback, 0, true);
                    return ucsResults.FirstOrDefault();

                case "gbfs":
                    var gbfs = new GBFS();
                    var gbfsResults = await gbfs.GreedyBestFirstSearch(startNode, nodes[layout.GoalLocations[0][1], layout.GoalLocations[0][0]], uiCallback, 0, true);
                    return gbfsResults.FirstOrDefault();

                case "hcs":
                    var hcs = new HCS();
                    var hcsResults = await hcs.HillClimbingSearch(startNode, nodes[layout.GoalLocations[0][1], layout.GoalLocations[0][0]], uiCallback, 0, true);
                    return hcsResults.FirstOrDefault();

                default:
                    Console.WriteLine($"Search method '{method}' not recognized.");
                    return null;
            }
        }


    }

}


