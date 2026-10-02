using Raylib_cs;

namespace Frogger;

public enum LaneType { Grass, Road, River }

// Något som glider i sidled på en bana: en bil på vägen eller en stock i floden.
public class Mover
{
    public float X;
    public int Width;
    public Color Color;
}

// En rad i världen. Gräs är säkert, väg har bilar, flod har stockar.
public class Lane
{
    public const int TileSize = 16;
    public const int Columns = 20;
    public const int ScreenWidth = Columns * TileSize;

    // Movers lever i ett område som är lite bredare än skärmen, så de glider in och ut ur bild
    public const float Margin = 32;
    public const float WrapLength = ScreenWidth + 2 * Margin;

    public LaneType Type;
    public int Row;
    public float Speed; // pixlar per sekund, negativt = åt vänster
    public bool Fast;   // en snabb "sportbilsbana", farligare och går att känna igen
    public List<Mover> Movers = new();

    public void Update(float dt)
    {
        foreach (var mover in Movers)
        {
            mover.X += Speed * dt;
            // Åker den ut på ena sidan dyker den upp på den andra
            if (mover.X >= ScreenWidth + Margin) mover.X -= WrapLength;
            else if (mover.X + mover.Width <= -Margin) mover.X += WrapLength;
        }
    }

    // Krockar grodan (vars vänsterkant är playerX) med någon bil?
    public bool HitsCar(float playerX)
    {
        if (Type != LaneType.Road) return false;
        float left = playerX + 3;
        float right = playerX + TileSize - 3;
        foreach (var mover in Movers)
            if (mover.X < right && mover.X + mover.Width > left) return true;
        return false;
    }

    // Står grodans mitt (centerX) på en stock?
    public bool IsOnLog(float centerX)
    {
        foreach (var mover in Movers)
            if (centerX >= mover.X && centerX <= mover.X + mover.Width) return true;
        return false;
    }
}
