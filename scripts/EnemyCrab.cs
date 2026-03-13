using Godot;
using System;
using System.Linq;

// TODO: play a special animation when an enemy is killed...
// TODO: Create an Enemy group... might be useful for later

public partial class EnemyCrab : CharacterBody2D {
    [Export] public AnimatedSprite2D Sprite { get; set; }
    [Export] public Node2D PatrolPoints { get; set; }
    [Export] public float Gravity = 1000f;
    [Export] public int Speed = 3000;

    private enum AnimationState {
        Idle,
        Walk
    }
    private AnimationState CurrentAnimationState;
    private Godot.Collections.Array<Vector2> PointPositions;
    private Vector2 WalkingDirection;
    private Vector2 TargetPointPosition;
    private int NumberOfPoints;
    private int TargetPointPositionIndex = 0;

    private Timer WaitAtPatrolPoint;
    private bool IsWaiting = false;

    public override void _Ready() {
        CurrentAnimationState = AnimationState.Idle;

        PatrolPoints = GetNode<Node2D>("PatrolPoints");

        PointPositions = new Godot.Collections.Array<Vector2>();
        Godot.Collections.Array<Node> PatrolPointChildren = PatrolPoints.GetChildren();
        NumberOfPoints = PatrolPointChildren.Count;

        for (int i = 0; i < NumberOfPoints; ++i) {
            Node2D Point = (Node2D)PatrolPointChildren[i];
            PointPositions.Add(Point.GlobalPosition);
        }

        // set default target marker and walking direction
        TargetPointPosition = PointPositions[0];
        WalkingDirection = Vector2.Left; // arbitrary - maybe randomize this?

        // wait logic with Timer node
        WaitAtPatrolPoint = GetNode<Timer>("WaitAtPatrolPoint");
        WaitAtPatrolPoint.SetOneShot(true);

        // connect timeout signal
        WaitAtPatrolPoint.Timeout += OnWaitAtPatrolPointTimeout;
    }

    public override void _PhysicsProcess(double delta) {
        float dt = (float)delta;

        CrabFalling(dt);
        CrabWalking(dt);

        MoveAndSlide();

        PlayAnimation();
    }

    private void CrabFalling(float delta) {
        if (!IsOnFloor()) {
            Vector2 DownwardVelocity = Velocity;
            DownwardVelocity.Y += Gravity * delta;
            Velocity = DownwardVelocity;
        }
    }

    private void CrabWalking(float delta) {
        // crab must wait after reaching a patrol point
        if (IsWaiting)
            return;

        double Distance = Math.Abs(Position.X - TargetPointPosition.X);
        bool CrabReachedPatrolPoint = Distance < 0.5;

        if (CrabReachedPatrolPoint) {
            Velocity = Vector2.Zero;
            IsWaiting = true;
            TargetPointPositionIndex = (TargetPointPositionIndex + 1) % NumberOfPoints;

            // start timer
            float WaitTime = 2f;
            WaitAtPatrolPoint.Start(WaitTime);
        }
        else {  
            Vector2 TargetVelocity = Velocity;
            TargetVelocity.X = WalkingDirection.X * Speed * delta;
            Velocity = TargetVelocity;
        }
        TargetPointPosition = PointPositions[TargetPointPositionIndex];
        WalkingDirection = TargetPointPosition.X > Position.X ? Vector2.Right : Vector2.Left;
    }

    private void PlayAnimation() {
        if (Sprite == null) {
            GD.Print("ERROR - EnemyCrab sprite not set. Unable to set animations");
            return;
        }

        bool IsMoving = Math.Abs(Velocity.X) > 0;

        if (IsMoving) {
            Sprite.FlipH = Velocity.X > 0;
            CurrentAnimationState = AnimationState.Walk;
        }
        else {
            CurrentAnimationState = AnimationState.Idle;
        }

        switch (CurrentAnimationState) {
            case AnimationState.Idle: {
                Sprite.Play("crab-idle");
            } break;
            case AnimationState.Walk: {
                Sprite.Play("crab-walk");
            } break;
        }
    }

    private void OnWaitAtPatrolPointTimeout() {
        IsWaiting = false;
    }
}
