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

    public List<Tuple<int, int>> GetPositionsNoTail()
    {
        List<Tuple<int, int>> positions = new List<Tuple<int, int>>();
        positions.Add(new Tuple<int, int>(X, Y));
        if (Orientation == 0)
            positions.Add(new Tuple<int, int>(X + 1, Y));
        if (Orientation == 3)
            positions.Add(new Tuple<int, int>(X - 1, Y));

        if (Orientation == 1)
        {
            if (Y % 2 == 0)
                positions.Add(new Tuple<int, int>(X, Y - 1));
            else
                positions.Add(new Tuple<int, int>(X + 1, Y - 1));
        }
        if (Orientation == 4)
        {
            if (Y % 2 == 0)
                positions.Add(new Tuple<int, int>(X - 1, Y + 1));
            else
                positions.Add(new Tuple<int, int>(X, Y + 1));
        }
        if (Orientation == 2)
        {
            if (Y % 2 == 0)
                positions.Add(new Tuple<int, int>(X - 1, Y - 1));
            else
                positions.Add(new Tuple<int, int>(X, Y - 1));
        }
        if (Orientation == 5)
        {
            if (Y % 2 == 0)
                positions.Add(new Tuple<int, int>(X, Y + 1));
            else
                positions.Add(new Tuple<int, int>(X + 1, Y + 1));
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
    static int HexagonDist(int x1, int y1, int x2, int y2, int orientation = -1)
    {
        int a1 = x1 - (int)Math.Floor((double)y1 / 2);
        int b1 = y1;
        int a2 = x2 - (int)Math.Floor((double)y2 / 2);
        int b2 = y2;
        int dx = a1 - a2;
        int dy = b1 - b2;

        return Math.Max(Math.Abs(dx), Math.Max(Math.Abs(dy), Math.Abs(dx + dy)))
               + ((orientation == -1) ? 0 : 3 * Math.Abs(orientation - GetOrientation(x1, y1, x2, y2)));
    }
    #endregion

    #region Static global game state
    static int MaxFireDistance = 4;
    static List<Ship> ships = new List<Ship>();
    static List<Barrel> barrels = new List<Barrel>();
    static List<Mine> mines = new List<Mine>();
    static List<CannonBall> cannonBalls = new List<CannonBall>();
    static Dictionary<int, Tuple<int, int>> oldPositions = new Dictionary<int, Tuple<int, int>>();
    static Dictionary<int, int> mineCooldown = new Dictionary<int, int>();
    #endregion

    static Barrel FindClosestBarrel(int x, int y, int currentOrientation)
    {
        Barrel res = null;
        int minDist = int.MaxValue;
        foreach (var bar in barrels)
        {
            int dist = HexagonDist(x, y, bar.X, bar.Y, currentOrientation);
            if (dist < minDist)
            {
                minDist = dist;
                res = bar;
            }
        }
        return res;
    }

    static Ship FindClosestRival(int x, int y, int currentOrientation)
    {
        Ship res = null;
        int minDist = int.MaxValue;
        foreach (var ship in ships.Where(s => s.Player == 0))
        {
            int dist = HexagonDist(x, y, ship.X, ship.Y, currentOrientation);
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

    static int GetOrientation(int x1, int y1, int x2, int y2)
    {

        if (x2 >= x1 && y1 == y2)
            return 0;
        if (x2 < x1 && y1 == y2)
            return 3;
        if (y1 % 2 == 0)
        {
            if (x2 == x1)
            {
                if (y2 < y1)
                    return 1;
                else
                    return 5;
            }
            else
            {
                if (y2 < y1)
                    return 2;
                else
                    return 4;
            }
        }
        else
        {
            if (x2 == x1)
            {
                if (y2 < y1)
                    return 2;
                else
                    return 4;
            }
            else
            {
                if (y2 < y1)
                    return 1;
                else
                    return 5;
            }
        }
    }

    static int GetNextOrientation(int orientation, int diff)
    {
        int resOrientation = (orientation + diff) % 6;
        if (resOrientation == -1)
            resOrientation = 5;
        return resOrientation;
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

    static string BestActionToApproach(Ship ship, int x, int y, int direction = 1)
    {
        int bonus = int.MaxValue;
        string action = "MINE";
        Tuple<int, int> newPos;
        Ship s1 = null;
        // There are 5 actions possible 
        // Faster
        int fasterBonus = 0;
        s1 = new Ship(0, ship.Orientation, 2, 0, 0, ship.X, ship.Y);
        if (ship.Speed == 0)
        {
            fasterBonus -= 50;
        }
        if (ship.Speed == 1)
        {
            newPos = s1.GetForward();
            s1.X = newPos.Item1;
            s1.Y = newPos.Item2;
            fasterBonus = direction * s1.GetPositions().Select(c => HexagonDist(c.Item1, c.Item2, x, y, s1.Orientation)).Min();
            fasterBonus += GetDamageInPos(s1);
        }
        if (ship.Speed == 2)
        {
            fasterBonus += 50;
        }
        Deb($"Faster bonus: {fasterBonus} including damage ({GetDamageInPos(s1)}) | {s1.Orientation}");
        DebList(s1.GetPositions());
        if (bonus > fasterBonus)
        {
            bonus = fasterBonus;
            action = "FASTER";
        }

        // Slower
        int slowerBonus = 0;
        s1 = new Ship(0, ship.Orientation, 1, 0, 0, ship.X, ship.Y);
        if (ship.Speed < 2)
        {
            slowerBonus += 50;
        }
        else
        {
            newPos = s1.GetForward();
            s1.X = newPos.Item1;
            s1.Y = newPos.Item2;
            slowerBonus = direction * s1.GetPositions().Select(c => HexagonDist(c.Item1, c.Item2, x, y, s1.Orientation)).Min();
            slowerBonus += GetDamageInPos(s1);
        }
        Deb($"Slower bonus: {slowerBonus} including damage ({GetDamageInPos(s1)}) | {s1.Orientation}");
        DebList(s1.GetPositions());
        if (bonus > slowerBonus)
        {
            bonus = slowerBonus;
            action = "SLOWER";
        }

        // Left
        s1 = new Ship(0, ship.Orientation, ship.Speed, 0, 0, ship.X, ship.Y);
        newPos = s1.GetForward();
        s1.X = newPos.Item1;
        s1.Y = newPos.Item2;
        int leftBonus = direction * s1.GetPositions().Select(c => HexagonDist(c.Item1, c.Item2, x, y, GetNextOrientation(s1.Orientation, +1))).Min();
        leftBonus += GetDamageInPos(s1);
        Deb($"Left bonus: {leftBonus} including damage ({GetDamageInPos(s1)}) | {GetNextOrientation(s1.Orientation, +1)}");
        DebList(s1.GetPositions());
        if (bonus > leftBonus)
        {
            bonus = leftBonus;
            action = "PORT";
        }

        // Right
        s1 = new Ship(0, ship.Orientation, ship.Speed, 0, 0, ship.X, ship.Y);
        newPos = s1.GetForward();
        s1.X = newPos.Item1;
        s1.Y = newPos.Item2;
        int rightBonus = direction * s1.GetPositions().Select(c => HexagonDist(c.Item1, c.Item2, x, y, GetNextOrientation(s1.Orientation, -1))).Min();
        rightBonus += GetDamageInPos(s1);
        Deb($"Right bonus: {rightBonus} including damage ({GetDamageInPos(s1)}) | {GetNextOrientation(s1.Orientation, -1)}");
        DebList(s1.GetPositions());
        if (bonus > rightBonus)
        {
            bonus = rightBonus;
            action = "STARBOARD";
        }

        // Wait
        s1 = new Ship(0, ship.Orientation, ship.Speed, 0, 0, ship.X, ship.Y);
        newPos = s1.GetForward();
        s1.X = newPos.Item1;
        s1.Y = newPos.Item2;
        int waitBonus = direction * s1.GetPositions().Select(c => HexagonDist(c.Item1, c.Item2, x, y, ship.Orientation)).Min();
        waitBonus += GetDamageInPos(s1);
        Deb($"Wait bonus: {waitBonus} including damage ({GetDamageInPos(s1)}) | {s1.Orientation}");
        DebList(s1.GetPositions());
        if (bonus > waitBonus)
        {
            bonus = waitBonus;
            action = "WAIT";
        }

        return action;
    }

    static int GetDamageInPos(Ship s)
    {
        int damage = 0;
        foreach (var pos in s.GetPositions())
        {
            if (mines.Where(x => x.X == s.X && x.Y == s.Y).Any())
                damage += 60;
            if (cannonBalls.Where(x => x.X == s.X && x.Y == s.Y && x.TimeLeft <= 1).Any())
                damage += 30;
            if (pos.Item1 > 21 || pos.Item1 < 0 || pos.Item2 < 0 || pos.Item2 > 19)
            {
                damage += 10;
            }
        }
        return damage;
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
            #region init parse state
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
            #endregion
            foreach (var myShip in ships.Where(x => x.Player == 1))
            {
                Deb($"Ship ({myShip.ID}) ({myShip})");
                string action = "";

                if (!mineCooldown.ContainsKey(myShip.ID))
                    mineCooldown[myShip.ID] = 0;
                else
                    mineCooldown[myShip.ID]--;

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
                    Barrel bar = FindClosestBarrel(myShip.X, myShip.Y, myShip.Orientation);
                    if (bar != null) // Go for rhum
                    {
                        Deb($"Looking for rhum in ({bar.X},{bar.Y})");
                        string bestAction = BestActionToApproach(myShip, bar.X, bar.Y);
                        action = bestAction;
                    }
                    else // No rhum left on the map
                    {
                        if (myShip.Rhum > ships.Where(x => x.Player == 0).Select(x => x.Rhum).Max())
                        {
                            Deb($"My position is better, hiding from the enemy");
                            // Avoid (best action to hide)
                            if (mineCooldown[myShip.ID] <= 0)
                            {
                                Deb("Mine the escapeway");
                                action = "MINE";
                                mineCooldown[myShip.ID] = 4;
                            }
                            else
                            {
                                var victim = FindClosestRival(myShip.X, myShip.Y, myShip.Orientation);
                                string bestAction = BestActionToApproach(myShip, victim.X, victim.Y, -1);
                                action = bestAction;
                            }
                        }
                        else
                        {
                            // Attack
                            Deb($"My position is better, attacking the enemy");
                            var victim = FindClosestRival(myShip.X, myShip.Y, myShip.Orientation);
                            string bestAction = BestActionToApproach(myShip, victim.X, victim.Y);
                            action = bestAction;
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