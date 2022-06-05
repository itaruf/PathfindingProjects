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
  
            if (connectedSwitch.Count <= 0)
                return;

            /*Début Initialisation - Étape initiale*/

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

            /*Fin Initialisation*/

            /*Dijkstra*/
            // Tant qu'il reste des switchs pas encore visités
            while (unvisitedSwitch.Count > 0)
            {
                // On parcourt les switchs voisins
                foreach (var sw in currentSwitch.connectedSwitch)
                {
                    // On ne check que les switch voisins pas encore visités
                    if (unvisitedSwitch.Contains(sw.Key))
                    {
                        // coût cumulé jusque-là + coût du switch
                        int distance = distances[currentSwitch] + sw.Value;

                        // Si le précédent coût total est supérieur au nouveau coût calculé, alors nous avons trouvé un chemin plus court
                        if (distances[sw.Key] > distance)
                        {
                            // On change la valeur et on met à jour le chemin de ce switch voisin
                            distances[sw.Key] = distance;

                            // On efface le précédent chemin enregistré
                            paths[sw.Key].Clear();

                            // On enregistre le nouveau chemin
                            paths[sw.Key].Add(this); // On commence par notre switch this

                            for (int i = 0; i < paths[currentSwitch].Count; ++i)
                            {
                                if (!paths[sw.Key].Contains(paths[currentSwitch][i]))
                                    paths[sw.Key].Add(paths[currentSwitch][i]);
                            }

                            paths[sw.Key].Add(sw.Key); // On termine par la destination
                        }
                    }
                }

                // On a fini de parcourir les voisins du switch étudié
                unvisitedSwitch.Remove(currentSwitch);

                // On cherche le nouveau switch à étudier avec la distance la plus faible
                int min = int.MaxValue;
                foreach (var sw in distances)
                {
                    // On ne check que les switchs pas encore visités
                    if (unvisitedSwitch.Contains(sw.Key))
                    {
                        if (min > sw.Value)
                        {
                            min = sw.Value;
                            // Nouveau switch ayant la distance la plus faible actuellement
                            currentSwitch = sw.Key;
                        }
                    }
                }
            }

            // Affichage des chemins les plus courts trouvés entre chaque switch et notre switch this
            for (int i = 0; i < paths.Keys.Count; ++i)
            {
                if (paths.ContainsKey(allSwitches[i]))
                {
                    Console.WriteLine($"\nPathing from {ipAddress.GetStringAddress()} to {allSwitches[i].ipAddress.GetStringAddress()}");
                    foreach (var item in paths[allSwitches[i]])
                    {
                        Console.WriteLine(item.ipAddress.GetStringAddress());
                    }
                }
            }

            Console.WriteLine("\n");

            // Affichage des distances du switch this à un autre
            foreach (var sw in distances)
                Console.WriteLine($"Total distance from {ipAddress.GetStringAddress()} to {sw.Key.ipAddress.GetStringAddress()} : {sw.Value}");

            // Dressage de la route table pour notre switch this
            for (int i = 0; i < allSwitches.Count; ++i)
            {
                // Si notre switch this est dans la même plage que le switch étudié
                if (allSwitches[i].ipAddress.GetAdressRange() == ipAddress.GetAdressRange())
                    routeTable[allSwitches[i].ipAddress.GetAdressRange()] = this; // this est donc la destination
                else
                {
                    // Sinon, on regarde dans notre map de chemins entre chaque switch et notre switch this
                    if (paths.TryGetValue(allSwitches[i], out List<Switch> value))
                    {
                        try
                        {
                            // Le switch-passerelle le plus proche dans notre chemin est stocké à l'indice 1, 0 étant le switch this lui-même
                            routeTable[allSwitches[i].ipAddress.GetAdressRange()] = value[1];
                        }
                        catch (ArgumentOutOfRangeException ex) // Mesure de précaution
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
