using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IntroToAIAssignment1.MainWindow;

namespace IntroToAIAssignment1
{
    internal class GridInterpretation
    {
        public GridLayout InterpretGrid(string filename)
        {
            int[] sizeXY = { 0, 0 };
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
                    stringSizeXY = lines[i].Substring(openBrack + 1, closedBrack - openBrack - 1).Trim();
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
                        greenGoalLocation = s.Substring(openBrack + 1, closedBrack - openBrack - 1);
                        string[] goalStringsXY = greenGoalLocation.Split(",");
                        int[] goal = new int[2];
                        Int32.TryParse(goalStringsXY[0], out goal[0]);
                        Int32.TryParse(goalStringsXY[1], out goal[1]);
                        goalLocations.Add(goal);

                    }
                }
                else
                {
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

    }
}
