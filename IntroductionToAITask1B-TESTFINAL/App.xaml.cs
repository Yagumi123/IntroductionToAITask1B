using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using static IntroToAIAssignment1.MainWindow;

namespace IntroToAIAssignment1
{
    public partial class App : Application
    {
        private Nodes[,] nodes; // To store the Nodes array

        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Attach to the parent process via the command line
            AttachConsole(ATTACH_PARENT_PROCESS);
            Console.WriteLine("Console attached successfully.");
            //as this is a wpf app it needs this line to attach a command line
            string[] args = Environment.GetCommandLineArgs();
            Console.WriteLine($"Arguments received: {string.Join(", ", args)}");
            // determines the launch mode which determines the type of operations such as batch search or gui search functions
            if (args.Length >= 3)
            {
                Console.WriteLine("Batch Mode Launched!!!");
                string filename = args[1];
                string method = args[2];
                Console.WriteLine($"Starting search for '{method}' on file '{filename}'.");
                ExecuteSearch(filename, method).Wait(); // Ensure completion
                Console.WriteLine("Batch test completed.");
                Environment.Exit(0);
            }
            else
            {
                Console.WriteLine("Launching UI MODE");
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }
        }

       
        private async Task ExecuteSearch(string filename, string method)
        {
            try
            {
                Console.WriteLine("Entering search mode...");
                // Load the grid layout from the file
                GridLayout layout = LoadGridLayoutFromFile(filename);
                //creates a node tree
                nodes = Nodes.CreateNodeTree(layout);
                Nodes startNode = nodes[layout.StartLocationXY[1], layout.StartLocationXY[0]];

                // Perform the search using the SearchHandler for a single method
                SearchHandler searchHandler = new SearchHandler();
                var result = await searchHandler.PerformSingleSearch(method, startNode, nodes, layout);

                // Output the results to the console
                if (result.HasValue)
                {
                    var (path, cost) = result.Value;
                    Console.WriteLine($"{method.ToUpper()} Solution:");
                    Console.WriteLine(path != null ? string.Join("; ", path) + ";" : "No path found.");
                }
                else
                {
                    Console.WriteLine($"No solution found for method: {method.ToUpper()}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during search: {ex.Message}");
            }
        }

        // calls the interpret grid functions contained within the gridinterpretation class.
        private GridLayout LoadGridLayoutFromFile(string filename)
        {
            try
            {
               
                var GridInterpretation = new GridInterpretation();
                return GridInterpretation.InterpretGrid(filename);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load grid layout: {ex.Message}");
            }
        }

    }
}