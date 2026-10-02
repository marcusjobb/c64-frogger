using Raylib_cs;

namespace Frogger;

// Bygger banorna löpande framför grodan och glömmer de som ligger långt bakom.
public class World
{
    static readonly Color[] CarColors =
        { Palette.Red, Palette.Yellow, Palette.Cyan, Palette.Orange, Palette.Purple };

    const int FirstRiverRow = 4;     // inga floder förrän spelaren hunnit värma upp
    const int SportsCarFromRow = 12; // inga sportbilar de första raderna

    readonly Dictionary<int, Lane> _lanes = new();
    readonly Random _rng = new();
    int _topRow = -1;
    LaneType _lastType = LaneType.Grass;
    int _run; // hur många banor i rad av samma typ

    // Rad där svårigheten når max. Högre tal = långsammare ökning, mer tid att se landskapet.
    const float MaxDifficultyRow = 200f;

    // 0 = lätt, 1 = max
    public static float Difficulty(int row) => Math.Min(1f, row / MaxDifficultyRow);

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
        float d = Difficulty(row);
        if (type == LaneType.Road) FillRoad(lane, d);
        else if (type == LaneType.River) FillRiver(lane, d);
        _lanes[row] = lane;
    }

    LaneType ChooseType(int row)
    {
        if (row <= 2) return LaneType.Grass; // säker start

        // Aldrig för många av samma i rad
        if (_lastType == LaneType.Road && _run >= 3) return LaneType.Grass;
        if (_lastType == LaneType.River && _run >= 2) return LaneType.Grass;
        if (_lastType == LaneType.Grass && _run >= 2) return RoadOrRiver(row);

        // Slumpa. Gräs blir ovanligare och floder vanligare ju längre man kommit.
        float d = Difficulty(row);
        float roll = _rng.NextSingle();
        if (roll < 0.30f - 0.08f * d) return LaneType.Grass;
        return RoadOrRiver(row);
    }

    LaneType RoadOrRiver(int row)
    {
        if (row < FirstRiverRow) return LaneType.Road;
        float riverChance = 0.22f + 0.06f * Difficulty(row);
        return _rng.NextSingle() < riverChance ? LaneType.River : LaneType.Road;
    }

    void FillRoad(Lane lane, float d)
    {
        float speed = 30 + 60 * d + _rng.Next(0, 20);

        // Då och då en sportbilsbana: dubbelt så snabb, men färre bilar. Vanligare högre upp.
        lane.Fast = lane.Row >= SportsCarFromRow && _rng.NextSingle() < 0.10f + 0.20f * d;
        if (lane.Fast) speed *= 2f;
        lane.Speed = _rng.Next(2) == 0 ? speed : -speed;

        // Tätare trafik ju högre svårighet. Snabba banor får större luckor så de går att klara.
        int minGap = (int)(64 - 24 * d) + (lane.Fast ? 40 : 0);
        int maxGap = (int)(130 - 40 * d) + (lane.Fast ? 60 : 0);

        float x = -Lane.Margin + _rng.Next(0, 40);
        while (true)
        {
            int width = _rng.Next(4) == 0 ? 32 : 16; // ibland en lastbil
            if (x + width + minGap > -Lane.Margin + Lane.WrapLength) break;
            lane.Movers.Add(new Mover { X = x, Width = width, Color = CarColors[_rng.Next(CarColors.Length)] });
            x += width + _rng.Next(minGap, maxGap);
        }
    }

    void FillRiver(Lane lane, float d)
    {
        float speed = 20 + 35 * d + _rng.Next(0, 15);
        lane.Speed = _rng.Next(2) == 0 ? speed : -speed;

        // Längre svårighet = kortare stockar och större luckor
        int[] widths = d < 0.5f ? new[] { 48, 64, 80 } : new[] { 32, 48, 64 };
        int minGap = 24;
        int maxGap = (int)(48 + 40 * d);

        float x = -Lane.Margin + _rng.Next(0, 30);
        while (true)
        {
            int width = widths[_rng.Next(widths.Length)];
            if (x + width + minGap > -Lane.Margin + Lane.WrapLength) break;
            lane.Movers.Add(new Mover { X = x, Width = width, Color = Palette.Orange });
            x += width + _rng.Next(minGap, maxGap);
        }
    }
}
