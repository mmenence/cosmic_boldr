using Microsoft.Xna.Framework;
using System;
using System.Drawing.Text;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json.Serialization;
using IL.Terraria.Audio;

namespace vasaga.NPCs
{
    [AutoloadBossHead]

    public class cosmic_boldr : ModNPC
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("The cosmic boulder");
        }

        private int attack_timer = -60;
        private int phase;
        private int phase_attack = 1;
        private int prev_attack;

        //melee vals
        private float circle_radius = 750;
        private int rand_value;
        private int tp_charge_1;
        private int tp_charge_2;
        private int tp_charge_3;

        //ranged vals
        private Vector2 prev_pos;
        private Vector2 bullet_velocity;
        private int plus_or_cross;

        //changing from stage one to two
        private bool stage;

        //checking if summon 2 has happened
        private bool has_summon_happened;

        //setting the timer for the extra bullets
        private int set_timer;

        private int radius_timer;



        public override void SetDefaults()
        {
            npc.width = 180;
            npc.height = 180;
            //npc.life = 100;
            npc.lifeMax = 100000;
            npc.damage = 80;
            npc.defense = 5;
            npc.HitSound = SoundID.Item10;
            npc.DeathSound = SoundID.Item107;
            npc.value = 0f;
            npc.knockBackResist = 0.0f;
            npc.aiStyle = -1;
            npc.npcSlots = 5;
            npc.noGravity = true;
            npc.noTileCollide = true;
            npc.boss = true;
            //music = MusicID.Boss2;
            music = MusicID.LunarBoss;
            //music = mod.GetSoundSlot(SoundType.Music, ("Sounds/Custom/plok_boss_music"));
            npc.value = Item.buyPrice(gold: 5);

        }
        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            npc.lifeMax = (int)(npc.lifeMax * bossLifeScale * 0.65f);
            npc.damage = (int)(npc.damage * 1.3f);
        }
        public override void AI()
        {
            //targets closest player
            npc.TargetClosest(true);
            Player player = Main.player[npc.target];
            Vector2 target = npc.HasPlayerTarget ? player.Center : Main.npc[npc.target].Center;


            npc.netAlways = true;
            npc.TargetClosest(true);

            //makes sure npc life is not greater than max life
            //if (npc.life >= npc.lifeMax)
            //    npc.life = npc.lifeMax;



            //handles despawning

            if (npc.target < 0 || npc.target == 255 || player.dead || !player.active)
            {
                npc.TargetClosest(false);
                npc.direction = 1;
                npc.velocity.Y -= 0.3f;
                if (npc.timeLeft > 20)
                {
                    npc.timeLeft = 20;
                    return;
                }
            }

            //rotates in the direction of and proportional to how fast its moving
            npc.rotation += npc.velocity.X * 0.01f;

            Random rnd = new Random();



            //melee phase
            if (phase == 0)
            {
                //particles
                for (int i = 0; i < 13; i++)
                {
                    int dustType = 6;
                    int dustIndex = Dust.NewDust(npc.position, npc.width, npc.height, dustType);
                    Dust dust = Main.dust[dustIndex];
                    dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                    dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                    dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                }


                //setting next attack
                if (phase_attack == 0)
                {
                    if (attack_timer == 30)
                    {
                        //Random rnd = new Random();
                        phase_attack = decide_attack(4);

                        //giving extra time for the first attack
                        if (phase_attack == 1)
                        {
                            attack_timer = -30;
                        }
                        else
                        {
                            attack_timer = 0;
                        }

                        //moving to next phase
                        if (npc.life < npc.lifeMax * 0.7)
                        {
                            phase++;
                        }
                    }

                }

                //melee first attack

                if (phase_attack == 1)
                {
                    //three charges
                    if (attack_timer % 90 == 0 && attack_timer < 290)
                    {
                        Main.PlaySound(SoundID.Roar, npc.position, 0);
                        move_towards(npc, target, 30f, 0f);
                    }

                    //particles above player to indicate attack
                    if (attack_timer >= 330 && attack_timer < 390)
                    {
                        for (int i = 0; i < 100; i++)
                        {
                            int dustType = 6;
                            int dustIndex = Dust.NewDust(new Vector2(player.position.X - 90, player.position.Y - 400), npc.width, npc.height, dustType);
                            Dust dust = Main.dust[dustIndex];
                            dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                            dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                            dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                        }
                    }
                    //teleport above player and charge downwards
                    if (attack_timer == 390)
                    {

                        npc.position = new Vector2(player.position.X - 90, player.position.Y - 400);
                        Main.PlaySound(SoundID.Roar, npc.position, 0);
                        move_towards(npc, target, 25f, 0f);
                    }
                    if (attack_timer == 480)
                    {
                        phase_attack = 0;
                        attack_timer = 0;
                    }
                }

                //melee atack 2
                if (phase_attack == 2)
                {
                    //spawns dust at border of circle
                    //below keeps the amount of particles proportional to the circumfrence of the circle
                    for (int i = 0; i < circle_radius; i++)
                    {
                        rand_value = rnd.Next(360);
                        int dustType = 6;
                        int dustIndex = Dust.NewDust(new Vector2(player.position.X + (((float)Math.Sin(rand_value)) * circle_radius), player.position.Y + (((float)Math.Cos(rand_value)) * circle_radius)), (90), (90), dustType);
                        Dust dust = Main.dust[dustIndex];
                        dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                        dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                        dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                    }

                    //moving towards the player, if it is within the circle radius unable to turn. if the are out of the raduis increased speed and able to turn
                    move_towards(npc, target, (float)((int)Vector2.Distance(target, npc.Center) > circle_radius ? 150f : 20f), (float)((int)Vector2.Distance(target, npc.Center) > circle_radius ? 0f : 3000f));

                    if (circle_radius < 450)
                    {
                        phase_attack = 0;
                        attack_timer = 0;
                        circle_radius = 750;    // if changing circle_radius start value, remember to change this as well
                    }
                    circle_radius -= 1;


                    //repeat of code here to stop zooming after death
                    if (npc.target < 0 || npc.target == 255 || player.dead || !player.active)
                    {
                        npc.TargetClosest(false);
                        npc.direction = 1;
                        npc.velocity.Y -= 0.1f;
                        circle_radius = 1500;
                        if (npc.timeLeft > 20)
                        {
                            npc.timeLeft = 20;
                            return;
                        }
                    }
                }

                //third melee attack
                if (phase_attack == 3)
                {
                    //initial 
                    switch (attack_timer)
                    {


                        case 30:
                            tp_charge_1 = rnd.Next(4);
                            switch (tp_charge_1)
                            {
                                //dust above player
                                case 0:

                                    for (int i = 0; i < 700; i++)
                                    {
                                        int dustType = 6;
                                        int dustIndex = Dust.NewDust(new Vector2(player.position.X - 90, player.position.Y - 400), npc.width, npc.height, dustType);
                                        Dust dust = Main.dust[dustIndex];
                                        dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                                    }
                                    break;
                                // dust to right
                                case 1:
                                    for (int i = 0; i < 700; i++)
                                    {
                                        int dustType = 6;
                                        int dustIndex = Dust.NewDust(new Vector2(player.position.X + 400, player.position.Y - 90), npc.width, npc.height, dustType);
                                        Dust dust = Main.dust[dustIndex];
                                        dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                                    }
                                    break;
                                //dust to bottom
                                case 2:
                                    for (int i = 0; i < 700; i++)
                                    {
                                        int dustType = 6;
                                        int dustIndex = Dust.NewDust(new Vector2(player.position.X - 90, player.position.Y + 400), npc.width, npc.height, dustType);
                                        Dust dust = Main.dust[dustIndex];
                                        dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                                    }
                                    break;
                                //dust to left
                                case 3:
                                    for (int i = 0; i < 700; i++)
                                    {
                                        int dustType = 6;
                                        int dustIndex = Dust.NewDust(new Vector2(player.position.X - 400, player.position.Y - 90), npc.width, npc.height, dustType);
                                        Dust dust = Main.dust[dustIndex];
                                        dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                                    }
                                    break;

                            }
                            break;


                        //second charge dust
                        case 60:
                            tp_charge_2 = rnd.Next(4);
                            switch (tp_charge_2)
                            {
                                //dust above player
                                case 0:

                                    for (int i = 0; i < 700; i++)
                                    {
                                        int dustType = 6;
                                        int dustIndex = Dust.NewDust(new Vector2(player.position.X - 90, player.position.Y - 400), npc.width, npc.height, dustType);
                                        Dust dust = Main.dust[dustIndex];
                                        dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                                    }
                                    break;
                                // dust to right
                                case 1:
                                    for (int i = 0; i < 700; i++)
                                    {
                                        int dustType = 6;
                                        int dustIndex = Dust.NewDust(new Vector2(player.position.X + 400, player.position.Y - 90), npc.width, npc.height, dustType);
                                        Dust dust = Main.dust[dustIndex];
                                        dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                                    }
                                    break;
                                //dust to bottom
                                case 2:
                                    for (int i = 0; i < 700; i++)
                                    {
                                        int dustType = 6;
                                        int dustIndex = Dust.NewDust(new Vector2(player.position.X - 90, player.position.Y + 400), npc.width, npc.height, dustType);
                                        Dust dust = Main.dust[dustIndex];
                                        dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                                    }
                                    break;
                                //dust to left
                                case 3:
                                    for (int i = 0; i < 700; i++)
                                    {
                                        int dustType = 6;
                                        int dustIndex = Dust.NewDust(new Vector2(player.position.X - 400, player.position.Y - 90), npc.width, npc.height, dustType);
                                        Dust dust = Main.dust[dustIndex];
                                        dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                                    }
                                    break;

                            }
                            break;

                        //third charge dust
                        case 90:
                            tp_charge_3 = rnd.Next(4);
                            switch (tp_charge_3)
                            {
                                //dust above player
                                case 0:

                                    for (int i = 0; i < 700; i++)
                                    {
                                        int dustType = 6;
                                        int dustIndex = Dust.NewDust(new Vector2(player.position.X - 90, player.position.Y - 400), npc.width, npc.height, dustType);
                                        Dust dust = Main.dust[dustIndex];
                                        dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                                    }
                                    break;
                                // dust to right
                                case 1:
                                    for (int i = 0; i < 700; i++)
                                    {
                                        int dustType = 6;
                                        int dustIndex = Dust.NewDust(new Vector2(player.position.X + 400, player.position.Y - 90), npc.width, npc.height, dustType);
                                        Dust dust = Main.dust[dustIndex];
                                        dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                                    }
                                    break;
                                //dust to bottom
                                case 2:
                                    for (int i = 0; i < 700; i++)
                                    {
                                        int dustType = 6;
                                        int dustIndex = Dust.NewDust(new Vector2(player.position.X - 90, player.position.Y + 400), npc.width, npc.height, dustType);
                                        Dust dust = Main.dust[dustIndex];
                                        dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                                    }
                                    break;
                                //dust to left
                                case 3:
                                    for (int i = 0; i < 700; i++)
                                    {
                                        int dustType = 6;
                                        int dustIndex = Dust.NewDust(new Vector2(player.position.X - 400, player.position.Y - 90), npc.width, npc.height, dustType);
                                        Dust dust = Main.dust[dustIndex];
                                        dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                                    }
                                    break;

                            }
                            break;

                        case 150:

                            //teleporting and charging, charge one
                            switch (tp_charge_1)
                            {
                                //up
                                case 0:
                                    npc.position = new Vector2(player.position.X - 90, player.position.Y - 400);
                                    
                                    break;

                                //right
                                case 1:
                                    npc.position = new Vector2(player.position.X + 400, player.position.Y - 90);
                                    
                                    break;

                                //bottom
                                case 2:
                                    npc.position = new Vector2(player.position.X - 90, player.position.Y + 310);
                                    
                                    break;

                                //left
                                case 3:
                                    npc.position = new Vector2(player.position.X - 400, player.position.Y - 90);
                                    
                                    break;

                            }
                            Main.PlaySound(SoundID.Roar, npc.position, 0);
                            move_towards(npc, target, 23f, 0f);
                            break;

                        case 210:

                            //teleporting and charging, charge two
                            switch (tp_charge_2)
                            {
                                //up
                                case 0:
                                    npc.position = new Vector2(player.position.X - 90, player.position.Y - 400);
                                    
                                    break;

                                //right
                                case 1:
                                    npc.position = new Vector2(player.position.X + 400, player.position.Y - 90);
                                    
                                    break;

                                //bottom
                                case 2:
                                    npc.position = new Vector2(player.position.X - 90, player.position.Y + 310);
                                    
                                    break;

                                //left
                                case 3:
                                    npc.position = new Vector2(player.position.X - 400, player.position.Y - 90);
                                    
                                    break;

                            }
                            Main.PlaySound(SoundID.Roar, npc.position, 0);
                            move_towards(npc, target, 23f, 0f);
                            break;

                        case 270:

                            //teleporting and charging, charge three
                            switch (tp_charge_3)
                            {
                                //up
                                case 0:
                                    npc.position = new Vector2(player.position.X - 90, player.position.Y - 400);
                                    
                                    break;

                                //right
                                case 1:
                                    npc.position = new Vector2(player.position.X + 400, player.position.Y - 90);
                                    
                                    break;

                                //bottom
                                case 2:
                                    npc.position = new Vector2(player.position.X - 90, player.position.Y + 310);
                                    
                                    break;

                                //left
                                case 3:
                                    npc.position = new Vector2(player.position.X - 400, player.position.Y - 90);
                                    
                                    break;

                            }
                            Main.PlaySound(SoundID.Roar, npc.position, 0);
                            move_towards(npc, target, 23f, 0f);
                            break;

                        case 300:
                            attack_timer = 0;
                            phase_attack = 0;
                            break;






                    }
                }
            }

            //ranged phase
            if (phase == 1)
            {
                //particles
                for (int i = 0; i < 20; i++)
                {
                    int dustType = 56;
                    int dustIndex = Dust.NewDust(npc.position, npc.width, npc.height, dustType);
                    Dust dust = Main.dust[dustIndex];
                    dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                    dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                    dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                }


                //setting next attack
                if (phase_attack == 0)
                {
                    if (attack_timer == 30)
                    {
                        //Random rnd = new Random();
                        phase_attack = decide_attack(4);
                        attack_timer = 0;


                        //moving to next phase
                        if (npc.life < npc.lifeMax * 0.4)
                        {
                            phase++;
                        }
                    }
                }

                if (phase_attack == 1)
                {
                    //attempts to move to a space 500 above player
                    //move_towards(npc, new Vector2(target.X, target.Y - 500), (float)((int)Vector2.Distance(new Vector2(target.X, target.Y - 500), npc.Center) > 500 ? 40f : 13f), (float)((int)Vector2.Distance(target, npc.Center) > circle_radius ? 0f : 3f));
                    move_towards(npc, target, (float)((int)Vector2.Distance(target, npc.Center) > 850 ? 30f : 7f), 7f);

                    if (attack_timer == 0)
                    {
                        Main.PlaySound(36, npc.position, 0);
                    }

                    //give vaiable first value
                    if (attack_timer == 0)
                    {
                        prev_pos = player.position;

                    }

                    if (attack_timer % 15 == 0)
                    {

                        bullet_angle(prev_pos, target, 40f);


                        prev_pos = player.position;
                    }

                    if (attack_timer % 7 == 0 && attack_timer >  45)
                    {
                        int numberProjectiles = 1;
                        for (int i = 0; i < numberProjectiles; i++)
                        {
                            Vector2 perturbedSpeed = new Vector2(bullet_velocity.X, bullet_velocity.Y).RotatedByRandom(MathHelper.ToRadians(5)); // 30 degree spread.
                                                                                                                                                 // If you want to randomize the speed to stagger the projectiles
                            Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "sounds/Custom/Item_11.x"));                                                                                                                          // float scale = 1f - (Main.rand.NextFloat() * .3f);

                            Projectile.NewProjectile(npc.Center, perturbedSpeed * 2f, mod.ProjectileType("cosmic_boldr_bullet"), npc.damage / 6, 5f);

                        }
                    }



                    if (attack_timer == 475)
                    {
                        npc.velocity.Y = 0;
                        phase_attack = 0;
                        attack_timer = 0;
                    }
                }

                if (phase_attack == 2)
                {
                    //moves towards player at medium speed
                    move_towards(npc, new Vector2(target.X, target.Y - 500), (float)((int)Vector2.Distance(target, npc.Center) > 600 ? 26f : 13f), (float)((int)Vector2.Distance(target, npc.Center) > circle_radius ? 3f : 5f));

                    if (attack_timer == 0)
                    {
                        Main.PlaySound(36, npc.position, 0);
                        plus_or_cross = rnd.Next(2) + 1;
                        prev_pos = target;

                        if (plus_or_cross % 2 == 1)
                        {
                            ranged_cross(prev_pos);
                        }
                        else
                        {
                            ranged_plus(prev_pos);
                        }

                    }

                    if (attack_timer == 30)
                    {
                        if (plus_or_cross % 2 == 1)
                        {
                            ranged_cross_bullet(prev_pos, 40f);
                        }
                        else
                        {
                            ranged_plus_bullet(prev_pos, 40f);
                        }
                        plus_or_cross++;
                    }


                    if (attack_timer == 60)
                    {
                        prev_pos = target;

                        if (plus_or_cross % 2 == 1)
                        {
                            ranged_cross(prev_pos);
                        }
                        else
                        {
                            ranged_plus(prev_pos);
                        }

                    }

                    if (attack_timer == 90)
                    {
                        if (plus_or_cross % 2 == 1)
                        {
                            ranged_cross_bullet(prev_pos, 40f);
                        }
                        else
                        {
                            ranged_plus_bullet(prev_pos, 40f);
                        }
                        plus_or_cross++;
                    }

                    if (attack_timer == 90)
                    {
                        prev_pos = target;

                        if (plus_or_cross % 2 == 1)
                        {
                            ranged_cross(prev_pos);
                        }
                        else
                        {
                            ranged_plus(prev_pos);
                        }

                    }

                    if (attack_timer == 120)
                    {
                        if (plus_or_cross % 2 == 1)
                        {
                            ranged_cross_bullet(prev_pos, 40f);
                        }
                        else
                        {
                            ranged_plus_bullet(prev_pos, 40f);
                        }
                        plus_or_cross++;
                    }

                    if (attack_timer == 150)
                    {
                        attack_timer = 0;
                        phase_attack = 0;
                    }
                }


                // circle bullet attack
                if (phase_attack == 3)
                {

                    move_towards(npc, target, 7f, 3f);

                    if (attack_timer == 0)
                    {
                        Main.PlaySound(36, npc.position, 0);


                        //dust
                        for (int i = 0; i < 20; i++)
                        {
                            int dustType = 156;
                            int dustIndex = Dust.NewDust(npc.position, npc.width, npc.height, dustType);
                            Dust dust = Main.dust[dustIndex];
                            dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                            dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                            dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                        }
                    }
                    if (attack_timer % 2 == 0)
                    {
                        Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "sounds/Custom/Item_11.x"));
                        bullet_circle(npc.Center, (float)attack_timer / 30f + 1.5f, 40f);
                    }


                    if (attack_timer == 600)
                    {
                        attack_timer = 0;
                        phase_attack = 0;
                    }



                }
            }
            //summon phase
            if (phase == 2)
            {
                for (int i = 0; i < 20; i++)
                {
                    int dustType = 64;
                    int dustIndex = Dust.NewDust(npc.position, npc.width, npc.height, dustType);
                    Dust dust = Main.dust[dustIndex];
                    dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                    dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                    dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                }

                if (npc.target < 0 || npc.target == 255 || player.dead || !player.active)
                {
                    npc.TargetClosest(false);
                    npc.direction = 1;
                    npc.velocity.Y -= 0.1f;
                    if (npc.timeLeft > 20)
                    {
                        npc.timeLeft = 20;
                        return;
                    }
                }
                else
                {
                    //setting next attack
                    if (true)
                    {
                        if (attack_timer == 1)
                        {
                            Main.PlaySound(36, npc.position, 0);

                            npc.friendly = true;
                            npc.immortal = true;
                            NPC.SpawnOnPlayer(player.whoAmI, mod.NPCType("summon_melee_1"));
                            NPC.SpawnOnPlayer(player.whoAmI, mod.NPCType("summon_ranged_1"));
                            NPC.SpawnOnPlayer(player.whoAmI, mod.NPCType("summon_magic_1"));

                        }

                        move_towards(npc, target, ((int)Vector2.Distance(target, npc.Center) < 600 ? 1f : (int)Vector2.Distance(target, npc.Center) > 1250 ? 30f : 5f), 0f);


                        //moving to next phase
                        if (!NPC.AnyNPCs(mod.NPCType("summon_ranged_1")) && !NPC.AnyNPCs(mod.NPCType("summon_magic_1")) && !NPC.AnyNPCs(mod.NPCType("summon_melee_1")) && attack_timer > 300)

                        {
                            npc.friendly = false;
                            npc.immortal = false;
                            phase++;
                        }


                    }
                }
            }


            //magic phase
            if (phase == 3)
            {

                //dust
                for (int i = 0; i < 30; i++)
                {
                    int dustType = 72;
                    int dustIndex = Dust.NewDust(npc.position, npc.width, npc.height, dustType);
                    Dust dust = Main.dust[dustIndex];
                    dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                    dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                    dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                }


                if (phase_attack == 0)
                {
                    if (attack_timer == 1)
                    {
                        //Random rnd = new Random();
                        phase_attack = decide_attack(4);
                        attack_timer = 0;

                        //moving to next phase
                        if (npc.life < npc.lifeMax * 0.1)
                        {
                            phase++;
                        }
                    }

                }


                if (phase_attack == 1)
                {
                    move_towards(npc, target, 5, 0f);

                    if (attack_timer % 60 == 0)
                    {
                        Main.PlaySound(SoundID.Pixie, npc.position, 0);

                        int numberProjectiles = 3 + Main.rand.Next(2); // 4 or 5 shots
                        for (int i = 0; i < numberProjectiles; i++)
                        {
                            Vector2 perturbedSpeed = new Vector2(npc.velocity.X * 2.5f, npc.velocity.Y * 2.5f).RotatedByRandom(MathHelper.ToRadians(50)); // 30 degree spread.
                                                                                                                                                          // If you want to randomize the speed to stagger the projectiles
                                                                                                                                                          // float scale = 1f - (Main.rand.NextFloat() * .3f);
                                                                                                                                                          // perturbedSpeed = perturbedSpeed * scale; 
                            Projectile.NewProjectile(npc.Center.X, npc.Center.Y, perturbedSpeed.X, perturbedSpeed.Y, mod.ProjectileType("magic_spawner_projectile"), npc.damage / 6, 3);
                        }
                    }

                    if (attack_timer >= 210)
                    {
                        phase_attack = 0;
                        attack_timer = 0;
                    }
                }

                //circle around player attack
                if (phase_attack == 2)
                {
                    if (attack_timer == 0)
                    {
                        Main.PlaySound(36, npc.position, 0);
                    }



                    //stopping the boldr and rotating it
                    npc.velocity = new Vector2(0, 0);
                    npc.rotation += .5f;

                    //setting its position around the player
                    npc.position = new Vector2(player.position.X + ((float)Math.Sin(attack_timer * 0.11) * 500), player.position.Y + (((float)Math.Cos(attack_timer * 0.11f)) * 500));

                    if (attack_timer % 6 == 0)
                    {
                        Projectile.NewProjectile(npc.Center.X, npc.Center.Y, find_direction(npc, target, 7f).X, find_direction(npc, target, 7f).Y, 575, npc.damage / 6, 3);
                    }

                    if (attack_timer > 180)
                    {
                        phase_attack = 0;
                        attack_timer = -30;
                        move_towards(npc, target, 3, 0f);
                    }


                }

                if (phase_attack == 3)
                {

                    if (attack_timer == 0)
                    {
                        Main.PlaySound(36, npc.position, 0);
                    }

                    //stopping the boldr and rotating it
                    npc.velocity = new Vector2(0, 0);
                    npc.rotation += 1f;

                    move_towards(npc, target, 5, 0f);


                    if (attack_timer % 3 == 0)
                    {

                        Projectile.NewProjectile(npc.position, new Vector2(npc.velocity.X + rnd.Next(-30, 31), npc.velocity.Y + rnd.Next(-50, 51)), 575, (npc.damage / 6), 3);
                    }

                    if (attack_timer > 180)
                    {
                        attack_timer = 0;
                        phase_attack = 0;
                    }

                }

            }


            //making sure it isnt killed before refilling hp
            if ((npc.life <= npc.lifeMax * 0.05f && !stage))
            {
                phase = 4;
                attack_timer = 0;

            }


            //transitioning to stage 2
            if (phase == 4 && !stage)
            {

                npc.velocity = new Vector2(0, 0);
                npc.immortal = true;
                npc.friendly = true;

                npc.life += npc.lifeMax /200;

                if (npc.life > npc.lifeMax)
                {
                    npc.life = npc.lifeMax;
                }

                npc.rotation += attack_timer * 0.001f;


                Vector2 dust_offset = new Vector2(((float)Math.Sin(attack_timer * 0.01f)) * 4, ((float)Math.Sin(attack_timer * 0.01f)) * 4);

                //melee dust
                for (int i = 0; i < 13; i++)
                {
                    int dustType = 6;
                    int dustIndex = Dust.NewDust(npc.position, npc.width, npc.height, dustType);
                    Dust dust = Main.dust[dustIndex];
                    dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f + dust_offset.X * 5;
                    dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f + dust_offset.Y*5;
                    dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                }

                //ranged dust
                for (int i = 0; i < 10; i++)
                {
                    int dustType = 56;
                    int dustIndex = Dust.NewDust(npc.position, npc.width, npc.height, dustType);
                    Dust dust = Main.dust[dustIndex];
                    dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f + dust_offset.X * 5;
                    dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f - dust_offset.Y * 5;
                    dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                }

                //summon dust
                for (int i = 0; i < 20; i++)
                {
                    int dustType = 64;
                    int dustIndex = Dust.NewDust(npc.position, npc.width, npc.height, dustType);
                    Dust dust = Main.dust[dustIndex];
                    dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f - dust_offset.X * 5;
                    dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f + dust_offset.Y * 5;
                    dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                }

                //magic dust
                for (int i = 0; i < 10; i++)
                {
                    int dustType = 72;
                    int dustIndex = Dust.NewDust(npc.position, npc.width, npc.height, dustType);
                    Dust dust = Main.dust[dustIndex];
                    dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f - dust_offset.X * 5;
                    dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f - dust_offset.Y * 5;
                    dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                }

                if (attack_timer == 360 && npc.life == npc.lifeMax)
                {
                    phase++;
                    npc.immortal = false;
                    npc.friendly = false;
                    attack_timer = 0;
                    phase_attack = 0;
                    stage = true;
                }

            }


            if (phase == 5)
            {
                if (phase_attack == 0)
                {
                    if (attack_timer == 30)
                    {
                        //Random rnd = new Random();
                        phase_attack = decide_attack(6);

                        //giving extra time for the first attack
                        if (phase_attack == 1)
                        {
                            attack_timer = -30;
                        }
                        else
                        {
                            attack_timer = 0;
                        }

                        //moving to next phase
                        if (npc.life < npc.lifeMax * 0.66 && !has_summon_happened)
                        {
                            phase++;
                        }
                        if (npc.life < npc.lifeMax * 0.33 && has_summon_happened)
                        {
                            phase = 7;
                            attack_timer = 0;

                        }

                    }

                }

                // first attack

                if (phase_attack == 1)
                {
                    //melee dust
                    for (int i = 0; i < 13; i++)
                    {
                        int dustType = 6;
                        int dustIndex = Dust.NewDust(npc.position, npc.width, npc.height, dustType);
                        Dust dust = Main.dust[dustIndex];
                        dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                        dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                        dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                    }

                    //summon dust
                    for (int i = 0; i < 20; i++)
                    {
                        int dustType = 64;
                        int dustIndex = Dust.NewDust(npc.position, npc.width, npc.height, dustType);
                        Dust dust = Main.dust[dustIndex];
                        dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                        dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                        dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                    }





                    //three charges
                    if (attack_timer % 90 == 0 && attack_timer < 290)
                    {
                        Main.PlaySound(SoundID.Roar, npc.position, 0);
                        move_towards(npc, target, 30f, 0f);
                        NPC.NewNPC((int)npc.position.X, (int)npc.position.Y, mod.NPCType("summoned_boldr_1"));
                    }

                    //particles above player to indicate attack
                    if (attack_timer >= 330 && attack_timer < 390)
                    {
                        for (int i = 0; i < 100; i++)
                        {
                            int dustType = 6;
                            int dustIndex = Dust.NewDust(new Vector2(player.position.X - 90, player.position.Y - 400), npc.width, npc.height, dustType);
                            Dust dust = Main.dust[dustIndex];
                            dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                            dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                            dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                        }
                    }
                    //teleport above player and charge downwards
                    if (attack_timer == 390)
                    {

                        npc.position = new Vector2(player.position.X - 90, player.position.Y - 400);
                        Main.PlaySound(SoundID.Roar, npc.position, 0);
                        move_towards(npc, target, 25f, 0f);
                    }

                    //summons three pairs of boldrs on the way down
                    if (attack_timer > 420 && (attack_timer % 20 == 0))
                    {
                        NPC.NewNPC((int)npc.position.X - 30, (int)npc.position.Y, mod.NPCType("summoned_boldr_1"));
                        NPC.NewNPC((int)npc.position.X + 30, (int)npc.position.Y, mod.NPCType("summoned_boldr_1"));
                    }
                    if (attack_timer == 480)
                    {
                        phase_attack = 0;
                        attack_timer = 0;
                    }
                }

                //sceond attack
                if (phase_attack == 2)
                {

                    //melee dust
                    for (int i = 0; i < 10; i++)
                    {
                        int dustType = 6;
                        int dustIndex = Dust.NewDust(npc.position, npc.width, npc.height, dustType);
                        Dust dust = Main.dust[dustIndex];
                        dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                        dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                        dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                    }

                    //magic dust
                    for (int i = 0; i < 10; i++)
                    {
                        int dustType = 72;
                        int dustIndex = Dust.NewDust(npc.position, npc.width, npc.height, dustType);
                        Dust dust = Main.dust[dustIndex];
                        dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                        dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                        dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                    }



                    //initial 
                    switch (attack_timer)
                    {


                        case 30:
                            tp_charge_1 = rnd.Next(4);
                            switch (tp_charge_1)
                            {
                                //dust above player
                                case 0:

                                    for (int i = 0; i < 700; i++)
                                    {
                                        int dustType = 6;
                                        int dustIndex = Dust.NewDust(new Vector2(player.position.X - 90, player.position.Y - 400), npc.width, npc.height, dustType);
                                        Dust dust = Main.dust[dustIndex];
                                        dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                                    }
                                    break;
                                // dust to right
                                case 1:
                                    for (int i = 0; i < 700; i++)
                                    {
                                        int dustType = 6;
                                        int dustIndex = Dust.NewDust(new Vector2(player.position.X + 400, player.position.Y - 90), npc.width, npc.height, dustType);
                                        Dust dust = Main.dust[dustIndex];
                                        dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                                    }
                                    break;
                                //dust to bottom
                                case 2:
                                    for (int i = 0; i < 700; i++)
                                    {
                                        int dustType = 6;
                                        int dustIndex = Dust.NewDust(new Vector2(player.position.X - 90, player.position.Y + 400), npc.width, npc.height, dustType);
                                        Dust dust = Main.dust[dustIndex];
                                        dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                                    }
                                    break;
                                //dust to left
                                case 3:
                                    for (int i = 0; i < 700; i++)
                                    {
                                        int dustType = 6;
                                        int dustIndex = Dust.NewDust(new Vector2(player.position.X - 400, player.position.Y - 90), npc.width, npc.height, dustType);
                                        Dust dust = Main.dust[dustIndex];
                                        dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                                    }
                                    break;

                            }
                            break;


                        //second charge dust
                        case 60:
                            tp_charge_2 = rnd.Next(4);
                            switch (tp_charge_2)
                            {
                                //dust above player
                                case 0:

                                    for (int i = 0; i < 700; i++)
                                    {
                                        int dustType = 6;
                                        int dustIndex = Dust.NewDust(new Vector2(player.position.X - 90, player.position.Y - 400), npc.width, npc.height, dustType);
                                        Dust dust = Main.dust[dustIndex];
                                        dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                                    }
                                    break;
                                // dust to right
                                case 1:
                                    for (int i = 0; i < 700; i++)
                                    {
                                        int dustType = 6;
                                        int dustIndex = Dust.NewDust(new Vector2(player.position.X + 400, player.position.Y - 90), npc.width, npc.height, dustType);
                                        Dust dust = Main.dust[dustIndex];
                                        dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                                    }
                                    break;
                                //dust to bottom
                                case 2:
                                    for (int i = 0; i < 700; i++)
                                    {
                                        int dustType = 6;
                                        int dustIndex = Dust.NewDust(new Vector2(player.position.X - 90, player.position.Y + 400), npc.width, npc.height, dustType);
                                        Dust dust = Main.dust[dustIndex];
                                        dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                                    }
                                    break;
                                //dust to left
                                case 3:
                                    for (int i = 0; i < 700; i++)
                                    {
                                        int dustType = 6;
                                        int dustIndex = Dust.NewDust(new Vector2(player.position.X - 400, player.position.Y - 90), npc.width, npc.height, dustType);
                                        Dust dust = Main.dust[dustIndex];
                                        dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                                    }
                                    break;

                            }
                            break;

                        //third charge dust
                        case 90:
                            tp_charge_3 = rnd.Next(4);
                            switch (tp_charge_3)
                            {
                                //dust above player
                                case 0:

                                    for (int i = 0; i < 700; i++)
                                    {
                                        int dustType = 6;
                                        int dustIndex = Dust.NewDust(new Vector2(player.position.X - 90, player.position.Y - 400), npc.width, npc.height, dustType);
                                        Dust dust = Main.dust[dustIndex];
                                        dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                                    }
                                    break;
                                // dust to right
                                case 1:
                                    for (int i = 0; i < 700; i++)
                                    {
                                        int dustType = 6;
                                        int dustIndex = Dust.NewDust(new Vector2(player.position.X + 400, player.position.Y - 90), npc.width, npc.height, dustType);
                                        Dust dust = Main.dust[dustIndex];
                                        dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                                    }
                                    break;
                                //dust to bottom
                                case 2:
                                    for (int i = 0; i < 700; i++)
                                    {
                                        int dustType = 6;
                                        int dustIndex = Dust.NewDust(new Vector2(player.position.X - 90, player.position.Y + 400), npc.width, npc.height, dustType);
                                        Dust dust = Main.dust[dustIndex];
                                        dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                                    }
                                    break;
                                //dust to left
                                case 3:
                                    for (int i = 0; i < 700; i++)
                                    {
                                        int dustType = 6;
                                        int dustIndex = Dust.NewDust(new Vector2(player.position.X - 400, player.position.Y - 90), npc.width, npc.height, dustType);
                                        Dust dust = Main.dust[dustIndex];
                                        dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                                        dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                                    }
                                    break;

                            }
                            break;

                        case 150:

                            //teleporting and charging, charge one
                            switch (tp_charge_1)
                            {
                                //up
                                case 0:
                                    npc.position = new Vector2(player.position.X - 90, player.position.Y - 400);
                                    Main.PlaySound(SoundID.Roar, npc.position, 0);
                                    move_towards(npc, target, 25f, 0f);
                                    break;

                                //right
                                case 1:
                                    npc.position = new Vector2(player.position.X + 400, player.position.Y - 90);
                                    Main.PlaySound(SoundID.Roar, npc.position, 0);
                                    move_towards(npc, target, 25f, 0f);
                                    break;

                                //bottom
                                case 2:
                                    npc.position = new Vector2(player.position.X - 90, player.position.Y + 310);
                                    Main.PlaySound(SoundID.Roar, npc.position, 0);
                                    move_towards(npc, target, 25f, 0f);
                                    break;

                                //left
                                case 3:
                                    npc.position = new Vector2(player.position.X - 400, player.position.Y - 90);
                                    Main.PlaySound(SoundID.Roar, npc.position, 0);
                                    move_towards(npc, target, 25f, 0f);
                                    break;

                            }
                            Projectile.NewProjectile(npc.Center.X, npc.Center.Y, npc.velocity.X / 3, npc.velocity.Y / 3, mod.ProjectileType("magic_spawner_projectile"), npc.damage / 6, 3);
                            break;

                        case 210:

                            //teleporting and charging, charge two
                            switch (tp_charge_2)
                            {
                                //up
                                case 0:
                                    npc.position = new Vector2(player.position.X - 90, player.position.Y - 400);
                                    Main.PlaySound(SoundID.Roar, npc.position, 0);
                                    move_towards(npc, target, 25f, 0f);
                                    break;

                                //right
                                case 1:
                                    npc.position = new Vector2(player.position.X + 400, player.position.Y - 90);
                                    Main.PlaySound(SoundID.Roar, npc.position, 0);
                                    move_towards(npc, target, 25f, 0f);
                                    break;

                                //bottom
                                case 2:
                                    npc.position = new Vector2(player.position.X - 90, player.position.Y + 310);
                                    Main.PlaySound(SoundID.Roar, npc.position, 0);
                                    move_towards(npc, target, 25f, 0f);
                                    break;

                                //left
                                case 3:
                                    npc.position = new Vector2(player.position.X - 400, player.position.Y - 90);
                                    Main.PlaySound(SoundID.Roar, npc.position, 0);
                                    move_towards(npc, target, 25f, 0f);
                                    break;

                            }
                            Projectile.NewProjectile(npc.Center.X, npc.Center.Y, npc.velocity.X / 3, npc.velocity.Y / 3, mod.ProjectileType("magic_spawner_projectile"), npc.damage / 6, 3);
                            break;

                        case 270:

                            //teleporting and charging, charge three
                            switch (tp_charge_3)
                            {
                                //up
                                case 0:
                                    npc.position = new Vector2(player.position.X - 90, player.position.Y - 400);
                                    Main.PlaySound(SoundID.Roar, npc.position, 0);
                                    move_towards(npc, target, 25f, 0f);
                                    break;

                                //right
                                case 1:
                                    npc.position = new Vector2(player.position.X + 400, player.position.Y - 90);
                                    Main.PlaySound(SoundID.Roar, npc.position, 0);
                                    move_towards(npc, target, 25f, 0f);
                                    break;

                                //bottom
                                case 2:
                                    npc.position = new Vector2(player.position.X - 90, player.position.Y + 310);
                                    Main.PlaySound(SoundID.Roar, npc.position, 0);
                                    move_towards(npc, target, 25f, 0f);
                                    break;

                                //left
                                case 3:
                                    npc.position = new Vector2(player.position.X - 400, player.position.Y - 90);
                                    Main.PlaySound(SoundID.Roar, npc.position, 0);
                                    move_towards(npc, target, 25f, 0f);
                                    break;

                            }
                            Projectile.NewProjectile(npc.Center.X, npc.Center.Y, npc.velocity.X / 3, npc.velocity.Y / 3, mod.ProjectileType("magic_spawner_projectile"), npc.damage / 6, 3);
                            break;

                        case 300:
                            attack_timer = 0;
                            phase_attack = 0;
                            break;






                    }
                }

                if (phase_attack == 3)
                {
                    //magic dust
                    for (int i = 0; i < 10; i++)
                    {
                        int dustType = 72;
                        int dustIndex = Dust.NewDust(npc.position, npc.width, npc.height, dustType);
                        Dust dust = Main.dust[dustIndex];
                        dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                        dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                        dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                    }

                    //ranged dust
                    for (int i = 0; i < 10; i++)
                    {
                        int dustType = 56;
                        int dustIndex = Dust.NewDust(npc.position, npc.width, npc.height, dustType);
                        Dust dust = Main.dust[dustIndex];
                        dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                        dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                        dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                    }



                    //moves towards player at medium speed
                    move_towards(npc, new Vector2(target.X, target.Y - 500), (float)((int)Vector2.Distance(target, npc.Center) > 500 ? 26f : 13f), (float)((int)Vector2.Distance(target, npc.Center) > circle_radius ? 3f : 5f));

                    if (attack_timer == 0)
                    {
                        Main.PlaySound(36, npc.position, 0);
                        plus_or_cross = rnd.Next(2) + 1;
                        prev_pos = target;

                        if (plus_or_cross % 2 == 1)
                        {
                            ranged_cross(prev_pos);
                        }
                        else
                        {
                            ranged_plus(prev_pos);
                        }

                    }

                    if (attack_timer == 30)
                    {
                        if (plus_or_cross % 2 == 1)
                        {
                            mranged_cross_bullet(prev_pos, 20f);
                        }
                        else
                        {
                            mranged_plus_bullet(prev_pos, 20f);
                        }
                        plus_or_cross++;
                    }


                    if (attack_timer == 60)
                    {
                        prev_pos = target;

                        if (plus_or_cross % 2 == 1)
                        {
                            ranged_cross(prev_pos);
                        }
                        else
                        {
                            ranged_plus(prev_pos);
                        }

                    }

                    if (attack_timer == 90)
                    {
                        if (plus_or_cross % 2 == 1)
                        {
                            mranged_cross_bullet(prev_pos, 20f);
                        }
                        else
                        {
                            mranged_plus_bullet(prev_pos, 20f);
                        }
                        plus_or_cross++;
                    }

                    if (attack_timer == 90)
                    {
                        prev_pos = target;

                        if (plus_or_cross % 2 == 1)
                        {
                            ranged_cross(prev_pos);
                        }
                        else
                        {
                            ranged_plus(prev_pos);
                        }

                    }

                    if (attack_timer == 120)
                    {
                        if (plus_or_cross % 2 == 1)
                        {
                            mranged_cross_bullet(prev_pos, 20f);
                        }
                        else
                        {
                            mranged_plus_bullet(prev_pos, 20f);
                        }
                        plus_or_cross++;
                    }

                    if (attack_timer == 150)
                    {
                        attack_timer = 0;
                        phase_attack = 0;
                        npc.velocity.Y = 0;
                    }



                }


                if (phase_attack == 4)
                {
                    //magic dust
                    for (int i = 0; i < 10; i++)
                    {
                        int dustType = 72;
                        int dustIndex = Dust.NewDust(npc.position, npc.width, npc.height, dustType);
                        Dust dust = Main.dust[dustIndex];
                        dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                        dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                        dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                    }

                    //ranged dust
                    for (int i = 0; i < 10; i++)
                    {
                        int dustType = 56;
                        int dustIndex = Dust.NewDust(npc.position, npc.width, npc.height, dustType);
                        Dust dust = Main.dust[dustIndex];
                        dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                        dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                        dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                    }




                    move_towards(npc, new Vector2(target.X, target.Y - 500), (float)((int)Vector2.Distance(new Vector2(target.X, target.Y - 500), npc.Center) > 500 ? 40f : 13f), (float)((int)Vector2.Distance(target, npc.Center) > circle_radius ? 0f : 3f));
                    npc.velocity.Y = 0;
                    npc.position.Y = player.position.Y - 500;




                    if (attack_timer % 15 == 0)
                    {
                        int numberProjectiles = 1;
                        for (int i = 0; i < numberProjectiles; i++)
                        {

                            bullet_angle(prev_pos, target, 20f);
                            Vector2 perturbedSpeed = new Vector2(bullet_velocity.X, bullet_velocity.Y).RotatedByRandom(MathHelper.ToRadians(5)); // 30 degree spread.
                                                                                                                                                 // If you want to randomize the speed to stagger the projectiles
                            Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "sounds/Custom/Item_11.x"));                                                                                                                          // float scale = 1f - (Main.rand.NextFloat() * .3f);

                            Projectile.NewProjectile(npc.Center, perturbedSpeed * 2f, 575, npc.damage / 6, 10f);
                            prev_pos = player.position;

                        }
                    }


                    if (attack_timer == 164)
                    {
                        attack_timer = 0;
                        phase_attack = 0;
                    }

                }


                if (phase_attack == 5)
                {
                    //magic dust
                    for (int i = 0; i < 10; i++)
                    {
                        int dustType = 72;
                        int dustIndex = Dust.NewDust(npc.position, npc.width, npc.height, dustType);
                        Dust dust = Main.dust[dustIndex];
                        dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                        dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                        dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                    }

                    //summon dust
                    for (int i = 0; i < 10; i++)
                    {
                        int dustType = 64;
                        int dustIndex = Dust.NewDust(npc.position, npc.width, npc.height, dustType);
                        Dust dust = Main.dust[dustIndex];
                        dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                        dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                        dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                    }


                    if (attack_timer == 0)
                    {
                        Main.PlaySound(36, npc.position, 0);
                    }

                    //stopping the boldr and rotating it
                    npc.velocity = new Vector2(0, 0);
                    npc.rotation += 1f;

                    move_towards(npc, target, 5, 0f);


                    if (attack_timer % 3 == 0)
                    {

                        Projectile.NewProjectile(npc.position, new Vector2(npc.velocity.X + rnd.Next(-30, 31), npc.velocity.Y + rnd.Next(-50, 51)), 575, (npc.damage / 6), 3);
                    }

                    if (attack_timer % 30 == 0)
                    {
                        NPC.NewNPC((int)npc.position.X, (int)npc.position.Y, mod.NPCType("summoned_boldr_1"));
                    }



                    if (attack_timer > 180)
                    {
                        attack_timer = 0;
                        phase_attack = 0;
                    }

                }


            }

            //summon phase
            if (phase == 6)
            {
                for (int i = 0; i < 20; i++)
                {
                    int dustType = 64;
                    int dustIndex = Dust.NewDust(npc.position, npc.width, npc.height, dustType);
                    Dust dust = Main.dust[dustIndex];
                    dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                    dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                    dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                }



                if (true)
                {
                    if (attack_timer == 1)
                    {
                        Main.PlaySound(36, npc.position, 0);

                        npc.friendly = true;
                        npc.immortal = true;
                        NPC.SpawnOnPlayer(player.whoAmI, mod.NPCType("summon_melee_2"));
                        NPC.SpawnOnPlayer(player.whoAmI, mod.NPCType("summon_ranged_2"));
                        NPC.SpawnOnPlayer(player.whoAmI, mod.NPCType("summon_magic_2"));

                    }

                    move_towards(npc, target, ((int)Vector2.Distance(target, npc.Center) < 600 ? 1f : (int)Vector2.Distance(target, npc.Center) > 1250 ? 30f : 5f), 0f);


                    //moving to next phase
                    if (!NPC.AnyNPCs(mod.NPCType("summon_ranged_2")) && !NPC.AnyNPCs(mod.NPCType("summon_magic_2")) && !NPC.AnyNPCs(mod.NPCType("summon_melee_2")) && attack_timer > 300)

                    {
                        npc.friendly = false;
                        npc.immortal = false;
                        phase = 5;
                        has_summon_happened = true;
                        attack_timer = 0;
                        phase_attack = 0;
                    }

                }


            }

            if(phase == 7)
            {


                move_towards(npc, target, 5f, 3f);

                //setting radius
                circle_radius = (npc.life / npc.lifeMax)*400 + 600;


                if (attack_timer == 0)
                {
                        Main.PlaySound(36, npc.position, 0);
                        npc.position = new Vector2(player.position.X, player.position.Y - 500);

                }


                

                if (attack_timer % 3 == 0)
                {
                        //Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "sounds/Custom/Item_11.x"));
                        bullet_circle(npc.Center, (float)attack_timer * 2 / (180f - (120 - ((npc.life/ npc.lifeMax) * 120))) + 1.57f, 20f);
                        bullet_circle(npc.Center, (float)attack_timer * 2 / (180f - (120 - ((npc.life / npc.lifeMax) * 120))) - 1.57f, 20f);
                }

                //setting the extra spurts to activate one second after 1/6 hp is reached
                if(npc.life <= npc.lifeMax / 6)
                {
                    set_timer++;
                }

                //roaring
                if (set_timer == 1)
                {
                    Main.PlaySound(36, npc.position, 0);
                }

                //dust to indicate
                if(set_timer >= 1 && set_timer < 60)
                {
                    bullet_circle_dust(npc.Center, (float)attack_timer * 2 / (180f - (120 - ((npc.life / npc.lifeMax) * 120))), 10f);
                    bullet_circle_dust(npc.Center, (float)attack_timer * 2 / (180f - (120 - ((npc.life / npc.lifeMax) * 120))) + 3.14f, 10f);
                }

                //extra spurts
                if(set_timer >= 60 && attack_timer % 3 == 0)
                {
                    bullet_circle(npc.Center, (float)attack_timer * 2 / (180f - (120 - ((npc.life / npc.lifeMax) * 120))), 20f);
                    bullet_circle(npc.Center, (float)attack_timer * 2 / (180f - (120 - ((npc.life / npc.lifeMax) * 120))) +3.14f, 20f);
                }





                //    dust circle and teleporting



                // dust at radius
                for (int i = 0; i < circle_radius/4; i++)
                {
                    rand_value = rnd.Next(360);
                    int dustType = 6;
                    int dustIndex = Dust.NewDust(new Vector2(npc.Center.X + (((float)Math.Sin(rand_value)) * circle_radius), npc.Center.Y + (((float)Math.Cos(rand_value)) * circle_radius)), (60), (60), dustType);
                    Dust dust = Main.dust[dustIndex];
                    dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                    dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                    dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                }

                var distance = player.position - npc.Center;
                float length = distance.Length();
                if (length > circle_radius)
                {
                    radius_timer++;

                    //Dust circle around player
                    for (int i = 0; i < 50; i++)
                    {
                        rand_value = rnd.Next(360);
                        int dustType = 6;
                        int dustIndex = Dust.NewDust(new Vector2(player.position.X + (((float)Math.Sin(rand_value)) * 50), player.position.Y + (((float)Math.Cos(rand_value)) * 50)), (20), (20), dustType);
                        Dust dust = Main.dust[dustIndex];
                        dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                        dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                        dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                    }

                    //dust circle around where player is to be teleported to

                    var tp_location = (player.position - npc.Center);
                    tp_location.Normalize();



                    for (int i = 0; i < 50; i++)
                    {
                        rand_value = rnd.Next(360);
                        int dustType = 6;
                        int dustIndex = Dust.NewDust(new Vector2(npc.Center.X + (((float)Math.Sin(rand_value)) * 50) +tp_location.X*circle_radius/2, npc.Center.Y + (((float)Math.Cos(rand_value)) * 50) + tp_location.Y * circle_radius / 2), (20), (20), dustType);
                        Dust dust = Main.dust[dustIndex];
                        dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                        dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                        dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                    }

                    if (radius_timer == 90)
                    {
                        player.position = new Vector2(npc.position.X + tp_location.X * (circle_radius /2),npc.position.Y+ tp_location.Y * (circle_radius / 2));
                        
                    }
                    
                }
                else
                {
                    radius_timer = 0;
                }

                

            }


            attack_timer++;

                //handles despawning

                if (npc.target < 0 || npc.target == 255 || player.dead || !player.active)
                {
                    npc.TargetClosest(false);
                    npc.direction = 1;
                    npc.velocity.Y -= 0.1f;
                    if (npc.timeLeft > 20)
                    {
                        npc.timeLeft = 20;
                        return;
                    }
                }




            
        }










        private void move_towards(NPC npc, Vector2 playerTarget, float speed, float turnResistance)
        {
            //find velocity
            var move = playerTarget - npc.Center;
            float length = move.Length();
            if (length > speed)
            {
                move *= speed / length;
            }
            //turn resistance
            move = (npc.velocity * turnResistance + move) / (turnResistance + 1f);
            length = move.Length();
            {
                move *= speed / length;
            }
            //change the velocity
            npc.velocity = move;
        }


        //checking if the previous attack is up next

        private int decide_attack(int max)
        {
            var next_attack = new Random().Next(1, max);
            if(next_attack != prev_attack)
            {
                prev_attack = next_attack;
                return next_attack;
            }
            else
            {
                return decide_attack(max);
            }

        }




        // ranged


        private void bullet_angle(Vector2 previous_position, Vector2 player_target, float speed)
        {
            var move = (player_target + ((player_target - previous_position)/2.6f) )- npc.Center;
            

            float length = move.Length();
            if (length > speed)
            {
                move *= speed / length;
            }
            bullet_velocity = move;
        }


        private void ranged_plus(Vector2 portals_centre)
        {
            Projectile.NewProjectile((portals_centre.X-465), portals_centre.Y-45, 1, 0, mod.ProjectileType("portal"), npc.damage /2, 5f);
            Projectile.NewProjectile((portals_centre.X+300), portals_centre.Y-45, 1, 0, mod.ProjectileType("portal"), npc.damage / 2, 5f);
            Projectile.NewProjectile((portals_centre.X -45 ), portals_centre.Y - 400, 0, 1, mod.ProjectileType("portal"), npc.damage / 2, 5f);
            Projectile.NewProjectile((portals_centre.X -45), portals_centre.Y + 375, 0, 1, mod.ProjectileType("portal"), npc.damage / 2, 5f);
        }

        private void ranged_cross(Vector2 portals_centre)
        {
            //awful fix
            portals_centre.X -= 45;

            //up
            //left
            Projectile.NewProjectile((portals_centre.X -283), portals_centre.Y - 283, -1, -1, mod.ProjectileType("portal"), npc.damage / 2, 5f);
            //right
            Projectile.NewProjectile((portals_centre.X + 237), portals_centre.Y - 283, 1, 1, mod.ProjectileType("portal"), npc.damage / 2, 5f);

            //down
            //right
            Projectile.NewProjectile((portals_centre.X +263), portals_centre.Y +263, -1, -1, mod.ProjectileType("portal"), npc.damage / 2, 5f);
            //left
            Projectile.NewProjectile((portals_centre.X -328), portals_centre.Y + 283, 1, 1, mod.ProjectileType("portal"), npc.damage / 2, 5f);
        }

        private void ranged_cross_bullet(Vector2 portals_centre, float bullet_velocity)
        {
            bullet_velocity = bullet_velocity / 2;
            //up
            //left
            Projectile.NewProjectile((portals_centre.X - 283), portals_centre.Y - 283, bullet_velocity/1.414f, bullet_velocity / 1.414f, mod.ProjectileType("cosmic_boldr_bullet"), npc.damage / 6, 5f);
            //right
            Projectile.NewProjectile((portals_centre.X + 283), portals_centre.Y - 283, -bullet_velocity / 1.414f, bullet_velocity / 1.414f, mod.ProjectileType("cosmic_boldr_bullet"), npc.damage / 6, 5f);

            //down
            //left
            Projectile.NewProjectile((portals_centre.X - 283), portals_centre.Y + 283, bullet_velocity / 1.414f, -bullet_velocity / 1.414f, mod.ProjectileType("cosmic_boldr_bullet"), npc.damage / 6, 5f);
            //right
            Projectile.NewProjectile((portals_centre.X + 283), portals_centre.Y + 283, -bullet_velocity / 1.414f, -bullet_velocity / 1.414f, mod.ProjectileType("cosmic_boldr_bullet"), npc.damage / 6, 5f);
        }

        private void ranged_plus_bullet(Vector2 portals_centre, float bullet_velocity)
        {

            //left
            Projectile.NewProjectile((portals_centre.X - 400), portals_centre.Y, bullet_velocity, 0, mod.ProjectileType("cosmic_boldr_bullet"), npc.damage/6, 5f);
            //right
            Projectile.NewProjectile((portals_centre.X + 400), portals_centre.Y, -bullet_velocity, 0, mod.ProjectileType("cosmic_boldr_bullet"), npc.damage/ 6, 5f);

            //down
            Projectile.NewProjectile((portals_centre.X), portals_centre.Y + 400, 0, -bullet_velocity / 1.414f, mod.ProjectileType("cosmic_boldr_bullet"), npc.damage /6, 5f);
            //up
            Projectile.NewProjectile((portals_centre.X), portals_centre.Y - 400, 0, bullet_velocity / 1.414f, mod.ProjectileType("cosmic_boldr_bullet"), npc.damage/ 6, 5f);
        }

        private void mranged_cross_bullet(Vector2 portals_centre, float bullet_velocity)
        {
            bullet_velocity = bullet_velocity / 2;

            //up
            //left
            Projectile.NewProjectile((portals_centre.X - 283), portals_centre.Y - 283, bullet_velocity / 1.414f, bullet_velocity / 1.414f, mod.ProjectileType("magic_spawner_projectile"), npc.damage / 6, 5f);
            //right
            Projectile.NewProjectile((portals_centre.X + 283), portals_centre.Y - 283, -bullet_velocity / 1.414f, bullet_velocity / 1.414f, mod.ProjectileType("magic_spawner_projectile"), npc.damage / 6, 5f);

            //down
            //left
            Projectile.NewProjectile((portals_centre.X - 283), portals_centre.Y + 283, bullet_velocity / 1.414f, -bullet_velocity / 1.414f, mod.ProjectileType("magic_spawner_projectile"), npc.damage / 6, 5f);
            //right
            Projectile.NewProjectile((portals_centre.X + 283), portals_centre.Y + 283, -bullet_velocity / 1.414f, -bullet_velocity / 1.414f, mod.ProjectileType("magic_spawner_projectile"), npc.damage / 6, 5f);
        }

        private void mranged_plus_bullet(Vector2 portals_centre, float bullet_velocity)
        {

            //left
            Projectile.NewProjectile((portals_centre.X - 400), portals_centre.Y, bullet_velocity, 0, mod.ProjectileType("magic_spawner_projectile"), npc.damage / 6, 5f);
            //right
            Projectile.NewProjectile((portals_centre.X + 400), portals_centre.Y, -bullet_velocity, 0, mod.ProjectileType("magic_spawner_projectile"), npc.damage / 6, 5f);

            //down
            Projectile.NewProjectile((portals_centre.X), portals_centre.Y + 400, 0, -bullet_velocity / 1.414f, mod.ProjectileType("magic_spawner_projectile"), npc.damage / 6, 5f);
            //up
            Projectile.NewProjectile((portals_centre.X), portals_centre.Y - 400, 0, bullet_velocity / 1.414f, mod.ProjectileType("magic_spawner_projectile"), npc.damage / 6, 5f);
        }

        private void bullet_circle(Vector2 centre,float rotation, float bullet_velocity)
        {
            Projectile.NewProjectile(centre.X, centre.Y, ((float)Math.Sin(rotation)) * bullet_velocity, ((float)Math.Cos(rotation)) * bullet_velocity, mod.ProjectileType("cosmic_boldr_bullet"), npc.damage / 8, 5f);
        }

        private void bullet_circle_dust(Vector2 centre, float rotation, float dust_velocity)
        {
            //Projectile.NewProjectile(centre.X, centre.Y, ((float)Math.Sin(rotation)) * dust_velocity, ((float)Math.Cos(rotation)) * dust_velocity, mod.ProjectileType("cosmic_boldr_bullet"), npc.damage / 8, 5f);

            for (int i = 0; i < 5; i++)
            {
                int dustType = 56;
                int dustIndex = Dust.NewDust(npc.position, 30, 30, dustType);
                Dust dust = Main.dust[dustIndex];
                dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f + ((float)Math.Sin(rotation)) * dust_velocity;
                dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f + ((float)Math.Cos(rotation)) * dust_velocity;
                dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
            }
        }




        //magic
        private Vector2 find_direction(NPC npc, Vector2 playerTarget, float speed)
        {
            //find velocity
            var move = playerTarget - npc.Center;
            float length = move.Length();
            if (length > speed)
            {
                move *= speed / length;
            }
            
            //change the velocity
            return move;
        }


        //stage transition
        //private Vector2 transition_dust_speed(int timer, float speed)
        //{
        //    return new Vector2(((float)Math.Sin(timer * 0.01f)) * speed, ((float)Math.Sin(timer*0.01f)) * speed);
        //}




        public override void HitEffect(int hitDirection, double damage)
        {
            for (int i = 0; i < 10; i++)
            {
                int dustType = 1;
                int dustIndex = Dust.NewDust(npc.position, npc.width, npc.height, dustType);
                Dust dust = Main.dust[dustIndex];
                dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
            }
        }
        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            scale = 1.75f;
            return null;
        }

    }
}   
