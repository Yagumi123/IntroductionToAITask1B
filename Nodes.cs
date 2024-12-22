using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    }

    }

