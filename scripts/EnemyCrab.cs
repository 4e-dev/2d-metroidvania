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
    private AnimationState _animationState;
    private Godot.Collections.Array<Vector2> _pointPositions;
    private Vector2 _crabWalkingDirection;
    private Vector2 _targetPointPosition;
    private int _numberOfPoints;
    private int _targetPointPositionIndex = 0;

    private Timer _waitAtPatrolPoint;
    private bool _isWaiting = false;

    public override void _Ready() {
        // default animation state
        _animationState = AnimationState.Idle;

        // setting up enemy patrol points
        if (PatrolPoints == null) {
            GD.Print("ERROR - Enemy has no patrol points. Make sure it's assigned via level inspector!");
            return;
        } 

        _pointPositions = new Godot.Collections.Array<Vector2>();
        Godot.Collections.Array<Node> PatrolPointChildren = PatrolPoints.GetChildren();
        _numberOfPoints = PatrolPointChildren.Count;

        for (int i = 0; i < _numberOfPoints; ++i) {
            Node2D Point = (Node2D)PatrolPointChildren[i];
            _pointPositions.Add(Point.GlobalPosition);
        }

        // set default target marker and walking direction
        _targetPointPosition = _pointPositions[0];
        _crabWalkingDirection = Vector2.Left; // arbitrary - maybe randomize this?

        // wait logic with Timer node
        _waitAtPatrolPoint = GetNode<Timer>("WaitAtPatrolPoint");
        _waitAtPatrolPoint.SetOneShot(true);

        // connect timeout signal
        _waitAtPatrolPoint.Timeout += OnWaitAtPatrolPointTimeout;
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
        if (_isWaiting)
            return;

        double Distance = Math.Abs(Position.X - _targetPointPosition.X);
        bool CrabReachedPatrolPoint = Distance < 0.5;

        if (CrabReachedPatrolPoint) {
            Velocity = Vector2.Zero;
            _isWaiting = true;
            _targetPointPositionIndex = (_targetPointPositionIndex + 1) % _numberOfPoints;

            // start timer
            float WaitTime = 2f;
            _waitAtPatrolPoint.Start(WaitTime);
        }
        else {  
            Vector2 TargetVelocity = Velocity;
            TargetVelocity.X = _crabWalkingDirection.X * Speed * delta;
            Velocity = TargetVelocity;
        }
        _targetPointPosition = _pointPositions[_targetPointPositionIndex];
        _crabWalkingDirection = _targetPointPosition.X > Position.X ? Vector2.Right : Vector2.Left;
    }

    private void PlayAnimation() {
        if (Sprite == null) {
            GD.Print("ERROR - EnemyCrab sprite not set. Unable to set animations");
            return;
        }

        bool IsMoving = Math.Abs(Velocity.X) > 0;

        if (IsMoving) {
            Sprite.FlipH = Velocity.X > 0;
            _animationState = AnimationState.Walk;
        }
        else {
            _animationState = AnimationState.Idle;
        }

        switch (_animationState) {
            case AnimationState.Idle: {
                Sprite.Play("crab-idle");
            } break;
            case AnimationState.Walk: {
                Sprite.Play("crab-walk");
            } break;
        }
    }

    private void OnWaitAtPatrolPointTimeout() {
        _isWaiting = false;
    }
}
