using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    HitpointType MainDamage(Damage damage);
    void OffsetPoise(int stagger);
    void OffsetRadiation(int radiation);
    void OffsetFrost(int frost);
    void SetHemorrhage(Hemorrhage level);
    void SetShock(bool newShocked);
    string GetActorID();
    float GetPoise();
    float GetRadiation();
    float GetFrost();
    Hemorrhage GetHemorrhage();
    bool GetShocked();
}

public enum Hemorrhage
{
    none,
    minor,
    major,
    size
}

public enum HitpointType
{
    health,
    armor,
    shields,
    none
}

[Serializable]
public struct Damage
{
    public float baseDamage;
    public float shieldMulti;
    public float armorMulti;
    public float healthMulti;
    public float critMulti;
    public int penetrationLevel;
    public bool isCrit;
}
