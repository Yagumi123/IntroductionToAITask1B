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
        private bool isSearchInProgress = false;
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded; // Hook into the Loaded event
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string filename = "NAVFILES/RobotNavTEST.txt";
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
        // draws out the visual representation of the grid
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
        // ui interactions allowing the selection of found paths and will display them on the grid
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
        // more ui interactions
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
            if (row >= 0 && row < gridLayout.SizeXY[0] && col >= 0 && col < gridLayout.SizeXY[1])
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
        }

        // clears the arrows etc off the grid
        private void ClearGridVisuals()
        {
          
            var nonBorders = SearchAlgorithmGrid.Children.Cast<UIElement>()
                .Where(element => !(element is Border))
                .ToList();

            foreach (var element in nonBorders)
            {
                SearchAlgorithmGrid.Children.Remove(element);
            }
        }


        // adds a red x over the goal whenit is found
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





        // simple search function that calls the search handler functions 

        private async void PerformSearch(string method, Nodes startNode, Nodes[,] nodes, GridLayout layout)
        {
            try
            {
                GoalsFoundList.Items.Clear(); // Clear existing items
                PathsFoundList.Items.Clear(); // Clear existing items
                allPathsFound.Clear();

                // Perform the search
                var results = await searchHandler.PerformSearches(method, startNode, nodes, layout, UpdateUIWithSearchResults);
                Debug.WriteLine($"Completed search with {results.Count} results");

                // Combine Goals Reached and Found Paths into a SearchResultData instance
                var combinedData = new SearchResultData
                {
                    FoundPaths = PathsFoundList.Items.Cast<ListBoxItem>().Select(item => (List<string>)item.Tag).ToList(),
                    GoalsReached = GoalsFoundList.Items.Cast<ListBoxItem>().Select(item => (List<string>)item.Tag).ToList()
                };

                // Add to CompletedSearchTypes
                string displayName = method.ToUpper().Replace(",", " / ") + $" - {combinedData.GoalsReached.Count} paths";
                ListBoxItem completedItem = new ListBoxItem
                {
                    Content = displayName,
                    Tag = combinedData // Store the combined data
                };
                CompletedSearchTypes.Items.Add(completedItem);

                // Save results to CSV
                PerformanceMonitoring performance = new PerformanceMonitoring();
                performance.SaveResultsToCsv(results, "SearchPerformanceReport.csv");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during search: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                isSearchInProgress = false; // Reset the flag
                UpdateSearchControlState(true); // Re-enable search controls
            }
        }


        // updates the ui lists witht he results
        private void UpdateUIWithSearchResults(int row, int col, string direction, int cost, bool goal, List<string> fullPath)
        {
            Dispatcher.Invoke(() =>
            {
                if (goal)
                {
                    // Add the goal information to the "Goals Reached" list
                    ListBoxItem goalItem = new ListBoxItem
                    {
                        Content = $"Goal reached at ({row},{col}) with cost: {cost}",
                        Tag = fullPath // Store the full path in the Tag for later use
                    };
                    GoalsFoundList.Items.Add(goalItem);
                    GoalsFoundList.SelectedItem = goalItem;

                    AddGoalMarker(row, col);
                    allPathsFound.Add(fullPath);
                    Debug.WriteLine("Added goal: " + string.Join(", ", fullPath));
                }
                else
                {
                    // Add the path information to the "Found Paths" list
                    ListBoxItem pathItem = new ListBoxItem
                    {
                        Content = $"Goal reached at ({row},{col}) with cost: {cost}",
                        Tag = fullPath // Store the full path in the Tag for later use
                    };
                    PathsFoundList.Items.Add(pathItem);
                    PathsFoundList.SelectedItem = pathItem;
                }
            });
        }

        // ui functionality showing the paths foudn
        private void CompletedSearchTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CompletedSearchTypes.SelectedItem is ListBoxItem selectedItem && selectedItem.Tag is SearchResultData combinedData)
            {
                // Clear existing items
                GoalsFoundList.Items.Clear();
                PathsFoundList.Items.Clear();

                // Populate Goals Reached
                foreach (var goal in combinedData.GoalsReached)
                {
                    string goalContent = $"Goal reached with cost: {goal.Count - 1}";
                    GoalsFoundList.Items.Add(new ListBoxItem
                    {
                        Content = goalContent,
                        Tag = goal
                    });
                }

                // Populate Found Paths
                foreach (var path in combinedData.FoundPaths)
                {
                    string pathContent = $"Pathing with cost: {path.Count - 1}";
                    PathsFoundList.Items.Add(new ListBoxItem
                    {
                        Content = pathContent,
                        Tag = path
                    });
                }

                // Refresh the lists
                GoalsFoundList.Items.Refresh();
                PathsFoundList.Items.Refresh();
            }
        }

        // Method to enable or disable search-related controls
        private void UpdateSearchControlState(bool enabled)
            {
                DFSBtn.IsEnabled = enabled;
                BFSBtn.IsEnabled = enabled;
                AStarBtn.IsEnabled = enabled;
                UCSBtn.IsEnabled = enabled;
                GBFSBtn.IsEnabled = enabled;
                HCSBtn.IsEnabled = enabled;
                AllBtn.IsEnabled = enabled;
                StartBtn.IsEnabled = enabled;
            }

        // grid struct to create a gridlayout structure for use withing the node tree and visualisations
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

        // simple uo interaction functions
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
            if (isSearchInProgress)
            {
                MessageBox.Show("Search is already in progress.");
                return;
            }

            isSearchInProgress = true; // Set the flag to true to indicate search is in progress
            UpdateSearchControlState(false); // Disable search controls

            MessageBox.Show("Search Begun:");
            GoalsFoundList.Items.Clear(); // Clear existing items
            PathsFoundList.Items.Clear(); // Clear existing items
            MainLoop(selectedMethod);
        }
    }
    internal class SearchResultData
    {
        public List<List<string>> FoundPaths { get; set; }
        public List<List<string>> GoalsReached { get; set; }

        public SearchResultData()
        {
            FoundPaths = new List<List<string>>();
            GoalsReached = new List<List<string>>();
        }
    }


}