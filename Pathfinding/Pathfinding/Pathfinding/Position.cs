using System;

namespace Pathfinding
{
    struct Position : IEquatable<Position>
    {
        private int x;
        private int y;

        public int X { get => x; set => x = value; }
        public int Y { get => y; set => y = value; }

        public Position(int x, int y)
        {
            this.x = x;
            this.y = y;

        }

        public bool Equals(Position other)
        {
            return (this.x == other.x && this.y == other.y);
        }

        //redéfinition de GetHashCode n'ayant aucine idée de la fonction de base
        //et de si elle sera valide pour nous
        public override int GetHashCode()
        {
            //peu de risque de collisions ici
            //vu la taille de la carte, on a peu de chance d'avoir des valeurs de plus de 30000 en x ou y
            //on peu faire un bitmask et tout faire tenir sur un int (partie haute pour x, partie basse pour y
            //faut juste gérer le signe de chacun en sus
            return ((x & 0x7FFF) << 16) + (y & 0x7FFF) + (x & 0x8000000) + ((y & 0x8000000) >> 16);
        }

        public override string ToString()
        {
            return "( " + x + " - " + y + " )";
        }

    }
}
