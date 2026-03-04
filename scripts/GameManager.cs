using Godot;
using System;

public partial class GameManager : Node
{
    public override void _Ready()
    {
        Color BackgroundColor = new Color(0.44f, 0.12f, 0.53f, 1f);
        RenderingServer.SetDefaultClearColor(BackgroundColor);
    }
}
