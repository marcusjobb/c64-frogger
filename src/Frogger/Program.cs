using System.Numerics;
using Frogger;
using Raylib_cs;

// Fönstret är 4 gånger större än C64-ytan på 320x200
const int Scale = 4;

Raylib.InitWindow(Game.W * Scale, Game.H * Scale, "C64 Frogger");
Raylib.SetTargetFPS(60);

// Allt ritas till en liten textur som sedan skalas upp utan filtrering
var screen = Raylib.LoadRenderTexture(Game.W, Game.H);
Raylib.SetTextureFilter(screen.Texture, TextureFilter.Point);

var game = new Game();

while (!Raylib.WindowShouldClose())
{
    game.Update(Raylib.GetFrameTime());

    // Rita i lilla världen
    Raylib.BeginTextureMode(screen);
    game.Draw();
    Raylib.EndTextureMode();

    // Skala upp till fönstret. Höjden är negativ eftersom render-texturer är upp-och-ned.
    Raylib.BeginDrawing();
    Raylib.ClearBackground(Palette.Black);
    Raylib.DrawTexturePro(
        screen.Texture,
        new Rectangle(0, 0, Game.W, -Game.H),
        new Rectangle(0, 0, Game.W * Scale, Game.H * Scale),
        Vector2.Zero, 0, Color.White);
    Raylib.EndDrawing();
}

Raylib.UnloadRenderTexture(screen);
Raylib.CloseWindow();
