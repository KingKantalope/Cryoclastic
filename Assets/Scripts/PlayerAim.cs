using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAim : MonoBehaviour
{
    [Header("Basic Look Variables")]
    [SerializeField] private Transform camHolder;
    [SerializeField] private Vector2 mouseSensitivity;
    [SerializeField] private Vector2 gamepadSensitivity;
    private Vector2 mouseLook, gamepadLook;
    private float lookRotation;
    private float gamepadAcceleration;
    private float gamepadFriction, gamepadMagnetism;
    private float mouseFriction, mouseMagnetism;
    private bool stickAim;

    // stuff
    private Vector3 currentRotation;
    private Vector3 targetRotation;
    private Vector3 returnRotation;

    [Header("Recoil Stuff")]
    [SerializeField] private Transform zRotation;
    [SerializeField] private float snappiness;
    [SerializeField] private float returnSpeed;

    public void OnMouseLook(InputAction.CallbackContext context)
    {
        mouseLook = context.ReadValue<Vector2>();
    }

    public void OnGamepadLook(InputAction.CallbackContext context)
    {
        gamepadLook = context.ReadValue<Vector2>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        returnRotation = Vector3.zero;
    }

    void Update()
    {
        HandleRecoil();
    }

    private void LateUpdate()
    {
        GamepadLookAcceleration();
        AimMagnetism();
        Look();
    }

    private void HandleRecoil()
    {
        targetRotation = Vector3.Lerp(targetRotation, returnRotation, returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.deltaTime);
        zRotation.localRotation = Quaternion.Euler(new Vector3(0f,0f,currentRotation.z));
    }

    public void SetRecoilValues(float newSnappiness, float newReturnSpeed)
    {
        snappiness = newSnappiness;
        returnSpeed = newReturnSpeed;
    }

    public void AddRecoil(Vector3 recoil, Vector3 variance, float maxMagnitude)
    {
        targetRotation += new Vector3(
            Random.Range(recoil.x - variance.x, recoil.x + variance.x),
            Random.Range(recoil.y - variance.y, recoil.y + variance.y),
            Random.Range(recoil.z - variance.z, recoil.z + variance.z));

        Vector3 netRecoil = new Vector3(targetRotation.x - returnRotation.x, targetRotation.y - returnRotation.y,0f) ;
        
        if (netRecoil.magnitude > maxMagnitude)
        {
            returnRotation = new Vector3(targetRotation.x - (netRecoil.normalized.x * maxMagnitude), targetRotation.y - (netRecoil.normalized.y * maxMagnitude),0f);
        }
    }

    private void Look()
    {
        // prep recoil
        Vector3 netRecoil = targetRotation - currentRotation;
        Vector3 recoilRotation = new Vector3(0f,netRecoil.y,0f);

        // left/right inputs
        Vector3 mouseTurn = Vector3.up * mouseLook.x * mouseSensitivity.x * (1f - mouseFriction);
        Vector3 gamepadTurn = Vector3.up * gamepadLook.x * gamepadSensitivity.x * gamepadAcceleration * (1f - gamepadFriction);

        // turn
        camHolder.Rotate(mouseTurn + gamepadTurn + recoilRotation);

        // up/down inputs
        float mouseRotation = -mouseLook.y * mouseSensitivity.y * (1f - mouseFriction);
        float gamepadRotation = -gamepadLook.y * gamepadSensitivity.y * gamepadAcceleration * (1f - gamepadFriction);

        // look
        lookRotation += (mouseRotation + gamepadRotation + netRecoil.x);
        lookRotation = Mathf.Clamp(lookRotation, -90, 90);
        zRotation.eulerAngles = new Vector3(lookRotation, zRotation.eulerAngles.y, zRotation.eulerAngles.z);
    }

    /* Set up acceleration based on the magnitude and duration of gamepad aim input
     * Should not instantly reset when quickly changing from left to right or up to down and vice versa
     */
    private void GamepadLookAcceleration()
    {

    }

    public void AimMagnetism()
    {

    }

    public void UpdateAimAssist(float gpFriction, float gpMagnetism, float mFriction, float mMagnetism)
    {
        gamepadFriction = gpFriction;
        gamepadMagnetism = gpMagnetism;
        mouseFriction = mFriction;
        mouseMagnetism = mMagnetism;
    }
}