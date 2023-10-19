﻿using CalamityMod;
using CalamityMod.NPCs.ProfanedGuardians;
using CalamityMod.Projectiles.Boss;
using FargowiltasCrossmod.Content.Calamity.Bosses.SlimeGod;
using FargowiltasCrossmod.Content.Common.Projectiles;
using FargowiltasCrossmod.Core;
using FargowiltasCrossmod.Core.Calamity;
using FargowiltasCrossmod.Core.Utils;
using FargowiltasSouls;
using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Content.Bosses.MutantBoss;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static Mono.CompilerServices.SymbolWriter.CodeBlockEntry;

namespace FargowiltasCrossmod.Content.Common.Bosses.Mutant
{
    public class MutantDLC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        private static bool Thorium => ModCompatibility.ThoriumMod.Loaded;
        private static bool Calamity => ModCompatibility.Calamity.Loaded;
        public static bool ShouldDoDLC 
        {
            get => Calamity;
        }
        public override bool AppliesToEntity(NPC npc, bool lateInstantiation) => npc.type == ModContent.NPCType<MutantBoss>() && ShouldDoDLC;

        private bool PlayStoria = false;
        public void ManageMusicAndSky(NPC npc)
        {
            
            if (npc.localAI[3] >= 3 || npc.ai[0] == 10) //p2 or phase transition
            {
                
                if (npc.life < npc.lifeMax / 2 && npc.ai[0] != 10) //play storia under half health
                {
                    PlayStoria = true;
                }
                if (PlayStoria && ModLoader.TryGetMod("FargowiltasMusic", out Mod musicMod) && musicMod.Version >= Version.Parse("0.1.1"))
                {
                    if (npc.ai[0] < 0) //desperation
                    {
                        npc.ModNPC.Music = MusicLoader.GetMusicSlot(musicMod, "Assets/Music/StoriaShort");
                    }
                    else
                    {
                        npc.ModNPC.Music = MusicLoader.GetMusicSlot(musicMod, "Assets/Music/Storia");
                    }
                }
                else
                {
                    npc.ModNPC.Music = MusicLoader.GetMusicSlot("FargowiltasCrossmod/Assets/Music/IrondustRePrologue");
                }
            }
            else //p1
            {
                
            }
            if (DLCAttackChoice == DLCAttack.Calamitas)
            {
                ManageMusicFade(true);
            }
            else
            {
                ManageMusicFade(false);
            }

            void ManageMusicFade(bool fade)
            {
                if (fade)
                {
                    Main.musicFade[Main.curMusic] = MathHelper.Lerp(Main.musicFade[Main.curMusic], 0.5f, 0.05f);
                }
                else
                {
                    Main.musicFade[Main.curMusic] = MathHelper.Lerp(Main.musicFade[Main.curMusic], 1, 0.01f);
                }
            }
            /*
            if (npc.ai[0] == 10)
            {
                if (!SkyManager.Instance["FargowiltasCrossmod:MutantDLC"].IsActive())
                    SkyManager.Instance.Activate("FargowiltasCrossmod:MutantDLC");
            }

            if (SkyManager.Instance["FargowiltasSouls:MutantBoss"].IsActive())
                SkyManager.Instance.Deactivate("FargowiltasSouls:MutantBoss");
            */
        }
        public enum DLCAttack
        {
            BumbleDrift = -2,
            BumbleDash = -1,
            None = 0,
            PrepareAresNuke,
            AresNuke,
            SlimeGodSlam,
            Calamitas,
            BumbleDrift2,
            BumbleDash2,
            Providence
        };
        public DLCAttack DLCAttackChoice = 0;
        public Vector2 LockVector1;
        public int Timer = 0;
        public int Counter = 0;
        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            binaryWriter.Write((int)DLCAttackChoice);
            binaryWriter.WriteVector2(LockVector1);
            binaryWriter.Write(Timer);
            binaryWriter.Write(Counter);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            DLCAttackChoice = (DLCAttack)binaryReader.Read();
            LockVector1 = binaryReader.ReadVector2();
            Timer = binaryReader.Read();
            Counter = binaryReader.Read();
        }
        public override bool PreAI(NPC npc)
        {
            if (!ShouldDoDLC)
            {
                return true;
            }

            Player player = Main.player[npc.target];
            if (player == null || !player.active || player.dead)
            {
                return true;
            }

            ref float attackChoice = ref npc.ai[0];
            //DO NOT TOUCH npc.ai[1] DURING PHASE 1 CUSTOM ATTACKS

            switch (attackChoice) 
            {
                #region Attack Reroutes
                case 4: //straight dash spam
                    if (Calamity)
                    {
                        DLCAttackChoice = DLCAttack.BumbleDrift;
                        npc.netUpdate = true;
                    }
                    break;

                case 20: //eoc star
                    if (Calamity)
                    {
                        DLCAttackChoice = DLCAttack.Calamitas;
                        npc.netUpdate = true;
                    }
                    break;
                case 21: //straight dash spam p2
                    if (Calamity)
                    {
                        DLCAttackChoice = DLCAttack.BumbleDrift2;
                        npc.netUpdate = true;
                    }
                    break;
                case 33: //nuke
                    if (Calamity)
                    {
                        DLCAttackChoice = DLCAttack.PrepareAresNuke;
                        npc.netUpdate = true;
                    }
                    break;
                case 35: //slime rain
                    if (Calamity)
                    {
                        DLCAttackChoice = DLCAttack.SlimeGodSlam;
                        npc.netUpdate = true;
                    }
                    break;
                case 41: //straight penetrator throw full circle rotation
                    if (Calamity)
                    {
                        if (npc.ai[2] > npc.localAI[1] / 2) //after half of the attack, reroute to providence attack
                        {
                            DLCAttackChoice = DLCAttack.Providence;
                            npc.netUpdate = true;
                        }
                    }
                    break;
                #endregion
                #region Attack Additions
                //attack additions

                case 38:
                case 30:
                    if (Calamity) CalamityFishron(); break;

                case 27:
                    if (Calamity) CalamityMechRayFan(); break;

                #endregion
            }
            if (DLCAttackChoice < DLCAttack.None) //p1
            {
                attackChoice = 6; //p1 "while dashing", has very few effects
                npc.ai[1]--; //negate normal timer
            }
            else if (DLCAttackChoice > DLCAttack.None) //p2
            {
                attackChoice = 28; //empty normal attack
            }
            switch (DLCAttackChoice) //new attacks
            {
                case DLCAttack.BumbleDrift: BumbleDrift(); break;
                case DLCAttack.BumbleDash: BumbleDash(); break;
                case DLCAttack.PrepareAresNuke: PrepareAresNuke(); break;
                case DLCAttack.AresNuke: AresNuke(); break;
                case DLCAttack.SlimeGodSlam: SlimeGodSlam(); break;
                case DLCAttack.Calamitas: Calamitas(); break;
                case DLCAttack.BumbleDrift2: BumbleDrift2(); break;
                case DLCAttack.BumbleDash2: BumbleDash2(); break;
                case DLCAttack.Providence: Providence(); break;
                default: //reset variables
                    {
                        Timer = 0;
                        Counter = 0;
                    }
                    break;

            }
            return base.PreAI(npc);

            #region Checks and Commons
            bool AliveCheck(Player p, bool forceDespawn = false)
            {
                if (WorldSavingSystem.SwarmActive || forceDespawn || (!p.active || p.dead || Vector2.Distance(npc.Center, p.Center) > 3000f) && npc.localAI[3] > 0)
                {
                    npc.TargetClosest();
                    p = Main.player[npc.target];
                    if (WorldSavingSystem.SwarmActive || forceDespawn || !p.active || p.dead || Vector2.Distance(npc.Center, p.Center) > 3000f)
                    {
                        if (npc.timeLeft > 30)
                            npc.timeLeft = 30;
                        npc.velocity.Y -= 1f;
                        if (npc.timeLeft == 1)
                        {
                            if (npc.position.Y < 0)
                                npc.position.Y = 0;
                            if (DLCUtils.HostCheck && ModContent.TryFind("Fargowiltas", "Mutant", out ModNPC modNPC) && !NPC.AnyNPCs(modNPC.Type))
                            {
                                FargoSoulsUtil.ClearHostileProjectiles(2, npc.whoAmI);
                                int n = NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X, (int)npc.Center.Y, modNPC.Type);
                                if (n != Main.maxNPCs)
                                {
                                    Main.npc[n].homeless = true;
                                    if (Main.netMode == NetmodeID.Server)
                                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, n);
                                }
                            }
                        }
                        return false;
                    }
                }

                if (npc.timeLeft < 3600)
                    npc.timeLeft = 3600;

                if (player.Center.Y / 16f > Main.worldSurface)
                {
                    npc.velocity.X *= 0.95f;
                    npc.velocity.Y -= 1f;
                    if (npc.velocity.Y < -32f)
                        npc.velocity.Y = -32f;
                    return false;
                }

                return true;
            }

            bool Phase2Check()
            {
                if (Main.expertMode && npc.life < npc.lifeMax / 2)
                {
                    if (DLCUtils.HostCheck)
                    {
                        npc.ai[0] = 10;
                        npc.ai[1] = 0;
                        npc.ai[2] = 0;
                        npc.ai[3] = 0;
                        npc.netUpdate = true;
                        FargoSoulsUtil.ClearHostileProjectiles(1, npc.whoAmI);
                        EdgyBossText("Time to stop playing around.");
                    }
                    return true;
                }
                return false;
            }
            void EdgyBossText(string text) //because it's FUCKING private
            {
                if (Main.zenithWorld) //edgy boss text
                {
                    Color color = Color.Cyan;
                    FargoSoulsUtil.PrintText(text, color);
                    CombatText.NewText(npc.Hitbox, color, text, true);
                    /*
                    if (Main.netMode == NetmodeID.SinglePlayer)
                        Main.NewText(text, Color.LimeGreen);
                    else if (Main.netMode == NetmodeID.Server)
                        ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(text), Color.LimeGreen);
                    */
                }
            }
            void Movement(Vector2 target, float speed, float maxSpeed = 24, bool fastX = true, bool obeySpeedCap = true)
            {
                float turnaroundModifier = 1f;

                if (WorldSavingSystem.MasochistModeReal)
                {
                    speed *= 2;
                    turnaroundModifier *= 2f;
                    maxSpeed *= 1.5f;
                }

                if (Math.Abs(npc.Center.X - target.X) > 10)
                {
                    if (npc.Center.X < target.X)
                    {
                        npc.velocity.X += speed;
                        if (npc.velocity.X < 0)
                            npc.velocity.X += speed * (fastX ? 2 : 1) * turnaroundModifier;
                    }
                    else
                    {
                        npc.velocity.X -= speed;
                        if (npc.velocity.X > 0)
                            npc.velocity.X -= speed * (fastX ? 2 : 1) * turnaroundModifier;
                    }
                }
                if (npc.Center.Y < target.Y)
                {
                    npc.velocity.Y += speed;
                    if (npc.velocity.Y < 0)
                        npc.velocity.Y += speed * 2 * turnaroundModifier;
                }
                else
                {
                    npc.velocity.Y -= speed;
                    if (npc.velocity.Y > 0)
                        npc.velocity.Y -= speed * 2 * turnaroundModifier;
                }

                if (obeySpeedCap)
                {
                    if (Math.Abs(npc.velocity.X) > maxSpeed)
                        npc.velocity.X = maxSpeed * Math.Sign(npc.velocity.X);
                    if (Math.Abs(npc.velocity.Y) > maxSpeed)
                        npc.velocity.Y = maxSpeed * Math.Sign(npc.velocity.Y);
                }
            }
            void ChooseNextAttack(params int[] args)
            {
                MutantBoss mutantBoss = npc.ModNPC as MutantBoss;
                float buffer = npc.ai[0] + 1;
                npc.ai[0] = 48;
                npc.ai[1] = 0;
                npc.ai[2] = buffer;
                npc.ai[3] = 0;
                npc.localAI[0] = 0;
                npc.localAI[1] = 0;
                npc.localAI[2] = 0;
                //NPC.TargetClosest();
                npc.netUpdate = true;

                /*string text = "-------------------------------------------------";
                Main.NewText(text);

                text = "";
                foreach (float f in attackHistory)
                    text += f.ToString() + " ";
                Main.NewText($"history: {text}");*/

                if (WorldSavingSystem.EternityMode)
                {
                    //become more likely to use randoms as life decreases
                    bool useRandomizer = npc.localAI[3] >= 3 && (WorldSavingSystem.MasochistModeReal || Main.rand.NextFloat(0.8f) + 0.2f > (float)Math.Pow((float)npc.life / npc.lifeMax, 2));

                    if (DLCUtils.HostCheck)
                    {
                        Queue<float> recentAttacks = new(mutantBoss.attackHistory); //copy of attack history that i can remove elements from freely

                        //if randomizer, start with a random attack, else use the previous state + 1 as starting attempt BUT DO SOMETHING ELSE IF IT'S ALREADY USED
                        if (useRandomizer)
                            npc.ai[2] = Main.rand.Next(args);

                        //Main.NewText(useRandomizer ? "(Starting with random)" : "(Starting with regular next attack)");

                        while (recentAttacks.Count > 0)
                        {
                            bool foundAttackToUse = false;

                            for (int i = 0; i < 5; i++) //try to get next attack that isnt in this queue
                            {
                                if (!recentAttacks.Contains(npc.ai[2]))
                                {
                                    foundAttackToUse = true;
                                    break;
                                }
                                npc.ai[2] = Main.rand.Next(args);
                            }

                            if (foundAttackToUse)
                                break;

                            //couldn't find an attack to use after those attempts, forget 1 attack and repeat
                            recentAttacks.Dequeue();

                            //Main.NewText("REDUCE");
                        }

                        /*text = "";
                        foreach (float f in recentAttacks)
                            text += f.ToString() + " ";
                        Main.NewText($"recent: {text}");*/
                    }
                }

                if (DLCUtils.HostCheck)
                {
                    int maxMemory = WorldSavingSystem.MasochistModeReal ? 10 : 16;

                    if (mutantBoss.attackCount++ > maxMemory * 1.25) //after doing this many attacks, shorten queue so i can be more random again
                    {
                        mutantBoss.attackCount = 0;
                        maxMemory /= 4;
                    }

                    mutantBoss.attackHistory.Enqueue(npc.ai[2]);
                    while (mutantBoss.attackHistory.Count > maxMemory)
                        mutantBoss.attackHistory.Dequeue();
                }

                mutantBoss.endTimeVariance = WorldSavingSystem.MasochistModeReal ? Main.rand.NextFloat() : 0;

                /*text = "";
                foreach (float f in attackHistory)
                    text += f.ToString() + " ";
                Main.NewText($"after: {text}");*/
            }
            void Reset()
            {
                DLCAttackChoice = DLCAttack.None;
                Timer = 0;
                Counter = 0;
                npc.netUpdate = true;
            }
            #endregion
            #region Calamity Attacks
            #region Attack Additions
            [JITWhenModsEnabled(ModCompatibility.Calamity.Name)]
            void CalamityFishron()
            {
                const int fishronDelay = 3;
                int maxFishronSets = WorldSavingSystem.MasochistModeReal ? 3 : 2;
                if (npc.ai[1] == fishronDelay * maxFishronSets + 35)
                {
                    SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Custom/OldDukeHuff"), Main.LocalPlayer.Center);
                    if (DLCUtils.HostCheck)
                    {
                        for (int j = -1; j <= 1; j += 2) //to both sides of player
                        {
                            Vector2 offset = npc.ai[2] == 0 ? Vector2.UnitX * -450f * j : Vector2.UnitY * 475f * j;
                            Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<MutantOldDuke>(), FargoSoulsUtil.ScaledProjectileDamage(npc.damage), 0f, Main.myPlayer, offset.X, offset.Y);
                        }
                    }
                }
            }
            void CalamityMechRayFan()
            {
                float timer = npc.ai[3];
                if (timer % 90 == 0 && timer > 90)
                {
                    if (DLCUtils.HostCheck)
                    {
                        int distance = 550;
                        Vector2 pos = player.Center + distance * Vector2.UnitX.RotatedBy(MathHelper.Pi * (((Main.rand.NextBool() ? 1f : -1f) / 5f) + Main.rand.Next(2)));
                        Vector2 vel = pos.DirectionTo(player.Center);
                        Projectile.NewProjectile(npc.GetSource_FromThis(), pos, vel, ModContent.ProjectileType<MutantPBG>(), FargoSoulsUtil.ScaledProjectileDamage(npc.damage), 0f, Main.myPlayer);
                    }
                }
            }
            #endregion
            #region Phase 1 Attacks
            [JITWhenModsEnabled(ModCompatibility.Calamity.Name)]
            void BumbleDrift()
            {
                if (!AliveCheck(player))
                    return;
                if (Phase2Check())
                    return;
                const int WindupTime = 60;
                const int DriftTime = 200;
                const float TotalDriftAngle = MathHelper.TwoPi * 2f;
                if (Timer == 0)
                {
                    LockVector1 = player.DirectionTo(npc.Center) * 520;
                }
                if (Timer == WindupTime)
                {
                    
                    if (DLCUtils.HostCheck)
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<DLCMutantSpearSpin>(), FargoSoulsUtil.ScaledProjectileDamage(npc.damage), 0f, Main.myPlayer, npc.whoAmI, DriftTime);
                }
                if (npc.Distance(player.Center + LockVector1) < 400)
                {
                    LockVector1 = LockVector1.RotatedBy(TotalDriftAngle / DriftTime);
                }
                
                Vector2 targetPos = player.Center + LockVector1;
                Movement(targetPos, 2f, player.velocity.Length() / 2 + 30, false);
                if (Timer > WindupTime + DriftTime)
                {
                    Timer = 0;
                    DLCAttackChoice = DLCAttack.BumbleDash;
                    return;
                }
                Timer++;
            }
            [JITWhenModsEnabled(ModCompatibility.Calamity.Name)]
            void BumbleDash()
            {
                if (Phase2Check())
                    return;
                const int WindupTime = 30;

                if (Timer == 1)
                {
                    if (DLCUtils.HostCheck)
                    {
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<DLCMutantSpearDash>(), FargoSoulsUtil.ScaledProjectileDamage(npc.damage), 0f, Main.myPlayer, npc.whoAmI);
                    }
                }
                if (Timer < WindupTime)
                {
                    npc.velocity *= 0.9f;
                }
                if (Timer == WindupTime)
                {
                    npc.netUpdate = true;
                    float speed = 45f;
                    npc.velocity = speed * npc.DirectionTo(player.Center + player.velocity * 10);
                    if (DLCUtils.HostCheck)
                    {
                        if (WorldSavingSystem.MasochistModeReal)
                        {
                            Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Normalize(npc.velocity), ModContent.ProjectileType<MutantDeathray2>(), FargoSoulsUtil.ScaledProjectileDamage(npc.damage), 0f, Main.myPlayer);
                            Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, -Vector2.Normalize(npc.velocity), ModContent.ProjectileType<MutantDeathray2>(), FargoSoulsUtil.ScaledProjectileDamage(npc.damage), 0f, Main.myPlayer);
                        }
                    }
                }
                if (Timer > WindupTime + 30)
                {
                    Reset();
                    npc.ai[0] = 5;
                    npc.ai[2] = 5;
                    return;
                }
                Timer++;
            }
            #endregion
            #region Phase 2 Attacks
            [JITWhenModsEnabled(ModCompatibility.Calamity.Name)]
            void PrepareAresNuke()
            {
                if (!AliveCheck(player))
                    return;
                /*
                Vector2 targetPos = player.Center;
                targetPos.X += 400 * (npc.Center.X < targetPos.X ? -1 : 1);
                targetPos.Y -= 400;
                */
                int nukeTime = (Counter > 0 ? 90 : 180);
                if (Timer == 0)
                {
                    MutantBoss mutantBoss = (npc.ModNPC as MutantBoss);
                    Vector2 pos = FargoSoulsUtil.ProjectileExists(mutantBoss.ritualProj, ModContent.ProjectileType<MutantRitual>()) == null ? npc.Center : Main.projectile[mutantBoss.ritualProj].Center;
                    
                    if (Counter <= 0)
                    {
                        SoundEngine.PlaySound(SoundID.Roar, npc.Center);
                        Vector2 targetPos = player.Center;
                        targetPos.X += 400 * (npc.Center.X < targetPos.X ? -1 : 1);
                        targetPos.Y -= 400;
                        LockVector1 = targetPos;
                    }
                    else
                    {
                        Vector2 dir = Utils.SafeNormalize(npc.Center - pos, Vector2.UnitY.RotatedByRandom(MathHelper.TwoPi));
                        LockVector1 = pos + (-dir).RotatedByRandom(MathHelper.TwoPi / 4) * 500;
                    }
                        

                    if (DLCUtils.HostCheck)
                    {
                        float gravity = 0.2f;
                        float time = nukeTime;
                        Vector2 distance = player.Center - npc.Center;
                        distance.X /= time;
                        distance.Y = distance.Y / time - 0.5f * gravity * time;
                        int ritual = ModContent.ProjectileType<DLCMutantFishronRitual>();
                        if (Main.LocalPlayer.ownedProjectileCounts[ritual] <= 0)
                            Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ritual, FargoSoulsUtil.ScaledProjectileDamage(npc.damage, 4f / 3f), 0f, Main.myPlayer, npc.whoAmI);
                        int nuke = Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, distance, ModContent.ProjectileType<MutantAresNuke>(), WorldSavingSystem.MasochistModeReal ? FargoSoulsUtil.ScaledProjectileDamage(npc.damage, 4f / 3f) : 0, 0f, Main.myPlayer, gravity);
                        if (nuke.WithinBounds(Main.maxNPCs))
                        {
                            Main.npc[nuke].timeLeft = nukeTime;
                        }
                        
                    }
                }
                for (int i = 0; i < 20; i++)
                {
                    Vector2 offset = new();
                    offset.Y -= 100;
                    double angle = Main.rand.NextDouble() * 2d * Math.PI;
                    offset.X += (float)(Math.Sin(angle) * 150);
                    offset.Y += (float)(Math.Cos(angle) * 150);
                    Dust dust = Main.dust[Dust.NewDust(npc.Center + offset - new Vector2(4, 4), 0, 0, DustID.Vortex, 0, 0, 100, Color.White, 1.5f)];
                    dust.velocity = npc.velocity;
                    if (Main.rand.NextBool(3))
                        dust.velocity += Vector2.Normalize(offset) * 5f;
                    dust.noGravity = true;
                }

                if (npc.Distance(LockVector1) > 80)
                {
                    Movement(LockVector1, 1.2f, fastX: false);
                }
                else
                {
                    npc.velocity *= 0.9f;
                }

                if (++Timer > nukeTime)
                {
                    foreach (Projectile projectile in Main.projectile.Where(p => p != null && p.active && p.type == ModContent.ProjectileType<MutantAresNuke>()))
                    {
                        projectile.Kill();
                    }
                    DLCAttackChoice = DLCAttack.AresNuke;
                    Timer = 0;

                    if (Math.Sign(player.Center.X - npc.Center.X) == Math.Sign(npc.velocity.X))
                        npc.velocity.X *= -1f;
                    if (npc.velocity.Y < 0)
                        npc.velocity.Y *= -1f;
                    npc.velocity.Normalize();
                    npc.velocity *= 3f;
                    
                    npc.netUpdate = true;
                    //NPC.TargetClosest();
                }
            }
            [JITWhenModsEnabled(ModCompatibility.Calamity.Name)]
            void AresNuke()
            {
                if (!AliveCheck(player))
                    return;

                if (WorldSavingSystem.MasochistModeReal)
                {
                    Vector2 target = npc.Bottom.Y < player.Top.Y
                        ? player.Center + 300f * Vector2.UnitX * Math.Sign(npc.Center.X - player.Center.X)
                        : npc.Center + 30 * npc.DirectionFrom(player.Center).RotatedBy(MathHelper.ToRadians(60) * Math.Sign(player.Center.X - npc.Center.X));
                    Movement(target, 0.1f);
                    if (npc.velocity.Length() > 2f)
                        npc.velocity = Vector2.Normalize(npc.velocity) * 2f;
                }
                if (!Main.dedServ && Main.LocalPlayer.active)
                    Main.LocalPlayer.FargoSouls().Screenshake = 2;

                if (DLCUtils.HostCheck)
                {
                    Vector2 safeZone = npc.Center;
                    safeZone.Y -= 100;
                    const float safeRange = 150 + 200;
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 spawnPos = npc.Center + Main.rand.NextVector2Circular(1200, 1200);
                        if (Vector2.Distance(safeZone, spawnPos) < safeRange)
                        {
                            Vector2 directionOut = spawnPos - safeZone;
                            directionOut.Normalize();
                            spawnPos = safeZone + directionOut * Main.rand.NextFloat(safeRange, 1200);
                        }
                        Projectile.NewProjectile(npc.GetSource_FromThis(), spawnPos, Vector2.Zero, ModContent.ProjectileType<MutantAresBomb>(), FargoSoulsUtil.ScaledProjectileDamage(npc.damage, 4f / 3f), 0f, Main.myPlayer);
                    }
                }

                if (++Timer > (Counter > 2 ? 80 : 30))
                {
                    if (++Counter > 3)
                    {
                        ChooseNextAttack(11, 13, 16, 19, 24, WorldSavingSystem.MasochistModeReal ? 26 : 29, 31, 35, 37, 39, 41, 42);
                        Counter = 0;
                        DLCAttackChoice = DLCAttack.None;
                    }
                    else
                    {
                        DLCAttackChoice = DLCAttack.PrepareAresNuke;
                    }
                    Timer = 0;
                    
                }
            }
            [JITWhenModsEnabled(ModCompatibility.Calamity.Name)]
            void SlimeGodSlam()
            {
                if (!AliveCheck(player))
                    return;
                ref float side = ref npc.ai[2];
                const int Windup = 40;
                if (Counter == 0 && Timer == 0)
                {
                    SoundEngine.PlaySound(SlimeGodCoreEternity.ExitSound, npc.Center);
                    side = Math.Sign(npc.Center.X - player.Center.X);
                }
                float distance = 500f;
                Vector2 desiredPos = player.Center + Vector2.UnitX * side * distance - Vector2.UnitY * 100;
                Movement(desiredPos, 1.2f);
                if (Timer == 30 + Windup)
                {
                    SoundEngine.PlaySound(SlimeGodCoreEternity.BigShotSound, npc.Center);
                    if (DLCUtils.HostCheck)
                    {
                        float random = (Main.rand.NextFloat() - 0.5f) / 3;
                        for (int i = -8; i < 2; i++)
                        {
                            float iX = i + 0.5f;
                            float xModifier = 6f;
                            float speedX = (iX - random) * xModifier * side;
                            float speedY = -20;

                            int crimson = i % 2 == 0 ? 1 : -1; //every other slime is crimulean, every other is ebonian
                            crimson = (int)(crimson * side);

                            Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, speedX * Vector2.UnitX + speedY * Vector2.UnitY, ModContent.ProjectileType<MutantSlimeGod>(), FargoSoulsUtil.ScaledProjectileDamage(npc.damage, 1f), 1f, Main.myPlayer, crimson);
                        }

                        float speed = 8;
                        Vector2 aureusVel = Vector2.Normalize(Vector2.UnitX * -Math.Sign(player.Center.X - npc.Center.X) + Vector2.UnitY) * speed;
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, aureusVel, ModContent.ProjectileType<MutantAureusSpawn>(), FargoSoulsUtil.ScaledProjectileDamage(npc.damage, 1f), 1f, Main.myPlayer, player.whoAmI);
                    }
                    side = -side; //switch side
                }
                if (++Timer >= MutantSlimeGod.SlamTime + Windup)
                {
                    Timer = Windup;
                    if (++Counter >= 3)
                    {
                        ChooseNextAttack(11, 16, 19, 20, WorldSavingSystem.MasochistModeReal ? 26 : 29, 31, 33, 37, 39, 41, 42, 45);
                        Reset();
                        return;
                    }
                }
            }
            [JITWhenModsEnabled(ModCompatibility.Calamity.Name)]
            void Calamitas()
            {
                const int Startup = 20;
                const int Distance = 450;
                int brimstoneMonster = -1;
                if (ModContent.TryFind(ModCompatibility.Calamity.Name, "BrimstoneMonster", out ModProjectile monster))
                {
                    brimstoneMonster = monster.Type;
                }
                
                if (Timer < Startup)
                {
                    Vector2 targetPos = player.Center + Vector2.UnitX * Math.Sign(npc.Center.X - player.Center.X) * Distance;
                    Movement(targetPos, 1.2f);
                }
                if (Timer == Startup)
                {
                    if (DLCUtils.HostCheck)
                    {
                        MutantBoss mutantBoss = (npc.ModNPC as MutantBoss);
                        Vector2 pos = FargoSoulsUtil.ProjectileExists(mutantBoss.ritualProj, ModContent.ProjectileType<MutantRitual>()) == null ? npc.Center : Main.projectile[mutantBoss.ritualProj].Center;
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2 offset = Vector2.UnitX.RotatedBy(i * MathHelper.TwoPi / 4);
                            Vector2 targetPos = pos + (offset * 1300f);
                            Projectile.NewProjectile(npc.GetSource_FromAI(), targetPos, targetPos.DirectionTo(player.Center), brimstoneMonster, FargoSoulsUtil.ScaledProjectileDamage(npc.damage, 4f / 3f), 0f, Main.myPlayer, 0f, 2f, 0f);
                        }
                        
                    }
                }
                if (Timer > Startup)
                {
                    Vector2 dir = Utils.SafeNormalize(npc.Center - player.Center, -Vector2.UnitY);
                    Vector2 targetPos = player.Center + dir * Distance;
                    Movement(targetPos, 1.2f);

                    const int CalamitasTime = 80;
                    const int TelegraphTime = 40;
                    const int Attacks = 3;
                    if ((Timer - Startup) % CalamitasTime == CalamitasTime - 1)
                    {
                        
                        int calamiti = 8;

                        int[] rotations = Enumerable.Range(1, calamiti).ToArray();
                        rotations = rotations.OrderBy(a => Main.rand.Next()).ToArray(); //randomize list
                        Vector2 random = Main.rand.NextVector2Unit();
                        for (int i = 0; i < calamiti; i++)
                        {
                            int spawnDistance = Main.rand.Next(1200, 1400);
                            int aimDistance = Main.rand.Next(80, 400);

                            float spawnRot = MathHelper.TwoPi * ((float)i / calamiti);
                            Vector2 spawnPos = player.Center + random.RotatedBy(spawnRot) * spawnDistance;
                            float aimRot = (float)rotations[i] / calamiti;
                            Vector2 predict = player.velocity * TelegraphTime / 2;
                            Vector2 aimPos = player.Center + predict + aimRot.ToRotationVector2() * aimDistance;
                            Vector2 aim = spawnPos.DirectionTo(aimPos);
                            Projectile.NewProjectile(npc.GetSource_FromAI(), spawnPos, aim, ModContent.ProjectileType<DLCBloomLine>(), 0, 0, Main.myPlayer, 1, npc.whoAmI, TelegraphTime + 10);
                            Projectile.NewProjectile(npc.GetSource_FromAI(), spawnPos, aim, ModContent.ProjectileType<MutantSCal>(), FargoSoulsUtil.ScaledProjectileDamage(npc.damage, 4f / 3f), 0f, Main.myPlayer, TelegraphTime);
                        }
                    }
                    if (Timer > Startup + (CalamitasTime * Attacks) + 40)
                    {
                        
                        foreach (Projectile projectile in Main.projectile.Where(p => p != null && p.active && p.type == brimstoneMonster))
                        {
                            SoundEngine.PlaySound(SoundID.Item14, projectile.Center);
                            for (int i = 0; i < 30; i++)
                            {
                                Vector2 pos = projectile.Center + Main.rand.NextVector2Circular(projectile.width / 2, projectile.height / 2);
                                Dust.NewDust(pos, 1, 1, DustID.LifeDrain, 0f, 0f, 100, default(Color), 2f);
                            }
                            projectile.Kill();
                        }
                        
                        Reset();
                        ChooseNextAttack(11, 13, 16, 21, 26, 29, 31, 33, 35, 37, 41, 44, 45);
                        return;
                    }
                }
                Timer++;
            }
            [JITWhenModsEnabled(ModCompatibility.Calamity.Name)]
            void BumbleDrift2()
            {
                if (!AliveCheck(player))
                    return;
                const int StartupTimeOtherwiseItKindaTelefragsYouSometimes = 45;
                const int WindupTime = 180;
                if (Timer == StartupTimeOtherwiseItKindaTelefragsYouSometimes)
                {
                    if (DLCUtils.HostCheck)
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<DLCMutantSpearSpin>(), FargoSoulsUtil.ScaledProjectileDamage(npc.damage), 0f, Main.myPlayer, npc.whoAmI, WindupTime - StartupTimeOtherwiseItKindaTelefragsYouSometimes);
                }

                Vector2 targetPos = player.Center;
                targetPos.Y += 450f * Math.Sign(npc.Center.Y - player.Center.Y); //can be above or below
                Movement(targetPos, 0.7f, fastX: false);
                if (npc.Distance(player.Center) < 200)
                    Movement(npc.Center + npc.DirectionFrom(player.Center), 1.4f);

                if (Timer > WindupTime)
                {
                    Timer = 0;
                    DLCAttackChoice = DLCAttack.BumbleDash2;
                    return;
                }
                Timer++;
            }
            [JITWhenModsEnabled(ModCompatibility.Calamity.Name)]
            void BumbleDash2()
            {
                if (!AliveCheck(player))
                    return;
                const int WindupTime = 30;

                if (Timer < WindupTime)
                {
                    npc.velocity *= 0.9f;
                }
                if (Timer == WindupTime)
                {
                    
                    foreach (Projectile projectile in Main.projectile.Where(p => p != null && p.active && p.type == ModContent.ProjectileType<DLCMutantSpearDash>()))
                    {
                        projectile.Kill();
                    }

                    npc.netUpdate = true;
                    float speed = 45f;
                    npc.velocity = speed * npc.DirectionTo(player.Center).RotatedBy(MathHelper.PiOver2 * 0.7f);
                    if (DLCUtils.HostCheck)
                    {
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<DLCMutantSpearDash>(), FargoSoulsUtil.ScaledProjectileDamage(npc.damage), 0f, Main.myPlayer, npc.whoAmI);
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Normalize(npc.velocity), ModContent.ProjectileType<MutantDeathray2>(), FargoSoulsUtil.ScaledProjectileDamage(npc.damage), 0f, Main.myPlayer);
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, -Vector2.Normalize(npc.velocity), ModContent.ProjectileType<MutantDeathray2>(), FargoSoulsUtil.ScaledProjectileDamage(npc.damage), 0f, Main.myPlayer);
                    }
                }
                if (Timer > WindupTime && Timer % 6 == 0 && DLCUtils.HostCheck)
                {
                    Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, -Vector2.Normalize(npc.velocity).RotatedByRandom(MathHelper.PiOver4), ModContent.ProjectileType<RedLightningFeather>(), FargoSoulsUtil.ScaledProjectileDamage(npc.damage), 0f, Main.myPlayer, 250);
                }
                if (Timer >= WindupTime + 15)
                {
                    if (Counter > 7)
                    {
                        if (WorldSavingSystem.MasochistModeReal)
                            ChooseNextAttack(11, 13, 16, 19, 20, 31, 33, 35, 39, 42, 44);
                        else
                            ChooseNextAttack(11, 16, 26, 29, 31, 35, 37, 39, 42, 44);
                        Reset();
                        return;
                    }
                    Timer = WindupTime - 5;
                    Counter++;
                }
                Timer++;
            }
            [JITWhenModsEnabled(ModCompatibility.Calamity.Name)]
            void Providence()
            {
                if (!AliveCheck(player))
                    return;

                const int PrepareTime = 55;
                const int DashTime = 60;
                const int LaserPrepareTime = 30;
                const int LaserTime = 80;

                if (Timer < PrepareTime)
                {
                    MutantBoss mutantBoss = (npc.ModNPC as MutantBoss);
                    Projectile arena = FargoSoulsUtil.ProjectileExists(mutantBoss.ritualProj, ModContent.ProjectileType<MutantRitual>());
                    if (arena != null)
                    {
                        arena.position -= arena.velocity;
                        arena.position += arena.DirectionTo(player.Center) * 4;
                        arena.netUpdate = true;
                    }
                }
                if (Timer == 0)
                {
                    SoundEngine.PlaySound(SoundID.ForceRoar, npc.Center);
                    Particle p = new ExpandingBloomParticle(npc.Center, Vector2.Zero, Color.Goldenrod, Vector2.One * 40f, Vector2.Zero, PrepareTime, true, Color.White);
                    p.Spawn();
                }
                if (Timer < PrepareTime * 2f/3)
                {
                    int dirX = Math.Sign(npc.Center.X - player.Center.X);
                    int dirY = Math.Sign(npc.Center.Y - player.Center.Y);

                    if (dirX == 0)
                        dirX = 1;
                    if (dirY == 0)
                        dirY = 1;

                    int distanceX = 800;
                    int distanceY = 400;
                    Vector2 targetPos = player.Center + Vector2.UnitX * dirX * distanceX + Vector2.UnitY * dirY * distanceY;
                    Movement(targetPos, 1.5f, fastX: true);
                }
                else if (Timer < PrepareTime)
                {
                    npc.velocity *= 0.9f;
                }
                else if (Timer == PrepareTime)
                {
                    const int dashSpeed = 28;
                    npc.velocity.Y = 0;
                    npc.velocity.X = Math.Sign(player.Center.X - npc.Center.X) * dashSpeed;

                    SoundEngine.PlaySound(ProfanedGuardianCommander.DashSound, npc.Center);
                    npc.netUpdate = true;
                }
                else if (Timer - PrepareTime < DashTime)
                {
                    if (Timer % 3 == 0)
                    {
                        if (DLCUtils.HostCheck)
                        {
                            float spearSpeed = 18f;
                            Vector2 spearVel = Vector2.UnitY * Math.Sign(player.Center.Y - npc.Center.Y) * spearSpeed;
                            Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, spearVel, ModContent.ProjectileType<HolySpear>(), FargoSoulsUtil.ScaledProjectileDamage(npc.damage), 0f, Main.myPlayer, 1f, 0f, 0f);
                        }
                    }
                }
                else if (Timer - PrepareTime == DashTime)
                {
                    
                    int dirX = Math.Sign(npc.Center.X - player.Center.X);
                    int dirY = -Math.Sign(npc.Center.Y - player.Center.Y);

                    if (dirX == 0)
                        dirX = 1;
                    if (dirY == 0)
                        dirY = 1;

                    int distanceX = 400;
                    int distanceY = 500;
                    LockVector1 = Vector2.UnitX * dirX * distanceX + Vector2.UnitY * dirY * distanceY;
                    npc.netUpdate = true;
                }
                else if (Timer - PrepareTime - DashTime < LaserPrepareTime) //move to deathray position
                {
                    Movement(player.Center + LockVector1, 1.2f);
                }
                else if (Timer - PrepareTime - DashTime == LaserPrepareTime)
                {
                    //deathray
                    SoundEngine.PlaySound(CalamityMod.NPCs.Providence.Providence.HolyRaySound, npc.Center);
                    if (DLCUtils.HostCheck)
                    {
                        float rotation = 435f;
                        Vector2 velocity2 = player.Center - npc.Center;
                        velocity2.Normalize();
                        float beamDirection = -1f;
                        if (velocity2.X < 0f)
                        {
                            beamDirection = 1f;
                        }
                        velocity2 = Utils.RotatedBy(velocity2, (0.0 - (double)beamDirection) * 6.2831854820251465 / 6.0, default(Vector2));
                        Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center.X, npc.Center.Y, velocity2.X, velocity2.Y, ModContent.ProjectileType<MutantHolyRay>(), FargoSoulsUtil.ScaledProjectileDamage(npc.damage, 3f / 2), 0f, Main.myPlayer, beamDirection * ((float)Math.PI * 2f) / rotation, npc.whoAmI, 0f);
                        Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center.X, npc.Center.Y, 0f - velocity2.X, 0f - velocity2.Y, ModContent.ProjectileType<MutantHolyRay>(), FargoSoulsUtil.ScaledProjectileDamage(npc.damage, 3f / 2), 0f, Main.myPlayer, (0f - beamDirection) * ((float)Math.PI * 2f) / rotation, npc.whoAmI, 0f);
                    }
                    npc.netUpdate = true;
                }
                else
                {
                    npc.velocity *= 0.96f;
                    if (Timer - PrepareTime - DashTime - LaserPrepareTime == LaserTime)
                    {
                        Reset();
                        ChooseNextAttack(11, 16, 19, 20, WorldSavingSystem.MasochistModeReal ? 44 : 26, 31, 33, 35, 42, 44, 45);
                        return;
                    }
                }
                Timer++;
            }
            #endregion
            #endregion

        }

        public override void PostAI(NPC npc)
        {
            ManageMusicAndSky(npc);
            if (DLCAttackChoice < DLCAttack.None) //p1, negate "while dashing" code that makes him face his velocity
            {
                npc.direction = npc.spriteDirection = npc.Center.X < Main.player[npc.target].Center.X ? 1 : -1;
            }

            if (DLCAttackChoice == DLCAttack.BumbleDash2 && Timer > 30)
            {
                npc.direction = npc.spriteDirection = Math.Sign(npc.velocity.X);
            }
        }

    }
}
