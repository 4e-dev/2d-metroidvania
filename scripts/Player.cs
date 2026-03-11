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

    private enum AnimationState {
        Cling,
        Duck,
        Hurt,
        Idle,
        Jump,
        Run,
        RunAndShoot,
        Shoot,
        SentinelCount
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

        AnimationNameByState = new string[(int)AnimationState.SentinelCount];
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

        if (Input.IsActionJustPressed("jump"))
            PrematureJumpTimer = PrematureJumpBuffer;
        else
            PrematureJumpTimer -= delta;

        bool AllowedToJump = CoyoteTimer > 0f && PrematureJumpTimer > 0f;
        if (AllowedToJump) {
            Velocity = new Vector2(Velocity.X, -JumpImpulse);

            // reset
            CoyoteTimer = 0f;
            PrematureJumpTimer = 0f;
        }

        bool QuickJumpRelease = Input.IsActionJustReleased("jump") && Velocity.Y < 0;
        if (QuickJumpRelease) {
            float ReducedJumpImpulse = JumpImpulse * 0.5f;
            Velocity = new Vector2(Velocity.X, -ReducedJumpImpulse);
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
            Bullets.AddChild(Bullet);
        }
    }

    private void PlayAnimation() {
        bool Moving = !ApproximatelyZero(HorizontalDirection);

        // Determine animation state
        if (!IsOnFloor()) { // Being airborne has the GREATEST priority.
            CurrentAnimationState = AnimationState.Jump;
        } else {
            if (Moving)
                CurrentAnimationState = AnimationState.Run;
            else
                CurrentAnimationState = AnimationState.Idle;
        }
        if (Input.IsActionPressed("shoot")) {
            if (Moving)
                CurrentAnimationState = AnimationState.RunAndShoot;
            else
                CurrentAnimationState = AnimationState.Shoot;
        }
        if (Moving) Sprite.FlipH = HorizontalDirection < 0;

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
