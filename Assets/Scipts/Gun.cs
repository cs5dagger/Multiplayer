using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public bool IsAutomatic;
    public float TimeBetweenShots = 0.1f, HeatPerShot = 1f;
    public GameObject MuzzleFlash;
    public int ShotDamage;
}
