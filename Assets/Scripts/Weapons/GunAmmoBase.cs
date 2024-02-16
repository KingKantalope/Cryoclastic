using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class GunAmmoBase : MonoBehaviour, IGunComponent
{
    [Header("Actions to Inform Other Scripts")]
    [SerializeField] private UnityEvent StartReloadAction;
    [SerializeField] private UnityEvent UpdateAmmoAction;
    [SerializeField] private UnityEvent EndReloadAction;
    [SerializeField] private UnityEvent EmptyAction;

    [Header("Capacity Details")]
    [SerializeField] private int magSize;
    [SerializeField] private int maxReserves;
    [SerializeField] private bool plusOne;
    private int magCurrent;
    private int reserves;
    private bool isReloading;

    [Header("Main Reload Stats")]
    [SerializeField] private float mainReloadTime;
    [SerializeField] private float mainAddTime;
    [SerializeField] private bool mainIsPartial;
    [SerializeField] private int mainAddPerPartial;
    [SerializeField] private float mainPartAddTime;

    [Header("Alternate Reload Stats")]
    [SerializeField] private float altReloadTime;
    [SerializeField] private float altAddTime;
    [SerializeField] private bool altIsPartial;
    [SerializeField] private int altAddPerPartial;
    [SerializeField] private float altPartAddTime;

    private void Start()
    {
        magCurrent = magSize;
        reserves = maxReserves;
        UpdateAmmoAction?.Invoke();
    }

    public void OnReloadAction(InputAction.CallbackContext context)
    {
        if (context.performed && !isReloading)
        {
            StartCoroutine(Reload());
        }
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        if (magSize <= 0)
        {
            // tell other components that we are reloading
            StartReloadAction?.Invoke();

            // take initial reload delay
            yield return new WaitForSeconds(mainAddTime);

            // loop through partial adds
            if (mainIsPartial)
            {
                while (magCurrent < magSize)
                {
                    yield return new WaitForSeconds(mainPartAddTime);

                    // add to magazine;
                    magCurrent += mainAddPerPartial;
                    if (magCurrent > magSize)
                        magCurrent = magSize;

                    // tell other components that ammo has been added
                    UpdateAmmoAction?.Invoke();
                }
            }
            else
            {
                // reset magazine capacity
                magCurrent = magSize;

                // tell other components that ammo has been added
                UpdateAmmoAction?.Invoke();
            }

            yield return new WaitForSeconds(mainReloadTime - mainAddTime);

            EndReloadAction?.Invoke();
        }
        else
        {
            // tell other components that we are reloading
            StartReloadAction?.Invoke();

            // take initial reload delay
            yield return new WaitForSeconds(altAddTime);

            // loop through partial adds or add in total
            if (altIsPartial)
            {
                while (magCurrent < magSize)
                {
                    yield return new WaitForSeconds(altPartAddTime);

                    // add to magazine;
                    magCurrent += altAddPerPartial;
                    if (magCurrent > (plusOne ? (magSize + 1) : magSize))
                        magCurrent = (plusOne ? (magSize + 1) : magSize);

                    // tell other components that ammo has been added
                    UpdateAmmoAction?.Invoke();
                }
            }
            else
            {
                // reset magazine capacity
                magCurrent = (plusOne ? (magSize + 1) : magSize);


                // tell other components that ammo has been added
                UpdateAmmoAction?.Invoke();
            }

            yield return new WaitForSeconds(altReloadTime - altAddTime);
        }

        EndReloadAction?.Invoke();
    }

    public void OnShoot(int ammoSpent)
    {
        if (ammoSpent > magSize)
        {
            magSize = 0;
            EmptyAction?.Invoke();
        }
        else magSize -= ammoSpent;

        UpdateAmmoAction?.Invoke();
    }

    public void Equip()
    {

    }

    public void Stow()
    {
        if (isReloading)
        {
            StopCoroutine(Reload());
        }
    }
}
