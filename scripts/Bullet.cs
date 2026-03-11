using Godot;
using System;
using System.Diagnostics; // for Debug.Assert

public partial class Bullet : Area2D {
    // TODO: Bullet's position must come out of player's gun
    // * How can we get the position of the player here?
    //      * Perhaps I need to pass it in from Player.cs?
    //      * Recall that each bullet is grouped into the Player's "Bullets" subgroup node.

    [Export] public int BulletSpeed = 1000;

    public override void _Ready() {

    }

    public override void _PhysicsProcess(double delta) {
        // TODO: Give the bullet some velocity and direction.
        // TODO: When the bullet hits a collision shape, play the correct animation then 
        //      despawn the bullet.
    }
}
