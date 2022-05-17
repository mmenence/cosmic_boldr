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
    public class portal : ModProjectile
    {
        private int curse_thing2;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            projectile.height = 180;
            projectile.width = 30;
            projectile.aiStyle = -1;

            projectile.penetrate = -1;
            projectile.timeLeft = 700;

            projectile.ignoreWater = true;
            projectile.tileCollide = true;

            projectile.extraUpdates = 1;

        }

        public override void AI()
        {
            if (!NPC.AnyNPCs(mod.NPCType("cosmic_boldr")))
            {
                projectile.timeLeft = 1;
            }
            curse_thing2++;
            if (curse_thing2 == 70)
            {
                projectile.timeLeft = 1;

            }



            //deciding the angle


            //rotated by 45
            if (projectile.velocity.X == 1 && projectile.velocity.Y == 1)
            {
                projectile.rotation = -0.75125f;//1.525f;
                projectile.velocity.X = 0;
                projectile.velocity.Y = 0;
            }


            //rotated by -45
            else if (projectile.velocity.X == -1 && projectile.velocity.Y == -1)
            {
                projectile.rotation = 0.75125f;
                projectile.velocity.X = 0;
                projectile.velocity.Y = 0;
            }

            //rotated by 90
            else if (projectile.velocity.Y == 1)
            {
                projectile.rotation = 1.525f;
                projectile.velocity.X = 0;
                projectile.velocity.Y = 0;
            }


            //not rotated
            else if (projectile.velocity.X == 1)
            {
                projectile.velocity.X = 0;
            }



        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 50; i++)
            {
                int dustType = 163; //75 may be better, but ignores Y velocity
                int dustIndex = Dust.NewDust(projectile.position, projectile.width, projectile.height, dustType);
                Dust dust = Main.dust[dustIndex];
                dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
            }
        }


    }
}