using Godot;
using System;
using System.Diagnostics; // for assert statements

public partial class Player : CharacterBody2D {
    [Export] private AnimatedSprite2D Sprite;
    [Export] private int Gravity = 1000;
    [Export] private int JumpImpulse = 405; // clears about 5 blocks
    [Export] private int Speed = 15000;

    private AnimationState CurrentAnimationState;
    private Marker2D BulletSpawner;
    private Node2D Bullets;
    private PackedScene BulletScene;
    private float CoyoteTimer = 0f;
    private float HorizontalDirection;
    private readonly float CoyoteBuffer = .05f;
    private readonly string BulletScenePath = "res://scenes/bullet.tscn";

    private enum Direction {
        Left,
        Right,
        Up,
        Down,
        Count
    };
    private Direction LookingAt;

    private enum AnimationState {
        Cling,
        Duck,
        Hurt,
        Idle,
        Jump,
        Run,
        RunAndShoot,
        Shoot,
        Count
    }
    private string[] AnimationNameByState;

    public override void _Ready() {
        Bullets = GetTree().CurrentScene.GetNode<Node2D>("Bullets");
        BulletScene = ResourceLoader.Load<PackedScene>(BulletScenePath);
        BulletSpawner = GetNode<Marker2D>("BulletSpawner");

        Debug.Assert(Bullets != null, MissingNodeMessage("Bullets"));
        Debug.Assert(BulletScene != null, $"ERROR - Unable to load the following scene: 'bullet'\nPath: {BulletScenePath}");
        Debug.Assert(BulletSpawner != null, MissingNodeMessage("BulletSpawner"));

        CurrentAnimationState = AnimationState.Idle;
        LookingAt = Direction.Right;

        AnimationNameByState = new string[(int)AnimationState.Count];
        AnimationNameByState[(int)AnimationState.Cling]         = "cling";
        AnimationNameByState[(int)AnimationState.Duck]          = "duck";
        AnimationNameByState[(int)AnimationState.Hurt]          = "hurt";
        AnimationNameByState[(int)AnimationState.Idle]          = "idle";
        AnimationNameByState[(int)AnimationState.Jump]          = "jump";
        AnimationNameByState[(int)AnimationState.Run]           = "run";
        AnimationNameByState[(int)AnimationState.RunAndShoot]   = "run_and_shoot";
        AnimationNameByState[(int)AnimationState.Shoot]         = "shoot";
    }

    public override void _PhysicsProcess(double delta) {
        HorizontalDirection = Input.GetAxis("move_left", "move_right");

        // Physics related processes
        float dt = (float)delta;
        PlayerFall(dt);
        PlayerRun(dt);
        PlayerJump(dt);
        PlayerShoot(dt);

        MoveAndSlide();

        // Animation must be played *AFTER* physics related processes!
        PlayAnimation();
    }

    private void PlayerFall(float delta) {
        if (!IsOnFloor()) {
            float Down = Velocity.Y + (Gravity * delta);
            Velocity = new Vector2(Velocity.X, Down);
        }
    }

    private void PlayerRun(float delta) {
        float HorizontalVelocity;
        if (ApproximatelyZero(HorizontalDirection))
            HorizontalVelocity = 0;
        else
            HorizontalVelocity = HorizontalDirection * Speed * delta;
        Velocity = new Vector2(HorizontalVelocity, Velocity.Y);
    }

    private void PlayerJump(float delta) {
        if (IsOnFloor())
            CoyoteTimer = CoyoteBuffer;
        else
            CoyoteTimer -= delta;

        bool Jump = Input.IsActionJustPressed("jump");
        bool Released = Input.IsActionJustReleased("jump");

        if (IsOnFloor()) {
            if (Jump) {
                Velocity = new Vector2(Velocity.X, -JumpImpulse);
            }
        }
        else {
            bool MidJump = Velocity.Y < 0;
            if (MidJump && Released) {
                float CutOffVelocity = Velocity.Y * 0.5f;
                Velocity = new Vector2(Velocity.X, CutOffVelocity);
            }
            else if (Jump && CoyoteTimer > 0f) {
                Velocity = new Vector2(Velocity.X, -JumpImpulse);
                CoyoteTimer = 0f;
            }
        }
    }

    private void PlayerShoot(float delta) {
        if (Input.IsActionPressed("shoot")) {
            if (BulletScene == null) {
                GD.Print("ERROR - AnimatedBullet scene not instantiated");
                return;
            }

            Area2D Bullet = BulletScene.Instantiate<Area2D>();
            Bullet.GlobalPosition = BulletSpawner.GlobalPosition;

            // Flip the bullet around depending on LookingAt
            if (LookingAt == Direction.Left) {
                float RelativeXPosition = BulletSpawner.Position.X;
                float FlippedX = Bullet.GlobalPosition.X - (2 * RelativeXPosition);
                Bullet.GlobalPosition = new Vector2(FlippedX, Bullet.GlobalPosition.Y);
            }

            Bullets.AddChild(Bullet);
        }
    }

    private void PlayAnimation() {
        // Determine LookingAt direction.
        bool Moving = false;

        if (Input.IsActionPressed("move_left")) {
            LookingAt = Direction.Left;
            Moving = true;
        }
        if (Input.IsActionPressed("move_right")) {
            LookingAt = Direction.Right;
            Moving = true;
        }

        // Determine CurrentAnimationState
        if (Input.IsActionPressed("jump") || !IsOnFloor()) {
            CurrentAnimationState = AnimationState.Jump;
        } else if (Moving) {
            if (Input.IsActionPressed("shoot"))
                CurrentAnimationState = AnimationState.RunAndShoot;
            else
                CurrentAnimationState = AnimationState.Shoot;
        }

        Sprite.FlipH = LookingAt == Direction.Left;
        Sprite.Play(AnimationNameByState[(int)CurrentAnimationState]);
    }

    private bool ApproximatelyZero(float value) {
        const float EPSILON = 1e-6f;
        return MathF.Abs(value) < EPSILON;
    }

    private string MissingNodeMessage(string NodeName) {
        return $"ERROR - Missing required node: '{NodeName}'\nMake sure it exists within the current SceneTree!";
    }
}
