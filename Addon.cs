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

namespace LeeSin
{
    class Addon
    {
        private Spell.Skillshot Q;
        private Spell.Active secondQ;
        private Spell.Active E;
        private Spell.SpellBase W;
        private Spell.Targeted R;
        private Spell.Targeted flash;

        private Spell.Targeted smite;
        private Menu myMenu;
        private Menu comboMenu, smiteMenu;

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
            string currentVersion = "0.5";
            Chat.Print("LeeSin - Neva Series LOADED.");
            Chat.Print("Checking version..");
            if (new WebClient().DownloadString("http://pastebin.com/raw.php?i=WeD1mMzH") != currentVersion)
                Chat.Print("OLD VERSION!");
            else
                Chat.Print("You have the last version of LeeSin script.");
            Game.Drop();
            myMenu = MainMenu.AddMenu("LeeSin -", "title");
            myMenu.AddLabel("NevaBudy :D - Press C to insec,if you're close of target it will be more accurate.");

           comboMenu =myMenu.AddSubMenu("Combo settings","comboSection");
           comboMenu.AddGroupLabel("Configuration");
           comboMenu.AddSeparator();

           comboMenu.Add("combo.Q", new CheckBox("Use Q"));
           comboMenu.Add("combo.W", new CheckBox("Use W"));
           comboMenu.Add("combo.R", new CheckBox("Use R"));
           comboMenu.Add("combo.E", new CheckBox("Use E"));

           smiteMenu = myMenu.AddSubMenu("Smite settings", "smiteSection");
           smiteMenu.AddGroupLabel("Smite here");
           smiteMenu.AddSeparator();
           comboMenu.Add("smite.RED", new CheckBox("Smite RED"));
           comboMenu.Add("smite.BLUE", new CheckBox("smite BLUE"));
           comboMenu.Add("smite.DRAGON", new CheckBox("smite DRAGON"));
           comboMenu.Add("smite.PINKPENISH", new CheckBox("smite BARON"));
           
           GameObject.OnCreate += castWinWard;
           Game.OnTick += actives;
           Game.OnUpdate += gameUpdate;
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
        public string[] MinionNames = 
        {
            "TT_Spiderboss",
            "SRU_Blue",
            "SRU_Red",
            "SRU_Baron"
        };
     
        public Obj_AI_Minion GetNearest(Vector3 pos)
        {
            var minions =
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(minion => minion.IsValid && MinionNames.Any(name => minion.Name.StartsWith(name)) && !MinionNames.Any(name => minion.Name.Contains("Mini")) && !MinionNames.Any(name => minion.Name.Contains("Spawn")));
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
        public void insec()
        {
            if (flash.IsReady() && Q.IsReady() && R.IsReady())
            {
                var target = TargetSelector.GetTarget((Q.IsReady()) ? Q.Range : (R.Range + 100), DamageType.Physical);
                if (Q.GetPrediction(target).HitChance >= HitChance.Medium)
                {
                    Q.Cast(target);
                    Thread.Sleep(20);
                    smite.Cast(target);                  
                    flash.Cast(new Vector3(new Vector2(target.Position.X + 20,target.Position.Y + 20),target.Position.Z));
                    R.Cast(target);
                    Q.Cast(target);
                }
            }
        }
        public void castWinWard(GameObject sender,EventArgs arg)
        {
           
            foreach (var ward in ObjectManager.Get<Obj_AI_Minion>())
            {
                if (ward.Name.ToLower().Contains("ward") && W.IsReady())
                {
                    W.Cast(ward);
                }
            }
           
        }
        public void Combo()
        {
            Boolean useQ = comboMenu["combo.Q"].Cast<CheckBox>().CurrentValue;
            Boolean useW = comboMenu["combo.W"].Cast<CheckBox>().CurrentValue;
            Boolean useR = comboMenu["combo.R"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget((useQ && Q.IsReady()) ? Q.Range : (R.Range + 100), DamageType.Physical);
            if (E.IsReady() && E.IsInRange(target))
            {
                E.Cast();
            }
            if(useW && W.IsReady())
            {
                foreach (var ward in ObjectManager.Get<Obj_Ward>())
                {
               
                        W.Cast(ward);
                   
                }
            }
            if (useR && R.IsReady())
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
                    foreach(var minions in ObjectManager.Get<Obj_AI_Minion>().Where(m => m.IsValidTarget(Q.Range) && Q.GetPrediction(m).HitChance >= HitChance.Medium).ToArray())
                    {
                    smite.Cast(minions);
                    Q.Cast(target);
                    }

                }
                     if (Q.GetPrediction(target).HitChance >= HitChance.Medium)
                    {
                        Q.Cast(target);
                        smite.Cast(target);

                    }
                
            }



        }
        public void fuckMinion(Obj_AI_Minion minion)
        {
            smite.Cast(minion);
        }
        public void harass()
        {
            if (Q.IsReady())
            {
                foreach (var target in HeroManager.Enemies.Where(o => o.IsValidTarget(Q.Range) && !o.IsDead && !o.IsZombie))
                {
                    if (Q.GetPrediction(target).HitChance >= HitChance.High)
                    {
                        Q.Cast(target);
                    }
                }
            }
            if (E.IsReady())
            {
                foreach (var target in HeroManager.Enemies.Where(o => o.IsValidTarget(E.Range) && !o.IsDead && !o.IsZombie
                    && o.HasBuffOfType(BuffType.Poison)))
                {
                    E.Cast(target);
                }
            }
        }

    }
}
