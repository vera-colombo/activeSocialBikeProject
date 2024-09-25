using Fusion;
using Photon.Voice;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FusionConnector : MonoBehaviour
{
    public string LocalPlayerName { get; set; }

    public string LocalRoomName { get; set; }
    public Dictionary<string, SessionProperty> customProps = new Dictionary<string, SessionProperty>();
    public string LocalScenario { get; set; }

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
    [Tooltip("The GameObject that displays the settings of the game.")]
    public GameObject gameSettingsPanel;

    [Tooltip("The GameObject that displays the personal settings of the game.")]
    public GameObject mySettingsPanel;

    [Tooltip("Text object that displays the room name.")]
    public TextMeshProUGUI roomName;

    [Tooltip("Prefab for the ASB game itself.")]
    public NetworkObject asbGamePrefab;

    public Transform[] playerContainer;
    public EditorPathScripts[] park_pathContainer;
    public GameObject park_pathContainerObj;
    public EditorPathScripts[] city_pathContainer;
    public GameObject city_pathContainerObj;
    public GameObject park_Scenario;
    public GameObject city_Scenario;
    public Transform playerCanvasContainer;

    [Tooltip("The message shown before starting the game.")]
    public TextMeshProUGUI preGameMessage;

    public static FusionConnector Instance { get; private set; }

    public bool isGameStarted = false; // Dichiarazione di isGameStarted

    // Modalità di gioco coperativo o competitivo (sincronizzata allo stesso valore per entrambi i player)
    [Networked, Tooltip("Syncronized Type of Gameplay")]
    public bool isCooperative { get; set; }

    // Qui dobbiamo fare selezionare tramite tasti il livello per entrambi i giocatori (essendo una variabile Networked)
    // Il livello è uguale entrambi i player all'interno della stessa lobby.
    // Poi dovremmo fare lo slider (prefab già datoci da Vera), e tutti gli altri bottoni quali punti bonus e velocità di spawn.
    // Dopodiché possiamo definire questa piccola parte conclusa
    [Networked,Tooltip("Livello")]
    public int level { get; set; }

    // Frequenza di spawn dei prefab all'interno della scena di gioco
    [Networked, Tooltip("Frequenza di spawn")]


    public int frequency { get; set; }

    public int bonus;
    public bool isBonus;
    public GameObject bonusnumbers;
    public int workload;
    public Slider workloadSlider;
    public TextMeshProUGUI infotext;
    // numero di punti bonus
    public int n;

    // InputField per lunghezza gioco
    [Tooltip("Lunghezza di gioco")]
    public TMP_InputField duratafield;

    // InputField per lunghezza turno
    [Tooltip("durata del turno")]
    public TMP_InputField durataturnifield;

    public int durata;
    public int durata_turni;

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
        if (LocalScenario != null)
        {
            canvasGroup.interactable = false;
            customProps["scenario"] = LocalScenario;
            StartGameArgs startGameArgs = new StartGameArgs()
            {
                GameMode = GameMode.Shared,
                SessionName = joinRandomRoom ? string.Empty : LocalRoomName,
                SessionProperties = customProps,
                PlayerCount = 2,
            };

            NetworkRunner newRunner = Instantiate(_networkRunnerPrefab);

            StartGameResult result = await newRunner.StartGame(startGameArgs);

            if (result.Ok)
            {
                roomName.text = "Room:  " + newRunner.SessionInfo.Name;

                GoToGame(newRunner.SessionInfo.Properties["scenario"]);
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
        
      //  SettingsPanel.SetActive(true);
    }

    public void SetScenario(string _s) 
    {
        LocalScenario = _s;
    }

    public void GoToMainMenu()
    {
        mainMenuObject.SetActive(true);
        mainGameObject.SetActive(false);
        isGameStarted = false;
    }

    public void GoToGame(string _scenario)
    {
        mainMenuObject.SetActive(false);
        mainGameObject.SetActive(true);
        ActivateScenario(_scenario);
    }
    public void ActivateScenario(string _scenario)
    {
        //Debug.LogError("I am " + ASBPlayer.LocalPlayer.PlayerName + "- ActivateScenarioRPC ");

        
        if (_scenario == "Park")
        {
            park_pathContainerObj.SetActive(true);
            park_Scenario.SetActive(true);
        }
        else
        {
            city_pathContainerObj.SetActive(true);
            city_Scenario.SetActive(true);
        }
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
                gameSettingsPanel.SetActive(false);

                // showGameButton.SetActive(false);
            }
        

    }
 

    public void Exit()
    {
        if (NetworkRunner.Instances[0].IsSharedModeMasterClient == true)
        {
            infotext.text = "Premi play per partire";
        }
        else
        {
            gameSettingsPanel.SetActive(false);
        }
    }
    public void OnCooperativeEnter()
    {
        isCooperative = true;
    }
    public void OnCompetitiveEnter()
    {
        isCooperative = false;
    }
    public void OnLvlEnter(int l)
    {
        level = l;
    }
    public bool isReallyCooperative()
    {
        return isCooperative;
    }
    public void Frequency(int f)
    {
        frequency = f;
    }
    public void isReallyBonus()
    {
        isBonus = !isBonus;
        bonusnumbers.SetActive(isBonus);
    }
    public void Bonus(int n)
    {
        bonus = n;
    }

    public void Workload() 
    {
        workload = (int)workloadSlider.value * 10;
    }
    public void DurataArea()
    {
        try
        {
            durata = int.Parse(duratafield.text)*60;
        }
        catch (Exception e)
        {
            infotext.text = "Numero non valido";
            duratafield.text = "";
        }
    }
    public void TurniArea()
    {
        try
        {
            durata_turni = int.Parse(durataturnifield.text);
        }
        catch (Exception ex)
        {
            infotext.text = "Numero non valido";
            durataturnifield.text = "";
        }
    }
}
