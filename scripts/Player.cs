using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public AnimatedSprite2D Sprite { get; set; } // set via inspector
	[Export] public float Gravity = 1000f;
	[Export] public float JumpImpulse = 350f;
	[Export] public float Speed = 250f;

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
	private AnimationState CurrentAnimationState;

	public override void _Ready() {
		CurrentAnimationState = AnimationState.Idle;
	}

	public override void _PhysicsProcess(double delta) {
		float dt = (float)delta;

		PhysicsFalling(dt);
		PhysicsRun();
		PhysicsJump();

		MoveAndSlide();

		PlayAnimation();
	}

	private void PhysicsFalling(float delta) {
		if (!IsOnFloor()) {
			Vector2 DownwardVelocity = Velocity;
			DownwardVelocity.Y += Gravity * delta;
			Velocity = DownwardVelocity;
		}
	}

	private void PhysicsRun() {
		float HorizontalDirection = Input.GetAxis("move_left", "move_right");
		Vector2 HorizontalVelocity = Velocity;
		if (HorizontalDirection != 0) {
			HorizontalVelocity.X = HorizontalDirection * Speed;
		} else {
			HorizontalVelocity.X = 0;
		}
		Velocity = HorizontalVelocity;
	}

	private void PhysicsJump() {
		if (Input.IsActionJustPressed("jump")) {
			Vector2 UpwardVelocity = Velocity;
			UpwardVelocity.Y -= JumpImpulse;
			Velocity = UpwardVelocity;
		}
	}

	// TODO: stop playing jump-roll animation after a certain amount of time
	private void PlayAnimation() {
		// being airborne has GREATEST priority
		bool IsMoving = Math.Abs(Velocity.X) > 0;

		if (!IsOnFloor()) {
			CurrentAnimationState = AnimationState.Jump;
		} else {
			if (IsMoving)
				CurrentAnimationState = AnimationState.Run;
			else
				CurrentAnimationState = AnimationState.Idle;
		}

		switch (CurrentAnimationState) {
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
