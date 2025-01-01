using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using static IntroToAIAssignment1.MainWindow;

namespace IntroToAIAssignment1
{
    public partial class App : Application
    {
        private Nodes[,] nodes; // To store the Nodes array
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Get command-line arguments (args[0] is the executable path, so actual arguments start from args[1])
            string[] args = Environment.GetCommandLineArgs();
            Console.WriteLine($"Arguments received: {string.Join(", ", args)}");
            Debug.WriteLine($"Arguments received: {string.Join(", ", args)}");
            if (args.Length >= 3) // Ensure there are enough arguments for filename and method
            {
         
                string filename = args[1];
                string method = args[2];

                // Implement non-GUI batch processing
                ExecuteSearch(filename, method);
                Environment.Exit(0);
                Shutdown(); // Exit the application after processing
            }
            else
            {
                Console.WriteLine("Launching UI MODE");
                // Start the GUI normally if not enough arguments are provided
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }
        }
        // allows for the batch functionality requirements
        private void ExecuteSearch(string filename, string method)
        {
            try
            {
                // Load the grid layout from the file
                GridLayout layout = LoadGridLayoutFromFile(filename);
                nodes = Nodes.CreateNodeTree(layout);
                Nodes startNode = nodes[layout.StartLocationXY[1], layout.StartLocationXY[0]];

                // Perform the search using the SearchHandler
                SearchHandler searchHandler = new SearchHandler();
                var result = searchHandler.PerformSingleSearch(method, startNode, nodes, layout).Result;


                // Output the first solution path to the console
                if (result != null)
                {
                    Console.WriteLine($"{method.ToUpper()} Solution:");
                    Console.WriteLine(string.Join("; ", result.Path));
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

        private GridLayout LoadGridLayoutFromFile(string filename)
        {
            try
            {
                // Assuming you have a method in MainWindow or another class to interpret the grid
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