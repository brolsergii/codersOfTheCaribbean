using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Ship
{
    public Ship(int id, int orientation, int speed, int rhum, int player, int x, int y)
    {
        Orientation = orientation;
        Speed = speed;
        Rhum = rhum;
        Player = player;
        X = x;
        Y = y;
        ID = id;
        UnderAttack = false;
    }

    public List<Tuple<int, int>> GetPositions()
    {
        List<Tuple<int, int>> positions = new List<Tuple<int, int>>();
        positions.Add(new Tuple<int, int>(X, Y));
        if (Orientation == 0 || Orientation == 3)
        {
            positions.Add(new Tuple<int, int>(X - 1, Y));
            positions.Add(new Tuple<int, int>(X + 1, Y));
        }
        if (Orientation == 1 || Orientation == 4)
        {
            if (Y % 2 == 0)
            {
                positions.Add(new Tuple<int, int>(X, Y - 1));
                positions.Add(new Tuple<int, int>(X - 1, Y + 1));
            }
            else
            {
                positions.Add(new Tuple<int, int>(X + 1, Y - 1));
                positions.Add(new Tuple<int, int>(X, Y + 1));
            }
        }
        if (Orientation == 2 || Orientation == 5)
        {
            if (Y % 2 == 0)
            {
                positions.Add(new Tuple<int, int>(X - 1, Y - 1));
                positions.Add(new Tuple<int, int>(X, Y + 1));
            }
            else
            {
                positions.Add(new Tuple<int, int>(X, Y - 1));
                positions.Add(new Tuple<int, int>(X + 1, Y + 1));
            }
        }
        return positions;
    }

    public Tuple<int, int> GetForward()
    {
        //if (Orientation == 0)
        return null;
    }

    public int Orientation;
    public int Speed;
    public int Rhum;
    public int Player;
    public int X;
    public int Y;
    public int ID;
    public bool UnderAttack;

    public override string ToString() => $"{Player}) ¤{Orientation}, ->{Speed}, rhum {Rhum} at ({X},{Y})";

}

class Barrel
{
    public Barrel(int capasity, int x, int y)
    {
        Capasity = capasity;
        X = x;
        Y = y;
    }

    public int Capasity;
    public int X;
    public int Y;

    public override string ToString() => $"{Capasity} at ({X},{Y})";
}

class Mine
{
    public Mine(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int X;
    public int Y;
}

class CannonBall
{
    public CannonBall(int shipId, int timeLeft, int x, int y)
    {
        ShipId = shipId;
        TimeLeft = timeLeft;
        X = x;
        Y = y;
    }

    public int ShipId;
    public int TimeLeft;
    public int X;
    public int Y;
}

class Player
{
    #region Auxilary methods
    static void TryCatch(Action a) { try { a(); } catch (Exception) { } }
    static int TryGetInt(Func<int> a) { try { return a(); } catch (Exception) { return 0; } }
    static void Deb(object o) => Console.Error.WriteLine(o);
    static void DebList(IEnumerable<object> e) => Console.Error.WriteLine(e.Aggregate((x, y) => $"{x} {y}"));
    static void DebObjList(IEnumerable<object> e) => TryCatch(() => Console.Error.WriteLine(e.Aggregate((x, y) => $"{x}\n{y}")));
    static void DebDict(Dictionary<int, int> d)
    {
        foreach (var pair in d)
            Console.Error.WriteLine($"[{pair.Key}]:{pair.Value}");
    }
    static int HexagonDist(int x1, int y1, int x2, int y2)
    {
        int a1 = x1 - (int)Math.Floor((double)y1 / 2);
        int b1 = y1;
        int a2 = x2 - (int)Math.Floor((double)y2 / 2);
        int b2 = y2;
        int dx = a1 - a2;
        int dy = b1 - b2;
        return Math.Max(Math.Abs(dx), Math.Max(Math.Abs(dy), Math.Abs(dx + dy)));
    }
    #endregion

    #region Static global game state
    static int MaxFireDistance = 4;
    static List<Ship> ships = new List<Ship>();
    static List<Barrel> barrels = new List<Barrel>();
    static List<Mine> mines = new List<Mine>();
    static List<CannonBall> cannonBalls = new List<CannonBall>();
    static Dictionary<int, Tuple<int, int>> oldPositions = new Dictionary<int, Tuple<int, int>>();
    #endregion

    static Barrel FindClosestBarrel(int x, int y)
    {
        Barrel res = null;
        int minDist = int.MaxValue;
        foreach (var bar in barrels)
        {
            int dist = HexagonDist(x, y, bar.X, bar.Y);
            if (dist < minDist)
            {
                minDist = dist;
                res = bar;
            }
        }
        return res;
    }

    static Ship FindClosestRival(int x, int y)
    {
        Ship res = null;
        int minDist = int.MaxValue;
        foreach (var ship in ships.Where(s => s.Player == 0))
        {
            int dist = HexagonDist(x, y, ship.X, ship.Y);
            if (dist < minDist)
            {
                minDist = dist;
                res = ship;
            }
        }
        return res;
    }

    static Ship FindShipToFire(int x, int y)
    {
        Ship res = null;
        int dist = int.MaxValue;
        foreach (var rivalShip in ships.Where(s => s.Player == 0 && !s.UnderAttack))
        {
            int newDist = HexagonDist(x, y, rivalShip.X, rivalShip.Y);
            if (newDist < dist && newDist <= MaxFireDistance)
            {
                dist = newDist;
                //rivalShip.UnderAttack = true; // no need
                res = rivalShip;
            }
        }
        return res;
    }

    static bool CaseIsEmpty(int x, int y)
    {
        if (ships.Where(s => s.X == x && s.Y == y).Any())
            return false;
        if (mines.Where(s => s.X == x && s.Y == y).Any())
            return false;
        //if (cannonBalls.Where(s => s.X == x && s.Y == y && s.TimeLeft == 1).Any())
        //    return false;
        return true;
    }

    static Tuple<int, int> MoveCanProvokeMineExplosion(int x1, int y1, int x2, int y2)
    {
        int x3 = x2;
        int y3 = y2;
        if (x2 - x1 == 1 && y1 == y2)
        {
            x3 = x2 + 1;
        }
        if (x2 - x1 == 1 && y1 == y2)
        {
            x3 = x2 - 1;
        }
        if (y1 % 2 == 0)
        {
            if (x2 - x1 == -1)
            {
                if (y2 - y1 == -1)
                {
                    x3 = x2 - 1;
                    y3 = y2 - 1;
                }
                if (y2 - y1 == 1)
                {
                    x3 = x2 - 1;
                    y3 = y2 + 1;
                }
            }
            if (x2 - x1 == 0)
            {
                if (y2 - y1 == -1)
                {
                    y3 = y2 - 1;
                }
                if (y2 - y1 == 1)
                {
                    y3 = y2 + 1;
                }
            }
        }
        if (y1 % 2 == 1)
        {
            if (x2 - x1 == 1)
            {
                if (y2 - y1 == -1)
                {
                    x3 = x2 + 1;
                    y3 = y2 - 1;
                }
                if (y2 - y1 == 1)
                {
                    x3 = x2 + 1;
                    y3 = y2 + 1;
                }
            }
            if (x2 - x1 == 0)
            {
                if (y2 - y1 == -1)
                {
                    y3 = y2 - 1;
                }
                if (y2 - y1 == 1)
                {
                    y3 = y2 + 1;
                }
            }
        }
        var bombOnTheWay = mines.Where(x => x.X == x3 && x.Y == y3);
        if (bombOnTheWay.Any())
            bombOnTheWay.FirstOrDefault();
        return null;
    }

    static List<Tuple<int, int>> GetNextNodes(int x, int y)
    {
        var list = new List<Tuple<int, int>>();
        if (x > 0)
            list.Add(new Tuple<int, int>(x - 1, y));
        if (x < 22)
            list.Add(new Tuple<int, int>(x + 1, y));
        if (y > 0)
            list.Add(new Tuple<int, int>(x, y - 1));
        if (y < 20)
            list.Add(new Tuple<int, int>(x, y + 1));
        if (y % 2 == 0)
        {
            if (x > 0 && y > 0)
                list.Add(new Tuple<int, int>(x - 1, y - 1));
            if (x > 0 && y < 20)
                list.Add(new Tuple<int, int>(x - 1, y + 1));
        }
        if (y % 2 == 1)
        {
            if (x < 22 && y < 20)
                list.Add(new Tuple<int, int>(x + 1, y + 1));
            if (x < 22 && y > 0)
                list.Add(new Tuple<int, int>(x + 1, y - 1));
        }
        return list;
    }

    static List<Tuple<int, int>> GetPath(int x1, int y1, int x2, int y2)
    {
        Deb($"From ({x1},{y1}) to ({x2},{y2})");
        List<Tuple<int, int>> allNodes = new List<Tuple<int, int>>();
        List<Tuple<int, int>> queueNodes = new List<Tuple<int, int>>();
        Dictionary<Tuple<int, int>, Tuple<int, int>> fromTo = new Dictionary<Tuple<int, int>, Tuple<int, int>>();
        allNodes.Add(new Tuple<int, int>(x1, y1));
        queueNodes.Add(new Tuple<int, int>(x1, y1));
        Tuple<int, int> currentNode;
        while (queueNodes.Count > 0)
        {
            currentNode = queueNodes.OrderBy(x => HexagonDist(x.Item1, x.Item2, x2, y2)).First();
            queueNodes.Remove(currentNode);
            if (currentNode.Item1 == x2 && currentNode.Item2 == y2)
            {
                break;
            }
            var nextNodes = GetNextNodes(currentNode.Item1, currentNode.Item2).Where(s => CaseIsEmpty(s.Item1, s.Item2));
            foreach (var node in nextNodes)
            {
                if (!allNodes.Contains(node))
                {
                    fromTo[node] = currentNode;
                    queueNodes.Add(node);
                    allNodes.Add(node);
                }
            }
        }
        var res = new List<Tuple<int, int>>();
        Tuple<int, int> cNode = new Tuple<int, int>(x2, y2);
        while (!(cNode.Item1 == x1 && cNode.Item2 == y1))
        {
            Deb(cNode);
            res.Add(cNode);
            if (!fromTo.ContainsKey(cNode))
                return new List<Tuple<int, int>>() { new Tuple<int, int>(x2, y2) };
            cNode = fromTo[cNode];
        }
        res.Reverse();
        return res;
    }

    static Tuple<int, int> GetClosestSafePosition(int x, int y)
    {
        var closeNodes = GetNextNodes(x, y);
        foreach (var closeNode in closeNodes)
        {
            var closeSubNodes = GetNextNodes(closeNode.Item1, closeNode.Item2);
            bool isSafe = true;
            foreach (var subNote in closeSubNodes)
            {
                if (cannonBalls.Where(n => n.X == subNote.Item1 && n.Y == subNote.Item2 && n.TimeLeft == 1).Any())
                    isSafe = false;
                if (mines.Where(n => n.X == subNote.Item1 && n.Y == subNote.Item2).Any())
                    isSafe = false;
            }
            if (isSafe)
                return new Tuple<int, int>(closeNode.Item1, closeNode.Item2);
        }
        return null;
    }

    static void Main(string[] args)
    {
        while (true)
        {
            ships.Clear();
            barrels.Clear();
            mines.Clear();
            cannonBalls.Clear();
            int myShipCount = int.Parse(Console.ReadLine()); // the number of remaining ships
            int entityCount = int.Parse(Console.ReadLine()); // the number of entities (e.g. ships, mines or cannonballs)
            for (int i = 0; i < entityCount; i++)
            {
                string[] inputs = Console.ReadLine().Split(' ');
                int entityId = int.Parse(inputs[0]);
                string entityType = inputs[1];
                int x = int.Parse(inputs[2]);
                int y = int.Parse(inputs[3]);
                int arg1 = int.Parse(inputs[4]);
                int arg2 = int.Parse(inputs[5]);
                int arg3 = int.Parse(inputs[6]);
                int arg4 = int.Parse(inputs[7]);
                switch (entityType)
                {
                    case "BARREL":
                        {
                            barrels.Add(new Barrel(arg1, x, y));
                            break;
                        }
                    case "SHIP":
                        {
                            ships.Add(new Ship(entityId, arg1, arg2, arg3, arg4, x, y));
                            break;
                        }
                    case "MINE":
                        {
                            mines.Add(new Mine(x, y));
                            break;
                        }
                    case "CANNONBALL":
                        {
                            cannonBalls.Add(new CannonBall(arg1, arg2, x, y));
                            break;
                        }
                    default:
                        {
                            Deb($"{entityType} is no recognized");
                            break;
                        }
                }
            }
            //Deb("Barels:");
            //DebObjList(barrels);
            foreach (var myShip in ships.Where(x => x.Player == 1))
            {
                Deb($"Ship ({myShip.ID})");
                string action = "";

                /*
                // Save youself fron cannonballs
                if (string.IsNullOrEmpty(action))
                {
                    bool cannonsOnMe = false;
                    foreach (var cannonBall in cannonBalls)
                    {
                        foreach (var pos in myShip.GetPositions())
                        {
                            if (cannonBall.X == pos.Item1 && cannonBall.Y == pos.Item2)
                            {
                                cannonsOnMe = true;
                                break;
                            }
                        }
                    }
                    if (cannonsOnMe)
                    {
                        Deb($"Ship ({myShip.ID}) is under attack");
                        // Run from cannonballs. Set into safe position
                        var safePosition = GetClosestSafePosition(myShip.X, myShip.Y);
                        if (safePosition != null)
                        {
                            Deb($"Going for a safe place in {safePosition}");
                            action = $"MOVE {safePosition.Item1} {safePosition.Item2}";
                        }
                        else
                        {
                            Deb($"Nowhere to hide from ({myShip.X},{myShip.Y}). Continue attack");
                        }
                    }
                }*/

                // Deblock
                if (string.IsNullOrEmpty(action))
                {
                    if (oldPositions.ContainsKey(myShip.ID))
                    {
                        if (oldPositions[myShip.ID].Item1 == myShip.X && oldPositions[myShip.ID].Item2 == myShip.Y)
                        {
                            action = $"MOVE {0} {0}"; //TODO: fixme
                        }
                    }
                }

                // Attack rival ships 
                if (string.IsNullOrEmpty(action))
                {
                    Ship rivalShip = FindShipToFire(myShip.X, myShip.Y);
                    if (rivalShip != null)
                    {
                        Deb($"Enemy is close ({rivalShip.ID})...Fire!");
                        action = $"FIRE {rivalShip.X} {rivalShip.Y}";
                    }
                    else
                    {
                        Deb($"No enemy is close");
                    }
                }

                // Go for rhum
                if (string.IsNullOrEmpty(action))
                {
                    Barrel bar = FindClosestBarrel(myShip.X, myShip.Y);
                    if (bar != null)
                    {
                        var pathToBar = GetPath(myShip.X, myShip.Y, bar.X, bar.Y);
                        DebList(pathToBar);
                        //var cannonballsOnTheWay = cannonBalls.Where(x => pathToBar.Where(y => y.Item1 == x.X && y.Item2 == x.Y).Any());
                        //if (cannonballsOnTheWay.Any())
                        //{
                        //    // Run from cannonballs. Set into safe position
                        //    var safePosition = GetClosestSafePosition(myShip.X, myShip.Y);
                        //    if (safePosition != null)
                        //    {
                        //        Deb($"Going for a safe place in {safePosition}");
                        //        action = $"MOVE {safePosition.Item1} {safePosition.Item2}";
                        //    }
                        //    else
                        //    {
                        //        Deb($"Nowhere to hide from ({myShip.X},{myShip.Y})");
                        //    }
                        //}
                        //else
                        //{
                        var pathToBarNextMove = pathToBar.FirstOrDefault();
                        var mineOnTheWay = mines.Where(x => pathToBar.Where(y => y.Item1 == x.X && y.Item2 == x.Y).Any());
                        if (mineOnTheWay.Any())
                        {
                            // Shoot the mine on the way
                            var theMine = mineOnTheWay.FirstOrDefault();
                            action = $"FIRE {theMine.X} {theMine.Y}";
                        }
                        else
                        {
                            /*var bombOnTheWay = MoveCanProvokeMineExplosion(myShip.X, myShip.Y, pathToBarNextMove.Item1, pathToBarNextMove.Item2);
                            if (bombOnTheWay != null)
                            {
                                Deb($"Has a bomb on the way");
                                // The move can provoke a bomb explosion
                                action = $"FIRE {bombOnTheWay.Item1} {bombOnTheWay.Item2}";
                            }*/
                            // else
                            // {
                            // The way is clear
                            action = $"MOVE {bar.X} {bar.Y}";
                            // }
                        }
                        //}
                    }
                    else
                    {
                        // No rhum left on the map. Go hunt 
                        var victim = FindClosestRival(myShip.X, myShip.Y);
                        action = $"MOVE {victim.X} {victim.Y}";
                    }
                }

                if (string.IsNullOrEmpty(action))
                    action = "WAIT"; // Nothing to do
                Console.WriteLine(action);
                oldPositions[myShip.ID] = new Tuple<int, int>(myShip.X, myShip.Y);
            }
        }
    }
}