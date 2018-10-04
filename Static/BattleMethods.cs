using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Magicians
{
	static class BattleMethods
    {
        public static Battler GetFastestBattler(List<Battler> battlers)
        {
            Battler fastest = null;
            int fastestSpeed = 0;
            for (int i = 0; i < battlers.Count; i++)
            {
                if (!battlers[i].BattleStats.canAct && !battlers[i].turnOver)
                {
                    battlers[i].turnOver = true;
                    battlers[i].OnBeginTurn();
                }
                if (!battlers[i].turnOver && battlers[i].BattleStats.canAct)
                {
                    if (battlers[i].BattleStats.Attributes[Attributes.Dexterity] >= fastestSpeed)
                    {
                        fastestSpeed = battlers[i].BattleStats.Attributes[Attributes.Dexterity];
                        fastest = battlers[i];
                    }
                }
            }
            if (fastest != null)
                fastest.OnBeginTurn();
            return fastest;
        }

        public static bool HaveAllBattlersTakenTurn(List<Battler> battlers)
        {
            foreach (Battler battler in battlers)
            {
                if (battler.turnOver == false)
                    return false;
            }
            return true;
        }
        //produces a new list of targets depending upon the entity that used the action and the action itself
        public static List<Battler> GetTargets(Battler caller, BattleAction action, Battle battle)
        {
            var Battlers = battle.Battlers;
            var team = caller.BattleStats.team;
            var battlers = new List<Battler>();
            battlers.AddRange(Battlers);
            var targets = new List<Battler>();
            for (int e = 0; e < battlers.Count; e++)
            {
                if (battlers[e].BattleStats.HP <= 0)
                {
                    battlers.RemoveAt(e);
                    e--;
                }
            }
            if (action.targetType == BattleAction.TargetType.Self)
            {
                var singleTarget = new List<Battler>();
                singleTarget.Add(caller);
                return singleTarget;
            }
            switch (action.bDirection)
            {
                case (BattleAction.Direction.Self):
                    {
                        for (int i = 0; i < battlers.Count; i++)
                        {
                            if (battlers[i].BattleStats.team == team)
                                targets.Add(battlers[i]);
                        }
                        break;
                    }
                case (BattleAction.Direction.Enemy):
                    {
                        for (int i = 0; i < battlers.Count; i++)
                        {
                            if (battlers[i].BattleStats.team != team)
                                targets.Add(battlers[i]);
                        }
                        break;
                    }
                case (BattleAction.Direction.Both):
                    {
                        for (int i = 0; i < battlers.Count; i++)
                            targets.Add(battlers[i]);
                        break;
                    }
            }
            if (action.targetType == BattleAction.TargetType.Random)
            {
                return GetRandomTarget(battle, targets, false).ReturnInList();
            }
            if (action.targetType == BattleAction.TargetType.Single && team == Team.Enemy)
            {
                var singleTarget = new List<Battler>();
                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i].BattleStats.canTarget == false)
                    {
                        targets.RemoveAt(i);
                    }
                }
                if (action.tag == "HEAL")
                {
                    int losthealth = 0;
                    int target = 0;
                    for (int i = 0; i < battlers.Count; i++)
                    {
                        if (targets[i].BattleStats.team == caller.BattleStats.team)
                        {
                            if (targets[i].BattleStats.HP < losthealth)
                            {
                                target = i;
                                losthealth = targets[i].BattleStats.HP;
                            }
                        }
                    }
                    return targets[target].ReturnInList();
                }
                if (action.tag == "DAMAGE")
                {
                    float damage = 0;
                    for (int i = 0; i < action.actionEffects.Count; i++)
                    {
                        if (action.actionEffects[i] is DoDamage)
                        {
                            for (int b = 0; b < targets.Count; b++)
                            {
                                var dmg = (DoDamage)action.actionEffects[i];
                                damage += dmg.GetHighestResult(caller.BattleStats, targets[b].BattleStats);
                                if (damage > targets[b].BattleStats.HP && targets[b].BattleStats.HP != targets[b].BattleStats.MaxHP)
                                {
                                    singleTarget.Add(targets[b]);
                                    Console.WriteLine("BATTLE: single target chosen " + targets[b].Name + ", has low health");
                                    return singleTarget;
                                }
                                damage = 0;
                            }
                        }
                    }
                    singleTarget.Add(GetRandomTarget(battle, targets, false));
                    if (singleTarget[0] != null)
                    {
                        Console.WriteLine("BATTLE: single target chosen " + singleTarget[0].Name + " randomly");
                    }
                    else
                        singleTarget.Clear();
                    return singleTarget;
                }
                for (int i = 0; i < action.actionEffects.Count; i++)
                {
                    if (action.actionEffects[i] is DrainMana)
                    {
                        var drain = action.actionEffects[i] as DrainMana;
                        Battler highest = null;
                        for (int b = 0; b < targets.Count; b++)
                        {
                            //always be on the side of caution
                            if (drain.lower <= targets[b].BattleStats.SP)
                            {
                                highest = targets[b];
                            }
                        }
                        var highestTarget = new List<Battler>(); highestTarget.Add(highest);
                        if (highest != null)
                        {
                            return highestTarget;
                        }
                    }
                }
            }
            return targets;
        }

        public static Battler GetRandomTarget(Battle battle, List<Battler> targets, bool removeInvis)
        {
            //TODO: add thing that excludes invisible enemies in both here and the smart battle action chooser
            if (removeInvis)
                for (int i = 0; i < targets.Count; i++)
                {
                    if (!targets[i].BattleStats.canTarget)
                    {
                        targets.RemoveAt(i);
                        i--;
                    }
                }
            try { return (targets[battle.Random.Next(0, targets.Count)]); }
            catch { return null; }
        }
        public static BattleAction GetEnemyBattleAction(Battle battle, Battler caller, List<Battler> battlers) //battlers is a list containing everyone EXCEPT the target battler
        {
            var actions = caller.battleActions.ToList();
            var enemies = new List<int>();
            var players = new List<int>();
            BattleAction battleAction = null;
            BattleAction healAction = null;
            BattleAction manaAction = null;
            var callerHasLowHealth = false;
            var teamHasLowHealth = false;
            var callerHasLowMana = false;
            var teamHasLowMana = false;
            battlers.Remove(caller);

            for (int i = 0; i < battlers.Count; i++)
            {
                if (battlers[i].BattleStats.team == Team.Enemy)
                {
                    enemies.Add(i);
                }
                else
                {
                    players.Add(i);
                }
            }
            //first, get rid of actions we cant use
            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i] == null)
                {
                    actions.RemoveAt(i);
                    i--;
                    i = MathHelper.Clamp(i--, 0, 999);
                }
                if (actions[i].ManaCost > caller.BattleStats.SP)
                {
                    actions.RemoveAt(i);
                    i--;
                    continue;
                }
            }
            //if the battler has two turns, remove the previous action
            if (caller.twoTurns)
            {
                if (actions.Count > 1)
                {
                    for (int i = 0; i < actions.Count; i++)
                    {
                        if (actions[i] == caller.lastAction)
                        {
                            actions.RemoveAt(i);
                            i--;
                            continue;
                        }
                    }
                }
            }
            //if the caller has in order, simply return with the chosen turn
            if (caller.inOrder)
            {
                battleAction = actions[MathHelper.Clamp(caller.actionNumber, 0, actions.Count - 1)];
                goto ChooseTargets;
            }
            if (actions.Count == 0)
            {

            }
            //check if we can use dispel

            //check if we can heal anyone
            healAction = actions.Find(b => b.tag == "HEAL");
            if (healAction != null)
            {
                if ((caller.BattleStats.MaxHP / 100) * 35 > caller.BattleStats.HP)
                {
                    callerHasLowHealth = true;
                }
                for (int i = 0; i < battlers.Count; i++)
                {
                    if (battlers[i].BattleStats.team == caller.BattleStats.team && (caller.BattleStats.MaxHP / 100) * 35 > caller.BattleStats.HP)
                    {
                        teamHasLowHealth = true;
                    }
                }

                if (callerHasLowHealth && healAction != null)
                {
                    battle.SetTargets(caller.ReturnInList());
                    return healAction;
                }
                if (teamHasLowHealth)
                {
                    if (healAction.targetType == BattleAction.TargetType.All)
                    {
                        battle.SetTargets(BattleMethods.GetTargets(caller, healAction, battle));
                        return healAction;
                    }
                    goto End;
                }
            }
            //check if anyone needs mana
            manaAction = actions.Find(b => b.tag == "MANA");
            if (manaAction != null)
            {
                if ((caller.BattleStats.MaxSP / 100) * 35 > caller.BattleStats.SP)
                {
                    callerHasLowMana = true;
                }
                for (int i = 0; i < battlers.Count; i++)
                {
                    if (battlers[i].BattleStats.team == caller.BattleStats.team && (caller.BattleStats.MaxSP / 100) * 35 > caller.BattleStats.SP)
                    {
                        teamHasLowMana = true;
                    }
                }

                if (callerHasLowMana && manaAction != null)
                {
                    battle.SetTargets(caller.ReturnInList());
                    return manaAction;
                }
                if (teamHasLowMana)
                {
                    if (healAction.targetType == BattleAction.TargetType.All)
                    {
                        battle.SetTargets(BattleMethods.GetTargets(caller, manaAction, battle));
                        return manaAction;
                    }
                    goto End;
                }
            }
            //do we have a default action
            if (actions.Count == 0)
            {
                if (caller.defaultAction != null)
                    battleAction = caller.defaultAction;
            }
        SelectAction:
            int stop = 0;
            while (battleAction == null)
            {
                if (actions.Count == 0)
                {
                    return battle.GetWaitAction();
                }
                battleAction = actions[battle.Random.Next(0, actions.Count)];
                if (battleAction.tag == "HEAL" || battleAction.tag == "DISPEL" || battleAction.tag == "MANA")
                {
                    battleAction = null;
                    stop += 1;
                    if (stop > 500)
                    {
                        return battle.GetWaitAction();
                    }
                }
            }
        ChooseTargets:;
            //prevents applying debuff if the debuff is already present on the character
            if (battleAction.tag == "DEBUFF" && battleAction.targetType == BattleAction.TargetType.Single)
            {
                for (int i = 0; i < battleAction.actionEffects.Count; i++)
                {
                    if (battleAction.actionEffects[i] is ApplyModifier)
                    {
                        var applyModifier = (ApplyModifier)battleAction.actionEffects[i];
                        for (int e = 0; ;)
                        {
                            var random = players.ToList();
                            while (random.Count > 0)
                            {
                                e = battle.Random.Next(0, random.Count);
                                if (battlers[players[e]].BattleStats.Modifiers.Find(m => m.Name == applyModifier.mod.Name) == null && battlers[players[e]].BattleStats.HP > 0)
                                {
                                    battle.SetTargets(battlers[players[e]].ReturnInList());
                                    return battleAction;
                                }
                                random.RemoveAt(e);
                            }
                            if (random.Count == 0)
                            {
                                actions.Remove(battleAction);
                                battleAction = null;
                                goto SelectAction;
                            }
                            break;
                        }
                    }
                }
            }
            //the same, but inverse
            if (battleAction.tag == "BUFF" && battleAction.targetType == BattleAction.TargetType.Single)
            {
                for (int i = 0; i < battleAction.actionEffects.Count; i++)
                {
                    if (battleAction.actionEffects[i] is ApplyModifier)
                    {
                        var applyModifier = (ApplyModifier)battleAction.actionEffects[i];
                        if (enemies.Count > 0)
                        {
                            var random = enemies.ToList();
                            for (int e = 0; ;)
                            {
                                e = battle.Random.Next(0, random.Count);
                                if (battlers[enemies[e]].BattleStats.Modifiers.Find(m => m.Name == applyModifier.mod.Name) == null)
                                {
                                    battle.SetTargets(battlers[enemies[e]].ReturnInList());
                                    return battleAction;
                                }
                                random.RemoveAt(e);
                                if (random.Count == 0)
                                {
                                    actions.Remove(battleAction);
                                    battleAction = null;
                                    goto SelectAction;
                                }
                            }
                        }
                        //to prevent casting it on self when the battler already has it
                        if (caller.BattleStats.Modifiers.Find(m => m.Name == applyModifier.mod.Name) != null)
                        {
                            actions.Remove(battleAction);
                            battleAction = null;
                            goto SelectAction;
                        }
                        battle.SetTargets(caller.ReturnInList());
                        return battleAction;
                    }
                }
            }
            if (battleAction.tag == "BUFF" && battleAction.targetType == BattleAction.TargetType.Self)
            {
                for (int i = 0; i < battleAction.actionEffects.Count; i++)
                {
                    if (battleAction.actionEffects[i] is ApplyModifier)
                    {
                        var applyModifier = (ApplyModifier)battleAction.actionEffects[i];
                        if (caller.BattleStats.Modifiers.Find(m => m.Name == applyModifier.mod.Name) == null)
                        {
                            battle.SetTargets(caller.ReturnInList());
                            return battleAction;
                        }
                        actions.Remove(battleAction);
                        battleAction = null;
                        goto SelectAction;
                    }
                }
            }
        End:
            Console.WriteLine("BATTLE: chosen action " + battleAction.Name);
            battle.SetTargets(BattleMethods.GetTargets(caller, battleAction, battle));
            return battleAction;
        }


        public static int GetHealthChange(int old, int newh)
        {
            return old - newh;
        }
        public static string ActionText(BattleAction action, Battler user)
        {
            return user.Name + action.IntText;
        }      
        //call in draw method
        public static void DrawNumbers(int damage, Vector2 location, SpriteBatch batch, SpriteFont font)
        {
            Color color;
            if (damage < 0)
            {
                color = Color.Red;
            }
            if (damage > 0)
            {
                color = Color.Green;
            }
            else
                color = Color.White;
            batch.DrawString(font, damage.ToString(), location, color);
        }

        public static int ReturnStatValue(Attributes s, Battler b)
        {
            switch (s)
            {
                case (Attributes.Strength): { return b.BattleStats.Attributes[Attributes.Strength]; }
                case (Attributes.Magic): { return b.BattleStats.Attributes[Attributes.Magic]; }
            }
            return 1;
        }
        public static bool GetDodgeChance(Battle battle, BattleStats attacker, BattleStats target)
        {
            if (!target.canAct)
                return false;
            float bonus = 0;
            if (attacker.Attributes[Attributes.Dexterity] < target.Attributes[Attributes.Dexterity])
                bonus = (target.Attributes[Attributes.Dexterity] - attacker.Attributes[Attributes.Dexterity]) / 4f;
            if (battle.Random.Next(0, 100) <= MathHelper.Clamp(target.DodgeChance + attacker.MissRate + bonus, -99, 99))
                return true;
            return false;
        }
        public static LuckResult GetLuckResult(Battle battle, BattleStats caller)
        {
            if (caller.Luck > 0)
            {
                int luckBase = 100 - (20 * caller.Luck);
                if (battle.Random.Next(0, 100) < luckBase)
                    return LuckResult.Positive;
                return LuckResult.Neutral;
            }
            if (caller.Luck < 0)
            {
                int luckBase = 100 - (20 * caller.Luck);
                if (battle.Random.Next(0, 100) < luckBase)
                    return LuckResult.Negative;
                return LuckResult.Neutral;
            }
            return LuckResult.Neutral;
        }
    }
}
