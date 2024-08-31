using SBPScripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    public static PlayerCharacter Instance;

    bool isMounted;

    [SerializeField] GameObject visuals;
    [SerializeField] CharacterController characterController;

    [SerializeField] GameObject cameraGO;

    private void Awake()
    {
        Instance = this;
    }

    public void ToggleMount()
    {
        if(isMounted)
        {
            transform.SetParent(null);
        }
        else
        {
            foreach(Collider collider in Physics.OverlapSphere(transform.position, 2f))
            {
                if(collider.TryGetComponent(out BicycleController bike))
                {
                    GameManager.Instance.ActiveBike = bike;
                    bike.GetComponentInChildren<CyclistAnimController>().ToggleGetOnBike(this);
                    transform.SetParent(bike.transform);
                    return;
                }
            }
        }
    }

    public void ToggleThirdPersonMode(bool toggle)
    {
        visuals.SetActive(toggle);
        characterController.enabled = toggle;

    }
}
