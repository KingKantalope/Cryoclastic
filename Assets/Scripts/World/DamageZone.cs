using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DamageZone : MonoBehaviour
{
    private List<IDamageable> damageables;

    [Header("When to Deal Damage")]
    [SerializeField] private bool DamageOnEnter;
    [SerializeField] private bool DamageOnExit;
    [SerializeField] private bool DamageOnStay;

    [Header("Damage Zone Values")]
    [SerializeField] private Damage damage = new Damage();
    [SerializeField] private int stagger = 0;
    [SerializeField] private int radiation = 0;
    [SerializeField] private int frost = 0;
    [SerializeField] private Hemorrhage hemorrhage = Hemorrhage.none;
    [SerializeField] private bool shock = false;

    private void OnCollisionEnter(Collision collision)
    {
        IDamageable enteringDamageable = collision.gameObject.GetComponent<IDamageable>();

        if (!damageables.Contains(enteringDamageable))
        {
            damageables.Add(enteringDamageable);

            if (DamageOnEnter)
            {
                foreach (var damageable  in damageables)
                {
                    if (damage.baseDamage > 0f) damageable.MainDamage(damage);
                    if (stagger > 0) damageable.OffsetPoise(stagger);
                    if (radiation > 0) damageable.OffsetRadiation(radiation);
                    if (frost > 0) damageable.OffsetFrost(frost);
                    if (hemorrhage != Hemorrhage.none) damageable.SetHemorrhage(hemorrhage);
                    if (shock) damageable.SetShock(shock);
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        IDamageable leavingDamageable = collision.gameObject.GetComponent<IDamageable>();

        if (damageables.Contains(leavingDamageable))
        {
            damageables.Remove(leavingDamageable);

            if (DamageOnExit)
            {
                foreach (var damageable in damageables)
                {
                    if (damage.baseDamage > 0f) damageable.MainDamage(damage);
                    if (stagger > 0) damageable.OffsetPoise(stagger);
                    if (radiation > 0) damageable.OffsetRadiation(radiation);
                    if (frost > 0) damageable.OffsetFrost(frost);
                    if (hemorrhage != Hemorrhage.none) damageable.SetHemorrhage(hemorrhage);
                    if (shock) damageable.SetShock(shock);
                }
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (DamageOnStay)
        {
            foreach (var damageable in damageables)
            {
                if (damage.baseDamage > 0f) damageable.MainDamage(damage);
                if (stagger > 0) damageable.OffsetPoise(stagger);
                if (radiation > 0) damageable.OffsetRadiation(radiation);
                if (frost > 0) damageable.OffsetFrost(frost);
                if (hemorrhage != Hemorrhage.none) damageable.SetHemorrhage(hemorrhage);
                if (shock) damageable.SetShock(shock);
            }
        }
    }
}
