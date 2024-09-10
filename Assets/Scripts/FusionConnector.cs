using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class FusionConnector : MonoBehaviour
{
    public string LocalPlayerName { get; set; }

    public string LocalRoomName { get; set; }

    [SerializeField, Tooltip("The network runner prefab that will be instantiated when looking starting the game.")]
    private NetworkRunner _networkRunnerPrefab;

    [Tooltip("The canvas group that handles interactivity for the game.")]
    public CanvasGroup canvasGroup;

    [Tooltip("The GameObject that contains the main menu.")]
    public GameObject mainMenuObject;

    [Tooltip("The Game Object that handles the game itself")]
    public GameObject mainGameObject;

    [Tooltip("GameObject that appears if there is a network error when trying to join a room.")]
    public GameObject errorMessageObject;

    [Tooltip("The GameObject that displays the button to start the game.")]
    public GameObject showGameButton;

    [Tooltip("Text object that displays the room name.")]
    public TextMeshProUGUI roomName;

    [Tooltip("Prefab for the ASB game itself.")]
    public NetworkObject asbGamePrefab;

    public Transform[] playerContainer;
    public Transform playerCanvasContainer;

    [Tooltip("The message shown before starting the game.")]
    public TextMeshProUGUI preGameMessage;

    public static FusionConnector Instance { get; private set; }

    public bool isGameStarted = false; // Dichiarazione di isGameStarted

    // Modalità di gioco coperativo o competitivo (sincronizzata allo stesso valore per entrambi i player)
    [Networked, Tooltip("Syncronized Type of Gameplay")] 
    public bool isCollaborative {  get; set; }

    // Qui dobbiamo fare selezionare tramite tasti il livello per entrambi i giocatori (essendo una variabile Networked)
    // Il livello è uguale entrambi i player all'interno della stessa lobby.
    // Poi dovremmo fare lo slider (prefab già datoci da Vera), e tutti gli altri bottoni quali punti bonus e velocità di spawn.
    // Dopodiché possiamo definire questa piccola parte conclusa
    [Networked]
    public int level { get; set; }

    // Frequenza di spawn dei prefab all'interno della scena di gioco
    [Networked, Tooltip("Frequenza di spawn")]
    public int stimulusLenght { get; set; }

    public int bonus;
    public bool isBonus;
    public GameObject bonusnumbers;
    public int n;

    private void Awake()
    {
        Application.targetFrameRate = 144;

        if (Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public async void StartGame(bool joinRandomRoom)
    {
        canvasGroup.interactable = false;

        StartGameArgs startGameArgs = new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = joinRandomRoom ? string.Empty : LocalRoomName,
            PlayerCount = 20,
        };

        NetworkRunner newRunner = Instantiate(_networkRunnerPrefab);

        StartGameResult result = await newRunner.StartGame(startGameArgs);

        if (result.Ok)
        {
            roomName.text = "Room:  " + newRunner.SessionInfo.Name;

            GoToGame();
            isGameStarted = true;
        }
        else
        {
            roomName.text = string.Empty;

            GoToMainMenu();

            errorMessageObject.SetActive(true);
            TextMeshProUGUI gui = errorMessageObject.GetComponentInChildren<TextMeshProUGUI>();
            if (gui)
                gui.text = result.ErrorMessage;

            Debug.LogError(result.ErrorMessage);
        }

        canvasGroup.interactable = true;
    }

    public void GoToMainMenu()
    {
        mainMenuObject.SetActive(true);
        mainGameObject.SetActive(false);
        isGameStarted = false;
    }

    public void GoToGame()
    {
        mainMenuObject.SetActive(false);
        mainGameObject.SetActive(true);
    }

    internal void OnPlayerJoin(NetworkRunner runner)
    {
        // Only set pregame messages if the game hasn't started.
        if (ASBManager.ASBManagerPresent)
        {
            return;
        }

        if (runner.IsSharedModeMasterClient == true)
        {
            SetPregameMessage("Game Is Ready To Start");
        }
        else
        {
            SetPregameMessage("Waiting for master client to start game.");
        }
    }
    public void SetPregameMessage(string message)
    {
        preGameMessage.text = message;
    }

    public void StartASBGame()
    {
        NetworkRunner runner = null;
        // If no runner has been assigned, we cannot start the game
        if (NetworkRunner.Instances.Count > 0)
        {
            runner = NetworkRunner.Instances[0];
        }

        if (runner == null)
        {
            Debug.Log("No runner found.");
            return;
        }

        // If no ASB manager has been made and we are the master mode client.
        // Redundant but being safe.
        if (runner.IsSharedModeMasterClient && !ASBManager.ASBManagerPresent)
        {
            runner.Spawn(asbGamePrefab);

            showGameButton.SetActive(false);
        }
    }

    public void SetIsCollaborativeTrue()
    {
        // L'utente ha cliccato su modalità Cooperativa
        isCollaborative = true;
        Debug.LogError("Collaborative Mode Activated");
    }

    public void SetIsCollaborativeFalse()
    {
        // L'utente ha cliccato su modalità competitiva
        isCollaborative = false;
        Debug.LogError("Competitive Mode Activated");
    }

    public void SetLevel1()
    {
        level = 1;
        Debug.LogError("Level " + level);
    }

    public void SetLevel2()
    {
        level = 2;
        Debug.LogError("Level " + level);
    }

    public void SetLevel3()
    {
        level = 3;
        Debug.LogError("Level " + level);
    }

    public void SetFrequency5()
    {
        stimulusLenght = 5;
        Debug.LogError("Frequency: 5 seconds");
    }

    public void SetFrequency10()
    {
        stimulusLenght = 10;
        Debug.LogError("Frequency: 10 seconds");
    }

    public void SetFrequency12()
    {
        stimulusLenght = 12;
        Debug.LogError("Frequency: 12 seconds");
    }

    public void SetFrequency15()
    {
        stimulusLenght = 15;
        Debug.LogError("Frequency: 15 seconds");
    }

    public void BonusYes()
    {
        isBonus = true;
        bonusnumbers.SetActive(true);
    }
    public void BonusNo()
    {
        isBonus = false;
        bonusnumbers.SetActive(false);
    }
    public void Bonus(int n)
    {
        bonus = n;
    }
}
