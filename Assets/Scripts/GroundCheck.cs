using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    [SerializeField] private PlayerMovement player;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player.gameObject)
            return;

        player.SetGrounded(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == player.gameObject)
            return;

        player.SetGrounded(false);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject == player.gameObject)
            return;

        player.SetGrounded(true);
    }
}
