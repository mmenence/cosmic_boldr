using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace vasaga.projectiles
{
    public class magic_spawner_projectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            projectile.Size = new Vector2(32);
            projectile.aiStyle = -1;

            projectile.friendly = false;
            projectile.ranged = true;

            projectile.penetrate = 3;
            projectile.timeLeft = 60;

            projectile.ignoreWater = true;
            projectile.tileCollide = true;

            projectile.extraUpdates = 0;
            aiType = ProjectileID.Bullet;
        }

        public override void AI()
        {
            projectile.rotation += 0.1f;
        }

        public override void Kill(int timeLeft)
        {
            Random rnd = new Random();
            Main.PlaySound(SoundID.Item10, projectile.position);

            for (int i = 0; i < 50; i++)
            {
                int dustType = 72;
                int dustIndex = Dust.NewDust(projectile.position, projectile.width, projectile.height, dustType);
                Dust dust = Main.dust[dustIndex];
                dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
            }

            for (int i = 0; i < 5; i++)
            {

                Projectile.NewProjectile(projectile.position, new Vector2(rnd.Next(-5, 6), rnd.Next(-5, 6)), 575, (projectile.damage/2), projectile.knockBack);
            }
        }
    }
}