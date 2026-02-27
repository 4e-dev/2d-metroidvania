using Godot;
using System;

public partial class Player : CharacterBody2D
{
	public override void _PhysicsProcess(double delta) {
		if (!IsOnFloor()) {
			float dt = (float)delta;
			Vector2 TargetVelocity = Velocity;
			TargetVelocity.Y += 1000f * dt;
			Velocity = TargetVelocity;
		}
		MoveAndSlide();
	}
}
