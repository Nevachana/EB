using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using Microsoft.Win32;
using SharpDX;
using System.Threading;
using System.Net;
using EloBuddy.SDK.Rendering;

namespace LeeSin
{
    class Addon
    {
        private Spell.Skillshot Q;
        private Spell.Active secondQ;
        private Spell.Active E;
        private Spell.Targeted W;
        private Spell.Targeted R;
        private Spell.Targeted flash;

        private Spell.Targeted smite;
        private Spell.Targeted placeWard;
        private Spell.Targeted placeWard2;


        private Menu myMenu;
        private Menu comboMenu, smiteMenu, insecMenu, menuJungle,draws;
        private int[] smiteDMG = new int[]{390, 410, 430, 450, 480, 510, 540, 570, 600, 640, 680, 720, 760, 800, 850, 900, 950, 1000};
        private int actualLevel = 1;
        private int smiteDmg = 390;

        public void start(EventArgs args)
        {
            setUp_Menu();
            setUp_spells();

        }
        public void setUp_Menu()
        {

            string currentVersion = "0.85";
            Chat.Print("LeeSin2 - Neva Series LOADED.");
            Chat.Print("Checking version..");
            if (new WebClient().DownloadString("http://pastebin.com/raw.php?i=WeD1mMzH") != currentVersion)
                Chat.Print("OLD VERSION!");
            else
                Chat.Print("You have the last version of LeeSin script.");
            Game.Drop();
            myMenu = MainMenu.AddMenu("LeeSin - NevaSeries", "title");
            myMenu.AddLabel("  NevaBudy :D ");
            myMenu.AddLabel("  SpaceBar for starCombo. ");
            myMenu.AddLabel("  C for insec. ");
            myMenu.AddLabel("  T for WardJump. ");
            myMenu.AddLabel("  V for jungleCLear. ");
            myMenu.AddLabel("  Z for laneleCLear. ");

            myMenu.AddLabel(" More coming soon.. Actual version: 0.85");

           comboMenu =myMenu.AddSubMenu("Combo settings","comboSection");
           comboMenu.AddGroupLabel("Configuration");
           comboMenu.AddSeparator();

           comboMenu.Add("combo.Q", new CheckBox("Use Q"));
           comboMenu.Add("combo.W", new CheckBox("Use W"));
           comboMenu.Add("combo.R", new CheckBox("Use R"));
           comboMenu.Add("combo.E", new CheckBox("Use E"));
           comboMenu.Add("ks.R", new CheckBox("R for ks"));
            

           smiteMenu = myMenu.AddSubMenu("Smite settings", "smiteSection");
           smiteMenu.AddGroupLabel("Smite settings");
                       smiteMenu.AddSeparator();
           smiteMenu.Add("smite.RED", new CheckBox("Smite RED"));
           smiteMenu.Add("smite.BLUE", new CheckBox("smite BLUE"));
           smiteMenu.Add("smite.DRAGON", new CheckBox("smite DRAGON"));
           smiteMenu.Add("smite.PINKPENISH", new CheckBox("smite BARON"));
           insecMenu = myMenu.AddSubMenu("Insec settings", "insecSection");
           insecMenu.Add("insec.Flash", new CheckBox("Use flash"));
            insecMenu.Add("insec.Smart",new CheckBox("Smart insec"));
            menuJungle = myMenu.AddSubMenu("Jungle Clear", "jungleSection");
            menuJungle.AddGroupLabel("Jungle Clear settings");
            menuJungle.Add("jungle.Q", new CheckBox("Use Q"));
            menuJungle.Add("jungle.W", new CheckBox("Use W"));
            menuJungle.Add("jungle.E", new CheckBox("Use E"));

            draws = myMenu.AddSubMenu("Drawing settings", "drawinsSection");
            draws.AddGroupLabel("Drawins settings");
            draws.AddSeparator();
            draws.Add("draw.Q", new CheckBox("Draw Q range"));
            draws.Add("draw.W", new CheckBox("Draw W range"));
            draws.Add("draw.R", new CheckBox("Draw R range"));
            draws.Add("draw.target", new CheckBox("Draw current Target"));
           Game.OnTick += actives;
           Game.OnUpdate += gameUpdate;
           Drawing.OnDraw += onDraw;
        }
        //public int[] qDmg = new int[] { 0, 200 + (int)(ObjectManager.Player.BaseAttackDamage / 0.9), 400 + (int)(ObjectManager.Player.BaseAttackDamage / 0.9), 600 + (int)(ObjectManager.Player.BaseAttackDamage / 0.9) };
        //public void qKs(EventArgs args)
        //{            
           
        //}
        public void onDraw(EventArgs args)
        {
            Boolean drawQ = draws["draw.Q"].Cast<CheckBox>().CurrentValue;
            Boolean drawW = draws["draw.W"].Cast<CheckBox>().CurrentValue;
            Boolean drawR = draws["draw.R"].Cast<CheckBox>().CurrentValue;
            Boolean drawTarget = draws["draw.target"].Cast<CheckBox>().CurrentValue;
            if (drawQ)
            {
                Circle.Draw(Color.Green, Q.Range, ObjectManager.Player.Position);


            }
            if (drawW)
            {
                Circle.Draw(Color.Green, W.Range, ObjectManager.Player.Position);
            }
            if (drawR)
            {

                Circle.Draw(Color.Green, R.Range, ObjectManager.Player.Position);
                if (drawTarget)
                {
                    Circle.Draw(Color.Red, ObjectManager.Player.AttackRange, myTarget.Position);
                }

            }
        }
        public void setUp_spells()
        {
            //                               skill , range ,             type ,       delayC ,      speed      , radius
            this.Q = new Spell.Skillshot(SpellSlot.Q, 1060, SkillShotType.Linear, (int)0.30f, (Int32)1780, (int)60f);
            this.W = new Spell.Targeted(SpellSlot.W, 675);
            this.E = new Spell.Active(SpellSlot.E, 410);
            this.R = new Spell.Targeted(SpellSlot.R, 350);
            this.smite = new Spell.Targeted(SpellSlot.Summoner2, 500);
            this.secondQ = new Spell.Active(SpellSlot.Q, 1800);
            this.flash = new Spell.Targeted(SpellSlot.Summoner1, 400);
            this.placeWard = new Spell.Targeted(SpellSlot.Trinket, 520);
            this.placeWard2 = new Spell.Targeted(SpellSlot.Trinket, 400); // <-
        }
        public void actives(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo)
            {
                Combo();
            }
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Harass)
            {
                insec();
            }
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Flee)
            {
                jump(Game.CursorPos);
            }
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.JungleClear)
            {
                jungleClear();
            }
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.LaneClear)
            {
                laneClear();
            }

        }

        public void gameUpdate(EventArgs args)
        {
            if (ObjectManager.Player.Level > actualLevel)
            {
                actualLevel = ObjectManager.Player.Level;
                smiteDmg = smiteDMG[actualLevel - 1];
            }

            var mob = GetNearest(ObjectManager.Player.ServerPosition);
            if (mob != null)
            {
                if (smite.IsReady() && smiteDmg >= mob.Health && Vector3.Distance(ObjectManager.Player.ServerPosition, mob.ServerPosition) <= smite.Range)
                {
                    smite.Cast(mob);
                }
            }
        
            
        }
        public void laneClear()
        {
            var minions = ObjectManager.Get<Obj_AI_Minion>().Where(m => m.IsValidTarget(Q.Range) && Q.GetPrediction(m).HitChance >= HitChance.Medium);
            if (Q.IsReady() && Q.IsInRange(minions.FirstOrDefault().Position) && Q.GetPrediction(minions.FirstOrDefault()).HitChance >= HitChance.Low)
            {
                Q.Cast(minions.FirstOrDefault().Position);
            }
            if (E.IsReady() && E.IsInRange(minions.FirstOrDefault().Position))
            {
                E.Cast();
            }
        }
        public string[] MinionNames = 
        {
            "TT_Spiderboss",
            "SRU_Blue",
            "SRU_Red",
            "SRU_Baron",
              "SRU_Murkwolf", 
              "SRU_Razorbeak", 
              "SRU_Gromp", 
              "SRU_Krug"
        };
        public void jungleClear()
        {
            var minions = ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValid && MinionNames.Any(name => minion.Name.StartsWith(name)) && !MinionNames.Any(name => minion.Name.Contains("Mini")) && !MinionNames.Any(name => minion.Name.Contains("Spawn")));
            var objAiMinions = minions as Obj_AI_Minion[] ?? minions.ToArray();
            Obj_AI_Minion sMinion = objAiMinions.FirstOrDefault();
            if(E.IsInRange(sMinion) && E.IsReady())
            {
                E.Cast();
            }
            if(W.IsReady())
            {
                W.Cast();
            }
            if(Q.IsReady() && Q.IsInRange(sMinion) && Q.GetPrediction(sMinion).HitChance >= HitChance.Low)
            {
                Q.Cast(sMinion);
            }
        }
        public Obj_AI_Minion GetNearest(Vector3 pos)
        {
            var minions =ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValid && MinionNames.Any(name => minion.Name.StartsWith(name)) && !MinionNames.Any(name => minion.Name.Contains("Mini")) && !MinionNames.Any(name => minion.Name.Contains("Spawn")));
            var objAiMinions = minions as Obj_AI_Minion[] ?? minions.ToArray();
            Obj_AI_Minion sMinion = objAiMinions.FirstOrDefault();
            double? nearest = null;
            foreach (Obj_AI_Minion minion in objAiMinions)
            {
                double distance = Vector3.Distance(pos, minion.Position);
                if (nearest == null || nearest > distance)
                {
                    nearest = distance;
                    sMinion = minion;
                }
            }
            return sMinion;
        }
        bool wCasted = false;
        bool castQ = false;
        bool castedR = false;
        bool flashUsed = false;
        bool insecQ = false;
        bool castedW = false;
        bool castedQ = false;
        bool hasStarted = false;
        public AIHeroClient myTarget = null;

        public void insec()
        {
            Boolean useFlash = insecMenu["insec.Flash"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget((Q.IsReady()) ? Q.Range :1 , DamageType.Physical);
            myTarget = target;
                if (W.IsInRange(target.Position) && W.IsReady() && !hasStarted)
                {
                    if (wCasted == false)
                    {
                        jump(insec(target).To3D());
                        hasStarted = true;
                        wCasted = true;
                    }
                    if (wCasted && !castQ && R.IsReady())
                    {
                        
                        Core.DelayAction((() => R.Cast(target)), 500);
                        castQ = true;

                    }
                    if (Q.IsInRange(target.Position) && castQ && Q.GetPrediction(target).HitChance >= HitChance.Medium && Q.IsReady())
                    {
                        Core.DelayAction((() => Q.Cast(target)), 700);
                        castQ = false;
                        wCasted = true;


                    }
                }
                else if (R.IsInRange(target.Position) && useFlash && flash.IsReady() && R.IsReady() && insecQ && !hasStarted)
                {
                    if (!castedR)
                    {
                        R.Cast(target);
                        hasStarted = true;
                        castedR = true;
                    }
                    if (castedR)
                    {
                        flash.Cast(insec(target).To3D());
                        flashUsed = true;
                    }
                    if (castedR && flashUsed && Q.IsInRange(target.Position))
                    {
                        if (Q.GetPrediction(target).HitChance >= HitChance.Collision)
                        {
                            foreach (var minions in ObjectManager.Get<Obj_AI_Minion>().Where(m => m.IsValidTarget(Q.Range) && Q.GetPrediction(m).HitChance >= HitChance.Medium).ToArray())
                            {
                                if (minions.CountEnemiesInRange(Q.Range) <= 1)
                                    smite.Cast(minions);
                                Q.Cast(target);
                            }

                        }
                        else if (Q.GetPrediction(target).HitChance >= HitChance.Medium)
                        {
                            Q.Cast(target);
                            castedQ = true;
                        }
                        castedR = false;
                        flashUsed = false;
                    }                  
                }
                else
                {
                    if (!castedQ && Q.IsReady())
                    {
                        if (Q.GetPrediction(target).HitChance >= HitChance.Collision)
                        {
                            foreach (var minions in ObjectManager.Get<Obj_AI_Minion>().Where(m => m.IsValidTarget(Q.Range) && Q.GetPrediction(m).HitChance >= HitChance.Medium).ToArray())
                            {
                                if (minions.CountEnemiesInRange(Q.Range) <= 1)
                                    smite.Cast(minions);
                                Q.Cast(target);
                                hasStarted = true;
                                 castedQ = true;
                            }

                        }
                        else if (Q.GetPrediction(target).HitChance >= HitChance.Medium)
                        {
                            Q.Cast(target);
                            hasStarted = true;
                            castedQ = true;
                        }
                    }
                    if (target.HasBuff("BlindMonkQOne") && Q.IsReady() && castedQ)
                    {
                        Q.Cast(target);
                    }
                    if (castedQ && W.IsInRange(insec(target).To3D()) && W.IsReady() && !castedW)
                    {

                            jump(insec(target).To3D());
                            castedW = true;
                    }
                    if (R.IsInRange(target.Position) && castedW && castedQ && R.IsReady() && !castedR)
                    {
                        Core.DelayAction(() => R.Cast(target), 500) ;
                        castedR = true;                
                    }
                
            }
        }
        public Vector2 insec(AIHeroClient target)
        {
            Vector3 myPos = ObjectManager.Player.Position;
           return ObjectManager.Player.Position.Extend(target.Position, myPos.Distance(target.Position + 100));
           
            
        }
        public void jump(Vector3 pos,bool isInsec = false)
        {

            var ward = ObjectManager.Get<Obj_AI_Base>().Where(a => a.IsAlly && a.Distance(pos) < 200).FirstOrDefault();
            if (ward != null)
            {
                W.Cast(ward);
            }
            else
            {
                var wardSlot = ObjectManager.Player.InventoryItems.FirstOrDefault(a => a.IsWard && a.CanUseItem());
                if (wardSlot != null && W.IsReady())
                {
                    ObjectManager.Player.Spellbook.CastSpell(wardSlot.SpellSlot, pos);
                    W.Cast(ward);

                }
            }
                    
    

            //this.placeWard.Cast(pos); // here you will be casting a ward even if it already exists. bad :P let me fix.

            //if (W.IsReady()) // :'D
            //{
            //    foreach (var ward in ObjectManager.Get<Obj_Ward>())
            //    {

            //        W.Cast(ward);
            //    }
            //}
        }
        public int[] Rdmg = new int[] {0, 200 + (int)(ObjectManager.Player.BaseAttackDamage / 2), 400 + (int)(ObjectManager.Player.BaseAttackDamage / 2), 600 + (int)(ObjectManager.Player.BaseAttackDamage / 2) };
        public void Combo()
        {
            Boolean useQ = comboMenu["combo.Q"].Cast<CheckBox>().CurrentValue;
            Boolean useW = comboMenu["combo.W"].Cast<CheckBox>().CurrentValue;
            Boolean useR = comboMenu["combo.R"].Cast<CheckBox>().CurrentValue;
            Boolean useE = comboMenu["combo.E"].Cast<CheckBox>().CurrentValue;
            Boolean rKs = comboMenu["ks.R"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget((useQ && Q.IsReady()) ? Q.Range : (R.Range + 100), DamageType.Physical);
            myTarget = target;
            if (E.IsReady() && E.IsInRange(target) && useE)
            {
                E.Cast();
            }
            if (useR && R.IsReady() || ObjectManager.Player.CalculateDamageOnUnit(target, DamageType.Physical, (float)Rdmg[R.Level]) > target.Health)
            {

                if (Q.GetPrediction(target).HitChance >= HitChance.Medium && Q.IsReady() )
                    {
                        

                        R.Cast(target);
                        
                    }
            }
            if (useQ && Q.IsReady())
            {
                if (Q.GetPrediction(target).HitChance >= HitChance.Collision)
                {
                    foreach(var minions in ObjectManager.Get<Obj_AI_Minion>().Where(m => m.IsValidTarget(Q.Range) && Q.GetPrediction(m).HitChance >= HitChance.Medium ).ToArray())
                    {
                    if(minions.CountEnemiesInRange(Q.Range) <= 1)
                    smite.Cast(minions);
                    Q.Cast(target);
                    }

                }
                     if (Q.GetPrediction(target).HitChance >= HitChance.Medium )
                    {
                        Q.Cast(target);
                      //  smite.Cast(target);

                    }
                
            }



        }

        
        }

    }
