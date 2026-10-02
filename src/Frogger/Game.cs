using Raylib_cs;

namespace Frogger;

public enum GameState { Title, Playing, GameOver }

// Håller reda på spelets tillstånd och ritar allt till den lilla 320x200-ytan.
public class Game
{
    public const int W = 320, H = 200;
    const int PlayerScreenY = 150; // var grodans rad hamnar på skärmen (pixlar uppifrån)

    GameState _state = GameState.Title;
    World _world = new();
    Player _player = new();
    float _camera;  // vilken rad kameran tittar på (decimaltal = mjuk scroll)
    bool _started;  // kameran börjar tvinga fram scroll först efter första hoppet

    void Reset()
    {
        _world = new World();
        _player = new Player();
        _camera = 0;
        _started = false;
        _world.EnsureUpTo(16);
        _state = GameState.Playing;
    }

    public void Update(float dt)
    {
        switch (_state)
        {
            case GameState.Title:
            case GameState.GameOver:
                if (Raylib.IsKeyPressed(KeyboardKey.Space)) Reset();
                break;
            case GameState.Playing:
                UpdatePlaying(dt);
                break;
        }
    }

    void UpdatePlaying(float dt)
    {
        ReadInput();
        _world.Update(dt);

        // Kameran följer grodan uppåt och scrollar dessutom långsamt av sig själv
        // så att man inte kan stå still. Det blir snabbare ju längre man kommit.
        if (_started)
        {
            float autoScroll = 0.25f + 0.4f * World.Difficulty(_player.Row);
            _camera += autoScroll * dt;
        }
        _camera = Math.Max(_camera, _camera + (_player.Row - _camera) * Math.Min(1f, 6f * dt));

        _world.EnsureUpTo((int)_camera + 16);
        _world.RemoveBelow((int)_camera - 6);

        // Död: bil, eller för långt bakom skärmen
        if (_world.Get(_player.Row).HitsCar(_player.Col) || _player.Row <= _camera - 3)
            _state = GameState.GameOver;
    }

    void ReadInput()
    {
        int dCol = 0, dRow = 0;
        if (Raylib.IsKeyPressed(KeyboardKey.Up) || Raylib.IsKeyPressed(KeyboardKey.W)) dRow = 1;
        else if (Raylib.IsKeyPressed(KeyboardKey.Down) || Raylib.IsKeyPressed(KeyboardKey.S)) dRow = -1;
        else if (Raylib.IsKeyPressed(KeyboardKey.Left) || Raylib.IsKeyPressed(KeyboardKey.A)) dCol = -1;
        else if (Raylib.IsKeyPressed(KeyboardKey.Right) || Raylib.IsKeyPressed(KeyboardKey.D)) dCol = 1;

        if (dCol == 0 && dRow == 0) return;
        _started = true;
        _player.Hop(dCol, dRow);
    }

    // ---- Ritning ----

    public void Draw()
    {
        Raylib.ClearBackground(Palette.LightBlue);
        switch (_state)
        {
            case GameState.Title: DrawTitle(); break;
            case GameState.Playing: DrawWorld(); DrawHud(); break;
            case GameState.GameOver: DrawWorld(); DrawHud(); DrawGameOver(); break;
        }
    }

    int ScreenY(int row) => (int)(PlayerScreenY - (row - _camera) * Lane.TileSize);

    void DrawWorld()
    {
        int from = (int)_camera - 4;
        for (int row = from; row <= (int)_camera + 11; row++)
        {
            if (row < 0) { Raylib.DrawRectangle(0, ScreenY(row), W, Lane.TileSize, Palette.Black); continue; }
            var lane = _world.Get(row);
            if (lane.Type == LaneType.Grass) DrawGrass(lane);
            else DrawRoad(lane);
        }
        DrawFrog();
    }

    void DrawGrass(Lane lane)
    {
        int y = ScreenY(lane.Row);
        Raylib.DrawRectangle(0, y, W, Lane.TileSize, Palette.Green);
        // Små ljusa gräsrester, placerade av ett enkelt tal så de inte flimrar
        for (int col = 0; col < Lane.Columns; col++)
        {
            int h = (col * 7 + lane.Row * 13) % 5;
            if (h == 0) Raylib.DrawRectangle(col * Lane.TileSize + 4, y + 5, 2, 2, Palette.LightGreen);
            if (h == 2) Raylib.DrawRectangle(col * Lane.TileSize + 10, y + 10, 2, 2, Palette.LightGreen);
        }
    }

    void DrawRoad(Lane lane)
    {
        int y = ScreenY(lane.Row);
        Raylib.DrawRectangle(0, y, W, Lane.TileSize, Palette.DarkGrey);
        for (int x = 0; x < W; x += 16)
            Raylib.DrawRectangle(x + 4, y + 7, 8, 2, Palette.Grey);

        foreach (var car in lane.Cars)
        {
            int x = (int)car.X;
            Raylib.DrawRectangle(x, y + 3, car.Width, 10, car.Color);
            Raylib.DrawRectangle(x + 2, y + 1, car.Width - 4, 3, car.Color);   // tak
            Raylib.DrawRectangle(x + 2, y + 12, 3, 3, Palette.Black);          // hjul
            Raylib.DrawRectangle(x + car.Width - 5, y + 12, 3, 3, Palette.Black);
            // Strålkastare på den sida bilen kör mot
            int light = lane.Speed > 0 ? x + car.Width - 2 : x;
            Raylib.DrawRectangle(light, y + 5, 2, 3, Palette.White);
        }
    }

    void DrawFrog()
    {
        int x = _player.Col * Lane.TileSize;
        int y = ScreenY(_player.Row);
        var body = _state == GameState.GameOver ? Palette.Red : Palette.Yellow;
        Raylib.DrawRectangle(x + 3, y + 4, 10, 9, body);
        Raylib.DrawRectangle(x + 2, y + 2, 4, 4, body);    // ögonklot
        Raylib.DrawRectangle(x + 10, y + 2, 4, 4, body);
        Raylib.DrawRectangle(x + 3, y + 3, 2, 2, Palette.Black);
        Raylib.DrawRectangle(x + 11, y + 3, 2, 2, Palette.Black);
        Raylib.DrawRectangle(x + 1, y + 11, 3, 4, body);   // bakben
        Raylib.DrawRectangle(x + 12, y + 11, 3, 4, body);
    }

    void DrawHud()
    {
        Raylib.DrawRectangle(0, 0, W, 12, Palette.Black);
        Raylib.DrawText($"SCORE {_player.BestRow:0000}", 4, 1, 10, Palette.White);
    }

    void DrawCentered(string text, int y, Color color)
        => Raylib.DrawText(text, (W - Raylib.MeasureText(text, 10)) / 2, y, 10, color);

    void DrawTitle()
    {
        DrawCentered("**** C64 FROGGER ****", 16, Palette.Blue);
        Raylib.DrawText("READY.", 8, 80, 10, Palette.Blue);
        DrawCentered("PRESS SPACE TO START", 130, Palette.Blue);
        DrawCentered("ARROWS / WASD TO HOP", 146, Palette.Blue);
    }

    void DrawGameOver()
    {
        Raylib.DrawRectangle(60, 70, 200, 50, Palette.Black);
        DrawCentered("SPLAT!  GAME OVER", 78, Palette.White);
        DrawCentered($"SCORE {_player.BestRow:0000}", 94, Palette.Yellow);
        DrawCentered("PRESS SPACE", 106, Palette.LightBlue);
    }
}
