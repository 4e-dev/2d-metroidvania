using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody2D
{
    // TODO: Set all of these properties / functions private
	[Export] public AnimatedSprite2D Sprite { get; set; }
	[Export] public int Gravity = 1000;
	[Export] public int JumpImpulse = 405; // clears about 5 blocks
	[Export] public int Speed = 15000;

    private AnimationState _animationState;
    private Node2D bullets; // TODO: _camelCase!
    private PackedScene bulletScene; // This one too.
    private float _coyoteBuffer = 0.15f;
    private float _coyoteTimer = 0f;
    private float _horizontalDirection;
    private float _prematureJumpBuffer = 0.15f;
    private float _prematureJumpTimer = 0f;
    private readonly string _bulletScenePath = "res://scenes/bullet.tscn";

	private enum AnimationState {
		Cling,
		Duck,
		Hurt,
		Idle,
		Jump,
		Run,
		RunAndShoot,
		Shoot,
        SentinelCount // NOT an animation state! Used to get size of AnimationState.
	}
    private string[] AnimationNameByState;

	public override void _Ready() {
        // Load scenes & nodes.
        bullets = GetTree().CurrentScene.GetNode<Node2D>("Bullets");
        System.Diagnostics.Debug.Assert(bullets != null, "ERROR - Missing required node: 'Bullets'\nMake sure it exists within the current SceneTree");

        bulletScene = ResourceLoader.Load<PackedScene>(_bulletScenePath);
        System.Diagnostics.Debug.Assert(bulletScene != null, "ERROR - Unable to load the following scene: 'bullet'");

        // Default to idling.
		_animationState = AnimationState.Idle;

        // Update the name of the animations here in the event that they are changed, for whatever reason...
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
		_horizontalDirection = Input.GetAxis("move_left", "move_right");

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

	public void PlayerFall(float delta) {
        if (!IsOnFloor()) {
            float Down = Velocity.Y + (Gravity * delta);
            Velocity = new Vector2(Velocity.X, Down);
        }
	}

	public void PlayerRun(float delta) {
        float HorizontalVelocity;
        if (ApproximatelyZero(_horizontalDirection))
            HorizontalVelocity = 0;
        else
            HorizontalVelocity = _horizontalDirection * Speed * delta;
        Velocity = new Vector2(HorizontalVelocity, Velocity.Y);
	}

	public void PlayerJump(float delta) {
        if (IsOnFloor())
            _coyoteTimer = _coyoteBuffer;
        else
            _coyoteTimer -= delta;

        if (Input.IsActionJustPressed("jump"))
            _prematureJumpTimer = _prematureJumpBuffer;
        else
            _prematureJumpTimer -= delta;

        bool AllowedToJump = _coyoteTimer > 0f && _prematureJumpTimer > 0f;
        if (AllowedToJump) {
            Velocity = new Vector2(Velocity.X, -JumpImpulse);

            // reset
            _coyoteTimer = 0f;
            _prematureJumpTimer = 0f;
        }

        bool QuickJumpRelease = Input.IsActionJustReleased("jump") && Velocity.Y < 0;
        if (QuickJumpRelease) {
            //float ReducedJumpImpulse = Velocity.Y * 0.5f;
            float ReducedJumpImpulse = JumpImpulse * 0.5f;
            Velocity = new Vector2(Velocity.X, -ReducedJumpImpulse);
        }
	}

    // TODO: program player shooting animation and spawn AnimatedBullet
    public void PlayerShoot(float delta) {
        if (Input.IsActionPressed("shoot")) {
            if (bulletScene == null) {
                GD.Print("ERROR - AnimatedBullet scene not instantiated");
                return;
            }

            AnimatedSprite2D BulletInstance = bulletScene.Instantiate<AnimatedSprite2D>();
            bullets.AddChild(BulletInstance);

            // TODO: apply velocity to bullet and make it disappear when it hits anything with collision or goes off screen

        }
    }

	public void PlayAnimation() {
        bool Moving = !ApproximatelyZero(_horizontalDirection);

        // Determine animation state
        if (!IsOnFloor()) { // Being airborne has the GREATEST priority.
			_animationState = AnimationState.Jump;
        } else {
            if (Moving)
                _animationState = AnimationState.Run;
            else
                _animationState = AnimationState.Idle;
        }
        if (Input.IsActionPressed("shoot")) {
            if (Moving)
                _animationState = AnimationState.RunAndShoot;
            else
                _animationState = AnimationState.Shoot;
        }
        if (Moving) Sprite.FlipH = _horizontalDirection < 0;

        Sprite.Play(AnimationNameByState[(int)_animationState]);
	}

    private bool ApproximatelyZero(float value) {
        const float EPSILON = 1e-6f;
        return MathF.Abs(value) < EPSILON;
    }
}
