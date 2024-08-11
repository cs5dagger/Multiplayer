using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOverTime : MonoBehaviour
{
    public float LifeTime = 1.5f;
    
    // Start is called before the first frame update
    void Start()
    {
        Destroy(this.gameObject, LifeTime);
    }
}
