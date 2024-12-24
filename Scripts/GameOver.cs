using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameOver : NetworkBehaviour
{
    [SerializeField]
    private Canvas canvas;
    public void restart()
    {
        if (IsHost)
        {
            Canvas bingoCard = Instantiate(canvas);
            GetComponent<NetworkObject>().Despawn();
            bingoCard.GetComponent<NetworkObject>().Spawn();
            
        }
        
    }
}
