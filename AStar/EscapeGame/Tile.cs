using System.Collections.Generic;

namespace EscapeGame
{
    public class Tile
    {
        public Tile previousTile = null;
        public Position position;    // coords
        public int total => c + h;  // total : cost + heuristic
        public int c = 0;       // cost
        public int h = 0;       // heuristic

        public List<Tile> neighbors = new List<Tile>();

        public Tile(Position position, int c, int h)
        {
            this.position = position;
            this.c = c;
            this.h = h;
        }
    }
}
