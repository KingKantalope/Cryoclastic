using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    HitpointType MainDamage(float baseDamage, float shieldMulti, float armorMulti, float healthMulti, float critMulti, int penetrationLevel, bool isCrit);
    void OffsetPoise(int stagger);
    void OffsetRadiation(int radiation);
    void OffsetFrost(int frost);
    void SetHemorrhage(Hemorrhage level);
    void SetShock(bool newShocked);
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