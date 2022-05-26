using System;
using System.Collections.Generic;

namespace Routeurs
{

    public class Switch
    {

        IpAddress ipAddress;

        Dictionary<uint, Computer> connectedPCs = new Dictionary<uint, Computer>();

        Dictionary<Switch, int> connectedSwitch = new Dictionary<Switch, int>();

        //les uint correspondent a des ranges distance'adresse IP.
        //Chaque routeur dessevira uniquement des machines situées dans la même
        //range que lui même, on n'a pas besoin de stocker une entrée par adresse
        //mais uniquement une entrée par switch de destination (entrée dont la valeur
        //correspondra au range de son ip). La Value enregistrée à chaque Key
        //sera le switch du voisinage (connectedSwitch) par lequel transiter
        //pour arriver à destination.
        Dictionary<uint, Switch> routeTable = new Dictionary<uint, Switch>();

        uint nextMachineIp;

        public Switch(IpAddress ipAddress)
        {
            this.ipAddress = ipAddress;
            nextMachineIp = 100;
        }

        public void ConnectSwitch(Switch neighbor, int latency)
        {
            if (connectedSwitch.ContainsKey(neighbor))
            {
                connectedSwitch[neighbor] = latency;
                neighbor.connectedSwitch[this] = latency;
            }
            else
            {
                connectedSwitch.Add(neighbor, latency);
                neighbor.connectedSwitch.Add(this, latency);
            }
        }

        public void ConnectPC(Computer pc)
        {
            uint pcIp;
            do
            {
                pcIp = ipAddress.GetAddress() + nextMachineIp;
                ++nextMachineIp;
                if (nextMachineIp > 255)
                    nextMachineIp = 100;
            } while (connectedPCs.ContainsKey(pcIp));
            pc.SetConnectedSwitch(this);
            pc.SetIpAddress(new IpAddress(pcIp));
            connectedPCs.Add(pcIp, pc);
        }

        public void FillRouteTable(List<Switch> allSwitches)
        {
            routeTable.Clear();
            //TODO
            //On veut créer dans la table de routage une entrée par range distance'ip
            //en attendant on va juste mettre le voisinage direct

            if (connectedSwitch.Count <= 0)
                return;

            /*Console.WriteLine($"\n{ipAddress.GetStringAddress()} (SOURCE)\n");*/

            /*foreach(var sw in connectedSwitch)
            {
                Console.WriteLine($"{sw.Value} value");
            }*/

            int minLatency = int.MaxValue;

            /*Initialisation*/

            // Liste des switchs non-visités
            List<Switch> unvisitedSwitch = new List<Switch>();
            Dictionary<Switch, int> distances = new Dictionary<Switch, int>();
            Dictionary<Switch, List<Switch>> paths = new Dictionary<Switch, List<Switch>>();

            for (int i = 0; i < allSwitches.Count; ++i)
            {
                if (allSwitches[i].connectedSwitch.Count > 0)
                {
                    unvisitedSwitch.Add(allSwitches[i]);
                    distances.Add(allSwitches[i], int.MaxValue);
                    paths.Add(allSwitches[i], new List<Switch>());
                }
            }

            distances[this] = 0;
            Switch currentSwitch = this;

            while (unvisitedSwitch.Count > 0)
            {
                foreach (var sw in currentSwitch.connectedSwitch)
                {
                    // On ne check que les switch pas encore visités
                    if (unvisitedSwitch.Contains(sw.Key))
                    {
                        int distance = distances[currentSwitch] + sw.Value;
                        if (distances[sw.Key] > distance)
                        {
                            // On change la valeur et on met à jour le chemin
                            distances[sw.Key] = distance;

                            // On efface le précédent chemin enregistré
                            paths[sw.Key].Clear();

                            for (int i = 0; i < paths[currentSwitch].Count; ++i)
                            {
                                if (!paths[sw.Key].Contains(paths[currentSwitch][i]))
                                    paths[sw.Key].Add(paths[currentSwitch][i]);
                            }
                            paths[sw.Key].Add(sw.Key);
                        }
                    }
                }

                unvisitedSwitch.Remove(currentSwitch);

                int min = int.MaxValue;

                // On cherche le switch ayant la distance la plus faible par rapport au switch this dans le graphe
                foreach (var sw in distances)
                {
                    // On ne check que les switchs pas encore visités
                    if (unvisitedSwitch.Contains(sw.Key))
                    {
                        if (min > sw.Value)
                        {
                            min = sw.Value;
                            currentSwitch = sw.Key;
                        }
                    }
                }
            }

            // Affichage des chemins les plus courts trouvés du switch this à un autre
            /*  for (int i = 0; i < paths.Keys.Count; ++i)
              {
                  if (paths.ContainsKey(allSwitches[i]))
                  {
                      Console.WriteLine($"\nPathing from : {ipAddress.GetStringAddress()} to : {allSwitches[i].ipAddress.GetStringAddress}");
                      foreach (var item in paths[allSwitches[i]])
                      {
                          Console.WriteLine(item.ipAddress.GetStringAddress());
                      }
                  }
              }*/

            /*Console.WriteLine("\n");

              // Affichage des distances du switch this à un autre
              foreach (var sw in distances)
                  Console.WriteLine($"{Total distance from {ipAddress.GetStringAddress()} to sw.Key.ipAddress.GetStringAddress()} : {sw.Value}");*/

            // Dressage de la table
            for (int i = 0; i < allSwitches.Count; ++i)
            {
                if (allSwitches[i].ipAddress.GetAdressRange() == ipAddress.GetAdressRange())
                    routeTable[allSwitches[i].ipAddress.GetAdressRange()] = this;
                else
                {
                    List<Switch> value;
                    if (paths.TryGetValue(allSwitches[i], out value))
                    {
                        try
                        {
                            routeTable[allSwitches[i].ipAddress.GetAdressRange()] = value[0];
                        }
                        catch(ArgumentOutOfRangeException ex)
                        {
                            Console.WriteLine(ex);
                            routeTable[allSwitches[i].ipAddress.GetAdressRange()] = null;
                        }
                    }
                }
            }
        }

        public void SendMessage(IpAddress destination, string message, int latency = 0)
        {
            Console.WriteLine("Transiting Through {0} (latency={1}ms).", ipAddress.GetStringAddress(), latency);
            uint range = destination.GetAdressRange();
            if(range == ipAddress.GetAdressRange())
            {
                uint address = destination.GetAddress();
                if (!connectedPCs.ContainsKey(address))
                {
                    throw new Exception(String.Format("({0}) No PC has this adress: {1}",
                        ipAddress.GetStringAddress(), destination.GetStringAddress()));
                }
                connectedPCs[address].ReceiveMessage(message);
            }
            else if(routeTable.ContainsKey(range))
            {
                Switch nextSwitch = routeTable[range];
                if (connectedSwitch.ContainsKey(nextSwitch))
                {
                    nextSwitch.SendMessage(destination, message, latency + connectedSwitch[nextSwitch]);
                }else
                {
                    throw new Exception(String.Format("({0}) Trying to send a message to a switch we're not connected to: {1}",
                            ipAddress.GetStringAddress(), nextSwitch.ipAddress.GetStringAddress()));
                }
            }
            else
            {
                throw new Exception(String.Format("({0}) No known route to reach destination: {1}",
                        ipAddress.GetStringAddress(), destination.GetStringAddress()));
            }
        }
    }
}
