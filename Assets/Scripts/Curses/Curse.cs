using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum curses
{
    Hunger,
    Starvation,
    Thirst,
    Dehydration,
    Zombies,
    Darkness,
    FadingLight,
    Gun,
    UncontrollableFire,
    Ammo,
    Wealth,
    Bombs,
    Allergy,
    Skull,
    MoreWealth,
    MoreStarvation,
    MoreDehydration,
    MoreZombieHealth,
    StrongerZombies,
    FastZombies,
    UnstableGround,
    MoreZombies,
    Drought
}

[CreateAssetMenu(fileName = "NewCurse", menuName = "Curse")]
public class Curse : ScriptableObject
{
    public string curseName;
    public string curseDescription;
    public curses curseType;
}
