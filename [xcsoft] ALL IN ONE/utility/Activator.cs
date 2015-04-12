﻿using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

namespace _xcsoft__ALL_IN_ONE.utility
{
    class Activator
    {
        //아이템 목록 && 설명
        //http://lol.inven.co.kr/dataninfo/item/list.php
        //http://www.lolking.net/items/

        static Orbwalking.Orbwalker Orbwalker { get { return xcsoftMenu.Orbwalker; } }
        static Menu Menu { get { return xcsoftMenu.Menu_Manual.SubMenu("Activator"); } }

        internal static List<Items.Item> afterAttackItems;

        internal static void Load()
        {
            xcsoftMenu.addSubMenu("Activator");

            Menu.AddSubMenu(new Menu("AfterAttack", "AfterAttack"));

            items_initialize();

            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
        }

        static void items_initialize()
        {
            afterAttackItems.Add(new Items.Item((int)ItemId.Tiamat_Melee_Only, 250f));
            afterAttackItems.Add(new Items.Item((int)ItemId.Ravenous_Hydra_Melee_Only, 250f));

            foreach (var item in afterAttackItems)
            {
                Menu.SubMenu("AfterAttack").AddItem(new MenuItem("AfterAttack.Use " + item.ToString(), "Use " + item.ToString())).SetValue(true);
            }
        }

        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe || target == null)
                return;

            if (target.Type != GameObjectType.obj_AI_Base && 
                target.Type != GameObjectType.obj_AI_Minion && 
                target.Type != GameObjectType.obj_AI_Hero &&
                Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo &&
                Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Mixed &&
                Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LaneClear)
                return;

            foreach (var item in afterAttackItems.Where(x => x.IsReady() && Menu.Item("AfterAttack.Use " + x.ToString()).GetValue<bool>()))
                item.Cast();
        }
    }
}
