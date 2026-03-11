using Godot;
using System;
using System.Diagnostics; // for Debug.Assert

// TODO: Add an animation for hitting a collision shape (enemy, wall)
// TODO: Create an Enemy group... might be useful for later

public partial class Bullet : Area2D {
    [Export] private int Speed = 500;
    private Vector2 Direction;
    private Vector2 PlayerVelocity;

    public override void _Ready() {
        SetMonitorable(true);
        SetMonitoring(true);
        BodyEntered += OnBodyEntered;
    }

    public override void _PhysicsProcess(double delta) {
        Position += Direction * (Speed + MathF.Abs(PlayerVelocity.X)) * (float)delta;
    }

    public void SetDirection(Vector2 _Direction) {
        Direction = _Direction.Normalized();
    }

    public void SetPlayerVelocity(Vector2 _PlayerVelocity) {
        PlayerVelocity = _PlayerVelocity;
    }

    private string MissingNodeMessage(string Node) {
        return $"Missing required node: {Node}\nEnsure it exists within the current SceneTree";
    }

    // For signals
    private void OnBodyEntered(Node2D Body) {
        QueueFree();
    }
}
