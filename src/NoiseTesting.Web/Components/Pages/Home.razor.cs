namespace NoiseTesting.Web.Components.Pages;

public partial class Home
{
    private int PosX { get; set; }

    private int PosY { get; set; }

    private int Width { get; set; }

    private int Height { get; set; }

    private int Seed { get; set; }

    protected override void OnInitialized()
    {
        Width = 3;
        Height = 3;
    }

    private void MoveUp()
    {
        PosY--;
    }

    private void MoveDown()
    {
        PosY++;
    }

    private void MoveLeft()
    {
        PosX--;
    }

    private void MoveRight()
    {
        PosX++;
    }
}
