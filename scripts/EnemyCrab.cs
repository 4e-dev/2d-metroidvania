using Godot;
using System;

public partial class EnemyCrab : CharacterBody2D
{
	[Export] public AnimatedSprite2D Sprite { get; set; } // TODO: set via inspector
	[Export] public float Gravity = 1000f;

	public override void _PhysicsProcess(double delta) {
		float dt = (float)delta;

		PhysicsFalling(dt);

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

	private void PlayAnimation() {
		// TODO: create a switch statement to play animation states
	}
}
