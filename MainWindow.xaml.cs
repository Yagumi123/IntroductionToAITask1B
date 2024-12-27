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

namespace IntroToAIAssignment1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    internal delegate void SearchVisualizeAction(int row, int col, string direction, int cost, bool isGoal, List<string> fullPath);

    /// 
    public partial class MainWindow : Window
    {

        private string selectedMethod = "none";
        private Nodes[,] nodes; // To store the Nodes array
        private GridLayout gridLayout; // To store the grid layout
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded; // Hook into the Loaded event
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string filename = "C:/Users/Thomas/Documents/INTROTOAI/IntroductionToAITask1B/RobotNavTEST.txt";
            gridLayout = InterpretGrid(filename);
            DrawGrid(gridLayout, filename);
            // Check for command-line arguments
            string[] args = Environment.GetCommandLineArgs();
                if (args.Length > 1)
                {
                    selectedMethod = args[1].ToLower();
                    PerformSearch(selectedMethod, gridLayout); // Automatically start the search when CMD is used
                }
                else
                {
                    MessageBox.Show("No search method specified. Please select one in the UI.", "Info");
                }
            


            
        }


        private Nodes[,] CreateNodeTree(GridLayout gridLayout)
        {
            int rows = gridLayout.SizeXY[0];
            int cols = gridLayout.SizeXY[1];
            Nodes[,] nodes = new Nodes[rows, cols];

            // Get all wall cell coordinates
            List<(int Row, int Col)> wallCells = GetWallCells(gridLayout.WallLocations);

            // Initially set all nodes as passable
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    // Mark as impassable if it's in the wall cell list
                    bool isPassable = !wallCells.Contains((row, col));
                    nodes[row, col] = new Nodes((row, col), isPassable);
                }
            }

            // Connect nodes
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    if (!nodes[row, col].IsPassable) continue; 

                    if (row > 0) nodes[row, col].Up = nodes[row - 1, col]; 
                    if (row < rows - 1) nodes[row, col].Down = nodes[row + 1, col]; 
                    if (col > 0) nodes[row, col].Left = nodes[row, col - 1]; 
                    if (col < cols - 1) nodes[row, col].Right = nodes[row, col + 1]; 
                }
            }

            return nodes;
        }

        private List<(int Row, int Col)> GetWallCells(List<int[]> wallLocations)
        {
            List<(int Row, int Col)> wallCells = new List<(int Row, int Col)>();

            foreach (int[] wall in wallLocations)
            {
                int wallX = wall[0];
                int wallY = wall[1]; 
                int wallWidth = wall[2]; 
                int wallHeight = wall[3]; 

               
                for (int h = 0; h < wallHeight; h++)
                {
                    for (int w = 0; w < wallWidth; w++) 
                    {
                        int wallRow = wallY + h; 
                        int wallCol = wallX + w; 

                        
                        wallCells.Add((wallRow, wallCol));
                    }
                }
            }

            return wallCells;
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

                System.Diagnostics.Debug.WriteLine("test");

                int wallX = i[0];
                int wallY = i[1];
                int wallHeight = i[2];
                int wallWidth = i[3];
                for (int h = 1;  h <= wallHeight; h++) { 
                  for(int w =1; w <= wallWidth; w++)
                    {
                        ChangeCellColorWithContent(SearchAlgorithmGrid, wallY + (w - 1), wallX + (h - 1), Brushes.Gray);
               
                    }
                }
               

            }
            SearchAlgorithmGrid.UpdateLayout(); 
        }


        private void ChangeCellColorWithContent(Grid grid, int row, int column,  Brush color)
        {
            Border border = new Border
            {
                Background = color, 
               
            };

            Grid.SetRow(border, row);
            Grid.SetColumn(border, column);
            grid.Children.Add(border);
        }
        private GridLayout InterpretGrid(string filename)
        {
            int[] sizeXY = {0,0};
            int[] startLocationXY = { 0, 0 };
            List<int[]> goalLocations = new List<int[]>();
            List<int[]> wallLocations = new List<int[]>();
            GridLayout InterpretedGrid = new GridLayout(); 
            string[] lines = File.ReadAllLines(filename);
            for (int i = 0; i < lines.Length; i++)
            {
                if (i == 0)
                {
                    string stringSizeXY;
                    int openBrack = lines[i].IndexOf("[");
                    int closedBrack = lines[i].IndexOf("]");
                    stringSizeXY = lines[i].Substring(openBrack+1, closedBrack - openBrack - 1).Trim();
                    string[] XYstring = stringSizeXY.Split(",");

                    int.TryParse(XYstring[0], out sizeXY[0]);

                    int.TryParse(XYstring[1], out sizeXY[1]);

                }
                else if (i == 1)
                {
                    string startLocation;
                    int openBrack = lines[i].IndexOf("(");
                    int closedBrack = lines[i].IndexOf(")");
                    startLocation = lines[i].Substring(openBrack + 1, closedBrack - openBrack - 1).Trim();
                    string[] startStringsXY = startLocation.Split(",");
                    Int32.TryParse(startStringsXY[0], out startLocationXY[0]);
                    Int32.TryParse(startStringsXY[1], out startLocationXY[1]);
                }
                else if (i == 2)
                {
                    string greenGoalLocation;
                    string goalLocationStrings = lines[i];
                    string[] goalLocationStringsList = goalLocationStrings.Split("|");

                    foreach (string s in goalLocationStringsList)
                    {
                        
                        int openBrack = s.IndexOf("(");
                        int closedBrack = s.IndexOf(")");
                        greenGoalLocation= s.Substring(openBrack + 1, closedBrack - openBrack - 1);
                        string[] goalStringsXY = greenGoalLocation.Split(",");
                        int[] goal = new int[2];
                        Int32.TryParse(goalStringsXY[0], out goal[0]);
                        Int32.TryParse(goalStringsXY[1], out goal[1]);
                        goalLocations.Add(goal);
                      
                    }
                }
                else {
                    string wallLocationAndSizeString;
                    int openBrack = lines[i].IndexOf("(");
                    int closedBrack = lines[i].IndexOf(")");
                    wallLocationAndSizeString = lines[i].Substring(openBrack + 1, closedBrack - openBrack - 1);
                    string[] wallXYHW = wallLocationAndSizeString.Split(",");
                    int[] wall = new int[4];
                    Int32.TryParse(wallXYHW[0], out wall[0]);
                    Int32.TryParse(wallXYHW[1], out wall[1]);
                    Int32.TryParse(wallXYHW[2], out wall[2]);
                    Int32.TryParse(wallXYHW[3], out wall[3]);
                    wallLocations.Add(wall);
                   }


            }

            InterpretedGrid.SizeXY = sizeXY;
            InterpretedGrid.StartLocationXY = startLocationXY;
            InterpretedGrid.GoalLocations = goalLocations;
            InterpretedGrid.WallLocations = wallLocations;
            return InterpretedGrid;
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


        private async Task VisualizePath(Nodes[,] nodes, List<string> path, Nodes startNode, Nodes goalNode)
        {
            ClearGridVisuals(); // Clear previous visuals
            Nodes current = startNode;

            foreach (string direction in path)
            {
                Nodes next = direction switch
                {
                    "Right" => current.Right,
                    "Left" => current.Left,
                    "Up" => current.Up,
                    "Down" => current.Down,
                    _ => null
                };

                if (next != null)
                {
                    if (next == goalNode)
                    {
                        AddGoalMarker(next.Location.Row, next.Location.Col);
                    }
                    else
                    {
                        AddArrowToGrid(current.Location.Row, current.Location.Col, direction);
                        await Task.Delay(100); // Optional delay for animation
                    }
                    current = next;
                }
                Application.Current.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Render);
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






        private async void PerformSearch(string method, GridLayout layout)
        {
            gridLayout = layout;
            nodes = CreateNodeTree(gridLayout);
            Nodes startNode = nodes[gridLayout.StartLocationXY[1], gridLayout.StartLocationXY[0]];
            var outputPath = "PathsFOUND.txt";
            var methods = method.ToLower().Split(',');
            List<string> allPaths = new List<string>();

            foreach (var goalLocation in gridLayout.GoalLocations)
            {
                int goalRow = goalLocation[1];
                int goalCol = goalLocation[0];
                Nodes goalNode = nodes[goalRow, goalCol];

                foreach (var meth in methods)
                {
                    switch (meth)
                    {
                        case "dfs":
                            DFS dfs = new DFS();
                           var dfsResults = await dfs.DepthFirstSearch(startNode, goalNode, outputPath, (row, col, direction, cost, goal, path) =>
                            {
                                Dispatcher.Invoke(() => UpdateUIWithSearchResults(row, col, direction, cost, goal, path));
                            }, 100);
                            allPaths.AddRange(dfsResults.Select(p => string.Join(", ", p.Path)));
                            break;
                        case "bfs":
                            BFS bfs = new BFS();
                           var bfsResults = await bfs.BreadthFirstSearch(startNode, goalNode, outputPath, (row, col, direction, cost, goal, path) =>
                            {
                                Dispatcher.Invoke(() => UpdateUIWithSearchResults(row, col, direction, cost, goal, path));
                            }, 100);
                            allPaths.AddRange(bfsResults.Select(p => string.Join(", ", p.Path)));
                            break;
                        case "astar":
                            AStar astar = new AStar();
                            var astarResults = await astar.AStarSearch(startNode, goalNode, outputPath, (row, col, direction, cost, goal, path) =>
                             {
                                 Dispatcher.Invoke(() => UpdateUIWithSearchResults(row, col, direction, cost, goal, path));
                             }, 100);
                               allPaths.AddRange(astarResults.Select(p => string.Join(", ", p.Path)));
                            break;
                        case "ucs":
                            UCS ucs = new UCS();
                            var ucsResults = await ucs.UniformCostSearch(startNode, goalNode, outputPath, (row, col, direction, cost, goal, path) =>
                            {
                                Dispatcher.Invoke(() => UpdateUIWithSearchResults(row, col, direction, cost, goal, path));
                            }, 100);
                            allPaths.AddRange(ucsResults.Select(p => string.Join(", ", p.Path)));
                            break;
                        case "gbfs":
                            GBFS gbfs = new GBFS();
                           var gbfsResults = await gbfs.GreedyBestFirstSearch(startNode, goalNode, outputPath, (row, col, direction, cost, goal, path) =>
                            {
                                Dispatcher.Invoke(() => UpdateUIWithSearchResults(row, col, direction, cost, goal, path));
                            }, 100);
                            allPaths.AddRange(gbfsResults.Select(p => string.Join(", ", p.Path)));
                            break;
                        case "hcs":
                            HCS hcs = new HCS();
                           var hcsResults = await hcs.HillClimbingSearch(startNode, goalNode, outputPath, (row, col, direction, cost, goal, path) =>
                            {
                                Dispatcher.Invoke(() => UpdateUIWithSearchResults(row, col, direction, cost, goal, path));
                            }, 100);
                            allPaths.AddRange(hcsResults.Select(p => string.Join(", ", p.Path)));
                            break;

                        default:
                            Debug.WriteLine($"Error: Unsupported method '{meth}'. Use 'dfs', 'bfs', 'astar', 'ucs', 'gbfs', or other supported methods.");
                            break;
                    }
                }
            }
            // Update CompletedSearchTypes ListBox
            string displayName = method.ToUpper().Replace(",", " / ") + $" - {allPaths.Count} paths";

            Debug.WriteLine(allPaths.ToString());
            ListBoxItem completedItem = new ListBoxItem
            {

                Content = displayName,
                Tag = allPaths // Store all paths for later display
            };
            CompletedSearchTypes.Items.Add(completedItem);
        
    }






        private void UpdateUIWithSearchResults(int row, int col, string direction, int cost, bool goal, List<string> fullPath)
        {
            Dispatcher.Invoke(() =>
            {
                Debug.WriteLine("Updating UI with search results");
                if (goal)
                {
                    // Create a new ListBoxItem
                    ListBoxItem newItem = new ListBoxItem();
                    newItem.Content = $"Goal reached at ({row},{col}) with cost: {cost}";
                    newItem.Tag = fullPath;  // Store the entire path in the Tag
                    PathsFoundList.Items.Add(newItem);
                    PathsFoundList.SelectedItem = newItem;
                }
            });
        }
        private void CompletedSearchTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CompletedSearchTypes.SelectedItem is ListBoxItem selectedItem && selectedItem.Tag is List<string> paths)
            {
                PathsFoundList.Items.Clear(); // Clear existing items
                foreach (var path in paths)
                {
                    PathsFoundList.Items.Add(new ListBoxItem { Content = path });
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
            PathsFoundList.Items.Clear();
            PerformSearch(selectedMethod, gridLayout);
        }
    }
}
