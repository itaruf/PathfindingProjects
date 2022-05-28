using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Security.Cryptography.X509Certificates;

namespace Pathfinding
{

    class Game
    {
        const int MAP_WIDTH = 20;
        const int MAP_HEIGHT = 20;
        const int MAX_WEIGHT = 9;

        //ATTENTION: le tableau est construit en mode [hauteur][largeur].
        public int[][] map;

        public Position playerStartPos;
        public Position goal;
        public List<Position> doors = new List<Position>();

        public List<Case> cases = new List<Case>();
        public Dictionary<Case, List<Case>> paths = new Dictionary<Case, List<Case>>();

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
            {
                map[position.Y][position.X] = 1;
            }


            for (int i = 0; i < MAP_HEIGHT; ++i)
            {
                for (int j = 0; j < MAP_WIDTH; ++j)
                {
                    // On ne prend pas en compte les cases non-atteignables
                    if (map[i][j] != int.MaxValue)
                        cases.Add(new Case(new Position(j, i), 0, 0, 0));
                }
            }


            foreach (var c in cases)
            {
                c.g = map[c.pos.Y][c.pos.X];
                getNeighborsOfPoint(c);
            }
        }

        int Heuristique(Position start, Position goal)
        {
            // On renvoit la distance de manhattan entre la case de départ et la case d'arrivée
            return (Math.Abs(start.X - goal.X) + Math.Abs(start.Y - goal.Y));
        }

        Case GetCase(int x, int y)
        {
            return cases.Find(c => c.pos.X == x && c.pos.Y == y);
        }
        Case GetCase(Position position)
        {
            return cases.Find(c => c.pos.X == position.X && c.pos.Y == position.Y);
        }

        /*https://codereview.stackexchange.com/questions/68627/getting-the-neighbors-of-a-point-in-a-2d-grid*/
        private List<Case> getNeighborsOfPoint(int x, int y)
        {
            List<Case> neighbors = new List<Case>();
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
                        if (map[x+xx][y+yy] != int.MaxValue)
                        {
                            var result = GetCase(x + xx, y + yy);
                            if (result != null)
                                neighbors.Add(result);
                        }
                    }
                }
            }
            return neighbors;
        }
        void getNeighborsOfPoint(Case c)
        {
            for (int xx = -1; xx <= 1; ++xx)
            {
                for (int yy = -1; yy <= 1; ++yy)
                {
                    if (xx == 0 && yy == 0)
                    {
                        continue; // You are not neighbor to yourself
                    }
                    if (Math.Abs(xx) + Math.Abs(yy) > 1)
                    {
                        continue;
                    }
                    if (isOnMap(c.pos.X + xx, c.pos.Y + yy))
                    {
                        var result = GetCase(c.pos.X + xx, c.pos.Y + yy);
                        if (result != null)
                        {
                            if (result.g != int.MaxValue)
                            {
                                c.neighbors.Add(result);
                            }
                        }
                    }
                }
            }
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

            List<Case> p = new List<Case>();
 
            List<Case> uncheckedTiles = new List<Case>();
            foreach (var c in cases)
                uncheckedTiles.Add(c);

            Case currentCase = GetCase(playerStartPos);

            foreach (var c in currentCase.neighbors)
                Console.WriteLine($"{c.pos.X} {c.pos.Y} {c.g}");

            Console.WriteLine("\n");
            currentCase.g = 0;

            Console.WriteLine(GetCase(doors[0]).neighbors.Count);

            foreach(var c in GetCase(doors[0].X, doors[0].Y).neighbors)
                Console.WriteLine($"{c.pos.X} {c.pos.Y} {c.g}");

            Console.WriteLine("\n");

            /*Console.WriteLine(GetCase(goal).neighbors.Count);

            foreach (var c in GetCase(goal).neighbors)
                Console.WriteLine($"{c.pos.X} {c.pos.Y} {c.g}");*/

            while (uncheckedTiles.Count > 0)
            {
                uncheckedTiles.Remove(currentCase);
                Case nextCase = null;
                int min = int.MaxValue;

                if (currentCase == GetCase(doors[0]) || currentCase == null)
                    break;

                Console.WriteLine($"current case : {currentCase.pos.X} {currentCase.pos.Y}");

                foreach (var n in currentCase.neighbors)
                {
                    // On ne regarde que les cases voisines encore inexplorées
                    if (uncheckedTiles.Contains(n))
                    {
                        Console.WriteLine($"current neighbor : {n.pos.X} {n.pos.Y}");

                        n.g += currentCase.g;
                        n.h = Heuristique(n.pos, doors[0]);
                        n.f = n.g + n.h;

                        // On sélectionne la case dont le coût total est le moins élevé
                        if (n.f < min)
                        {
                            min = n.f;
                            nextCase = n;
                        }
                        // s'il y a une égalité, on prend celui dont le coût est moindre
                        /*else if (n.f == min)
                        {
                            if (n.g < nextCase.g)
                                nextCase = n;
                        }*/
                    }
                }
          
                // On ajoute au chemin la nouvelle case
                p.Add(nextCase);
                currentCase = nextCase;
                if (currentCase != null)
                    Console.WriteLine($"New Case : {currentCase.pos.X} {currentCase.pos.Y}");
            }

            foreach (var pa in p)
                if (pa != null)
                    Console.WriteLine($"{pa.pos.X} {pa.pos.Y}");

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

            Console.WriteLine($"PLAYER POS : {playerStartPos.X}, {playerStartPos.Y}");
            Console.WriteLine($"DOOR 0 POS : {doors[0].X}, {doors[0].Y}");
            Console.WriteLine($"GOAL POS : {goal.X}, {goal.Y}");

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
