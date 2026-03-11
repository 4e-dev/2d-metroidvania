using Godot;
using System;
using System.Diagnostics; // for Debug.Assert

public partial class AnimatedBullet : Area2D {
    // TODO: Bullet's position must come out of player's gun
    // * How can we get the position of the player here?
    //      * Perhaps I need to pass it in from Player.cs?
    //      * Recall that each bullet is grouped into the Player's "Bullets" subgroup node.

    [Export] public int BulletSpeed = 1000;

    private Node2D _parent;

    public override void _Ready() {
        _parent = GetParent<Node2D>();
        Debug.Assert(_parent != null, "ERROR - Unable to find the Parent node of AnimatedBullet");

        // TODO: Position the bullet (global_position)
    }

    public override void _PhysicsProcess(double delta) {
        // TODO: Give the bullet some velocity and direction.
        // TODO: When the bullet hits a collision shape, play the correct animation then 
        //      despawn the bullet.
    }
}
