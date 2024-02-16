using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class GunAttackBase : MonoBehaviour, IGunComponent
{
    private bool isAttacking;

    [Header("Actions to Inform Other Scripts")]
    [SerializeField] private UnityEvent ShootAction;
    [SerializeField] private UnityEvent ChargeAction;
    private bool wantToAttack;

    [Header("Class References")]
    [SerializeField] private Transform ProjectilePoint;
    [SerializeField] private GameObject Projectile;

    [Header("Basic Attack Information")]
    [SerializeField] private float attackDelay;
    [SerializeField] private float attackRate;
    [SerializeField] private bool isAutomatic;
    private bool hasAmmo;
    private bool reloadInterrupting;

    [Header("Spread and Recoil")]
    [SerializeField] private float startingSpread;
    [SerializeField] private float spreadPerShot;
    [SerializeField] private float spreadDecayRate;
    [SerializeField] private Vector2[] projectilePattern;
    [SerializeField] private Vector2[] recoilPattern; // (recoil.x, recoil.y)
    private float spread;
    private int roundCount = 0;

    [Header("Burst Information")]
    [SerializeField] private bool isBurst;
    [SerializeField] private float burstDelay;
    [SerializeField] private int burstLength;

    private void Start()
    {
        wantToAttack = false;
    }

    public void OnAttackAction(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            wantToAttack = true;

            if (!isAttacking) StartCoroutine(Attack());
        }
        else if (context.canceled) wantToAttack = false;
    }
    
    private IEnumerator Attack()
    {
        isAttacking = true;

        // initial delay, whther open bolt or charged or whatever
        yield return new WaitForSeconds(attackDelay);

        // loop while the attack is happening
        while (wantToAttack && hasAmmo && !reloadInterrupting)
        {
            // reset trigger if not automatic
            if (!isAutomatic) wantToAttack = false;

            // for loop for burst
            for (int i = 0; i < burstLength; i++)
            {
                Shoot();
                ShootAction?.Invoke();
                roundCount++;

                yield return new WaitForSeconds(attackRate);

                // exit loop if no ammo
                if (!hasAmmo) break;
            }

            if (isBurst) yield return new WaitForSeconds(burstLength);
        }

        isAttacking = true;
    }

    /* Make sure that this properly fires each projectile in the pattern
     */
    protected virtual void Shoot()
    {
        // spawn and launch each projectile with correct relative rotation
        foreach (var pattern in projectilePattern)
        {
            
        }
    }

    public void OnUpdateAmmo(int ammoCurrent)
    {
        if (ammoCurrent <= 0)
            hasAmmo = false;
        else hasAmmo = true;
    }

    public void Stow()
    {

    }

    public void Equip()
    {

    }
}
