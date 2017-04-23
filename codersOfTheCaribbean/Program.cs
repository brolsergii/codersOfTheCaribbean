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

    public List<Tuple<int, int>> GetPositions(bool withForward = false)
    {
        List<Tuple<int, int>> positions = new List<Tuple<int, int>>();
        positions.Add(new Tuple<int, int>(X, Y));
        if (withForward)
            positions.Add(GetForward());
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
        return positions.Distinct().ToList();
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
        if (Speed == 0)
            return new Tuple<int, int>(X, Y);
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

    public List<Tuple<int, int>> GetInFront()
    {
        Ship newShip = new Ship(-1, Orientation, Speed, 0, 0, X, Y);
        var newPos = newShip.GetForward();
        newShip.X = newPos.Item1;
        newShip.Y = newPos.Item2;
        return newShip.GetPositions();
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
        Used = false;
    }

    public int Capasity;
    public int X;
    public int Y;
    public bool Used;

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

class MainPlayer
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
               + ((orientation == -1) ? 0 : 3 * OrientationDist(orientation, GetOrientation(x1, y1, x2, y2)));
    }
    static int OrientationDist(int or1, int or2)
    {
        int min = (or1 < or2) ? or1 : or2;
        int max = (or1 < or2) ? or2 : or1;
        if (min == 1 && max == 5)
            return 2;
        if (min == 0 && max >= 4)
            return 6 - max;
        return max - min;
    }
    #endregion

    #region Static global game state
    static int MaxFireDistance = 5;
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
        foreach (var bar in barrels.Where(b => !b.Used))
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
        foreach (var rivalShip in ships.Where(s => s.Player == 0))
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

    static bool CaseIsEmpty(int x, int y, int id = -1)
    {
        if (x < 0 || y < 0 || x > 22 || y > 20)
            return false;
        if (ships.Where(s => s.ID != id && s.GetPositions().Select(xp => xp.Item1).Contains(x) && s.GetPositions().Select(xp => xp.Item2).Contains(y)).Any())
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

    public static Tuple<Ship, int> GetShipsNextTurn(Ship s1, int x, int y, int newOrientation, int direction = 1)
    {
        int bonus = 0;
        Tuple<int, int> newPos = s1.GetForward();
        s1.X = newPos.Item1;
        s1.Y = newPos.Item2;
        s1.Orientation = newOrientation;
        bonus = direction * s1.GetPositions().Select(c => HexagonDist(c.Item1, c.Item2, x, y, s1.Orientation)).Min();
        bonus += GetDamageInPos(s1);

        return new Tuple<Ship, int>(s1, bonus);
    }

    static Tuple<string, int> BestActionToApproach(Ship ship, int x, int y, int direction = 1)
    {
        int bonus = int.MaxValue;
        string action = "MINE";
        Ship s1 = null;
        // There are 5 actions possible 
        // Faster
        int fasterBonus = 0;
        s1 = new Ship(0, ship.Orientation, 2, 0, 0, ship.X, ship.Y);
        if (ship.Speed == 0)
            fasterBonus -= 50;
        if (ship.Speed == 1)
            fasterBonus = GetShipsNextTurn(s1, x, y, s1.Orientation, direction).Item2;
        if (ship.Speed == 2 || (HexagonDist(x, y, s1.X, s1.Y) <= 2 && ship.Speed > 0))
            fasterBonus += 50;
        Deb($"Faster bonus: {fasterBonus} including damage ({GetDamageInPos(s1)}) | {s1.Orientation}");
        DebList(s1.GetPositions(true));
        if (bonus > fasterBonus)
        {
            bonus = fasterBonus;
            action = "FASTER";
        }

        // Slower
        int slowerBonus = 0;
        s1 = new Ship(0, ship.Orientation, ship.Speed - 1, 0, 0, ship.X, ship.Y);
        if (ship.Speed < 2)
            slowerBonus += 50;
        else
            slowerBonus = GetShipsNextTurn(s1, x, y, s1.Orientation, direction).Item2;
        Deb($"Slower bonus: {slowerBonus} including damage ({GetDamageInPos(s1)}) | {s1.Orientation}");
        DebList(s1.GetPositions(true));
        if (bonus > slowerBonus)
        {
            bonus = slowerBonus;
            action = "SLOWER";
        }

        // Left
        s1 = new Ship(0, ship.Orientation, ship.Speed, 0, 0, ship.X, ship.Y);
        int leftBonus = GetShipsNextTurn(s1, x, y, GetNextOrientation(s1.Orientation, +1), direction).Item2;
        Deb($"Left bonus: {leftBonus} including damage ({GetDamageInPos(s1)}) | {GetNextOrientation(s1.Orientation, +1)}");
        DebList(s1.GetPositions(true));
        if (bonus > leftBonus)
        {
            bonus = leftBonus;
            action = "PORT";
        }

        // Right
        s1 = new Ship(0, ship.Orientation, ship.Speed, 0, 0, ship.X, ship.Y);
        int rightBonus = GetShipsNextTurn(s1, x, y, GetNextOrientation(s1.Orientation, -1), direction).Item2;
        Deb($"Right bonus: {rightBonus} including damage ({GetDamageInPos(s1)}) | {GetNextOrientation(s1.Orientation, -1)}");
        DebList(s1.GetPositions(true));
        if (bonus > rightBonus)
        {
            bonus = rightBonus;
            action = "STARBOARD";
        }

        // Wait
        s1 = new Ship(0, ship.Orientation, ship.Speed, 0, 0, ship.X, ship.Y);
        int waitBonus = GetShipsNextTurn(s1, x, y, s1.Orientation, direction).Item2;
        Deb($"Wait bonus: {waitBonus} including damage ({GetDamageInPos(s1)}) | {s1.Orientation}");
        DebList(s1.GetPositions(true));
        if (bonus > waitBonus)
        {
            bonus = waitBonus;
            action = "WAIT";
        }

        return new Tuple<string, int>(action, direction == -1 ? waitBonus : bonus);
    }

    static int GetDamageInPos(Ship s)
    {
        int damage = 0;
        foreach (var pos in s.GetPositions(true))
        {
            if (mines.Where(x => x.X == pos.Item1 && x.Y == pos.Item2).Any())
                damage += 60;
            if (cannonBalls.Where(x => x.X == pos.Item1 && x.Y == pos.Item2 && x.TimeLeft <= 2).Any())
                damage += 25;
            foreach (var ship in ships.Where(s2 => s2.ID != s.ID))
            {
                if (HexagonDist(s.X, s.Y, ship.X, ship.Y) < 1)
                    damage += 1;
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
                Deb($"==> Ship ({myShip.ID}) ({myShip})");
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
                            Deb($"The ship is blocked, trying to unblock {oldPositions[myShip.ID]}");
                            Ship nextShip = new Ship(0, myShip.Orientation, 1, 0, 0, myShip.X, myShip.Y);
                            var nextCase = nextShip.GetForward();
                            nextShip.X = nextCase.Item1;
                            nextShip.Y = nextCase.Item2;
                            var frontCase = nextShip.GetForward();
                            if (CaseIsEmpty(nextCase.Item1, nextCase.Item2, myShip.ID) &&
                                CaseIsEmpty(frontCase.Item1, frontCase.Item2, myShip.ID))
                            {
                                Deb($"Case in front is Empty ({nextCase.Item1},{nextCase.Item2} && ({frontCase.Item1},{frontCase.Item2}))");
                                action = "FASTER";
                            }
                            else
                            {
                                Deb($"Case in front is not Empty ({nextCase.Item1},{nextCase.Item2})");
                                //var positions = myShip.GetPositionsNoTail();
                                //int orientation = GetOrientation(positions[0].Item1, positions[0].Item2, positions[1].Item1, positions[1].Item2);
                                //Deb($"Front {positions[0]} middle {positions[1]}");

                                if ((myShip.X > 10 && myShip.Y < 10) || (myShip.X < 10 && myShip.Y > 10))
                                    action = "STARBOARD";
                                else
                                    action = "PORT";
                            }

                        }
                    }
                }
                /*
                // Attack rival barrels
                if (string.IsNullOrEmpty(action))
                {
                    foreach (var bar in barrels)
                    {
                        int dist = HexagonDist(bar.X, bar.Y, myShip.X, myShip.Y);
                        if (dist > MaxFireDistance)
                            continue;
                        var closestRival = ships.Where(s => s.Player == 0 && HexagonDist(bar.X, bar.Y, s.X, s.Y) <= dist);
                        if (closestRival.Any())
                        {
                            bar.Used = true;
                            Deb($"Need to drop enemy's barrel at ({bar.X},{bar.Y})");
                            var closestRivalShip = closestRival.First();
                            var actionCandidat = BestActionToApproach(myShip, closestRivalShip.X, closestRivalShip.Y, (barrels.Count == 0 ? 1 : -1));
                            Ship nextShip = GetShipsNextTurn(myShip, closestRivalShip.X, closestRivalShip.Y, myShip.Orientation, -1).Item1;
                            Deb($"Considering to {actionCandidat.Item1} ({actionCandidat.Item2})");
                            if (actionCandidat.Item2 < 15) // No bomb or mine on the way
                            {
                                Deb($"Enemy is close to a barrel at ({bar.X},{bar.Y})");
                                action = $"FIRE {bar.X} {bar.Y}";
                            }
                        }
                    }
                }*/

                // Attack rival ships 
                if (string.IsNullOrEmpty(action))
                {
                    Ship rivalShip = FindShipToFire(myShip.X, myShip.Y);
                    if (rivalShip != null)
                    {
                        var actionCandidat = BestActionToApproach(myShip, rivalShip.X, rivalShip.Y, (barrels.Count == 0 ? 1 : -1));
                        Ship nextShip = GetShipsNextTurn(myShip, rivalShip.X, rivalShip.Y, myShip.Orientation, -1).Item1;
                        Deb($"Considering to {actionCandidat.Item1} ({actionCandidat.Item2})");
                        //var nextActionCandidat = BestActionToApproach(nextShip, rivalShip.X, rivalShip.Y, -1);
                        if (actionCandidat.Item2 > 15) // Bomb or mine on the way
                        {
                            Deb($"Enemy is close ({rivalShip.ID}) but it's too dangerous {actionCandidat.Item2}");
                            action = actionCandidat.Item1;
                        }
                        else
                        {
                            var frontOfRivalShip = rivalShip.GetForward();
                            Deb($"Enemy is close ({rivalShip.ID})...Fire!");
                            action = $"FIRE {frontOfRivalShip.Item1} {frontOfRivalShip.Item2}";
                        }
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
                        bar.Used = true; // Avoid mutual blocks
                        string bestAction = BestActionToApproach(myShip, bar.X, bar.Y).Item1;
                        action = bestAction;
                    }
                    else // No rhum left on the map
                    {
                        if (myShip.Rhum > ships.Where(x => x.Player == 0).Select(x => x.Rhum).Max())
                        {
                            // Avoid (best action to hide)
                            var victim = FindClosestRival(myShip.X, myShip.Y, myShip.Orientation);
                            Ship nextShip = GetShipsNextTurn(myShip, victim.X, victim.Y, myShip.Orientation, -1).Item1;
                            var bestActionCandidat = BestActionToApproach(nextShip, victim.X, victim.Y, -1);
                            Deb($"My position is better, hiding from the enemy");
                            if (mineCooldown[myShip.ID] <= 0 && bestActionCandidat.Item2 < 20) // no bomb or mine on the way
                            {
                                Deb("Mine the escapeway");
                                action = "MINE";
                                mineCooldown[myShip.ID] = 4;
                            }
                            else
                            {
                                action = bestActionCandidat.Item1;
                            }
                        }
                        else
                        {
                            // Attack
                            Deb($"My position is bad, attacking the enemy");
                            var victim = FindClosestRival(myShip.X, myShip.Y, myShip.Orientation);
                            string bestAction = BestActionToApproach(myShip, victim.X, victim.Y).Item1;
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