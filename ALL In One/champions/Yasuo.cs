﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class Yasuo//RL144
    {
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}} //
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, QQ, W, E, EQ, R;

		

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 475f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W, 400f);
            E = new Spell(SpellSlot.E, 475f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 1200f, TargetSelector.DamageType.Physical);
            QQ = new Spell(SpellSlot.Q, 900f, TargetSelector.DamageType.Physical);
            EQ = new Spell(SpellSlot.Q, 375f, TargetSelector.DamageType.Physical);

            Q.SetSkillshot(0.25f, 50f, float.MaxValue, false, SkillshotType.SkillshotLine);
            QQ.SetSkillshot(0.25f, 60f, 1200f, false, SkillshotType.SkillshotLine);
			W.SetSkillshot(0.25f, 300f, float.MaxValue, false, SkillshotType.SkillshotCircle);
			E.SetTargetted(0.25f, float.MaxValue);
			EQ.SetTargetted(0.25f, float.MaxValue);
			R.SetTargetted(0.4f, float.MaxValue);
			
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE(false);
			AIO_Menu.Champion.Combo.addUseR();

			AIO_Menu.Champion.Harass.addUseQ();
			AIO_Menu.Champion.Harass.addUseW();
			AIO_Menu.Champion.Harass.addUseE(false);

            AIO_Menu.Champion.Lasthit.addUseQ();
            AIO_Menu.Champion.Lasthit.addUseE();

            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseE();


            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseE();


			AIO_Menu.Champion.Misc.addHitchanceSelector();
            Menu.SubMenu("Misc").AddItem(new MenuItem("Misc.Qtg", "Additional Range")).SetValue(new Slider(50, 0, 150));
            AIO_Menu.Champion.Misc.addItem("KillstealQ", true);
            AIO_Menu.Champion.Misc.addItem("KillstealE", true);
            AIO_Menu.Champion.Drawings.addQRange();
            AIO_Menu.Champion.Drawings.addItem("QQ Safe Range", new Circle(true, Color.Red));
            AIO_Menu.Champion.Drawings.addItem("EQ Range", new Circle(true, Color.Red));
            AIO_Menu.Champion.Drawings.addWRange();
            AIO_Menu.Champion.Drawings.addERange();
			AIO_Menu.Champion.Drawings.addRRange();
			
			AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
			
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalking.CanMove(35))
            {
                switch (Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        Orbwalker.SetAttack(true);
                        Combo();
                        break;
                    case Orbwalking.OrbwalkingMode.Mixed:
                        Orbwalker.SetAttack(true);
                        Harass();
                        break;
                    case Orbwalking.OrbwalkingMode.LastHit:
                        Orbwalker.SetAttack(!AIO_Menu.Champion.Lasthit.UseQ || !Q.IsReady());
                        Lasthit();
                        break;
                    case Orbwalking.OrbwalkingMode.LaneClear:
                        Orbwalker.SetAttack(true);
                        Laneclear();
                        Jungleclear();
                        break;
                    case Orbwalking.OrbwalkingMode.None:
                        Orbwalker.SetAttack(true);
                        break;
                }
            }

            #region Killsteal
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealQ"))
                KillstealQ();
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealE"))
                KillstealE();
            #endregion
			
			#region AfterAttack
			AIO_Func.AASkill(Q);
			if(AIO_Func.AfterAttack())
			AA();
			#endregion
			
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

		var drawQ = AIO_Menu.Champion.Drawings.QRange;
		var drawW = AIO_Menu.Champion.Drawings.WRange;
		var drawE = AIO_Menu.Champion.Drawings.ERange;
		var drawR = AIO_Menu.Champion.Drawings.RRange;
		var drawQQr = AIO_Menu.Champion.Drawings.getCircleValue("QQ Safe Range");
		var drawEQ = AIO_Menu.Champion.Drawings.getCircleValue("EQ Range");
		var QQTarget = TargetSelector.GetTarget(QQ.Range + Player.MoveSpeed * QQ.Delay, TargetSelector.DamageType.Magical);

	
		if (Q.IsReady() && drawQ.Active)
		Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);
		
		if (EQ.IsReady() && drawEQ.Active && Player.HasBuff("yasuoq3w") && Dash.IsDashing(Player))
		Render.Circle.DrawCircle(Player.Position, EQ.Range, drawEQ.Color);
		
		if (W.IsReady() && drawW.Active)
		Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);
	
		if (E.IsReady() && drawE.Active)
		Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);
		
		if (R.IsReady() && drawR.Active)
		Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
		
		if (QQ.IsReady() && drawQQr.Active && !Dash.IsDashing(Player) &&QQTarget != null)
		Render.Circle.DrawCircle(Player.Position, QQ.Range - QQTarget.MoveSpeed*QQ.Delay, drawQQr.Color);


        }
		
        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!AIO_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;


        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!AIO_Menu.Champion.Misc.UseInterrupter || Player.IsDead)
                return;

            if (QQ.IsReady() && Player.HasBuff("yasuoq3w") && !Dash.IsDashing(Player)
			&& Player.Distance(sender.Position) <= QQ.Range)
                QQ.Cast(sender.Position);

        }

        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe || Player.IsDead) // 야스오 바람장막 어떻게할까 고민중.
                return;
				

        }
		
		static void AA() // 챔피언 대상 평캔 ( 빼낸 이유는 AA방식 두개로 할시 두번 적어야 해서 단순화하기 위함.
		{
			var target = TargetSelector.GetTarget(Player.AttackRange + 50,TargetSelector.DamageType.Physical, true); //
			if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
			{
				if (AIO_Menu.Champion.Harass.UseW && Q.IsReady() && utility.Activator.AfterAttack.ALLCancelItemsAreCasted
					&& HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x)))
                AIO_Func.LCast(Q,target,Menu.Item("Misc.Qtg").GetValue<Slider>().Value,float.MaxValue);
			}
				
			if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
			{
				if (AIO_Menu.Champion.Combo.UseW && Q.IsReady() && utility.Activator.AfterAttack.ALLCancelItemsAreCasted
					&& HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x)))
                AIO_Func.LCast(Q,target,Menu.Item("Misc.Qtg").GetValue<Slider>().Value,float.MaxValue);
			}
		}
		
        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var Target = (Obj_AI_Base)target;
            if (!unit.IsMe || Target == null)
                return;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
			{
			AIO_Func.AALcJc(Q);
			}
			if(!utility.Activator.AfterAttack.AIO)
			AA();
        }
		
        static void Combo()
        {
			var buff = AIO_Func.getBuffInstance(Player, "yasuoq3w");
            if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady())
            {
				if(!Dash.IsDashing(Player) && !Player.HasBuff("yasuoq3w"))
				{
                var qTarget = TargetSelector.GetTarget(Q.Range, Q.DamageType, true);
				if(qTarget.Distance(Player.Position) >= Player.AttackRange + 50)
                AIO_Func.LCast(Q,qTarget,Menu.Item("Misc.Qtg").GetValue<Slider>().Value,float.MaxValue);
				}
				else if(Dash.IsDashing(Player))
				{
				var qTarget = TargetSelector.GetTarget(EQ.Range - 10, Q.DamageType, true);
				EQ.Cast();
				}
				else
				{
                var qTarget = TargetSelector.GetTarget(QQ.Range, Q.DamageType, true);
                AIO_Func.LCast(QQ,qTarget,Menu.Item("Misc.Qtg").GetValue<Slider>().Value,float.MaxValue);
				}
			}
			
            if (AIO_Menu.Champion.Combo.UseE && E.IsReady())
            {
                var ETarget = TargetSelector.GetTarget(E.Range, E.DamageType, true);
				if(!ETarget.HasBuff("YasuoDashWrapper"))
                E.Cast(ETarget);
            }
			
			if (AIO_Menu.Champion.Combo.UseR && R.IsReady())
			{
                var RTarget = TargetSelector.GetTarget(R.Range, R.DamageType, true);
				if(R.CanCast(RTarget))
				R.Cast(RTarget);
			}
				
        }

        static void Harass()
        {

            if (AIO_Menu.Champion.Harass.UseQ && Q.IsReady())
            {
				if(!Dash.IsDashing(Player) && !Player.HasBuff("yasuoq3w"))
				{
                var qTarget = TargetSelector.GetTarget(Q.Range, Q.DamageType, true);
				if(qTarget.Distance(Player.Position) >= Player.AttackRange + 50)
                AIO_Func.LCast(Q,qTarget,Menu.Item("Misc.Qtg").GetValue<Slider>().Value,float.MaxValue);
				}
				else if(Dash.IsDashing(Player))
				{
				var qTarget = TargetSelector.GetTarget(EQ.Range - 10, Q.DamageType, true);
				EQ.Cast();
				}
				else
				{
                var qTarget = TargetSelector.GetTarget(QQ.Range, Q.DamageType, true);
                AIO_Func.LCast(QQ,qTarget,Menu.Item("Misc.Qtg").GetValue<Slider>().Value,float.MaxValue);
				}
            }
				
            if (AIO_Menu.Champion.Harass.UseE && E.IsReady())
            {
                var ETarget = TargetSelector.GetTarget(E.Range, E.DamageType, true);
				if(!ETarget.HasBuff("YasuoDashWrapper"))
                E.Cast(ETarget);
            }

        }
		
        static void Lasthit()
        {

            var Minions = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;
				
				if(AIO_Menu.Champion.Lasthit.UseQ && Q.IsReady())
				AIO_Func.LH(Q,float.MaxValue);
				if(AIO_Menu.Champion.Lasthit.UseE && E.IsReady())
				AIO_Func.LH(E,0f);

        }
		
        static void Laneclear()
        {

            var Minions = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;
				
				
            if (AIO_Menu.Champion.Laneclear.UseQ && Q.IsReady())
            {
                if (Q.CanCast(Minions[0]))
                AIO_Func.LCast(Q,Minions[0],Menu.Item("Misc.Qtg").GetValue<Slider>().Value,float.MaxValue);
			}
            if (AIO_Menu.Champion.Laneclear.UseE && E.IsReady())
            {
                if (E.CanCast(Minions[0]) && !Minions[0].HasBuff("YasuoDashWrapper"))
                E.Cast(Minions[0]);
			}


        }

        static void Jungleclear()
        {

            var Mobs = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;
            if (AIO_Menu.Champion.Jungleclear.UseQ && Q.IsReady())
            {
                if (Q.CanCast(Mobs[0]))
                AIO_Func.LCast(Q,Mobs[0],Menu.Item("Misc.Qtg").GetValue<Slider>().Value,float.MaxValue);
			}
            if (AIO_Menu.Champion.Jungleclear.UseE && E.IsReady())
            {
                if (E.CanCast(Mobs[0]) && !Mobs[0].HasBuff("YasuoDashWrapper"))
                E.Cast(Mobs[0]);
			}
        }

        static void KillstealQ()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
                Q.Cast(target);
            }
        }
        static void KillstealE()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
				if(E.CanCast(target) && AIO_Func.isKillable(target, E) && !target.HasBuff("YasuoDashWrapper"))
				E.Cast(target);
			}
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
			{
                damage += Q.GetDamage(enemy);
			}
			
            if (Items.CanUseItem((int)ItemId.Tiamat_Melee_Only))
			{
			damage += (float)Player.GetItemDamage(enemy, Damage.DamageItems.Tiamat);
			damage += (float)Player.GetAutoAttackDamage(enemy, true);
			}
			
            if (Items.CanUseItem((int)ItemId.Ravenous_Hydra_Melee_Only))
			{
			damage += (float)Player.GetItemDamage(enemy, Damage.DamageItems.Hydra);
			damage += (float)Player.GetAutoAttackDamage(enemy, true);
			}

            if (E.IsReady())
                damage += E.GetDamage(enemy);
			
            if (R.IsReady())
                damage += R.GetDamage(enemy);

            if(!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage(enemy, true);
				
            return damage;
        }
    }
}