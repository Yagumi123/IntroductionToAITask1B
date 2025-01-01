using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;
using System.Windows.Markup;
using System.DirectoryServices;
using static System.Net.Mime.MediaTypeNames;

namespace IntroToAIAssignment1
{

    internal delegate void SearchVisualizeAction(int row, int col, string direction, int cost, bool isGoal, List<string> fullPath);

    /// 
    public partial class MainWindow : Window
    {

        private string selectedMethod = "none";
        private Nodes[,] nodes; // To store the Nodes array
        private GridLayout gridLayout; // To store the grid layout
        private string reportFilePath = "SearchPerformanceReport.csv";
        private SearchHandler searchHandler = new SearchHandler();
        private List<List<string>> allPathsFound = new List<List<string>>();
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded; // Hook into the Loaded event
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string filename = "RobotNavTEST.txt";
            var GridInterpretation = new GridInterpretation();
            gridLayout = GridInterpretation.InterpretGrid(filename);
            DrawGrid(gridLayout, filename);
           

        }
        private void MainLoop(string selectedMethod)
        {

            nodes = Nodes.CreateNodeTree(gridLayout);
            Nodes startNode = nodes[gridLayout.StartLocationXY[1], gridLayout.StartLocationXY[0]];





            // Check for command-line arguments
            string[] args = Environment.GetCommandLineArgs();
            PerformSearch(selectedMethod, startNode, nodes, gridLayout);
          




        }

        private void DrawGrid(GridLayout InterpretedGridLayout, string filename)
        {

            SearchAlgorithmGrid.RowDefinitions.Clear();
            SearchAlgorithmGrid.ColumnDefinitions.Clear();
            int rows = InterpretedGridLayout.SizeXY[0];
            int columns = InterpretedGridLayout.SizeXY[1];
            double cellSize = 30;
            for (int i = 0; i < rows; i++)
            {
                SearchAlgorithmGrid.RowDefinitions.Add(new RowDefinition
                { Height = new GridLength(50) });

            }
            for (int j = 0; j < columns; j++)
            {
                SearchAlgorithmGrid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(50)

                });
            }
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    Border cellBorder = new Border
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1),
                        Background = Brushes.Transparent
                    };


                    Grid.SetRow(cellBorder, row);
                    Grid.SetColumn(cellBorder, col);


                    SearchAlgorithmGrid.Children.Add(cellBorder);
                }
            }



            ChangeCellColorWithContent(SearchAlgorithmGrid, InterpretedGridLayout.StartLocationXY[1], InterpretedGridLayout.StartLocationXY[0], Brushes.Red);

            InterpretedGridLayout.GoalLocations.ForEach(delegate (int[] i)
            {

                ChangeCellColorWithContent(SearchAlgorithmGrid, i[1], i[0], Brushes.Green);
            });

            foreach (int[] i in InterpretedGridLayout.WallLocations)
            {

               

                int wallX = i[0];
                int wallY = i[1];
                int wallHeight = i[2];
                int wallWidth = i[3];
                for (int h = 1; h <= wallHeight; h++)
                {
                    for (int w = 1; w <= wallWidth; w++)
                    {
                        ChangeCellColorWithContent(SearchAlgorithmGrid, wallY + (w - 1), wallX + (h - 1), Brushes.Gray);

                    }
                }


            }
            SearchAlgorithmGrid.UpdateLayout();
        }


        private void ChangeCellColorWithContent(Grid grid, int row, int column, Brush color)
        {
            Border border = new Border
            {
                Background = color,

            };

            Grid.SetRow(border, row);
            Grid.SetColumn(border, column);
            grid.Children.Add(border);
        }

        private void GoalsFoundList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GoalsFoundList.SelectedItem is ListBoxItem selectedListItem)
            {
                // Clear any previous arrows or markers from the grid
                ClearGridVisuals();

                // Retrieve the full path from the tag of the ListBoxItem
                var fullPath = (List<string>)selectedListItem.Tag;
                int currentRow = gridLayout.StartLocationXY[1];
                int currentCol = gridLayout.StartLocationXY[0];

                // Iterate over each step in the path
                foreach (var step in fullPath)
                {
                    // Determine the new position based on the direction of the step
                    switch (step)
                    {
                        case "Right":
                            currentCol += 1;
                            break;
                        case "Left":
                            currentCol -= 1;
                            break;
                        case "Up":
                            currentRow -= 1;
                            break;
                        case "Down":
                            currentRow += 1;
                            break;
                    }

                    // Place an arrow on the grid at the new position
                    AddArrowToGrid(currentRow, currentCol, step);
                }
            }
        }

        private void PathsFoundList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PathsFoundList.SelectedItem is ListBoxItem selectedListItem)
            {
                // Clear any previous arrows or markers from the grid
                ClearGridVisuals();

                // Retrieve the full path from the tag of the ListBoxItem
                var fullPath = (List<string>)selectedListItem.Tag;
                int currentRow = gridLayout.StartLocationXY[1];
                int currentCol = gridLayout.StartLocationXY[0];

                // Iterate over each step in the path
                foreach (var step in fullPath)
                {
                    // Determine the new position based on the direction of the step
                    switch (step)
                    {
                        case "Right":
                            currentCol += 1;
                            break;
                        case "Left":
                            currentCol -= 1;
                            break;
                        case "Up":
                            currentRow -= 1;
                            break;
                        case "Down":
                            currentRow += 1;
                            break;
                    }

                    // Place an arrow on the grid at the new position
                    AddArrowToGrid(currentRow, currentCol, step);
                }
            }
        }


        private void AddArrowToGrid(int row, int col, string direction)
        {
            string arrow = direction switch
            {
                "Right" => "→",
                "Left" => "←",
                "Up" => "↑",
                "Down" => "↓",
                _ => "" // Handles cases where direction might be unspecified or empty
            };

            // Only add an arrow if a direction is specified
            if (!string.IsNullOrEmpty(arrow))
            {
                TextBlock arrowText = new TextBlock
                {
                    Text = arrow,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 24,
                    FontWeight = FontWeights.Bold // Making the arrow more visible
                };

                Grid.SetRow(arrowText, row);
                Grid.SetColumn(arrowText, col);
                SearchAlgorithmGrid.Children.Add(arrowText);
            }
        }


        private void ClearGridVisuals()
        {
            // Assuming you have a method like this to clear non-border elements from the grid
            var nonBorders = SearchAlgorithmGrid.Children.Cast<UIElement>()
                .Where(element => !(element is Border))
                .ToList();

            foreach (var element in nonBorders)
            {
                SearchAlgorithmGrid.Children.Remove(element);
            }
        }



        private void AddGoalMarker(int row, int col)
        {
            TextBlock goalMarker = new TextBlock
            {
                Text = "X",
                Foreground = Brushes.Red,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 24
            };

            Grid.SetRow(goalMarker, row);
            Grid.SetColumn(goalMarker, col);
            SearchAlgorithmGrid.Children.Add(goalMarker);
        }






        private async void PerformSearch(string method, Nodes startNode, Nodes[,] nodes, GridLayout layout)
        {
            GoalsFoundList.Items.Clear(); // Clear goal items at the start
            PathsFoundList.Items.Clear(); // Clear path items at the start
            PathsFoundList.Items.Refresh();
            GoalsFoundList.Items.Refresh();
            allPathsFound.Clear();
            try
            {
                // Call the search and pass the update method directly
                var results = await searchHandler.PerformSearches(method, startNode, nodes, layout, UpdateUIWithSearchResults);

                // Process results here if needed
                Debug.WriteLine($"Completed search with {results.Count} results");

             
                PerformanceMonitoring performance = new PerformanceMonitoring();
                string displayName = method.ToUpper().Replace(",", " / ") + $" - {allPathsFound.Count} paths";

                Debug.WriteLine(results.ToString());
                ListBoxItem completedItem = new ListBoxItem
                {

                    Content = displayName,
                    Tag = allPathsFound// Store all paths for later display
            };
                CompletedSearchTypes.Items.Add(completedItem);
                performance.SaveResultsToCsv(results, "SearchPerformanceReport.csv");
             
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during search: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }


        private void UpdateUIWithSearchResults(int row, int col, string direction, int cost, bool goal, List<string> fullPath)
        {
         
            Dispatcher.Invoke(() =>
            {
               
                if (!goal)
                {    // Create a new ListBoxItem
                    ListBoxItem newItem = new ListBoxItem();
                    newItem.Content = $"Goal reached at ({row},{col}) with cost: {cost}";
                    newItem.Tag = fullPath;  // Store the entire path in the Tag
                    PathsFoundList.Items.Add(newItem);
                    PathsFoundList.SelectedItem = newItem;
                }
                else
                {
                    // Create a new ListBoxItem
                    ListBoxItem newItem = new ListBoxItem();
                    newItem.Content = $"Goal reached at ({row},{col}) with cost: {cost}";
                    newItem.Tag = fullPath;  // Store the entire path in the Tag
                    GoalsFoundList.Items.Add(newItem);
                    GoalsFoundList.SelectedItem = newItem;
               
                    AddGoalMarker(row, col);
                    allPathsFound.Add(fullPath);
                    Debug.WriteLine("Added path: " + string.Join(", ", fullPath));
                }
                
                   
                
            });
        }


        private void CompletedSearchTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CompletedSearchTypes.SelectedItem is ListBoxItem selectedItem && selectedItem.Tag is List<List<string>> pathsList)
            {
              
                GoalsFoundList.Items.Clear(); // Clear existing items
                PathsFoundList.Items.Clear(); // Clear existing items

                Debug.WriteLine(pathsList.Count);
                foreach (var paths in pathsList)
                {
                
                        // Assuming each path is a list of directions, convert them to a single string
                        string formattedPath = string.Join(" -> ", paths);
                        GoalsFoundList.Items.Add(new ListBoxItem { Content = formattedPath });
                        PathsFoundList.Items.Refresh();
                        GoalsFoundList.Items.Refresh();

                    
                }
            }
        }





        public struct GridLayout
        {
            public int[] SizeXY { get; set; }
            public int[] StartLocationXY { get; set; }
            public List<int[]> GoalLocations { get; set; }
            public List<int[]> WallLocations { get; set; }

            public GridLayout(int[] sizeXY, int[] startLocationXY, List<int[]> goalLocations, List<int[]> wallLocations)
            {
                SizeXY = sizeXY;
                StartLocationXY = startLocationXY;
                GoalLocations = goalLocations;
                WallLocations = wallLocations;
            }
        }


        private void DFSBtn_Click(object sender, RoutedEventArgs e)
        {
            selectedMethod = "dfs";
            MessageBox.Show("DFS Selected", "Info");
        }

        private void BFSBtn_Click(object sender, RoutedEventArgs e)
        {
            selectedMethod = "bfs";
            MessageBox.Show("BFS Selected", "Info");
        }

        private void AStarBtn_Click_1(object sender, RoutedEventArgs e)
        {
            selectedMethod = "astar";
            MessageBox.Show("A* Selected", "Info");
        }

        private void UCSBtn_Click(object sender, RoutedEventArgs e)
        {
            selectedMethod = "ucs";
            MessageBox.Show("UCS Selected", "Info");
        }

        private void GBFSBtn_Click(object sender, RoutedEventArgs e)
        {
            selectedMethod = "gbfs";
            MessageBox.Show("GBFS Selected", "Info");
        }

        private void HCSBtn_Click(object sender, RoutedEventArgs e)
        {
            selectedMethod = "hcs";
            MessageBox.Show("HCS Selected", "Info");
        }

        private void AllBtn_Click(object sender, RoutedEventArgs e)
        {
            selectedMethod = "all";
            MessageBox.Show("ALL Selected", "Info");
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Search Begun:");
            GoalsFoundList.Items.Clear(); // Clear existing items
            PathsFoundList.Items.Clear(); // Clear existing items
            MainLoop(selectedMethod);
        }
    }
  
}