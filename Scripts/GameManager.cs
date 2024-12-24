using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{

    private NetworkVariable<bool> playerHost = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> playerClient = new NetworkVariable<bool>(false);

    private NetworkVariable<bool> isHostsTurn = new NetworkVariable<bool>(true);

    [SerializeField]
    private Button[] numberButton;
    private int[] CHECKING = new int[25];
    
    private int COUNTER=1;
    
    private bool gameStarted=false;
    
    private int CompletePairsCounts = 0;
    private int comitPairCount;

    [SerializeField]
    private Image[] BINGO;

    [SerializeField] 
    private Text lastSelectedNumber;

    private bool isGameOver = false;

    [SerializeField]
    private Canvas gameOverCard;




    private void Start()
    {
        setListener();
        setupArray();
    }


    private void setupArray()
    {
        for (int i = 0; i <= 24; i++)
        {
            CHECKING[i] = -1;
        }
    }







    public void startGameBTN()
    {
        if(COUNTER>=25)
        {
            if (IsHost)
            {
                playerHost.Value = true;
            }
            else
            {
                tmpServerRpc();
            }
        }

        Debug.Log("Player 0 is ready = "+playerHost.Value);
    }





    


    


    private void setListener()
    {
        for (int i = 0; i < numberButton.Length; i++)
        {
            int index = i;
            numberButton[index].onClick.AddListener(() =>
            {
                if (COUNTER <= 25)
                {


                    if (!int.TryParse(numberButton[index].GetComponentInChildren<Text>().text, out int result))
                    {
                        numberButton[index].GetComponentInChildren<Text>().text = COUNTER.ToString();
                        //Debug.Log(COUNTER);
                        COUNTER++;
                    }
                    


                }

                if(gameStarted)
                {
                    onNumberSelected(index);
                }


            });
        }


        playerHost.OnValueChanged += isBothPlayerReady;
        playerClient.OnValueChanged += isBothPlayerReady;
    }


    private void isBothPlayerReady(bool oldValue,bool newValue)
    {
        if(playerHost.Value && playerClient.Value)
        {
            gameStarted = true;
        }
        Debug.Log("game started ="+ gameStarted);

    }

    private void onNumberSelected(int index)
    {
        //Debug.Log(numberButton[index].GetComponentInChildren<Text>().text);
        int selectedNumber = int.Parse(numberButton[index].GetComponentInChildren<Text>().text.ToString());

        if (isHostsTurn.Value && IsHost)
        {
            tmpClientRpc(selectedNumber);
            isHostsTurn.Value = false;
        }
        else if(!isHostsTurn.Value && !IsHost) 
        {
            tmpServerRpc(selectedNumber);
        }

    }



    [ServerRpc(RequireOwnership = false)]
    private void tmpServerRpc(int index=-1)
    {
        if (!gameStarted)
        {
            playerClient.Value = true;
            Debug.Log("Player 1 is ready = " + playerClient.Value);
            return;
        }

        tmpClientRpc(index);
        isHostsTurn.Value = true;
        
    }


    [ClientRpc]
    private void tmpClientRpc(int selctedNumber)
    {
        lastSelectedNumber.text= selctedNumber.ToString();
        Color color = new Color(0.30588f, 0.43529f, 0.40392f);
        for (int i = 0; i < 25; i++)
        {
            if (numberButton[i].GetComponentInChildren<Text>().text==selctedNumber.ToString())
            {
                numberButton[i].GetComponent<Image>().color = color;
                CHECKING[i] = 1;
                checkIfPlayerWon();
                break;              
            }
        }
        
    }


    [ServerRpc(RequireOwnership =false)]
    private void gameOverServerRPC(string v)
    {
        gameOverClientRPC(v);
    }

    [ClientRpc]
    private void gameOverClientRPC(string v)
    {
        Canvas gameOverCardobj = Instantiate(gameOverCard);

        if (IsServer)
        {
            gameOverCardobj.GetComponent<NetworkObject>().Spawn();
            gameObject.GetComponent<NetworkObject>().Despawn();
        }

        gameOverCardobj.GetComponentInChildren<Text>().text = v;
    }


    private void checkIfPlayerWon()
    {
        checkingDiagonalSelection();
        checkingHorizontalSelection();
        checkingVerticalSelection();
        comitPairCount = CompletePairsCounts;
        CompletePairsCounts = 0;

        for (int i = 0; i < comitPairCount; i++)
        {
            if (i <= 4)
            {
                Color color = new Color(0.30588f, 0.43529f, 0.40392f);
                BINGO[i].color = color;
            }
        }

        if (comitPairCount>=4)
        {
            isGameOver = true;
        }
    }



    public void bingo()
    {
    
        if (isGameOver)
        {
            if(IsHost)
            {
                gameOverClientRPC("Host won");
            }
            else if (!IsHost)
            {
                gameOverServerRPC("Client won");
            }
        }
    }




    private void checkingVerticalSelection()
    {
        

        for (int column = 0; column < 5; column++)
        {
            int checkingElement = 0;

            for (int row=0; row < 5; row++)
            {
                if (CHECKING[row*5+column]==1)
                {
                    checkingElement++;
                }
            }

            

            if (checkingElement == 5)
            {
                CompletePairsCounts++;
            }

        }

    }

    private void checkingHorizontalSelection()
    {
        for (int row = 0; row < 5; row++)
        {
            int checkingElement = 0;

            for (int column = 0; column < 5; column++)
            {
                if (CHECKING[row*5 + column] == 1)
                {
                    checkingElement++;
                }
            }

            

            if (checkingElement == 5)
            {
                CompletePairsCounts++;
            }

        }
    }


    private void checkingDiagonalSelection()
    {
        int checkingElement = 0;

        for (int i = 0; i < 5; i++)
        {

            if (CHECKING[i * 5 + i] == 1)
            {
                checkingElement++;
            }
        }

        if(checkingElement == 5)
        {
            CompletePairsCounts++;
        }

        checkingElement = 0;

        for(int i = 0;i < 5; i++)
        {
            if (CHECKING[i * 5 + (5 - i - 1)] == 1)
            {
                checkingElement++;
            }
        }
        if(checkingElement == 5)
        {
            CompletePairsCounts++;
        }
    }

     /*0  1  2  3  4
       5  6  7  8  9
      10 11 12 13 14 
      15 16 17 18 19
      20 21 22 23 24*/


    //4E6F67
}
