using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntroToAIAssignment1
{
    public class PerformanceMonitoring
    {
        // Method to monitor resource usage during search execution
        public async Task<PerformanceData> MonitorSearchPerformance(Func<Task<List<(List<string> Path, int Cost)>>> searchFunction)
        {
            Stopwatch stopwatch = new Stopwatch();
            Process currentProcess = Process.GetCurrentProcess();
            currentProcess.Refresh();  // Ensure the process stats are up-to-date
            long initialMemory = currentProcess.WorkingSet64;
            TimeSpan initialCpuTime = currentProcess.TotalProcessorTime;

            stopwatch.Start();
            var results = await searchFunction();
            stopwatch.Stop();

            currentProcess.Refresh();  // Refresh again to get updated stats
            long finalMemory = currentProcess.WorkingSet64;
            TimeSpan finalCpuTime = currentProcess.TotalProcessorTime;

            double memoryUsedMB = (finalMemory - initialMemory) / (1024.0 * 1024.0);
            double cpuUsage = (finalCpuTime - initialCpuTime).TotalMilliseconds / stopwatch.Elapsed.TotalMilliseconds * 100;

            // Assuming you want to aggregate all paths and their total cost
            var allPaths = results.SelectMany(r => r.Path).ToList();
            int totalCost = results.Sum(r => r.Cost);

            return new PerformanceData
            {
                Paths = allPaths,
                Cost = totalCost,
                Duration = stopwatch.Elapsed.TotalMilliseconds,
                CpuUsage = cpuUsage,
                MemoryUsageMB = memoryUsedMB
            };
        }
        public void SaveResultsToCsv(List<PerformanceData> results, string filePath)
        {
            using (StreamWriter file = new StreamWriter(filePath))
            {
                file.WriteLine("Method,Duration (ms),Memory Usage (MB)");
                foreach (var result in results)
                {
                    file.WriteLine($"{result.Method},{result.Duration},{result.MemoryUsageMB},{result.CpuUsage},{result.Cost}");
                }
            }
        }

        public struct PerformanceData
        {
            public string Method;
            public List<string> Paths;
            public int Cost;
            public double Duration;
            public double CpuUsage;
            public double MemoryUsageMB;

            public PerformanceData(string method, List<string> paths, int cost, double duration, double cpuUsage, double memoryUsageMB)
            {
                Method = method;
                Paths = paths;
                Cost = cost;
                Duration = duration;
                CpuUsage = cpuUsage;
                MemoryUsageMB = memoryUsageMB;
            }
        }
    }


}