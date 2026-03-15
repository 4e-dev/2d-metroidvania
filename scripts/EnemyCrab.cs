using Godot;
using System;

public partial class EnemyCrab : CharacterBody2D {
    //private AnimatedSprite2D Sprite;
    //private Timer WaitTimer;

    //private Godot.Collections.Array<Marker2D> PatrolPoints;
    //private Marker2D TargetPoint;
    //private int TargetPointIndex = 0;
    //private Vector2 Direction;

    //private enum State { Waiting, Walking, Dead }
    //private State _state;

    //private int Health = 100;

    //private const float Gravity = 1000f;
    //private const int Speed = 50;
    //private const float RaycastDistance = 50f;

    //public override void _Ready() {
    //    AddToGroup("enemy");

    //    Sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

    //    _state = State.Walking;

    //    WaitTimer = GetNode<Timer>("WaitTimer");
    //    WaitTimer.Timeout += OnTimerTimeout;

    //    Node2D _patrolPoints = GetNode<Node2D>("PatrolPoints");
    //    PatrolPoints = new();
    //    foreach (Marker2D point in _patrolPoints.GetChildren())
    //        PatrolPoints.Add(point);

    //    TargetPoint = PatrolPoints[TargetPointIndex];
    //    if (TargetPoint.GlobalPosition.X <= GlobalPosition.X)
    //        Direction = Vector2.Left;
    //    else
    //        Direction = Vector2.Right;
    //}

    //public override void _PhysicsProcess(double delta) {
    //    if (_state == State.Dead) {
    //        Velocity = Vector2.Zero;

    //        if (Sprite.Animation != "death")
    //            Sprite.Play("death");
    //        
    //        if (!Sprite.IsPlaying())
    //            QueueFree();
    //        MoveAndSlide();
    //        return;
    //    }

    //    float d = (float)delta;

    //    // Apply gravity
    //    if (!IsOnFloor())
    //        Velocity = new Vector2(Velocity.X, Velocity.Y + Gravity * d);

    //    float distance = TargetPoint.GlobalPosition.X - GlobalPosition.X;
    //    bool reachPatrolPoint = MathF.Abs(distance) < 5f;

    //    if (_state == State.Walking) {
    //        if (reachPatrolPoint) {
    //            _state = State.Waiting;
    //            Velocity = Vector2.Zero;
    //            WaitTimer.Start(2f);


    //        } else {
    //            //Velocity = new Vector2(Direction.X * Speed, Velocity.Y);
    //            Velocity = new Vector2(MathF.Sign(distance) * Speed, Velocity.Y);
    //        }
    //    }
    //    else if (_state == State.Waiting) {
    //        Velocity = Vector2.Zero;
    //    }
    //    MoveAndSlide();

    //    // Animations
    //    switch (_state) {
    //        case State.Waiting:
    //            Sprite.Play("idle");
    //            break;

    //        case State.Walking:
    //            Sprite.Play("walk");
    //            break;
    //    }
    //    Sprite.FlipH = Direction == Vector2.Right;
    //}

    //public void TakeDamage(int damage) {
    //    Health -= damage;

    //    if (Health <= 0) {
    //        _state = State.Dead;
    //        Velocity = Vector2.Zero;
    //    }
    //}

    //private void OnTimerTimeout() {
    //    _state = State.Walking;

    //    TargetPointIndex = (TargetPointIndex + 1) % PatrolPoints.Count;
    //    TargetPoint = PatrolPoints[TargetPointIndex];

    //    Direction = TargetPoint.GlobalPosition.X < GlobalPosition.X
    //        ? Vector2.Left
    //        : Vector2.Right;
    //}



    private readonly float Gravity = 1000f;
    private readonly float Speed = 100f;


    [Export] private Node2D _patrolPoints;
    private Godot.Collections.Array<Marker2D> PatrolPoints = new();
    private int TargetPointIndex = 0;

    private Timer PatrolPointTimer;
    private bool IsWaiting = false;

    public override void _Ready() {
        foreach (Node node in _patrolPoints.GetChildren()) {
            if (node is Marker2D point)
                PatrolPoints.Add(point);
        }

        PatrolPointTimer = GetNode<Timer>("PatrolPointTimer");
        PatrolPointTimer.WaitTime = 2f;
        PatrolPointTimer.Timeout += OnPatrolPointTimerTimeout;
    }

    public override void _PhysicsProcess(double delta) {
        float d = (float)delta;
        ApplyGravity(d);
        Walking(d);
        MoveAndSlide();
        // TODO animation
    }

    private void ApplyGravity(float delta) {
        if (!IsOnFloor())
            Velocity = new Vector2(Velocity.X, Velocity.Y + Gravity * delta);
    }

    private void Walking(float delta) {
        if (IsWaiting)
            return;

        Marker2D targetPoint = PatrolPoints[TargetPointIndex];
        float distance = MathF.Abs(targetPoint.GlobalPosition.X - GlobalPosition.X);

        bool reachPatrolPoint = distance <= 5f;
        if (reachPatrolPoint) {
            Wait();

            // Snap enemy to patrol point
            GlobalPosition = new Vector2(targetPoint.GlobalPosition.X, GlobalPosition.Y);
        }
        else {
            Vector2 direction = targetPoint.GlobalPosition.X < GlobalPosition.X
                ? Vector2.Left
                : Vector2.Right;
            Velocity = new Vector2(Speed * direction.X, Velocity.Y);
        }
    }

    private void Wait() {
        IsWaiting = true;
        Velocity = Vector2.Zero;
        PatrolPointTimer.Start();
    }

    // Signals
    private void OnPatrolPointTimerTimeout() {
        IsWaiting = false;
        TargetPointIndex = (TargetPointIndex + 1) % PatrolPoints.Count;
    }
}
