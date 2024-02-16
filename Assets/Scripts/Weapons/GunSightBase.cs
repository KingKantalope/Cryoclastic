using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class GunSightBase : MonoBehaviour
{
    [Header("Actions to Inform Other Scripts")]
    [SerializeField] private UnityEvent ZoomSwitchAction;

    [Header("Class References")]
    [SerializeField] private GameObject something;

    [Header("Zooming Information")]
    [SerializeField] private float[] zoomLevels;
    [SerializeField] private float zoomDelay;
    private int level;
}
