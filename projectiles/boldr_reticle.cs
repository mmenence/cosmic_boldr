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
    public class boldr_reticle : ModProjectile
    {
        private int curse_thing2;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            projectile.height = 32;
            projectile.width = 32;
            projectile.aiStyle = -1;

            projectile.penetrate = -1;
            projectile.timeLeft = 700;

            projectile.ignoreWater = true;
            projectile.tileCollide = true;

            projectile.extraUpdates = 0;

        }

        public override void AI()
        {
            
            curse_thing2++;

            

            projectile.rotation += (30 - curse_thing2) * 0.1f;
            projectile.scale = ((60 - curse_thing2) / 60);





            for (int i = 0; i < 3; i++)
            {
                int dustType = 6;
                int dustIndex = Dust.NewDust(projectile.position, projectile.width, projectile.height, dustType);
                Dust dust = Main.dust[dustIndex];
                dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
            }


        }

        


    }
}