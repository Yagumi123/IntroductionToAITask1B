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
    /// 
    public partial class MainWindow : Window
    {

        private Nodes[,] nodes; // To store the Nodes array
        private GridLayout gridLayout; // To store the grid layout
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded; // Hook into the Loaded event
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string filename = "C:/Users/Thomas/Documents/IntroToAI/IntroToAIAssignment1/RobotNav_Test.txt";
            string method = "dfs";

            GridLayout gridLayout = InterpretGrid(filename);
            DrawGrid(gridLayout, filename);
            PerformSearch(method, gridLayout);
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
                        await Task.Delay(500); // Optional delay for animation
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




        private void AddArrowToGrid(int row, int col, string direction)
        {
            string arrow = direction switch
            {
                "Right" => "→",
                "Left" => "←",
                "Up" => "↑",
                "Down" => "↓",
                _ => ""
            };

            TextBlock arrowText = new TextBlock
            {
                Text = arrow,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 24
            };

   

            // Correct grid placement
            Grid.SetRow(arrowText, row);
            Grid.SetColumn(arrowText, col);

            SearchAlgorithmGrid.Children.Add(arrowText);
        }














        private async void PerformSearch(string method, GridLayout layout)
        {
            gridLayout = layout; // Save gridLayout to the class field
            nodes = CreateNodeTree(gridLayout); // Save Nodes to the class field
            Nodes startNode = nodes[gridLayout.StartLocationXY[1], gridLayout.StartLocationXY[0]];
            string outputPath = "path.txt";

            DFS dfs = new DFS();
            BFS bfs = new BFS();

            if (method.ToLower() == "dfs")
            {
                foreach (var goalLocation in gridLayout.GoalLocations)
                {
                    int goalRow = goalLocation[1];
                    int goalCol = goalLocation[0];
                    Nodes goalNode = nodes[goalRow, goalCol];
                    await dfs.DepthFirstSearch(startNode, goalNode, outputPath, (row, col, direction) =>
                    {
                        Dispatcher.Invoke(() => AddArrowToGrid(row, col, direction));
                    }, 500);

                    break;
                }
            }
            else if (method.ToLower() == "bfs")
            {
                foreach (var goalLocation in gridLayout.GoalLocations)
                {
                    int goalRow = goalLocation[1];
                    int goalCol = goalLocation[0];
                    Nodes goalNode = nodes[goalRow, goalCol];
                    await bfs.BreadthFirstSearch(startNode, goalNode, outputPath, (row, col, direction) =>
                    {
                        Dispatcher.Invoke(() => AddArrowToGrid(row, col, direction));
                    }, 500);

                    break;
                }
            }
           else if (method.ToLower() == "astar")
            {
                foreach (var goalLocation in gridLayout.GoalLocations)
                {
                    int goalRow = goalLocation[1];
                    int goalCol = goalLocation[0];
                    Nodes goalNode = nodes[goalRow, goalCol];
                    AStar astar = new AStar();
                    await astar.AStarSearch(startNode, goalNode, outputPath, (row, col, direction) =>
                    {
                        Dispatcher.Invoke(() => AddArrowToGrid(row, col, direction));
                    }, 500);

                  
                }
            
            }
            else
            {
                Debug.WriteLine($"Error: Unsupported method '{method}'. Use 'astar'.");
            }

            ParsePathsFromFile(outputPath);
        }





        private void ParsePathsFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.WriteLine($"Error: File '{filePath}' not found.");
                return;
            }

            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                Dispatcher.Invoke(() =>
                {
                    PathsFoundList.Items.Add($"Path: {line}");
                });
            }
        }


        private void PathsFoundList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PathsFoundList.SelectedItem is not string selectedPath) return;

            var pathData = selectedPath.Split(':')[1].Trim();
            var pathDirections = pathData.Split(" -> ");

            var startNode = nodes[gridLayout.StartLocationXY[1], gridLayout.StartLocationXY[0]];
            var goalNode = nodes[gridLayout.GoalLocations[0][1], gridLayout.GoalLocations[0][0]];

            _ = VisualizePath(nodes, pathDirections.ToList(), startNode, goalNode);
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


    }
}
