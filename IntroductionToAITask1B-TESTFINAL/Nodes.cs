using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IntroToAIAssignment1.MainWindow;

namespace IntroToAIAssignment1
{
    public class Nodes
    {

        public Nodes Left;
        public Nodes Right;
        public Nodes Up;
        public Nodes Down;
        public (int Row, int Col) Location { get; set; }
        public bool IsPassable { get; set; }
        public Nodes((int Row, int Col) location, bool isPassable)
        {
            Location = location;
            IsPassable = isPassable;
        }
        public int Heuristic(Nodes goal)
        {
            return Math.Abs(this.Location.Row - goal.Location.Row) + Math.Abs(this.Location.Col - goal.Location.Col);
        }
        public static Nodes[,] CreateNodeTree(GridLayout gridLayout)
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


        private static List<(int Row, int Col)> GetWallCells(List<int[]> wallLocations)
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



    }
}
