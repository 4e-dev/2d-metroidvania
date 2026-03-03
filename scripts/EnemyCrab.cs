using Godot;
using System;
using System.Linq;

public partial class EnemyCrab : CharacterBody2D
{
    // TODO: create an enemy patrol point
    // this function will assume that there are exactly TWO Marker2D nodes

    [Export] public float Gravity = 1000f;
    [Export] public Node2D PatrolPoints { get; set; }
    [Export] public int Speed = 3000;
    [Export] public AnimatedSprite2D Sprite { get; set; }

    private enum AnimationState
    {
        Idle,
        Walk
    }
    private AnimationState CurrentAnimationState;
    private int NumberOfPoints;
    private Godot.Collections.Array<Vector2> PointPositions;
    private Vector2 TargetMarker; // the point that the enemy is currently traveling towards
    private int TargetMarkerIndex;
    private Vector2 Direction;

    public override void _Ready()
    {
        if (PatrolPoints == null)
        {
            GD.Print("ERROR - Enemy has no patrol points. Make sure it's assigned via level inspector!");
            return;
        }
        PointPositions = new Godot.Collections.Array<Vector2>();
        CurrentAnimationState = AnimationState.Idle;
        NumberOfPoints = PatrolPoints.GetChildren().Count; // NOTE: unused
        foreach (Node2D Point in PatrolPoints.GetChildren().Cast<Node2D>())
            PointPositions.Add(Point.GlobalPosition);
        TargetMarker = PointPositions[TargetMarkerIndex];
        Direction = Vector2.Left;
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        CrabFalling(dt);
        CrabWalking(dt);

        MoveAndSlide();

        PlayAnimation();
    }

    private void CrabFalling(float delta)
    {
        if (!IsOnFloor())
        {
            Vector2 DownwardVelocity = Velocity;
            DownwardVelocity.Y += Gravity * delta;
            Velocity = DownwardVelocity;
        }
    }

    private void CrabWalking(float delta)
    {
        const double MinPatrolDistance = 0.5;
        double Distance = Math.Abs(Position.X - TargetMarker.X);

        if (Distance > MinPatrolDistance)
        {
            Vector2 TargetVelocity = Velocity;
            TargetVelocity.X = Direction.X * Speed * delta;
            Velocity = TargetVelocity;
        }
        else
        {  
            // crab has gone past the target patrol point
            TargetMarkerIndex = (TargetMarkerIndex + 1) % NumberOfPoints;
        }
        TargetMarker = PointPositions[TargetMarkerIndex];
        Direction = TargetMarker.X > Position.X ? Vector2.Right ; Vector2.Left;
    }

    private void PlayAnimation()
    {
        if (Sprite == null)
        {
            GD.Print("ERROR - EnemyCrab sprite not set. Unable to set animations");
            return;
        }

        bool IsMoving = Math.Abs(Velocity.X) > 0;

        if (IsMoving)
        {
            Sprite.FlipH = Velocity.X > 0;
            CurrentAnimationState = AnimationState.Walk;
        }
        else
        {
            CurrentAnimationState = AnimationState.Idle;
        }

        switch (CurrentAnimationState)
        {
            case AnimationState.Idle:
            {
                Sprite.Play("crab-idle");
            }
            break;
            case AnimationState.Walk:
            {
                Sprite.Play("crab-walk");
            }
            break;
        }
    }
}
