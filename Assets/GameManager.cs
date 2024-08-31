using SBPScripts;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //active player
    //active bike

    public static GameManager Instance;

    

    [SerializeField] PlayerCharacter character;
    public PlayerCharacter Character { get { return character; } }

	private BicycleController activeBike;

    System.Action<BicycleController> OnBikeSwitch;
    private void Awake()
    {
        Instance = this;
    }
    public BicycleController ActiveBike
    {
		get { return activeBike; }
		set { 
            activeBike = value;
            OnBikeSwitch?.Invoke(activeBike); //null check mb
        }
	}



}
