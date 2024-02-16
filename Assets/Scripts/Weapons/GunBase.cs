using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class GunBase : MonoBehaviour
{
    /* GunBase needs to act as an intermediary between this weapon and the player
     * Functionality is limited to managing input and swap/equip state
     */
    [Header("Actions to Assign to Gun Components")]
    [SerializeField] private UnityEvent PrimaryAction;
    [SerializeField] private UnityEvent SecondaryAction;
    [SerializeField] private UnityEvent ReloadAction;
    [SerializeField] private UnityEvent Interaction;
    [SerializeField] private UnityEvent DrawAction;
    [SerializeField] private UnityEvent EquipAction;
    [SerializeField] private UnityEvent StowAction;
    [SerializeField] private UnityEvent UnequipAction;
    private GunState state = GunState.Unequipped;

    [Header("Draw and Stow Info")]
    [SerializeField] private float drawTime;
    [SerializeField] private float stowTime;

    public void OnPrimaryAction(InputAction.CallbackContext context)
    {
        if (state == GunState.Equipped) PrimaryAction?.Invoke();
    }

    public void OnSecondaryAction(InputAction.CallbackContext context)
    {
        if (state == GunState.Equipped) SecondaryAction?.Invoke();
    }

    public void OnReloadAction(InputAction.CallbackContext context)
    {
        if (state == GunState.Equipped) ReloadAction?.Invoke();
    }

    public void OnInteraction(InputAction.CallbackContext context)
    {
        if (state == GunState.Equipped) Interaction?.Invoke();
    }

    /* Do problems occur when stopping a nonexistent coroutine?
     * Google that if this causes problems
     */
    public void OnSwapAction(InputAction.CallbackContext context)
    {
        switch (state)
        {
            case GunState.Drawing:
                StopCoroutine(Draw());
                StartCoroutine(Stow());
                break;
            case GunState.Equipped:
                StartCoroutine(Stow());
                break;
            case GunState.Stowing:
                StopCoroutine(Stow());
                StartCoroutine(Draw());
                break;
            case GunState.Unequipped:
                StartCoroutine(Draw());
                break;
        }
    }

    private IEnumerator Draw()
    {
        // Tell other components about drawing
        state = GunState.Drawing;
        DrawAction?.Invoke();

        yield return new WaitForSeconds(drawTime);

        // Tell other components about equipping
        state = GunState.Equipped;
        EquipAction?.Invoke();
    }

    private IEnumerator Stow()
    {
        // Tell other components about stowing
        state = GunState.Stowing;
        StowAction?.Invoke();

        yield return new WaitForSeconds(stowTime);

        // Tell other components about unequipping
        state = GunState.Unequipped;
        UnequipAction?.Invoke();
    }
}

public enum GunState
{
    Equipped,
    Stowing,
    Unequipped,
    Drawing,
    Size
}