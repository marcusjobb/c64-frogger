using Raylib_cs;

namespace Frogger;

// Bygger banorna löpande framför grodan och glömmer de som ligger långt bakom.
public class World
{
    static readonly Color[] CarColors =
        { Palette.Red, Palette.Yellow, Palette.Cyan, Palette.Orange, Palette.Purple };

    readonly Dictionary<int, Lane> _lanes = new();
    readonly Random _rng = new();
    int _topRow = -1;
    LaneType _lastType = LaneType.Grass;
    int _run; // hur många banor i rad av samma typ

    // 0 = lätt, 1 = max. Når max vid rad 80.
    public static float Difficulty(int row) => Math.Min(1f, row / 80f);

    public Lane Get(int row) => _lanes[row];

    public void EnsureUpTo(int row)
    {
        while (_topRow < row) Add(++_topRow);
    }

    public void RemoveBelow(int row)
    {
        foreach (var key in _lanes.Keys.Where(k => k < row).ToList())
            _lanes.Remove(key);
    }

    public void Update(float dt)
    {
        foreach (var lane in _lanes.Values) lane.Update(dt);
    }

    void Add(int row)
    {
        var type = ChooseType(row);
        _run = type == _lastType ? _run + 1 : 1;
        _lastType = type;

        var lane = new Lane { Row = row, Type = type };
        if (type == LaneType.Road) FillRoad(lane, Difficulty(row));
        _lanes[row] = lane;
    }

    LaneType ChooseType(int row)
    {
        if (row <= 2) return LaneType.Grass; // säker start

        // Aldrig för många av samma i rad
        if (_lastType == LaneType.Road && _run >= 3) return LaneType.Grass;
        if (_lastType == LaneType.Grass && _run >= 2) return LaneType.Road;

        float roadChance = 0.55f + 0.35f * Difficulty(row);
        return _rng.NextSingle() < roadChance ? LaneType.Road : LaneType.Grass;
    }

    void FillRoad(Lane lane, float d)
    {
        float speed = 30 + 60 * d + _rng.Next(0, 20);
        lane.Speed = _rng.Next(2) == 0 ? speed : -speed;

        // Tätare trafik ju högre svårighet
        int minGap = (int)(64 - 24 * d);
        int maxGap = (int)(130 - 40 * d);

        float x = -Lane.Margin + _rng.Next(0, 40);
        while (true)
        {
            int width = _rng.Next(4) == 0 ? 32 : 16; // ibland en lastbil
            if (x + width + minGap > -Lane.Margin + Lane.WrapLength) break;
            lane.Cars.Add(new Car { X = x, Width = width, Color = CarColors[_rng.Next(CarColors.Length)] });
            x += width + _rng.Next(minGap, maxGap);
        }
    }
}
