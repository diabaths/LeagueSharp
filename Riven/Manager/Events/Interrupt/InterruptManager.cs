﻿namespace Flowers_Riven_Reborn.Manager.Events
{
    using myCommon;
    using LeagueSharp;
    using LeagueSharp.Common;

    internal class InterruptManager : Logic
    {
        internal static void Init(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (Menu.GetBool("InterruptTargetW") && W.IsReady() && sender.IsValidTarget(W.Range) &&
                !sender.ServerPosition.UnderTurret(true))
            {
                W.Cast(true);
            }
        }
    }
}