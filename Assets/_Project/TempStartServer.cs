using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class TempStartServer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<NetworkManager>().StartHost();
    }

}
