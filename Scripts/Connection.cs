using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;


public class Connection : MonoBehaviour
{
    [SerializeField]
    private Canvas bingoCard;

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
        {
            Debug.Log("player "+id+" is connected");

            if(NetworkManager.Singleton.IsHost && NetworkManager.Singleton.ConnectedClientsList.Count > 1)
            {
                gameObject.GetComponent<NetworkObject>().Despawn();
                Canvas canvas = Instantiate(bingoCard);
                canvas.GetComponent<NetworkObject>().Spawn();
            }
        };
    }


    public async void start_Host()
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        GetComponentInChildren<Text>().text="Code:"+joinCode;
        NetworkManager.Singleton.StartHost();
    }

    public async void start_Client()
    {
        Debug.Log(GetComponentInChildren<InputField>().text);
        JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(GetComponentInChildren<InputField>().text);
        RelayServerData joinServerData = new RelayServerData(allocation,"dtls");
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(joinServerData);
        NetworkManager.Singleton.StartClient();
    }
}
