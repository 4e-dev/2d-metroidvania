using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public AnimatedSprite2D Sprite { get; set; } // set via inspector
	[Export] public int Gravity = 1000;

	[Export] public int JumpImpulse = 405; // clears about 5 blocks
	[Export] public int Speed = 15000;

    private float _coyoteBuffer = 0.15f; // NOTE: feels pretty good, not too forgiving
    private float _coyoteTimer = 0f;

    private float _prematureJumpBuffer = 0.15f;
    private float _prematureJumpTimer = 0f;

	private enum AnimationState {
		Cling,
		Duck,
		Hurt,
		Idle,
		Jump,
		RunShoot,
		Run,
		ShootUp,
		Stand
	}
	private AnimationState _animationState;

	public override void _Ready() {
		_animationState = AnimationState.Idle;
	}

	public override void _PhysicsProcess(double delta) {
		float dt = (float)delta;

		PhysicsFalling(dt);
		PhysicsRun(dt);
		PhysicsJump(dt);

		MoveAndSlide();

		PlayAnimation();
	}

	private void PhysicsFalling(float delta) {
        if (IsOnFloor()) {
            _coyoteTimer = _coyoteBuffer;
        } else {
            Velocity = new Vector2(Velocity.X, Velocity.Y + (Gravity * delta));
            _coyoteTimer -= delta;
        }
	}

	private void PhysicsRun(float delta) {
		float HorizontalDirection = Input.GetAxis("move_left", "move_right");
		Vector2 HorizontalVelocity = Velocity;
		if (HorizontalDirection != 0) {
			HorizontalVelocity.X = HorizontalDirection * Speed * delta;
		} else {
			HorizontalVelocity.X = 0;
		}
		Velocity = HorizontalVelocity;
	}

	private void PhysicsJump(float delta) {
        if (IsOnFloor())
            _coyoteTimer = _coyoteBuffer;
        else
            _coyoteTimer -= delta;

        if (Input.IsActionPressed("jump"))
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
            float ShortenedJumpImpulse = Velocity.Y * 0.5f;
            Velocity = new Vector2(Velocity.X, ShortenedJumpImpulse);
        }
	}

	private void PlayAnimation() {
		// being airborne has GREATEST priority
		bool IsMoving = Math.Abs(Velocity.X) > 0;

		if (IsOnFloor()) {
			if (IsMoving)
				_animationState = AnimationState.Run;
			else
				_animationState = AnimationState.Idle;
		} else {
			_animationState = AnimationState.Jump;
		}

		switch (_animationState) {
			case AnimationState.Idle: {
				Sprite.Play("idle");
			}
			break;
			case AnimationState.Run: {
				Sprite.FlipH = Velocity.X < 0;
				Sprite.Play("run");
			}
			break;
			case AnimationState.Jump: {
				if (IsMoving)
					Sprite.FlipH = Velocity.X < 0;
				Sprite.Play("jump");
			}
			break;
		}
	}
}
