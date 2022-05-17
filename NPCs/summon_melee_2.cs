using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace vasaga.NPCs
{
    public class summon_melee_2 : ModNPC
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("The first boulder, Reborn");
        }

        private int attack_timer;

        public override void SetDefaults()
        {
            npc.width = 32;
            npc.height = 32;
            npc.scale = 4f;
            //npc.life = 100;
            npc.lifeMax = 20000;
            npc.damage = 90;
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

            //rotates in the direction of and proportional to how fast its moving
            npc.rotation += npc.velocity.X * 0.01f;

            //dust
            for (int i = 0; i < 13; i++)
            {
                int dustType = 6;
                int dustIndex = Dust.NewDust(npc.position, npc.width, npc.height, dustType);
                Dust dust = Main.dust[dustIndex];
                dust.velocity.X = dust.velocity.X + Main.rand.Next(-50, 51) * 0.01f;
                dust.velocity.Y = dust.velocity.Y + Main.rand.Next(-50, 51) * 0.01f;
                dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
            }


            if (attack_timer >= 90 && attack_timer % 45 == 0)
            {
                Main.PlaySound(36, npc.position, 0);
                move_towards(npc, target, 25f, 0);

            }
            else if (attack_timer < 90)
            {
                move_towards(npc, target, (float)((int)Vector2.Distance(target, npc.Center) > 900 ? 30f : 7f), 3f);
            }
            if(NPC.AnyNPCs(mod.NPCType("summon_ranged_2")) && NPC.AnyNPCs(mod.NPCType("summon_magic_2")) && attack_timer > 214)
            {
                attack_timer = 0;
            }

            else if ((NPC.AnyNPCs(mod.NPCType("summon_ranged_2")) || NPC.AnyNPCs(mod.NPCType("summon_magic_2"))) && attack_timer > 214)
            {
                attack_timer = 45;
                npc.defense = 80;
            }

            else if (attack_timer > 214)
            {
                attack_timer = 90;
                npc.defense = 110;
            }

            attack_timer++;
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


        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            scale = 1.4f;
            return null;
        }
    }
}