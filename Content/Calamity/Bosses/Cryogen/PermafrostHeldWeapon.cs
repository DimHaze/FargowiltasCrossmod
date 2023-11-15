﻿using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items.Weapons.Ranged;
using FargowiltasCrossmod.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasCrossmod.Content.Calamity.Bosses.Cryogen
{
    [ExtendsFromMod(ModCompatibility.Calamity.Name)]
    public class PermafrostHeldWeapon : ModProjectile
    {
        public override string Texture => "CalamityMod/Items/Weapons/Magic/IcicleTrident";
        public override void SetStaticDefaults()
        {

        }
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 1;
            Projectile.timeLeft = 60;
            Projectile.hostile = false;
            Projectile.friendly = false;
            
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return base.Colliding(projHitbox, targetHitbox);
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            base.OnHitPlayer(target, info);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> t = TextureAssets.Projectile[Type];
            Vector2 origin = new Vector2(0, t.Height());
            float maxscale = 1;
            SpriteEffects se = SpriteEffects.None;
            if (Projectile.ai[0] == 1)
            {
                t = TextureAssets.Item[ModContent.ItemType<FrostbiteBlaster>()];
                origin = new Vector2(10, t.Height() / 2);
                float degrees = MathHelper.ToDegrees(Projectile.rotation + MathHelper.Pi);
                if (degrees > 270 || degrees < 90) se = SpriteEffects.FlipVertically;
            }
            if (Projectile.ai[0] == 2)
            {
                t = TextureAssets.Projectile[ModContent.ProjectileType<ArcticPaw>()];
                origin = t.Size() / 2;
                maxscale = 2;
            }
            if (Projectile.ai[0] == 3)
            {
                t = TextureAssets.Item[ModContent.ItemType<WintersFury>()];
                origin = new Vector2(2, 28);
                float degrees = MathHelper.ToDegrees(Projectile.rotation + MathHelper.Pi);
                if (degrees > 270 || degrees < 90) se = SpriteEffects.FlipVertically;
            }
            if (Projectile.ai[0] == 4)
            {
                t = TextureAssets.Item[ModContent.ItemType<AbsoluteZero>()];
                origin = new Vector2(0, t.Height());
                maxscale = 1.5f;
            }
            if (Projectile.ai[0] == 5)
            {
                t = TextureAssets.Item[ModContent.ItemType<EternalBlizzard>()];
                origin = new Vector2(10, t.Height() / 2);
                float degrees = MathHelper.ToDegrees(Projectile.rotation + MathHelper.Pi);
                if (degrees > 270 || degrees < 90) se = SpriteEffects.FlipVertically;
            }
            if (Projectile.localAI[0] < maxscale)
            {
                Projectile.localAI[0] += 0.03f;
            }
            float scale = MathHelper.Lerp(0, Projectile.scale, Projectile.localAI[0]);
            Main.EntitySpriteDraw(t.Value, Projectile.Center - Main.screenPosition, null, lightColor * Projectile.Opacity, Projectile.rotation, origin, scale, se);
            return false;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
        }
        public override void AI()
        {
            NPC owner = Main.npc[(int)Projectile.ai[2]];

            if (owner == null || !owner.active || owner.type != ModContent.NPCType<PermafrostBoss>())
            {
                Projectile.Kill();
                return;
            }
            float angle = Projectile.ai[1];
            float scale = 1;
            //Ice Trident
            if (Projectile.ai[0] == 0)
            {
                angle += MathHelper.PiOver4;
                if (owner.ai[1] < 60)
                    Projectile.rotation = owner.Center.AngleTo(Main.player[owner.target].Center) + MathHelper.PiOver4;
                if (Projectile.localAI[0] < scale)
                {
                    Vector2 thing = new Vector2(0, -50 * Projectile.localAI[0]).RotatedBy(Projectile.rotation);
                    if (Projectile.ai[0] == 0) thing = thing.RotatedBy(MathHelper.PiOver4);
                    for (int i = 0; i < 5; i++)
                        Dust.NewDustDirect(Projectile.Center + thing * Main.rand.NextFloat(0f, 1f), 0, 0, DustID.SnowflakeIce).noGravity = true;
                }
                if (Projectile.timeLeft == 1)
                {
                    Vector2 thing = new Vector2(0, -50 * Projectile.localAI[0]).RotatedBy(Projectile.rotation);
                    if (Projectile.ai[0] == 0) thing = thing.RotatedBy(MathHelper.PiOver4);
                    for (int i = 0; i < 50; i++)
                    {
                        Dust.NewDustDirect(Projectile.Center + thing * Main.rand.NextFloat(0f, 1f), 0, 0, DustID.SnowflakeIce).noGravity = true;
                    }
                }
            }
            //Frostbite Blaster
            if (Projectile.ai[0] == 1)
            {
                if (Projectile.localAI[0] < scale)
                {
                    Vector2 off = new Vector2(Main.rand.Next(0, 56) * Projectile.localAI[0], 0).RotatedBy(Projectile.rotation);
                    for (int i = 0; i < 5; i++)
                        Dust.NewDustDirect(Projectile.Center + off, Projectile.width, Projectile.height, DustID.SnowflakeIce).noGravity = true;
                }
                if (owner.HasValidTarget)
                {
                    Projectile.rotation = owner.AngleTo(Main.player[owner.target].Center);
                }
                if (Projectile.timeLeft == 1)
                {

                    for (int i = 0; i < 50; i++)
                    {
                        Vector2 off = new Vector2(Main.rand.Next(0, 56) * Projectile.localAI[0], 0).RotatedBy(Projectile.rotation);
                        Dust.NewDustDirect(Projectile.Center + off, Projectile.width, Projectile.height, DustID.SnowflakeIce).noGravity = true;
                    }
                }
            }
            //Arctic Bear Paw
            if (Projectile.ai[0] == 2)
            {
                Projectile.Opacity = 0.7f;
                Projectile.rotation = owner.velocity.ToRotation() + MathHelper.PiOver2;
                scale = 2;
                if (Projectile.localAI[0] < scale)
                {
                    Projectile.Resize((int)(Projectile.localAI[0] * 30), (int)(Projectile.localAI[0] * 30));
                    for (int i = 0; i < 5; i++)
                        Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.SnowflakeIce).noGravity = true;
                }
                if (Projectile.timeLeft == 1)
                {
                    
                    for (int i = 0; i < 50; i++)
                    {
                        Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.SnowflakeIce).noGravity = true;
                    }
                }
            }
            //Winter's fury
            if (Projectile.ai[0] == 3)
            {
                if (owner.HasValidTarget)
                {
                    Projectile.rotation = owner.AngleTo(Main.player[owner.target].Center);
                }
                if (Projectile.localAI[0] < scale)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        Dust.NewDustDirect(Projectile.position + new Vector2(Main.rand.Next(-26, 0) * Projectile.localAI[0], Main.rand.Next(0, 30) * Projectile.localAI[0]).RotatedBy(Projectile.rotation), 0, 0, DustID.SnowflakeIce).noGravity = true;
                    }
                }
                if (Projectile.timeLeft == 1)
                {
                    for (int i = 0; i < 50; i++)
                    {
                        Dust.NewDustDirect(Projectile.position + new Vector2(Main.rand.Next(-26, 0) * Projectile.localAI[0], Main.rand.Next(0, 30) * Projectile.localAI[0]).RotatedBy(Projectile.rotation), 0, 0, DustID.SnowflakeIce).noGravity = true;
                    }
                }
            }
            //Absolute zero
            if (Projectile.ai[0] == 4)
            {
                scale = 1.5f;
                if (Projectile.localAI[0] < scale)
                {
                    Vector2 off = new Vector2(Main.rand.Next(0, 40) * Projectile.localAI[0], Main.rand.Next(-40, 0) * Projectile.localAI[0]).RotatedBy(Projectile.rotation);
                    for (int i = 0; i < 5; i++)
                        Dust.NewDustDirect(Projectile.Center + off, Projectile.width, Projectile.height, DustID.SnowflakeIce).noGravity = true;
                }
                if (owner.HasValidTarget)
                {
                    float x = 1- ((Projectile.timeLeft - 20) / 10f);
                    if (Projectile.timeLeft < 20) x = 1;
                    if (Projectile.timeLeft > 30) x = 0;
                    float angleAdd = MathHelper.Lerp(0, MathHelper.PiOver2, x);
                    Projectile.rotation = owner.AngleTo(Main.player[owner.target].Center) + angleAdd;
                }
                if (Projectile.timeLeft == 1)
                {

                    for (int i = 0; i < 50; i++)
                    {
                        Vector2 off = new Vector2(Main.rand.Next(0, 40) * Projectile.localAI[0], Main.rand.Next(-40, 0) * Projectile.localAI[0]).RotatedBy(Projectile.rotation);
                        Dust.NewDustDirect(Projectile.Center + off, Projectile.width, Projectile.height, DustID.SnowflakeIce).noGravity = true;
                    }
                }
            }
            //Eternal Blizzard
            if (Projectile.ai[0] == 5)
            {
                if (Projectile.localAI[0] < scale)
                {
                    Vector2 off = new Vector2(Main.rand.Next(0, 56) * Projectile.localAI[0], 0).RotatedBy(Projectile.rotation);
                    for (int i = 0; i < 5; i++)
                        Dust.NewDustDirect(Projectile.Center + off, Projectile.width, Projectile.height, DustID.SnowflakeIce).noGravity = true;
                }
                if (owner.HasValidTarget)
                {
                    Projectile.rotation = owner.AngleTo(Main.player[owner.target].Center);
                }
                if (Projectile.timeLeft == 1)
                {

                    for (int i = 0; i < 50; i++)
                    {
                        Vector2 off = new Vector2(Main.rand.Next(0, 56) * Projectile.localAI[0], 0).RotatedBy(Projectile.rotation);
                        Dust.NewDustDirect(Projectile.Center + off, Projectile.width, Projectile.height, DustID.SnowflakeIce).noGravity = true;
                    }
                }
            }
            if (Projectile.localAI[0] == 0.06f)
            {
                if (Projectile.ai[0] == 1)
                    Projectile.timeLeft = 60;
                if (Projectile.ai[0] == 2)
                {
                    Projectile.timeLeft =160;
                }
                if (Projectile.ai[0] == 3)
                {
                    Projectile.timeLeft = 180;
                }
                if (Projectile.ai[0] == 4)
                {
                    Projectile.timeLeft = 120;
                }
                if (Projectile.ai[0] == 5)
                {
                    Projectile.timeLeft = 150;
                }
            }
            Projectile.Center = owner.Center;
            
            
            base.AI();
        }
    }
}
