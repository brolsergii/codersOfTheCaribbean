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

    public Tuple<int, int> GetForward(int x = -1, int y = -1, bool first = true)
    {
        Tuple<int, int> res = null;
        if (x == -1) { x = X; }
        if (y == -1) { y = Y; }
        if (Orientation == 0)
            res = new Tuple<int, int>(x + 1, y);
        if (Orientation == 1)
            res = new Tuple<int, int>(y % 2 == 0 ? x : x + 1, y - 1);
        if (Orientation == 2)
            res = new Tuple<int, int>(y % 2 == 0 ? x - 1 : x, y - 1);
        if (Orientation == 3)
            res = new Tuple<int, int>(x - 1, y);
        if (Orientation == 4)
            res = new Tuple<int, int>(y % 2 == 0 ? x - 1 : x, y + 1);
        if (Orientation == 5)
            res = new Tuple<int, int>(y % 2 == 0 ? x : x + 1, y + 1);
        if (Speed == 2 && first)
            return GetForward(res.Item1, res.Item2, false);
        return res;
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
        if (ships.Where(s => s.X == x && s.Y == y && s.Player == 0).Any())
            return false;
        if (mines.Where(s => s.X == x && s.Y == y).Any())
            return false;
        if (cannonBalls.Where(s => s.X == x && s.Y == y && s.TimeLeft == 1).Any())
            return false;
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
        //Deb($"From ({x1},{y1}) to ({x2},{y2})");
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
            //Deb(cNode);
            res.Add(cNode);
            if (!fromTo.ContainsKey(cNode))
            {
                Deb($"No path found from ({x1},{y1}) to ({x2},{y2})");
                return new List<Tuple<int, int>>() { new Tuple<int, int>(x2, y2) };
            }
            cNode = fromTo[cNode];
        }
        res.Reverse();
        return res;
    }

    static string BestActionToApproach(Ship ship, int x, int y)
    {
        int bonus = int.MaxValue;
        string action = "WAIT";

        // There are 4 actions possible 
        // Faster
        int fasterBonus = 0;
        if (ship.Speed == 0)
        {
            fasterBonus -= 50;
        }
        if (ship.Speed == 1)
        {
            Ship s1 = new Ship(0, ship.Orientation, 2, 0, 0, ship.X, ship.Y);
            var newPost = s1.GetForward();
            s1.X = newPost.Item1;
            s1.Y = newPost.Item2;
            fasterBonus = s1.GetPositions().Select(c => HexagonDist(c.Item1, c.Item2, x, y)).Min();
        }
        if (ship.Speed == 2)
        {
            fasterBonus += 50;
        }
        if (bonus > fasterBonus)
        {
            Deb($"Faster bonus: {fasterBonus}");
            bonus = fasterBonus;
            action = "FASTER";
        }

        // Slower
        int slowerBonus = 0;
        if (ship.Speed < 2)
        {
            slowerBonus += 50;
        }
        else
        {
            Ship s1 = new Ship(0, ship.Orientation, 1, 0, 0, ship.X, ship.Y);
            var newPost = s1.GetForward();
            s1.X = newPost.Item1;
            s1.Y = newPost.Item2;
            slowerBonus = s1.GetPositions().Select(c => HexagonDist(c.Item1, c.Item2, x, y)).Min();
        }
        if (bonus > slowerBonus)
        {
            Deb($"Slower bonus: {slowerBonus}");
            bonus = fasterBonus;
            action = "SLOWER";
        }

        // Left
        Ship sLeft = new Ship(0, (ship.Orientation + 1) % 6, ship.Speed, 0, 0, ship.X, ship.Y);
        int leftBonus = sLeft.GetPositions().Select(c => HexagonDist(c.Item1, c.Item2, x, y)).Min();
        if (bonus > leftBonus)
        {
            Deb($"Left bonus: {leftBonus}");
            bonus = leftBonus;
            action = "PORT";
        }

        // Right
        Ship sRight = new Ship(0, (ship.Orientation - 1) % 6, ship.Speed, 0, 0, ship.X, ship.Y);
        int rightBonus = sRight.GetPositions().Select(c => HexagonDist(c.Item1, c.Item2, x, y)).Min();
        if (bonus > rightBonus)
        {
            Deb($"Right bonus: {rightBonus}");
            bonus = rightBonus;
            action = "STARBOARD";
        }

        return action;
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
            foreach (var myShip in ships.Where(x => x.Player == 1))
            {
                Deb($"Ship ({myShip.ID})");
                string action = "";

                // Save youself from cannonballs
                if (string.IsNullOrEmpty(action))
                {
                    var newShipPos = myShip.GetForward();
                    var newShip = new Ship(myShip.ID, myShip.Orientation, myShip.Speed, 0, 0, newShipPos.Item1, newShipPos.Item2);
                    var shipPos = newShip.GetPositions();
                    bool isSafe = true;
                    foreach (var sp in shipPos)
                        if (!CaseIsEmpty(sp.Item1, sp.Item2))
                            isSafe = false;
                    if (!isSafe)
                    {
                        newShip.Orientation = (newShip.Orientation + 1) % 6;
                        isSafe = true;
                        foreach (var sp in shipPos)
                            if (!CaseIsEmpty(sp.Item1, sp.Item2))
                                isSafe = false;
                        if (isSafe)
                            action = "PORT";
                        else
                        {
                            newShip.Orientation = (newShip.Orientation - 2) % 6;
                            isSafe = true;
                            foreach (var sp in shipPos)
                                if (!CaseIsEmpty(sp.Item1, sp.Item2))
                                    isSafe = false;
                            if (isSafe)
                                action = "STARBOARD";
                        }
                    }
                }

                // Deblock
                if (string.IsNullOrEmpty(action))
                {
                    if (oldPositions.ContainsKey(myShip.ID))
                    {
                        if (oldPositions[myShip.ID].Item1 == myShip.X && oldPositions[myShip.ID].Item2 == myShip.Y)
                        {
                            var nextCase = myShip.GetForward();
                            if (nextCase.Item1 < 0 || nextCase.Item1 > 22 || nextCase.Item2 < 0 || nextCase.Item2 > 20)
                                action = "PORT";
                            else if (!CaseIsEmpty(nextCase.Item1, nextCase.Item2))
                            {
                                action = "STARBOARD";
                            }
                            else
                                action = "FASTER";
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
                    if (bar != null) // Go for rhum
                    {
                        string bestAction = BestActionToApproach(myShip, bar.X, bar.Y);
                        action = bestAction;
                    }
                    else // No rhum left on the map
                    {

                        if (myShip.Rhum > ships.Where(x => x.Player == 0).Select(x => x.Rhum).Max())
                        {
                            // TODO: Avoid
                            var victim = FindClosestRival(myShip.X, myShip.Y);
                            action = $"MOVE {victim.X} {victim.Y}";
                        }
                        else
                        {
                            // Attack
                            var victim = FindClosestRival(myShip.X, myShip.Y);
                            action = $"MOVE {victim.X} {victim.Y}";
                        }
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