using Godot;
using System;
using System.Threading;

public partial class Player : Area2D
{
	[Export]
	public int Speed { get; set; } = 30; // How fast the player will move
	[Signal]
	public delegate void HitEventHandler();
	private const float VELOCITY_ATTACK = 3;
	private const float VELOCITY = 10;
	private const double ATTACK_COOLDOWN = 0.5D;
	private const double ATTACK_DELAY = 0.2D;
	private double cooldown = ATTACK_COOLDOWN;
	private bool attacking = false;

	public Vector2 ScreenSize; // Size of the game window.

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Hide();
		Callable onViewportSizeChanged = new Callable(this, nameof(OnViewportSizeChanged));
		GetTree().Root.Connect("size_changed", onViewportSizeChanged);
		OnViewportSizeChanged();
		//Position = ScreenSize / 2;
	}

	public override void _Process(double delta)
	{
		var attack = GetAttack(delta);
		var velocity = GetInput(attack);

		AnimateBody(velocity, attack);

		velocity = velocity * Speed;

		Position += velocity * (float)delta;
		Position = new Vector2(
		x: Mathf.Clamp(Position.X, 0, ScreenSize.X),
		y: Mathf.Clamp(Position.Y, 0, ScreenSize.Y));

	}

	public void Start(Vector2 position)
	{
		Position = position;
		Show();
		GetNode<CollisionShape2D>("body_hitbox").Disabled = false;
	}

	private void OnBodyEntered(PhysicsBody2D body)
	{
		if(attacking)
			return;
		Hide(); // Player disappears after being hit.
		EmitSignal(SignalName.Hit);
		// Must be deferred as we can't change physics properties on a physics callback.
		GetNode<CollisionShape2D>("body_hitbox").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
	}

	private void OnViewportSizeChanged()
	{
		ScreenSize = GetViewportRect().Size;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	private Vector2 GetInput(bool attack)
	{
		var velocity_increment = attack ? VELOCITY_ATTACK : VELOCITY;

		var velocity = Vector2.Zero; // The player's movement vector.

		if (Input.IsActionPressed("move_right"))
		{
			velocity.X += velocity_increment;
		}

		if (Input.IsActionPressed("move_left"))
		{
			velocity.X -= velocity_increment;
		}

		if (Input.IsActionPressed("move_down"))
		{
			velocity.Y += velocity_increment;
		}

		if (Input.IsActionPressed("move_up"))
		{
			velocity.Y -= velocity_increment;
		}

		return velocity;
	}

	private bool GetAttack(double delta)
	{
		if (cooldown > 0)
			cooldown -= delta;
		var input = Input.IsActionPressed("attack");
		if (input == attacking)
		{
			return input;
		}

		if (cooldown < 0)
		{
			if (input)
				cooldown = ATTACK_DELAY;
			if (!input)
				cooldown = ATTACK_COOLDOWN;

			attacking = input;
			return attacking;
		}

		return attacking;

	}

	private void AnimateBody(Vector2 velocity, bool attack)
	{
		var bodySprite = GetNode<AnimatedSprite2D>("body_sprite");

		if (attack)
		{
			if (velocity.Length() == 0)
			{
				bodySprite.Play("attack");
				return;
			}

			if (velocity.X > 0)
			{
				bodySprite.Play("move_attack");
				return;
			}

			if (velocity.X < 0)
			{
				bodySprite.Play("move_attack", fromEnd: true);
				return;
			}

			bodySprite.Play("move_attack");
			return;
		}

		if (!attack)
		{
			if (velocity.Length() == 0)
			{
				bodySprite.Play("default");
				return;
			}
			if (velocity.Length() > 0)
			{
				bodySprite.Play("move");
				return;
			}
		}


	}
}
