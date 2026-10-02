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
    float _time;    // används för att animera vattnet
    string _deathReason = "";

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
        _time += dt;
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

        // På floden följer grodan med stocken den står på
        var lane = _world.Get(_player.Row);
        if (lane.Type == LaneType.River) _player.X += lane.Speed * dt;

        // Kameran följer grodan uppåt och scrollar dessutom långsamt av sig själv
        // så att man inte kan stå still. Det blir snabbare ju längre man kommit.
        if (_started)
        {
            float autoScroll = 0.25f + 0.4f * World.Difficulty(_player.Row);
            _camera += autoScroll * dt;
        }
        if (_player.Row > _camera)
            _camera += (_player.Row - _camera) * Math.Min(1f, 6f * dt);

        _world.EnsureUpTo((int)_camera + 16);
        _world.RemoveBelow((int)_camera - 6);

        CheckDeath(lane);
    }

    void CheckDeath(Lane lane)
    {
        if (lane.HitsCar(_player.X)) Die("SPLAT!");
        else if (lane.Type == LaneType.River &&
                 (!lane.IsOnLog(_player.CenterX) || _player.CenterX < 0 || _player.CenterX > W))
            Die("SPLASH!");
        else if (_player.Row <= _camera - 3) Die("TOO SLOW!");
    }

    void Die(string reason)
    {
        _deathReason = reason;
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

        // Landar man på fast mark hamnar man exakt i en ruta, men på floden följer man stocken
        if (_world.Get(_player.Row).Type != LaneType.River) _player.SnapToGrid();
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
            switch (lane.Type)
            {
                case LaneType.Grass: DrawGrass(lane); break;
                case LaneType.Road: DrawRoad(lane); break;
                case LaneType.River: DrawRiver(lane); break;
            }
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

        foreach (var mover in lane.Movers)
        {
            int x = (int)mover.X;
            if (lane.Fast) DrawSportsCar(mover, x, y, lane.Speed > 0);
            else DrawCar(mover, x, y, lane.Speed > 0);
        }
    }

    void DrawCar(Mover car, int x, int y, bool right)
    {
        Raylib.DrawRectangle(x, y + 3, car.Width, 10, car.Color);
        Raylib.DrawRectangle(x + 2, y + 1, car.Width - 4, 3, car.Color);   // tak
        Raylib.DrawRectangle(x + 2, y + 12, 3, 3, Palette.Black);          // hjul
        Raylib.DrawRectangle(x + car.Width - 5, y + 12, 3, 3, Palette.Black);
        // Strålkastare på den sida bilen kör mot
        Raylib.DrawRectangle(right ? x + car.Width - 2 : x, y + 5, 2, 3, Palette.White);
    }

    // Låg och spetsig sportbil med vit rand, så man känner igen den på långt håll
    void DrawSportsCar(Mover car, int x, int y, bool right)
    {
        Raylib.DrawRectangle(x, y + 6, car.Width, 6, car.Color);
        Raylib.DrawRectangle(right ? x + 4 : x + car.Width - 10, y + 3, 6, 4, car.Color); // låg kupé
        Raylib.DrawRectangle(x + 1, y + 8, car.Width - 2, 1, Palette.White);               // racingrand
        Raylib.DrawRectangle(x + 2, y + 11, 3, 3, Palette.Black);
        Raylib.DrawRectangle(x + car.Width - 5, y + 11, 3, 3, Palette.Black);
        Raylib.DrawRectangle(right ? x + car.Width - 2 : x, y + 7, 2, 2, Palette.White);
    }

    void DrawRiver(Lane lane)
    {
        int y = ScreenY(lane.Row);
        Raylib.DrawRectangle(0, y, W, Lane.TileSize, Palette.Blue);

        // Vågor som sakta glider åt samma håll som strömmen
        int shift = (int)(_time * lane.Speed * 0.5f);
        for (int x = -16; x < W + 16; x += 16)
        {
            int wx = x + ((shift % 16) + 16) % 16;
            Raylib.DrawRectangle(wx + 2, y + 4, 4, 1, Palette.LightBlue);
            Raylib.DrawRectangle(wx + 9, y + 11, 4, 1, Palette.LightBlue);
        }

        foreach (var log in lane.Movers)
        {
            int x = (int)log.X;
            Raylib.DrawRectangle(x, y + 2, log.Width, 12, Palette.Orange);
            Raylib.DrawRectangle(x, y + 2, log.Width, 2, Palette.Brown);   // bark upptill
            Raylib.DrawRectangle(x, y + 12, log.Width, 2, Palette.Brown);  // bark nedtill
            for (int k = 12; k < log.Width - 6; k += 14)                   // årsringar
                Raylib.DrawRectangle(x + k, y + 7, 4, 2, Palette.Brown);
        }
    }

    void DrawFrog()
    {
        int x = (int)_player.X;
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
        DrawCentered($"{_deathReason}  GAME OVER", 78, Palette.White);
        DrawCentered($"SCORE {_player.BestRow:0000}", 94, Palette.Yellow);
        DrawCentered("PRESS SPACE", 106, Palette.LightBlue);
    }
}
