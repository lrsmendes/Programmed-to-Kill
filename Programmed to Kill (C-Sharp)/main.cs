using System;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

class Program
{
    static void Main()
    {
        RenderWindow window = new RenderWindow(new VideoMode(800, 600), "Programmed to Kill: Prototype");
        window.SetFramerateLimit(60);

        Player player = new Player();
        List<Bullet> bullets = new List<Bullet>();
        List<Enemy> enemies = new List<Enemy>();

        // Add some initial enemies
        for (int i = 0; i < 5; ++i)
        {
            enemies.Add(new Enemy((float)new Random().Next(750), (float)new Random().Next(100)));
        }

        while (window.IsOpen)
        {
            window.DispatchEvents();

            // Check for collisions between player bullets and enemies
            foreach (Enemy enemy in enemies)
            {
                for (int i = bullets.Count - 1; i >= 0; i--)
                {
                    Bullet bullet = bullets[i];
                    if (CollisionChecker(bullet, enemy))
                    {
                        bullets.RemoveAt(i);
                        enemy.Respawn((float)new Random().Next(750));
                        break;
                    }
                }
            }

            // Player input
            if (Keyboard.IsKeyPressed(Keyboard.Key.Left))
                player.Move(-5, 0);
            if (Keyboard.IsKeyPressed(Keyboard.Key.Right))
                player.Move(5, 0);
            if (Keyboard.IsKeyPressed(Keyboard.Key.Up))
                player.Move(0, -5);
            if (Keyboard.IsKeyPressed(Keyboard.Key.Down))
                player.Move(0, 5);

            // Shoot bullets
            if (Keyboard.IsKeyPressed(Keyboard.Key.Space))
            {
                bullets.Add(new Bullet(player.GetSprite().Position.X + 22.5f, player.GetSprite().Position.Y));
            }

            // Move bullets
            foreach (Bullet bullet in bullets)
            {
                bullet.Move();
            }

            // Remove bullets that are out of the window
            bullets.RemoveAll(bullet => bullet.GetShape().Position.Y < 0);

            // Move enemies and check for shooting
            foreach (Enemy enemy in enemies)
            {
                float moveX = 0;
                float moveY = 2;

                enemy.Move(moveX, moveY);

                if (enemy.CanShoot())
                {
                    bullets.Add(enemy.Shoot());
                    enemy.ResetShootCooldown();
                }

                if (enemy.GetShape().Position.Y > 600)
                {
                    enemy.Respawn((float)new Random().Next(750));
                }

                // Check for collisions between bullets and enemies
                for (int i = bullets.Count - 1; i >= 0; i--)
                {
                    Bullet bullet = bullets[i];
                    if (CollisionChecker(bullet, enemy))
                    {
                        bullets.RemoveAt(i);
                        enemy.Respawn((float)new Random().Next(750));
                        break;
                    }
                }
            }

            // Enemy shooting logic
            foreach (Enemy enemy in enemies)
            {
                if (enemy.CanShoot())
                {
                    bullets.Add(enemy.Shoot());
                    enemy.ResetShootCooldown();
                }
            }

            // Check for collisions between enemy bullets and the player
            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                Bullet bullet = bullets[i];
                if (bullet.IsEnemyBullet() && CollisionChecker(bullet, player))
                {
                    // Handle player being hit by enemy bullet (you can implement your own logic here)
                    Console.WriteLine("Player hit by enemy bullet!");
                    bullets.RemoveAt(i);
                    break;
                }
            }

            window.Clear();

            // Draw the player
            window.Draw(player.GetSprite());

            // Draw bullets
            foreach (Bullet bullet in bullets)
            {
                window.Draw(bullet.GetShape());
            }

            // Draw enemies
            foreach (Enemy enemy in enemies)
            {
                window.Draw(enemy.GetShape());

                // Draw enemy bullets
                foreach (Bullet enemyBullet in enemy.GetEnemyBullets())
                {
                    window.Draw(enemyBullet.GetShape());
                }
            }

            window.Display();
        }
    }

    static bool CollisionChecker(Bullet bullet, Enemy enemy)
    {
        return bullet.GetShape().GetGlobalBounds().Intersects(enemy.GetShape().GetGlobalBounds());
    }

    static bool CollisionChecker(Bullet bullet, Player player)
    {
        return bullet.GetShape().GetGlobalBounds().Intersects(player.GetSprite().GetGlobalBounds());
    }
}

class Player
{
    private Texture texture;
    private Sprite sprite;

    public Player()
    {
        texture = new Texture("C:\\soldier.png");
        sprite = new Sprite(texture)
        {
            Scale = new Vector2f(0.5f, 0.5f),
            Position = new Vector2f(200, 200)
        };
    }

    public void Move(float dx, float dy)
    {
        sprite.Position += new Vector2f(dx, dy);
    }

    public Sprite GetSprite()
    {
        return sprite;
    }
}

class Bullet
{
    private RectangleShape shape;
    private float speed;
    private bool enemyBullet;

    public Bullet(float startX, float startY, bool isEnemyBullet = false)
    {
        shape = new RectangleShape(new Vector2f(5, 5))
        {
            FillColor = isEnemyBullet ? Color.Red : Color.Yellow,
            Position = new Vector2f(startX, startY)
        };
        speed = 5.0f;
        enemyBullet = isEnemyBullet;
    }

    public void Move()
    {
        shape.Position += new Vector2f(0, enemyBullet ? 5 : -speed);
    }

    public RectangleShape GetShape()
    {
        return shape;
    }

    public bool IsEnemyBullet()
    {
        return enemyBullet;
    }
}

class Enemy
{
    private RectangleShape shape;
    private int respawnTime;
    private int shootCooldown;
    private List<Bullet> enemyBullets = new List<Bullet>();

    public Enemy(float startX, float startY)
    {
        shape = new RectangleShape(new Vector2f(30, 30))
        {
            FillColor = Color.Blue,
            Position = new Vector2f(startX, startY)
        };
        respawnTime = 300;
        shootCooldown = 60;
    }

    public Bullet Shoot()
    {
        Bullet newBullet = new Bullet(shape.Position.X + 15.0f, shape.Position.Y + 30.0f, true);
        enemyBullets.Add(newBullet);
        return newBullet;
    }

    public List<Bullet> GetEnemyBullets()
    {
        return enemyBullets;
    }

    public bool IsHit(Player player)
    {
        return shape.GetGlobalBounds().Intersects(player.GetSprite().GetGlobalBounds());
    }

    public void Move(float x, float y)
    {
        shape.Position += new Vector2f(x, y);
        respawnTime -= 1;
        shootCooldown -= 1;
    }

    public void Respawn(float x)
    {
        shape.Position = new Vector2f(x, 0);
        respawnTime = 300;
    }

    public bool CanShoot()
    {
        return respawnTime <= 0 && shootCooldown <= 0;
    }

    public void UpdateRespawnTime()
    {
        if (respawnTime > 0)
        {
            respawnTime -= 1;
        }
    }

    public RectangleShape GetShape()
    {
        return shape;
    }

    public void ResetShootCooldown()
    {
        shootCooldown = 60;
    }
}
