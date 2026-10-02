using System.Numerics;
using Frogger;
using Raylib_cs;

// Intern upplösning som på en C64: 320x200 pixlar
const int W = 320, H = 200, Scale = 4;

Raylib.InitWindow(W * Scale, H * Scale, "C64 Frogger");
Raylib.SetTargetFPS(60);

// Allt ritas till en liten textur som sedan skalas upp utan filtrering
var screen = Raylib.LoadRenderTexture(W, H);
Raylib.SetTextureFilter(screen.Texture, TextureFilter.Point);

while (!Raylib.WindowShouldClose())
{
    // Rita i lilla världen
    Raylib.BeginTextureMode(screen);
    Raylib.ClearBackground(Palette.LightBlue);
    Raylib.DrawText("**** C64 FROGGER ****", 40, 40, 10, Palette.Blue);
    Raylib.DrawText("READY.", 8, 80, 10, Palette.Blue);
    Raylib.EndTextureMode();

    // Skala upp till fönstret. Höjden är negativ eftersom render-texturer är upp-och-ned.
    Raylib.BeginDrawing();
    Raylib.ClearBackground(Palette.Black);
    Raylib.DrawTexturePro(
        screen.Texture,
        new Rectangle(0, 0, W, -H),
        new Rectangle(0, 0, W * Scale, H * Scale),
        Vector2.Zero, 0, Color.White);
    Raylib.EndDrawing();
}

Raylib.UnloadRenderTexture(screen);
Raylib.CloseWindow();
