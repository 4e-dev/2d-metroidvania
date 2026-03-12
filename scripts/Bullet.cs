using Godot;
using System;
using System.Diagnostics; // for Debug.Assert

// TODO: Check for if hit was in Enemy group... might be useful
// TODO: Create a hitbox for the enemy... I think the current collisionshape works right?

public partial class Bullet : Area2D {
    private enum AnimationState {
        Impact,
        Shot,
        Count
    };

    [Export] private int BaseSpeed = 500;
    private AnimatedSprite2D Sprite;
    private AnimationState CurrentAnimationState;
    private string[] AnimationNameByState;
    private bool Collided = false;

    private Vector2 _direction;
    public Vector2 Direction {
        get => _direction;
        set => _direction = value.Normalized();
    }

    private Vector2 _playerVelocity;
    public Vector2 PlayerVelocity {
        get => _playerVelocity;
        set => _playerVelocity = value;
    }

    public override void _Ready() {
        // Set up signal
        SetMonitorable(true);
        SetMonitoring(true);
        BodyEntered += OnBodyEntered;

        // Aggregate animation names
        AnimationNameByState = new string[(int)AnimationState.Count];
        AnimationNameByState[(int)AnimationState.Shot]      = "shot";
        AnimationNameByState[(int)AnimationState.Impact]    = "impact";

        // Default animation
        CurrentAnimationState = AnimationState.Shot;

        // Sprite setup
        Sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        Sprite.AnimationFinished += OnAnimationFinished;
    }

    public override void _PhysicsProcess(double delta) {
        float dt = (float)delta;
        if (!Collided) {
            int TotalBulletVelocity = BaseSpeed + (int)MathF.Abs(PlayerVelocity.X);
            Position += _direction * TotalBulletVelocity * dt;
        }
    }

    // Signal callback functions
    private void OnAnimationFinished() {
        QueueFree();
    }

    private void OnBodyEntered(Node2D Body) {
        CurrentAnimationState = AnimationState.Impact;
        Collided = true;

        Sprite.Play("impact");
        Sprite.SpriteFrames.SetAnimationLoop("impact", false);

        // Prevent accidental hits
        CollisionLayer = 0;
        CollisionMask = 0;
    }

    // Helper
    private string MissingNodeMessage(string Node) {
        return $"Missing required node: {Node}\nEnsure it exists within the current SceneTree";
    }
}
