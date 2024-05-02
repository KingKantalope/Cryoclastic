using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HandheldGun : MonoBehaviour, IHandheldObject
{
    private CarrierSystem m_CarrierSystem;
    private Recoil m_Recoil;
    private CapsuleCollider m_MeleeCollider;
    private PlayerHUD m_PlayerHUD;
    private PlayerAim m_PlayerAim;

    #region variables

    [Header("Gun Animator")]
    [SerializeField] private Animator GunAnimator;

    [Header("HUD Elements")]
    [SerializeField] private GameObject ammoDisplayObject;
    [SerializeField] private GameObject reticleDisplayObject;
    [SerializeField] private Sprite displayImage;
    private HandheldDisplay ammoDisplay;
    private HandheldReticle reticleDisplay;
    private GameObject ammoObject;
    private GameObject reticleObject;

    [Header("Fire Mode")]
    [SerializeField] private bool isAutomatic;
    [SerializeField] private float attackDelay;
    [SerializeField] private float attackRate;
    [SerializeField] private int burstCount;
    [SerializeField] private float burstRate;
    private float attackTime;
    private int roundNum;

    [Header("Charging")]
    [SerializeField] private bool isChargeable;
    [SerializeField] private float minCharge;
    private float currentCharge;

    [Header("Accuracy and Recoil")]
    [SerializeField] private List<Vector3> recoilPattern;
    [SerializeField] private List<Vector3> recoilVariance;
    [SerializeField] private float maxRecoilReturn;
    [SerializeField] private float snappiness;
    [SerializeField] private float returnSpeed;
    [SerializeField] private float minSpread;
    [SerializeField] private float maxSpread;
    [SerializeField] private float spreadIncreasePerAttack;
    [SerializeField] private float spreadResetRate;
    private float spread;

    [Header("Ammo Consumption")]
    [SerializeField] private bool infiniteAmmo;
    [SerializeField] private int ammoPerAttack;

    [Header("Ammo Capacity")]
    [SerializeField] private int magSize;
    [SerializeField] private int startingMagSize;
    [SerializeField] private int maxReserves;
    [SerializeField] private int startingReserves;
    [SerializeField] private float lowThreshold;
    protected int magCurrent;
    protected int reservesCurrent;

    [Header("Reloading")]
    [SerializeField] private float mainAddTime;
    [SerializeField] private float mainRemainTime;
    [SerializeField] private float altAddTime;
    [SerializeField] private float altRemainTime;
    private float addTime;
    private float remainTime;
    private bool hasReloaded;

    [Header("Melee Stuff")]
    [SerializeField] private float meleeHitTime;
    [SerializeField] private float meleeHangTime;
    [SerializeField] private float meleeDamage;
    private float meleeTime;
    private bool hasMeleed;

    [Header("Equipping and Stowing")]
    [SerializeField] private float EquipDuration;
    [SerializeField] private float UnequipDuration;
    private float switchTime;

    [Header("Aim Assistance")]
    [SerializeField] private float redReticleRange;
    [SerializeField] private float maxAssistRange;
    [SerializeField] private float assistInnerAngle;
    [SerializeField] private float assistOuterAngle;
    [SerializeField] private float assistMaxFriction;
    [SerializeField] private float assistMaxMagnetism;
    [SerializeField] private float autoMaxAngle;

    private bool wantToAttack;
    private bool wantToAltAttack;
    private bool wantToCharge;
    private bool wantToReload;
    private bool wantToMelee;
    private bool isAttacking;
    private bool isReloading;
    private bool isEquipping;
    private bool isStowing;
    private bool isMeleeing;

    private int testMagCurrent = 8;
    private int testMagSize = 8;
    private float testLowThreshold = 0.4f;

    #endregion variables

    // Start is called before the first frame update
    void Start()
    {
        magCurrent = startingMagSize;
        reservesCurrent = startingReserves;
        isReloading = false;
        wantToAttack = false;
        wantToAltAttack = false;
        wantToReload = false;
        wantToMelee = false;
        isAttacking = false;
        isEquipping = false;
        isStowing = false;
        isMeleeing = false;

        if (infiniteAmmo)
        {
            ammoDisplay.SetResourceAmount(magCurrent, magSize, lowThreshold);
        }
        else
        {
            ammoDisplay.SetResourceAmount(magCurrent, magSize, lowThreshold, reservesCurrent);
        }
    }

    void Update()
    {
        // specific updates
        AttackUpdate();

        handleInput();
        //RedReticle();
    }

    void LateUpdate()
    {
        handleAccuracy();
    }

    protected virtual void handleInput()
    {
        // reloading and attacking
        if (!isReloading && !isEquipping && !isStowing && !isMeleeing)
        {
            if (!isAttacking && wantToAttack && magCurrent > 0)
            {
                handleTrigger();
            }

            if (wantToReload && magCurrent < magSize && reservesCurrent > 0)
            {
                if (isAttacking) StopCoroutine(AttackCoroutine());

                m_CarrierSystem.GetAnimator().SetBool("isReloading", true);
                GunAnimator.SetBool("isReloading", true);

                StartCoroutine(ReloadCoroutine());
            }

            if (wantToMelee)
            {
                if (isAttacking) StopCoroutine(AttackCoroutine());

                StartCoroutine(MeleeCoroutine());
            }
        }
    }

    protected virtual void handleTrigger()
    {
        if (isChargeable)
        {
            if (currentCharge < minCharge)
            {
                currentCharge += Time.deltaTime;
            }
            else
            {
                StartCoroutine(AttackCoroutine());

                currentCharge = 0f;
            }
        }
        else
        {
            StartCoroutine(AttackCoroutine());
        }
    }

    #region HandheldObject

    public void OnAttachedCarrier(CarrierSystem attachedCarrier)
    {
        m_CarrierSystem = attachedCarrier;
    }

    public void OnAttachedAim(PlayerAim attachedAim)
    {
        m_PlayerAim = attachedAim;

        m_PlayerAim.SetRecoilValues(snappiness, returnSpeed);
    }

    public void OnAttachedMeleeCollider(CapsuleCollider MeleeCollider)
    {
        m_MeleeCollider = MeleeCollider;
    }

    public void OnAttachedHUD(PlayerHUD playerHUD)
    {
        m_PlayerHUD = playerHUD;

        // playerHUD.OnAttachHandheldHUD(this, ammoDisplayObject, reticleDisplayObject, displayImage);

        Transform reticleParent, ammoParent;

        (reticleParent, ammoParent) = m_PlayerHUD.OnAttachHandheldHUDFIXED();

        reticleObject = Instantiate(reticleDisplayObject, reticleParent, false);
        ammoObject = Instantiate(ammoDisplayObject, ammoParent, false);

        reticleDisplay = reticleObject.GetComponent<HandheldReticle>();
        ammoDisplay = ammoObject.GetComponent<HandheldDisplay>();
    }

    public void OnAttachedDisplay(HandheldDisplay handheldDisplay)
    {
        //ammoDisplay = handheldDisplay;
    }

    public void OnAttachedReticle(HandheldReticle handheldReticle)
    {
        reticleDisplay = handheldReticle;
    }

    #region InputActions

    public void OnAltFire(InputAction.CallbackContext context)
    {
        if (context.performed) wantToAltAttack = true;
        else if (context.canceled) wantToAltAttack = false;
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.performed) wantToAttack = true;
        else if (context.canceled) wantToAttack = false;
    }

    public void OnJump(InputAction.CallbackContext context)
    {

    }

    public void OnLook(InputAction.CallbackContext context)
    {
        
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.performed) wantToReload = true;
        else if (context.canceled) wantToReload = false;
    }

    public void OnScrollThroughWeapons(InputAction.CallbackContext context)
    {
        
    }

    public void OnSwapWeapons(InputAction.CallbackContext context)
    {

    }

    public void OnMelee(InputAction.CallbackContext context)
    {
        if (context.performed) wantToMelee = true;
        else if (context.canceled) wantToMelee = false;
    }

    #endregion InputActions

    #endregion HandheldObject

    #region GunAttack

    /* AttackCoroutine accounts for the attack's attackDelay, attackRate,
     * burstCount, burstRate,
     */
    protected virtual void StartAttack()
    {
        isAttacking = true;
        roundNum = 0;
    }

    protected virtual void AttackUpdate()
    {
        // refactor coroutine functionality to work on update
        // this shouldn't be so taxing like all these coroutines are now
        if (attackDelay > 0f)
        {

        }
    }

    protected IEnumerator AttackCoroutine()
    {
        isAttacking = true;
        int roundNum = 0;

        // wait for delay if needed
        if (attackDelay <= 0f)
            yield return new WaitForSeconds(attackDelay);

        // attack pattern
        do
        {
            if (!isAutomatic) wantToAttack = false;

            // burst fire
            do
            {
                // perform attack
                Attack();

                // increase spread
                spread += spreadIncreasePerAttack;

                // call recoil
                m_PlayerAim.AddRecoil(recoilPattern[roundNum % recoilPattern.Count],recoilVariance[roundNum % recoilVariance.Count], maxRecoilReturn);

                // up round count
                roundNum++;

                // wait until next attack in burst
                yield return new WaitForSeconds(burstRate);

            } while (roundNum < burstCount && magCurrent > 0);

            // wait until next burst
            yield return new WaitForSeconds(attackRate);

        } while (wantToAttack && !isChargeable && magCurrent > 0);

        isAttacking = false;
    }

    /* Create a basic projectile spawning system,
     * the scriptable object will deal with multiple projectiles
     */
    protected virtual void Attack()
    {
        // spawn projectile

        // consume ammo
        ConsumeAmmo(ammoPerAttack);

        // play animation
        m_CarrierSystem.GetAnimator().SetTrigger("Attack");
        GunAnimator.SetTrigger("Attack");

        if (infiniteAmmo)
        {
            ammoDisplay.SetResourceAmount(magCurrent, magSize, lowThreshold);
        }
        else
        {
            ammoDisplay.SetResourceAmount(magCurrent, magSize, lowThreshold, reservesCurrent);
        }
    }

    #endregion

    #region GunSpread

    protected virtual void handleAccuracy()
    {
        spread = Mathf.Clamp(spread - (Time.deltaTime * spreadResetRate), minSpread, maxSpread);

        // update HUD
        reticleDisplay.SetReticleSpread(spread);
    }

    #endregion

    #region GunAmmo

    /* */
    protected virtual void ReloadUpdate()
    {
        // refactor coroutine functionality to work on update
        // this shouldn't be so taxing like all these coroutines are now
        if (addTime > 0f)
        {
            // wait to add ammo to mag
            addTime -= Time.deltaTime;
        }
        else if (!hasReloaded)
        {
            // add ammo
            Reload();
            hasReloaded = true;
        }
        else if (remainTime > 0f)
        {
            // wait to allow attack
            remainTime -= Time.deltaTime;
        }
        else
        {
            // end reload
            isReloading = false;
            // reset reloaded
            hasReloaded = false;
        }
    }

    protected virtual void StartReload()
    {

        isReloading = true;
        wantToAttack = false;
        wantToAltAttack = false;
        hasReloaded = false;

        addTime = mainAddTime;
        remainTime = mainRemainTime;
        bool isEmpty = false;

        if (magCurrent <= 0)
        {
            addTime = altAddTime;
            remainTime = altRemainTime;
            isEmpty = true;
        }

        // play animation
        m_CarrierSystem.GetAnimator().SetTrigger(isEmpty ? "ReloadNormal" : "ReloadEmpty");
        GunAnimator.SetTrigger(isEmpty ? "ReloadNormal" : "ReloadEmpty");
    }

    protected virtual IEnumerator ReloadCoroutine()
    {
        StopCoroutine(AttackCoroutine());
        isReloading = true;
        wantToAttack = false;
        wantToAltAttack = false;

        float addTime = mainAddTime;
        float remainTime = mainRemainTime;
        bool isEmpty = false;

        if (magCurrent <= 0)
        {
            addTime = altAddTime;
            remainTime = altRemainTime;
            isEmpty = true;
        }

        // play animation
        m_CarrierSystem.GetAnimator().SetTrigger(isEmpty ? "ReloadNormal" : "ReloadEmpty");
        GunAnimator.SetTrigger(isEmpty ? "ReloadNormal" : "ReloadEmpty");

        yield return new WaitForSeconds(addTime);

        // do stuff
        Reload();

        yield return new WaitForSeconds(remainTime);

        m_CarrierSystem.GetAnimator().SetBool("isReloading", false);
        GunAnimator.SetBool("isReloading", false);

        isReloading = false;
    }

    protected virtual void Reload()
    {
        if (infiniteAmmo)
        {
            magCurrent = magSize;
        }
        else if ((magCurrent + reservesCurrent) < magSize)
        {
            magCurrent += reservesCurrent;
            reservesCurrent = 0;
        }
        else
        {
            reservesCurrent -= (magSize - magCurrent);
            magCurrent = magSize;
        }

        if (infiniteAmmo)
        {
            ammoDisplay.SetResourceAmount(magCurrent, magSize, lowThreshold);
        }
        else
        {
            ammoDisplay.SetResourceAmount(magCurrent, magSize, lowThreshold, reservesCurrent);
        }
    }

    protected void ConsumeAmmo(int ammoAmount)
    {
        magCurrent -= ammoAmount;
        if (magCurrent < 0) magCurrent = 0;
        Debug.Log("magCurrent: " + magCurrent);
        
    }

    #endregion

    #region GunSwapping

    public void OnEquip()
    {
        m_CarrierSystem.GetAnimator().SetTrigger("Equip");
        GunAnimator.SetTrigger("Equip");

        isEquipping = true;
        switchTime = EquipDuration;

        // remove below once EquipUpdate() is implemented
        // stop any current coroutines
        if (isStowing)
        {
            StopCoroutine(UnequipCoroutine());
        }
        else if (isAttacking)
        {
            StopCoroutine(AttackCoroutine());
        }
        else if (isReloading)
        {
            StopCoroutine(ReloadCoroutine());
        }
        else if (isMeleeing)
        {
            StopCoroutine(MeleeCoroutine());
        }

        StartCoroutine(EquipCoroutine());
    }

    public void OnUnequip()
    {
        m_CarrierSystem.GetAnimator().SetTrigger("Unequip");
        GunAnimator.SetTrigger("Unequip");

        isStowing = true;
        switchTime = UnequipDuration;

        // remove below once UnequipUpdate() is implemented
        // stop any current coroutines
        if (isEquipping)
        {
            StopCoroutine(EquipCoroutine());
        }
        else if (isAttacking)
        {
            StopCoroutine(AttackCoroutine());
        }
        else if (isReloading)
        {
            StopCoroutine(ReloadCoroutine());
        }
        else if (isMeleeing)
        {
            StopCoroutine(MeleeCoroutine());
        }

        StartCoroutine(UnequipCoroutine());
    }

    private void EquipUpdate()
    {
        // refactor coroutine functionality to work on update
        // this shouldn't be so taxing like all these coroutines are now

        if (switchTime > 0f)
        {
            switchTime -= Time.deltaTime;
        }
        else
        {
            isEquipping = false;
        }
    }

    private IEnumerator EquipCoroutine()
    {
        isEquipping = true;

        yield return new WaitForSeconds(EquipDuration);

        isEquipping = false;
    }

    private void UnequipUpdate()
    {
        // refactor coroutine functionality to work on update
        // this shouldn't be so taxing like all these coroutines are now

        if (switchTime > 0f)
        {
            switchTime -= Time.deltaTime;
        }
        else
        {
            Destroy(reticleDisplay.gameObject);

            m_CarrierSystem.OnStow();

            isStowing = false;
        }
    }

    private IEnumerator UnequipCoroutine()
    {
        isStowing = true;

        yield return new WaitForSeconds(UnequipDuration);

        Destroy(reticleDisplay.gameObject);

        m_CarrierSystem.OnStow();

        isStowing = false;
    }

    #endregion

    #region GunMelee

    protected virtual void StartMelee()
    {
        wantToMelee = false;
        isMeleeing = true;
        hasMeleed = false;

        //animations
        m_CarrierSystem.GetAnimator().SetTrigger("Melee");
        GunAnimator.SetTrigger("Melee");

        meleeTime = meleeHitTime;
    }

    protected void MeleeUpdate()
    {
        if (meleeTime > 0f)
        {
            meleeTime -= Time.deltaTime;
        }
        else if (!hasMeleed)
        {
            Melee();
            hasMeleed = true;
            meleeTime = meleeHangTime;
        }
        else if (meleeTime > 0f)
        {
            meleeTime -= Time.deltaTime;
        }
        else
        {
            isMeleeing = false;
        }
    }

    protected IEnumerator MeleeCoroutine()
    {
        wantToMelee = false;

        isMeleeing = true;

        //animations
        m_CarrierSystem.GetAnimator().SetTrigger("Melee");
        GunAnimator.SetTrigger("Melee");

        yield return new WaitForSeconds(meleeHitTime);

        Melee();

        yield return new WaitForSeconds(meleeHangTime);

        isMeleeing = false;
    }

    protected void Melee()
    {
        // call for melee damage/physics
    }

    #endregion

    #region GunAimAssist

    /* Update this method to better 
     */
    private void RedReticle()
    {
        float gamepadFriction = 0f;
        float gamepadMagnetism = 0f;
        float mouseFriction = 0f;
        float mouseMagnetism = 0f;
        bool isEnemy = false;
        bool isFriendly = false;

        m_PlayerAim.UpdateAimAssist(gamepadFriction, gamepadMagnetism, mouseFriction, mouseMagnetism);
        reticleDisplay.SetReticleColor(isEnemy, isFriendly);
    }

    #endregion
}
