using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Ship
{
    public Ship(int orientation, int speed, int rhum, int player, int x, int y)
    {
        Orientation = orientation;
        Speed = speed;
        Rhum = rhum;
        Player = player;
        X = x;
        Y = y;
    }

    public int Orientation;
    public int Speed;
    public int Rhum;
    public int Player;
    public int X;
    public int Y;
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
    #endregion

    #region Static global game state
    static int[,] MAP;
    static List<Ship> ships = new List<Ship>();
    static List<Barrel> barrels = new List<Barrel>();
    #endregion

    static void Main(string[] args)
    {
        while (true)
        {
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
            }
            for (int i = 0; i < myShipCount; i++)
            {
                Console.WriteLine("MOVE 11 10"); // Any valid action, such as "WAIT" or "MOVE x y"
            }
        }
    }
}