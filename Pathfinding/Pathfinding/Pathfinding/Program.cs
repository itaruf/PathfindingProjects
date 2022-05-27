using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels;
using System.Security.Cryptography.X509Certificates;

namespace Pathfinding
{

    class Game
    {
        /*public class Case
        {
            public Position;
        };*/

        const int MAP_WIDTH = 20;
        const int MAP_HEIGHT = 20;
        const int MAX_WEIGHT = 9;

        //ATTENTION: le tableau est construit en mode [hauteur][largeur].
        public int[][] map;

        public Position playerStartPos;
        public Position goal;
        public List<Position> doors = new List<Position>();

        public Dictionary<Position, int> cases = new Dictionary<Position, int>();
        public Dictionary<Position, List<Position>> paths = new Dictionary<Position, List<Position>>();
        public Dictionary<Position, List<Position>> neighbors = new Dictionary<Position, List<Position>>();

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

                    if (map[i][j] != int.MaxValue)
                        cases.Add(new Position(i, j), int.MaxValue);
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
            {
                map[position.Y][position.X] = 1;
                /*Console.WriteLine($"{position.Y} / {position.X}");*/
            }

            foreach (var c in cases)
            {
                var list = getNeighborsOfPoint(c.Key.X, c.Key.Y);
                neighbors.Add(c.Key, list);
            }
        }

        /*https://codereview.stackexchange.com/questions/68627/getting-the-neighbors-of-a-point-in-a-2d-grid*/
        private List<Position> getNeighborsOfPoint(int x, int y)
        {
            List<Position> neighbors = new List<Position>();
            for (int xx = -1; xx <= 1; xx++)
            {
                for (int yy = -1; yy <= 1; yy++)
                {
                    if (xx == 0 && yy == 0)
                    {
                        continue; // You are not neighbor to yourself
                    }
                    if (Math.Abs(xx) + Math.Abs(yy) > 1)
                    {
                        continue;
                    }
                    if (isOnMap(x + xx, y + yy))
                    {
                        if (cases.ContainsKey(new Position(x + xx, y + yy)))
                            neighbors.Add(new Position(x + xx, y + yy));
                    }
                }
            }
            return neighbors;
        }

        public bool isOnMap(int x, int y)
        {
            return x >= 0 && y >= 0 && x < MAP_HEIGHT && y < MAP_WIDTH;
        }

        //Fonction de calcul du chemin. Elle doit retourner les éléments suivants:
        //path: liste des points à traverser pour aller du départ à l'arrivée
        //cost: coût du trajet
        //checkedTiles: liste de toutes les positions qui ont été testées pour trouver le chemin le plus court
        public bool GetShortestPath(List<Position> path, out int cost, HashSet<Position> checkedTiles)
        {
            /*Début Initialisation - Étape initiale*/
            cost = 0;
            cases[playerStartPos] = 0;

            List<Position> uncheckedTiles = new List<Position>();


            foreach (var tile in cases)
            {
                uncheckedTiles.Add(tile.Key);
                paths.Add(tile.Key, new List<Position>());
            }

            Position currentPosition = playerStartPos;

            /*Console.WriteLine(playerStartPos.X);
            Console.WriteLine(playerStartPos.Y);*/

            Console.WriteLine($"cases count : " +cases.Count);
            Console.WriteLine(neighbors[playerStartPos].Count);

            int pox = playerStartPos.X;
            int poy = playerStartPos.Y;

            Console.WriteLine(neighbors[new Position(pox, poy)].Count);

            // Tant qu'il reste des cases non-visitées
            while (uncheckedTiles.Count > 0)
            {
                /*Console.WriteLine($"{currentPosition.X} ; {currentPosition.Y}");*/
                // On parcourt les cases voisines
                foreach (var tile in neighbors[currentPosition])
                {
                    if (uncheckedTiles.Contains(tile) && map[tile.X][tile.Y] != int.MaxValue)
                    {
                        int distance = cases[currentPosition] + map[tile.X][tile.Y];

                        if (cases[tile] > distance)
                        {
                            cases[tile] = distance;

                            /*paths[tile].Clear();*/

                            paths[tile].Add(playerStartPos);

                            for (int i = 0; i < paths[currentPosition].Count; ++i)
                            {
                                if (!paths[tile].Contains(paths[currentPosition][i]))
                                    paths[tile].Add(paths[currentPosition][i]);
                            }
                            paths[tile].Add(tile);
                        }
                    }
                }

                uncheckedTiles.Remove(currentPosition);

                int min = int.MaxValue;
                foreach (var tile in cases)
                {
                    // On ne check que les switchs pas encore visités
                    if (uncheckedTiles.Contains(tile.Key) && map[tile.Key.X][tile.Key.Y] != int.MaxValue)
                    {
                        if (min > map[tile.Key.X][tile.Key.Y])
                        {
                            min = map[tile.Key.X][tile.Key.Y];
                            // Nouveau switch ayant la distance la plus faible actuellement
                            currentPosition = tile.Key;
                        }
                    }
                }
            }

            Console.WriteLine($"PLAYER POS : {playerStartPos.X}, {playerStartPos.Y}");
            Console.WriteLine(uncheckedTiles.Count);

            foreach (var p in paths[playerStartPos])
            {
                Console.WriteLine($"{p.X}, {p.Y}");
            }

            Console.WriteLine("----PRINTING GOAL----");
            /*Console.WriteLine(paths[playerStartPos].Count);
            Console.WriteLine(paths[goal].Count);*/

            
            foreach (var p in paths[doors[0]])
            {
                Console.WriteLine($"{p.X}, {p.Y}");
            }

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
                        Console.Write("S"); // Start Pos
                    }
                    else if (i == goal.Y && j == goal.X)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("A"); // End Pos
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

            Console.WriteLine($"PLAYER POS : {playerStartPos.X}, {playerStartPos.Y} And Neighbors :");
            foreach (var n in neighbors[playerStartPos])
                Console.WriteLine($"{n.X}, {n.Y}");

           /* foreach (var n in neighbors[new Position(0, 1)])
            {
                Console.WriteLine($"{n.X}, {n.Y}");
            }*/

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

            /*Console.WriteLine(game.doors.Count);*/
            Console.ReadKey();
        }
    }
}
