namespace Frogger;

// Grodan hoppar en ruta åt gången. X är i pixlar (inte kolumn) eftersom
// grodan följer med en stock i floden. Row räknas uppåt från startraden (0).
public class Player
{
    public float X = Lane.Columns / 2 * Lane.TileSize;
    public int Row;
    public int BestRow; // längsta raden hittills = poängen

    public float CenterX => X + Lane.TileSize / 2f;

    public void Hop(int dCol, int dRow)
    {
        X = Math.Clamp(X + dCol * Lane.TileSize, 0, Lane.ScreenWidth - Lane.TileSize);
        Row = Math.Max(0, Row + dRow);
        BestRow = Math.Max(BestRow, Row);
    }

    // På fast mark landar grodan exakt i en ruta igen
    public void SnapToGrid()
    {
        X = MathF.Round(X / Lane.TileSize) * Lane.TileSize;
        X = Math.Clamp(X, 0, Lane.ScreenWidth - Lane.TileSize);
    }
}
