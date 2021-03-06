﻿namespace xSaliceResurrected_Rework.Pluging
{
    using System;
    using System.Drawing;
    using System.Linq;
    using LeagueSharp;
    using LeagueSharp.Common;
    using Base;
    using Managers;
    using Utilities;

    internal class Vladimir : Champion
    {
        public Vladimir()
        {
            Game.PrintChat("Vladimir Need to Rewrite, please use others assembly!");

            SpellManager.Q = new Spell(SpellSlot.Q, 600);
            SpellManager.W = new Spell(SpellSlot.W);
            SpellManager.E = new Spell(SpellSlot.E, 575);
            SpellManager.R = new Spell(SpellSlot.R, 700);

            SpellManager.R.SetSkillshot(0.25f, 175, 700, false, SkillshotType.SkillshotCircle);

            var key = new Menu("Key", "Key");
            {
                key.AddItem(new MenuItem("Orbwalk", "Combo!", true).SetValue(new KeyBind(32, KeyBindType.Press)));
                key.AddItem(new MenuItem("Farm", "Harass!", true).SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("FarmT", "Harass (toggle)!", true).SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("LaneClear", "Farm!", true).SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("LastHitKey", "Last Hit!", true).SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("StackE", "StackE (toggle)!", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle)));
                //add to menu
                Menu.AddSubMenu(key);
            }

            var combo = new Menu("Combo", "Combo");
            {
                combo.AddItem(new MenuItem("UseQCombo", "Use Q", true).SetValue(true));
                combo.AddItem(new MenuItem("UseWCombo", "Use W", true).SetValue(true));
                combo.AddItem(new MenuItem("UseECombo", "Use E", true).SetValue(true));
                combo.AddItem(new MenuItem("UseRCombo", "Use R", true).SetValue(true));
                combo.AddItem(new MenuItem("R_Killable", "R If enemy Is killable with Combo", true).SetValue(true));
                Menu.AddSubMenu(combo);
            }

            var harass = new Menu("Harass", "Harass");
            {
                harass.AddItem(new MenuItem("UseQHarass", "Use Q", true).SetValue(true));
                harass.AddItem(new MenuItem("UseEHarass", "Use E", true).SetValue(true));
                Menu.AddSubMenu(harass);
            }

            var farm = new Menu("LaneClear", "LaneClear");
            {
                farm.AddItem(new MenuItem("UseQFarm", "Use Q", true).SetValue(true));
                farm.AddItem(new MenuItem("UseEFarm", "Use E", true).SetValue(true));
                Menu.AddSubMenu(farm);
            }

            var miscMenu = new Menu("Misc", "Misc");
            {
                miscMenu.AddSubMenu(AoeSpellManager.AddHitChanceMenuCombo(false, false, true, true));
                miscMenu.AddItem(new MenuItem("W_Gap_Closer", "Use W On Gap Closer", true).SetValue(true));
                miscMenu.AddItem(new MenuItem("smartKS", "Smart KS", true).SetValue(true));
                miscMenu.AddItem(new MenuItem("R_KS", "Use R to KS", true).SetValue(true));
                miscMenu.AddItem(new MenuItem("Q_Before_E", "Cast Q Before E", true).SetValue(true));
                Menu.AddSubMenu(miscMenu);
            }

            var drawMenu = new Menu("Drawing", "Drawing");
            {
                drawMenu.AddItem(new MenuItem("Draw_Disabled", "Disable All", true).SetValue(false));
                drawMenu.AddItem(new MenuItem("Draw_Q", "Draw Q", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_E", "Draw E", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_R", "Draw R", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_R_Killable", "Draw R Mark on Killable", true).SetValue(true));

                var drawComboDamageMenu = new MenuItem("Draw_ComboDamage", "Draw Combo Damage", true).SetValue(true);
                var drawFill = new MenuItem("Draw_Fill", "Draw Combo Damage Fill", true).SetValue(new Circle(true, Color.FromArgb(90, 255, 169, 4)));
                drawMenu.AddItem(drawComboDamageMenu);
                drawMenu.AddItem(drawFill);
                DamageIndicator.DamageToUnit = GetComboDamage;
                DamageIndicator.Enabled = drawComboDamageMenu.GetValue<bool>();
                DamageIndicator.Fill = drawFill.GetValue<Circle>().Active;
                DamageIndicator.FillColor = drawFill.GetValue<Circle>().Color;
                drawComboDamageMenu.ValueChanged +=
                    delegate (object sender, OnValueChangeEventArgs eventArgs)
                    {
                        DamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                    };
                drawFill.ValueChanged +=
                    delegate (object sender, OnValueChangeEventArgs eventArgs)
                    {
                        DamageIndicator.Fill = eventArgs.GetNewValue<Circle>().Active;
                        DamageIndicator.FillColor = eventArgs.GetNewValue<Circle>().Color;
                    };
                Menu.AddSubMenu(drawMenu);
            }

            var customMenu = new Menu("Custom Perma Show", "Custom Perma Show");
            {
                var myCust = new CustomPermaMenu();
                customMenu.AddItem(new MenuItem("custMenu", "Move Menu", true).SetValue(new KeyBind("L".ToCharArray()[0], KeyBindType.Press)));
                customMenu.AddItem(new MenuItem("enableCustMenu", "Enabled", true).SetValue(true));
                customMenu.AddItem(myCust.AddToMenu("Combo Active: ", "Orbwalk"));
                customMenu.AddItem(myCust.AddToMenu("Harass Active: ", "Farm"));
                customMenu.AddItem(myCust.AddToMenu("Harass(T) Active: ", "FarmT"));
                customMenu.AddItem(myCust.AddToMenu("Laneclear Active: ", "LaneClear"));
                customMenu.AddItem(myCust.AddToMenu("Stack E Active: ", "StackE"));
                Menu.AddSubMenu(customMenu);
            }
        }

        private float GetComboDamage(Obj_AI_Base target)
        {
            double comboDamage = 0;

            if (Q.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.Q);

            if (E.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.E);

            if (R.IsReady())
            {
                comboDamage += Player.GetSpellDamage(target, SpellSlot.R);
                comboDamage += comboDamage * 1.12;
            }
            else if (target.HasBuff("vladimirhemoplaguedebuff"))
            {
                comboDamage += comboDamage * 1.12;
            }

            comboDamage = ItemManager.CalcDamage(target, comboDamage);

            return (float)(comboDamage + Player.GetAutoAttackDamage(target));
        }

        private void Combo()
        {
            UseSpells(Menu.Item("UseQCombo", true).GetValue<bool>(),
                Menu.Item("UseECombo", true).GetValue<bool>(), Menu.Item("UseRCombo", true).GetValue<bool>(), "Combo");
        }

        private void Harass()
        {
            UseSpells(Menu.Item("UseQHarass", true).GetValue<bool>(),
                Menu.Item("UseEHarass", true).GetValue<bool>(), false, "Harass");
        }

        private void UseSpells(bool useQ, bool useE, bool useR, string source)
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

            if (target == null)
                return;

            var dmg = GetComboDamage(target);

            if (useR && R.IsReady() && target.IsValidTarget(R.Range))
                CastR(target, dmg);

            //items
            if (source == "Combo")
            {
                var itemTarget = TargetSelector.GetTarget(750, TargetSelector.DamageType.Physical);
                if (itemTarget != null)
                {
                    ItemManager.Target = itemTarget;

                    //see if killable
                    if (dmg > itemTarget.Health - 50)
                        ItemManager.KillableTarget = true;

                    ItemManager.UseTargetted = true;
                }
            }

            if (Menu.Item("Q_Before_E", true).GetValue<bool>())
            {
                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                    Q.Cast(target);

                if (useE && E.IsReady() && target.IsValidTarget(E.Range))
                    E.Cast();
            }
            else
            {
                if (useE && E.IsReady() && target.IsValidTarget(E.Range))
                    E.Cast();

                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                    Q.Cast(target);
            }
        }

        private void CastR(Obj_AI_Hero target, float dmg)
        {
            R.UpdateSourcePosition();

            if (Menu.Item("R_Killable", true).GetValue<bool>() && Q.IsReady() && E.IsReady())
            {
                var pred = R.GetPrediction(target);
                if (dmg > target.Health && pred.Hitchance >= HitChance.High)
                    R.Cast(target);
            }
        }

        private void LastHit()
        {
            var allMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range);

            if (Q.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget() && HealthPrediction.GetHealthPrediction(minion, (int)(Player.Distance(minion.Position) * 1000 / 1400)) < Player.GetSpellDamage(minion, SpellSlot.Q) - 10)
                    {
                        Q.Cast(minion);
                        return;
                    }
                }
            }
        }

        private void Farm()
        {
            var rangedMinionsE = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.NotAlly);

            var useQ = Menu.Item("UseQFarm", true).GetValue<bool>();
            var useE = Menu.Item("UseEFarm", true).GetValue<bool>();

            if (useQ)
                LastHit();

            if (useE && E.IsReady())
            {
                if (rangedMinionsE.Count > 1)
                    E.Cast();

            }
        }

        private void CheckKs()
        {
            foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(1300)).OrderByDescending(GetComboDamage))
            {
                if (Player.Distance(target.ServerPosition) <= E.Range && Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.E) > target.Health && Q.IsReady() && E.IsReady())
                {
                    E.Cast();
                    Q.Cast(target);
                    return;
                }

                if (Player.Distance(target.ServerPosition) <= Q.Range && Player.GetSpellDamage(target, SpellSlot.Q) > target.Health && Q.IsReady())
                {
                    Q.Cast(target);
                    return;
                }

                if (Player.Distance(target.ServerPosition) <= E.Range && Player.GetSpellDamage(target, SpellSlot.E) > target.Health && E.IsReady())
                {
                    E.Cast();
                    return;
                }

                if (Player.Distance(target.ServerPosition) <= R.Range && Player.GetSpellDamage(target, SpellSlot.R) > target.Health && R.IsReady() && Menu.Item("R_KS", true).GetValue<bool>())
                {
                    R.Cast(target);
                    return;
                }
            }
        }

        protected override void Game_OnGameUpdate(EventArgs args)
        {
            if (Menu.Item("smartKS", true).GetValue<bool>())
                CheckKs();

            if (Menu.Item("Orbwalk", true).GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                if (Menu.Item("LastHitKey", true).GetValue<KeyBind>().Active)
                    LastHit();

                if (Menu.Item("LaneClear", true).GetValue<KeyBind>().Active)
                    Farm();

                if (Menu.Item("Farm", true).GetValue<KeyBind>().Active)
                    Harass();

                if (Menu.Item("FarmT", true).GetValue<KeyBind>().Active)
                    Harass();
            }

            if (Player.IsRecalling())
                return;

            if (Menu.Item("StackE", true).GetValue<KeyBind>().Active)
            {
                if (E.IsReady() && Utils.TickCount - E.LastCastAttemptT >= 9900)
                    E.Cast();
            }
        }

        protected override void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            if (!unit.IsMe)
                return;

            if (args.SData.Name == "VladimirTidesofBlood")
            {
                E.LastCastAttemptT = Utils.TickCount + 250;
            }
        }

        protected override void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!Menu.Item("W_Gap_Closer", true).GetValue<bool>()) return;

            if (W.IsReady() && gapcloser.Sender.Distance(Player.Position) < 300)
                W.Cast();
        }

        protected override void Drawing_OnDraw(EventArgs args)
        {
            if (Menu.Item("Draw_Disabled", true).GetValue<bool>())
                return;

            if (Menu.Item("Draw_Q", true).GetValue<bool>())
                if (Q.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);

            if (Menu.Item("Draw_E", true).GetValue<bool>())
                if (E.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);

            if (Menu.Item("Draw_R", true).GetValue<bool>())
                if (R.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);
        }
    }
}
