﻿using System.Collections.Generic;

namespace Pathfinding
{
    public class Tile
    {
        public Tile previousTile = null;
        public Position position;    // coords
        public int f => g + h;  // total : cost + heuristic
        public int g = 0;       // cost
        public int h = 0;       // h

        public List<Tile> neighbors = new List<Tile>();

        public Tile(Position position, int g, int h)
        {
            this.position = position;
            this.g = g;
            this.h = h;
        }
    }
}
