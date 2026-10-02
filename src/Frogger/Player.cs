namespace Frogger;

// Grodan hoppar en ruta åt gången. Row räknas uppåt från startraden (0).
public class Player
{
    public int Col = Lane.Columns / 2;
    public int Row;
    public int BestRow; // längsta raden hittills = poängen

    public void Hop(int dCol, int dRow)
    {
        Col = Math.Clamp(Col + dCol, 0, Lane.Columns - 1);
        Row = Math.Max(0, Row + dRow);
        BestRow = Math.Max(BestRow, Row);
    }
}
