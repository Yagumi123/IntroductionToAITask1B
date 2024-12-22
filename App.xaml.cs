using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace IntroToAIAssignment1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Get command-line arguments
            string[] args = Environment.GetCommandLineArgs();
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
          /*  if (args.Length == 3) // Check for required arguments
            {
                string filename = args[1];
                string method = args[2];

                try
                {
                    // Process the file and perform the search
                   // PerformSearch(filename, method);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }

                Shutdown(); // Exit after processing
            }
            else
            {
                Console.WriteLine("Usage: search <filename> <method>");
                Shutdown(); // Exit if arguments are incorrect
            }
          */
        }

    }

}
