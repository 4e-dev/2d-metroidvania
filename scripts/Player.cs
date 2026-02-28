using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public AnimatedSprite2D Sprite { get; set; } // set via inspector

	private const float Gravity = 1000f;
	private const float JumpImpulse = 350f;
	private const float Speed = 300f;

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

		PlayerFalling(dt);
		PlayerIdle();
		PlayerRun();
		PlayerJump();

		MoveAndSlide();

		PlayerAnimation();
	}

	private void PlayerFalling(float delta) {
		if (!IsOnFloor()) {
			Vector2 DownwardVelocity = Velocity;
			DownwardVelocity.Y += Gravity * delta;
			Velocity = DownwardVelocity;
		}
	}

	private void PlayerIdle() {
		if (IsOnFloor()) {
			CurrentAnimationState = AnimationState.Idle;
		}
	}

	private void PlayerRun() {
		float HorizontalDirection = Input.GetAxis("move_left", "move_right");
		Vector2 HorizontalVelocity = Velocity;

		if (HorizontalDirection != 0) {
			// TODO: only change the animation if the player isn't jumping
            Sprite.FlipH = HorizontalDirection < 0 ? true : false;
            CurrentAnimationState = AnimationState.Run;
			HorizontalVelocity.X = HorizontalDirection * Speed;
		} else {
			HorizontalVelocity.X = 0;
		}
		Velocity = HorizontalVelocity;
	}

	// TODO: coding up jump state logic
	// jump should only last while in the air (?) --> let's see how that turns out
	private void PlayerJump() {
		if (Input.IsActionJustPressed("jump")) {
			CurrentAnimationState = AnimationState.Jump;

			Vector2 UpwardVelocity = Velocity; // apply upward velocity
			UpwardVelocity.Y -= JumpImpulse;
			Velocity = UpwardVelocity;
		}
	}

	// NOTE: jumping animation is weird. jumping straight up plays the
	// animation correctly but moving left/right will trigger the run animation instead...
	private void PlayerAnimation() {
		switch (CurrentAnimationState) {
			case AnimationState.Idle:
				Sprite.Play("idle");
				break;
			case AnimationState.Run:
				Sprite.Play("run");
				break;
			case AnimationState.Jump:
				Sprite.Play("jump");
				break;
		}
	}
}
