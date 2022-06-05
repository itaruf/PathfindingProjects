using System;
using System.Collections.Generic;

namespace Pathfinding
{

    class Game
    {
        const int MAP_WIDTH = 90;
        const int MAP_HEIGHT = 50;
        const int MAX_WEIGHT = 9;
        const int WEIGHT = 2;

        //ATTENTION: le tableau est construit en mode [hauteur][largeur].
        public int[][] map;

        public Position playerStartPos;
        public Position goal;
        public List<Position> doors = new List<Position>();

        public List<Tile> checkPoints = new List<Tile>();
        public List<Tile> tiles = new List<Tile>();

        public void Init()
        {
            Random random = new Random(10);

            map = new int[MAP_HEIGHT][];

            for (int i = 0; i < MAP_HEIGHT; ++i)
            {
                map[i] = new int[MAP_WIDTH];
                for (int j = 0; j < MAP_WIDTH; ++j)
                {
                    // On met des valeurs aléatoires dans chaque case, sauf sur les murs
                    map[i][j] = ((j == MAP_WIDTH / 3) || (j == MAP_WIDTH * 2 / 3) || (i == MAP_HEIGHT / 2)) ? int.MaxValue : random.Next(MAX_WEIGHT) + 1;

                    Tile t = new Tile(new Position(j, i), map[i][j], 0);
                    tiles.Add(t);
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
            while (lastDoor.X % (MAP_WIDTH / 3) == 0)
            {
                lastDoor.X = random.Next(MAP_WIDTH);
            };
            doors.Add(lastDoor);

            /*Now that we know where our horizontal door is, we can determine the checkpoints to reach when pathing*/
            if (lastDoor.X < MAP_WIDTH / 3)
            {
                checkPoints = new List<Tile>()
                {
                    GetTile(playerStartPos),
                    GetTile(doors[4]),
                    GetTile(doors[2]),
                    GetTile(doors[3]),
                    GetTile(goal)
                };
            }

            else if (lastDoor.X > 2 * MAP_WIDTH / 3)
            {
                checkPoints = new List<Tile>()
                {
                    GetTile(playerStartPos),
                    GetTile(doors[0]),
                    GetTile(doors[1]),
                    GetTile(doors[4]),
                    GetTile(goal)
                };
            }

            else
            {
                checkPoints = new List<Tile>()
                {
                    GetTile(playerStartPos),
                    GetTile(doors[0]),
                    GetTile(doors[4]),
                    GetTile(doors[3]),
                    GetTile(goal)
                };
            }

            foreach (Position position in doors)
            {
                map[position.Y][position.X] = 1;
                GetTile(position).c = 1;
            }

            // Fill all the neighbors of each tile
            foreach (var t in tiles)
                getNeighborsOfPoint(t);
        }

        int Heuristique(Position start, Position goal)
        {

            return (int)Math.Sqrt(Math.Pow(start.X - goal.X, 2) + Math.Pow(start.Y - goal.Y, 2)) * WEIGHT; // Distance euclidienne  * coeff : more weight =  more cost ; less checked tiles
            return (Math.Abs(start.X - goal.X) + Math.Abs(start.Y - goal.Y)) * WEIGHT; // Distance de manhattan * coeff : more weight = more cost ; less checked tiles
            return 0; // Dijkstra : less cost ; more checked tiles
            return (int)(Math.Pow(start.X - goal.X, 2) + Math.Pow(start.Y - goal.Y, 2)) * WEIGHT;  // Distance enclidienne carrée * coeff : more weight = more cost ; less checked tiles
        }

        Tile GetTile(int x, int y)
        {
            return tiles.Find(t => t.position.X == x && t.position.Y == y);
        }
        Tile GetTile(Position position)
        {
            return tiles.Find(t => t.position.X == position.X && t.position.Y == position.Y);
        }

        void getNeighborsOfPoint(Tile t)
        {
            if (t == null)
                return;

            for (int xx = -1; xx <= 1; ++xx)
            {
                for (int yy = -1; yy <= 1; ++yy)
                {
                    if (xx == 0 && yy == 0)
                        continue;
                    /*if (Math.Abs(xx) + Math.Abs(yy) > 1) // Comment the block for 8-connexity instead of 4
                    {
                        continue;
                    }*/
                    if (isOnMap(t.position.X + xx, t.position.Y + yy))
                    {
                        var result = GetTile(t.position.X + xx, t.position.Y + yy);
                        if (result == null)
                            return;
                        if (result.c != int.MaxValue)
                            t.neighbors.Add(result);
                    }
                }
            }
        }

        public bool isOnMap(int x, int y)
        {
            return x >= 0 && y >= 0 && x < MAP_WIDTH && y < MAP_HEIGHT;
        }

        //Fonction de calcul du chemin. Elle doit retourner les éléments suivants:
        //path: liste des points à traverser pour aller du départ à l'arrivée
        //cost: coût du trajet
        //checkedTiles: liste de toutes les positions qui ont été testées pour trouver le chemin le plus court
        public void AStar(Tile start, Tile end, List<Tile> path, ref int cost, HashSet<Tile> checkedTiles)
        {
            if (start == null || end == null)
                return;

            // all the tiles waiting to be treated
            List<Tile> pendingTiles = new List<Tile>();
            // all the tiles treated by the algorithm
            List<Tile> treatedCases = new List<Tile>();

            /*Initialization*/
            Tile currentTile = null;

            // the cost of the starting tile is now 0
            start.c = 0;
            start.h = Heuristique(start.position, end.position);
            pendingTiles.Add(start);

            while (true)
            {
                // Popping the first pending tile
                try
                {
                    currentTile = pendingTiles[0];

                    if (currentTile == null)
                        throw new Exception();

                } catch (ArgumentOutOfRangeException)
                {
                    return;
                } 
                catch (Exception)
                {
                    return;
                }

                foreach (var pendingTile in pendingTiles)
                {
                    // we reached the end
                    if (pendingTile == end)
                    {
                        currentTile = end;
                        goto exit;
                    }

                    // looking for the next pending tile which has the less total cost and, if 2 are equals, which has the less c cost
                    if (pendingTile.total < currentTile.total || (pendingTile.total == currentTile.total && (map[pendingTile.position.Y][pendingTile.position.X] < map[currentTile.position.Y][currentTile.position.X])))
                        currentTile = pendingTile;
                }


                // Check each non-treated neighbors of the current case
                foreach (var n in currentTile.neighbors)
                {
                    if (treatedCases.Contains(n))
                        continue;

                    // the neighbor has been checked atleast once
                    if (!checkedTiles.Contains(n))
                        checkedTiles.Add(n);
                    /*uncheckedTiles.Remove(n);*/

                    // calculating the cost so far + its heuristic
                    int c = n.c + currentTile.c;
                    int h = Heuristique(n.position, end.position);

                    // update the cost and the heuristic if we found a better path or if the neighbor isn't pending to be treated yet, calculate them for the first time
                    if ((pendingTiles.Contains(n) && c + h < n.total) || !pendingTiles.Contains(n))
                    {
                        n.c = c;
                        n.h = h;
                        n.previousTile = currentTile;
                    }

                    // Add each new explored neighbors tiles in the waiting list
                    if (!pendingTiles.Contains(n))
                        pendingTiles.Add(n);
                }

                /*the current tile is now treated*/
                /*uncheckedTiles.Remove(currentTile);*/
                pendingTiles.Remove(currentTile);
                if (!treatedCases.Contains(currentTile))
                    treatedCases.Add(currentTile);
                if (!checkedTiles.Contains(currentTile))
                    checkedTiles.Add(currentTile);
            }

        // Fill our path
        exit:
            while (currentTile != start)
            {
                // Gradually determine the cost so far
                cost += map[currentTile.position.Y][currentTile.position.X];
                path.Add(currentTile.previousTile);
                currentTile = currentTile.previousTile;
            }
        }

        public void DisplayMap(List<Tile> path, ref int cost, HashSet<Tile> checkedTiles)
        {
            ConsoleColor defaultColor = ConsoleColor.White;
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

                        if (path.Contains(GetTile(position)))
                            Console.ForegroundColor = ConsoleColor.Red;
                        else if (checkedTiles.Contains(GetTile(position)))
                            Console.ForegroundColor = ConsoleColor.Blue;
                        else if (Math.Abs(map[i][j]) == int.MaxValue)
                            Console.ForegroundColor = ConsoleColor.DarkGray;

                        Console.Write(Math.Abs(map[i][j]) != int.MaxValue ? "" + Math.Abs(map[i][j]) : "#");
                    }
                }
                Console.WriteLine();
            }

            Console.WriteLine("Trajet trouvé! longueur: {0}, coût: {1}, et on a du tester {2} positions pour l'obtenir.", path.Count, cost, checkedTiles.Count);

            Console.ForegroundColor = defaultColor;
        }
    };

    class MainClass
    {
        public static void Main(string[] args)
        {
            List<Tile> path = new List<Tile>();
            HashSet<Tile> checkedTiles = new HashSet<Tile>();
            int cost = 0;

            Game game = new Game();

            Console.WriteLine("Initialisation....");
            game.Init();

            Console.WriteLine("Calcul du trajet....");
            game.AStar(game.checkPoints[0], game.checkPoints[1], path, ref cost, checkedTiles);
            game.AStar(game.checkPoints[1], game.checkPoints[2], path, ref cost, checkedTiles);
            game.AStar(game.checkPoints[2], game.checkPoints[3], path, ref cost, checkedTiles);
            game.AStar(game.checkPoints[3], game.checkPoints[4], path, ref cost, checkedTiles);

            game.DisplayMap(path, ref cost, checkedTiles);

            Console.ReadKey();
        }
    }
}
