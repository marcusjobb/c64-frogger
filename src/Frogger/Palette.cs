using Raylib_cs;

namespace Frogger;

// De 16 klassiska C64-färgerna (Pepto-palett)
public static class Palette
{
    public static readonly Color Black     = new(0x00, 0x00, 0x00, 255);
    public static readonly Color White     = new(0xFF, 0xFF, 0xFF, 255);
    public static readonly Color Red       = new(0x68, 0x37, 0x2B, 255);
    public static readonly Color Cyan      = new(0x70, 0xA4, 0xB2, 255);
    public static readonly Color Purple    = new(0x6F, 0x3D, 0x86, 255);
    public static readonly Color Green     = new(0x58, 0x8D, 0x43, 255);
    public static readonly Color Blue      = new(0x35, 0x28, 0x79, 255);
    public static readonly Color Yellow    = new(0xB8, 0xC7, 0x6F, 255);
    public static readonly Color Orange    = new(0x6F, 0x4F, 0x25, 255);
    public static readonly Color Brown     = new(0x43, 0x39, 0x00, 255);
    public static readonly Color LightRed  = new(0x9A, 0x67, 0x59, 255);
    public static readonly Color DarkGrey  = new(0x44, 0x44, 0x44, 255);
    public static readonly Color Grey      = new(0x6C, 0x6C, 0x6C, 255);
    public static readonly Color LightGreen = new(0x9A, 0xD2, 0x84, 255);
    public static readonly Color LightBlue = new(0x6C, 0x5E, 0xB5, 255);
    public static readonly Color LightGrey = new(0x95, 0x95, 0x95, 255);
}
