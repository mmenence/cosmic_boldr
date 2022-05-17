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
    public class cosmic_boldr_bullet : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            projectile.Size = new Vector2(8);
            projectile.aiStyle = 1;

            projectile.friendly = false;
            projectile.hostile = true;
            projectile.ranged = true;

            projectile.penetrate = 3;
            projectile.timeLeft = 60;

            projectile.ignoreWater = true;
            projectile.tileCollide = false;

            projectile.extraUpdates = 1;
            aiType = ProjectileID.Bullet;
        }
        public override void AI()
        {
            //add lighting
            Lighting.AddLight(projectile.position, 0.5f, 0.25f, 0f);

            //dust
            for (int i = 0; i < 3; i++)
            {
                int dustType = 56;
                int dustIndex = Dust.NewDust(projectile.position, projectile.width, projectile.height, dustType);
                Dust dust = Main.dust[dustIndex];
                dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
            }
        }

    }
}