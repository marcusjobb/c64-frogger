namespace Frogger;

// Highscore sparas i en liten textfil i användarens egen datamapp,
// så den överlever att man stänger spelet.
public static class HighScore
{
    static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "c64-frogger", "highscore.txt");

    public static int Load()
    {
        try
        {
            return int.TryParse(File.ReadAllText(FilePath), out int score) ? Math.Max(0, score) : 0;
        }
        catch (Exception)
        {
            return 0; // ingen fil än, eller den går inte att läsa
        }
    }

    public static void Save(int score)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, score.ToString());
        }
        catch (Exception)
        {
            // Går det inte att spara är det inte värt att krascha spelet för
        }
    }
}
