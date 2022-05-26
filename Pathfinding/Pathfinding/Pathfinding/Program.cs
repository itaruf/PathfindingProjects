using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels;
using System.Security.Cryptography.X509Certificates;

namespace Pathfinding
{

    class Game
    {
        const int MAP_WIDTH = 90;
        const int MAP_HEIGHT = 50;
        const int MAX_WEIGHT = 9;

        //ATTENTION: le tableau est construit en mode [hauteur][largeur].
        int[][] map;

        Position playerStartPos;
        Position goal;
        List<Position> doors = new List<Position>();

        public void Init()
        {
            Random random = new Random();

            map = new int[MAP_HEIGHT][];

            for (int i = 0; i < MAP_HEIGHT; ++i)
            {
                map[i] = new int[MAP_WIDTH];
                for (int j = 0; j < MAP_WIDTH; ++j)
                {
                    // On met des valeurs aléatoires dans chaque case, sauf sur les murs

                    map[i][j] = ((j == MAP_WIDTH /3) || (j == MAP_WIDTH * 2 / 3) || (i == MAP_HEIGHT / 2)) ? int.MaxValue : random.Next(MAX_WEIGHT) + 1;
                }
            }

            //Joueur dans la salle 1
            playerStartPos.X = random.Next(MAP_WIDTH / 3);
            playerStartPos.Y = random.Next(MAP_HEIGHT / 2);

            //objectif dans la salle 3
            goal.X = (MAP_WIDTH * 2 / 3) + 1 + random.Next(MAP_WIDTH / 3 - 1);
            goal.Y = 1 + MAP_HEIGHT / 2 + random.Next(MAP_HEIGHT / 2 - 1);

            //Les portes
            //1 porte par mur vertical, + 1 sur le mur horizontal
            doors.Add(new Position(MAP_WIDTH / 3, random.Next(MAP_HEIGHT / 2)));
            doors.Add(new Position(MAP_WIDTH * 2 / 3, random.Next(MAP_HEIGHT / 2)));
            //1+ pour éviter qu'elles se retrouvent sur le mur horizontal
            doors.Add(new Position(MAP_WIDTH / 3, 1 + MAP_HEIGHT / 2 + random.Next(MAP_HEIGHT / 2 - 1)));
            doors.Add(new Position(MAP_WIDTH * 2 / 3, 1 + MAP_HEIGHT / 2 + random.Next(MAP_HEIGHT / 2 - 1)));
            //la porte horizontale, attention aux murs verticaux
            Position lastDoor = new Position(random.Next(MAP_WIDTH), MAP_HEIGHT / 2);
            while(lastDoor.X % (MAP_WIDTH / 3) == 0)
            {
                lastDoor.X = random.Next(MAP_WIDTH);
            };
            doors.Add(lastDoor);

            foreach (Position position in doors)
                map[position.Y][position.X] = 1;
        }

        //Fonction de calcul du chemin. Elle doit retourner les éléments suivants:
        //path: liste des points à traverser pour aller du départ à l'arrivée
        //cost: coût du trajet
        //checkedTiles: liste de toutes les positions qui ont été testées pour trouver le chemin le plus court
        public bool GetShortestPath(List<Position> path, out int cost, HashSet<Position> checkedTiles)
        {
            cost = 0;
            return true;
        }

        public void DisplayMap(List<Position> path, HashSet<Position> checkedTiles)
        {

            ConsoleColor defaultColor = ConsoleColor.White; // Console.ForegroundColor;
            Position position = new Position();

            for (int i = 0; i < MAP_HEIGHT; ++i)
            {
                for (int j = 0; j < MAP_WIDTH; ++j)
                {
                    if (i == playerStartPos.Y && j == playerStartPos.X)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("S");
                    }
                    else if (i == goal.Y && j == goal.X)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("A");
                    }
                    else
                    {
                        Console.ForegroundColor = defaultColor;
                        position.X = j;
                        position.Y = i;
                        if (path.Contains(position))
                            Console.ForegroundColor = ConsoleColor.Red;
                        else if(checkedTiles.Contains(position))
                            Console.ForegroundColor = ConsoleColor.Blue;
                        else if (Math.Abs(map[i][j]) == int.MaxValue)
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write(Math.Abs(map[i][j]) != int.MaxValue ? "" + Math.Abs(map[i][j]) : "#");
                    }
                }
                Console.WriteLine();
            }

            Console.ForegroundColor = defaultColor;
        }
    };

    class MainClass
    {


        public static void Main(string[] args)
        {
            List<Position> path = new List<Position>();
            HashSet<Position> checkedTiles = new HashSet<Position>();
            int cost;
            Game game = new Game();
            Console.WriteLine("Initialisation....");
            game.Init();
            Console.WriteLine("Calcul du trajet....");
            bool found = game.GetShortestPath(path, out cost, checkedTiles);
            if(found)
            {
                Console.WriteLine("Trajet trouvé! longueur: {0}, coût: {1}, et on a du tester {2} positions pour l'obtenir.", path.Count, cost, checkedTiles.Count);
            }
            else
            {
                Console.WriteLine("Aucun trajet trouvé.");
            }

            game.DisplayMap(path, checkedTiles);

            Console.ReadKey();
        }
    }
}
