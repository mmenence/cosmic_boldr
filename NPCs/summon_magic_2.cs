using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace vasaga.NPCs
{
    public class summon_magic_2 : ModNPC
    {
        public override void SetStaticDefaults() { 

            DisplayName.SetDefault("The newer boulder");
        }

        private int attack_timer;
        private Vector2 bullet_velocity;
        private int num_projectiles = 1;


        public override void SetDefaults()
        {
            npc.width = 32;
            npc.height = 32;
            npc.scale = 4f;
            //npc.life = 100;
            npc.lifeMax = 15000;
            npc.damage = 70;
            npc.defense = 50;
            npc.HitSound = SoundID.Item10;
            npc.DeathSound = SoundID.Item107;
            //npc.HitSound = NPCHit3;//SoundID.Dig;//NPCHit1;
            //npc.DeathSound = NPCDeath3;//SoundID.Dig;//NPCDeath2;
            npc.value = 0f;
            npc.knockBackResist = 0f;
            npc.aiStyle = -1;
            npc.noGravity = true;


            //aiType = NPCID.Zombie;
            //animationType = NPCID.Zombie;
            npc.noTileCollide = true;
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
            else
            {
                //rotates in the direction of and proportional to how fast its moving
                npc.rotation += npc.velocity.X * 0.01f;

                //dust
                for (int i = 0; i < 20; i++)
                {
                    int dustType = 72;
                    int dustIndex = Dust.NewDust(npc.position, npc.width, npc.height, dustType);
                    Dust dust = Main.dust[dustIndex];
                    dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                    dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                    dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
                }


                if (attack_timer % 60 == 0)
                {
                    int numberProjectiles = num_projectiles;
                    for (int i = 0; i < numberProjectiles; i++)
                    {

                        bullet_angle(target, target, 10f);
                        Vector2 perturbedSpeed = new Vector2(bullet_velocity.X, bullet_velocity.Y).RotatedByRandom(MathHelper.ToRadians(20)); // 30 degree spread.
                                                                                                                                              // If you want to randomize the speed to stagger the projectiles
                        Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "sounds/Custom/Item_11.x"));                                                                                                                          // float scale = 1f - (Main.rand.NextFloat() * .3f);


                        if (NPC.AnyNPCs(mod.NPCType("summon_ranged_2")) && NPC.AnyNPCs(mod.NPCType("summon_melee_2")))
                        {
                            Projectile.NewProjectile(npc.Center, perturbedSpeed * 2f, 575, npc.damage / 6, 10f);
                        }
                        else
                        {
                            Projectile.NewProjectile(npc.Center, perturbedSpeed * 2f, mod.ProjectileType("magic_spawner_projectile"), npc.damage / 6, 10f);
                            npc.defense = 70;
                        }

                        if (!NPC.AnyNPCs(mod.NPCType("summon_ranged_2")) && !NPC.AnyNPCs(mod.NPCType("summon_melee_2")))
                        {
                            num_projectiles = 3;
                            npc.defense = 90;
                        }

                    }
                }

                if (attack_timer == 120)
                {
                    //NPC.NewNPC((int)npc.position.X, (int)npc.position.Y, mod.NPCType("summoned_boldr_1"));

                }

                move_towards(npc, target, (float)((int)Vector2.Distance(target, npc.Center) > 1000 ? 30f : 5f), 0f);

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

                //if(NPC.AnyNPCs(mod.NPCType("summon_ranged_1")) && NPC.AnyNPCs(mod.NPCType("summon_melee_1")) && attack_timer > 120)
                //{
                //    attack_timer = 0;
                //}

                //else if ((NPC.AnyNPCs(mod.NPCType("summon_ranged_1")) || NPC.AnyNPCs(mod.NPCType("summon_melee_1"))))
                //{
                //    attack_timer = 60;
                //}
                //
                //else if (attack_timer > 120)
                //{
                //    attack_timer = 90;
                //}

                attack_timer++;

                //handles despawning

            }
        }


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

        private void bullet_angle(Vector2 previous_position, Vector2 player_target, float speed)
        {
            var move = (player_target + ((player_target - previous_position) / 2.6f)) - npc.Center;


            float length = move.Length();
            if (length > speed)
            {
                move *= speed / length;
            }
            bullet_velocity = move;
        }


        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            scale = 1.4f;
            return null;
        }
    }
}