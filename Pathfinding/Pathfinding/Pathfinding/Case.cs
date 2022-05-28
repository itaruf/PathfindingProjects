using System;

namespace Pathfinding
{
    public class Case
    {
        public Position pos; // coords
        public int f = 0;    // total : coût + heuristique
        public int g = 0;    // coût
        public int h = 0;    // heuristique

        public Case(Position position, int f, int g, int h)
        {
            this.pos = position;
            this.f = f;
            this.g = g;
            this.h = h;
        }
    }
}
