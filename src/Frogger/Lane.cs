using Raylib_cs;

namespace Frogger;

public enum LaneType { Grass, Road }

public class Car
{
    public float X;
    public int Width;
    public Color Color;
}

// En rad i världen. Gräs är säkert, väg har bilar som rör sig i sidled.
public class Lane
{
    public const int TileSize = 16;
    public const int Columns = 20;

    // Bilar lever i ett område som är lite bredare än skärmen, så de glider in och ut ur bild
    public const float Margin = 32;
    public const float WrapLength = Columns * TileSize + 2 * Margin;

    public LaneType Type;
    public int Row;
    public float Speed; // pixlar per sekund, negativt = åt vänster
    public List<Car> Cars = new();

    public void Update(float dt)
    {
        foreach (var car in Cars)
        {
            car.X += Speed * dt;
            // Åker bilen ut på ena sidan dyker den upp på den andra
            if (car.X >= Columns * TileSize + Margin) car.X -= WrapLength;
            else if (car.X + car.Width <= -Margin) car.X += WrapLength;
        }
    }

    // Krockar grodan (som står i kolumn col) med någon bil?
    public bool HitsCar(int col)
    {
        if (Type != LaneType.Road) return false;
        float left = col * TileSize + 3;
        float right = left + TileSize - 6;
        foreach (var car in Cars)
            if (car.X < right && car.X + car.Width > left) return true;
        return false;
    }
}
