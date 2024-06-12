using Fusion;
using Photon.Voice;
using System.Collections.Generic;
using TMPro;

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Singleton network behavoir that manages the game and is managed by the shared mode master client.
/// </summary>
public class ASBManager : NetworkBehaviour, IStateAuthorityChanged
{
    [Tooltip("The current task stimuli.")]
    public List<NetworkObject> asbStimuliList1p;
    [Tooltip("The current task stimuli.")]
    public List<NetworkObject> asbStimuliList2p;
    [Tooltip("The current task stimuli.")]
    public List<NetworkObject> asbStimuliList3p;
    [Tooltip("The current task stimuli.")]
    public List<NetworkObject> asbStimuliList1c;
    [Tooltip("The current task stimuli.")]
    public List<NetworkObject> asbStimuliList2c;
    [Tooltip("The current task stimuli.")]
    public List<NetworkObject> asbStimuliList3c;
    [Tooltip("The current task stimuli.")]
    public List<int> asbStimuliList1pIds; // Equals to 0
    [Tooltip("The current task stimuli.")]
    public List<int> asbStimuliList2pIds; // Equals to 1
    [Tooltip("The current task stimuli.")]
    public List<int> asbStimuliList3pIds; // Equals to 2
    [Tooltip("The current task stimuli.")]
    public List<int> asbStimuliList1cIds; // Equals to 3
    [Tooltip("The current task stimuli.")]
    public List<int> asbStimuliList2cIds; // Equals to 4
    [Tooltip("The current task stimuli.")]
    public List<int> asbStimuliList3cIds; // Equals to 5
    [Tooltip("Container for the stimulus element")]
    public GameObject stimuliContainerObj = null;

    [Tooltip("Stimuli obj position right")]
    public Transform stimuliObjRight;

    [Tooltip("Stimuli obj position left")]
    public Transform stimuliObjLeft;

    #region Networked Properties
    [Networked, Tooltip("Timer used for showing stimuli and transitioning between different states.")]
    public TickTimer stimTimer { get; set; }

    // Turn Timer for Players (now only a timer, then check the input from all players)
    [Networked, Tooltip("Timer used for player turns.")]
    public TickTimer turnTimer { get; set; }

    [Networked]
    public float turnTimerLength { get; set; }

    // Bool variable for Player turns
    [SerializeField, Networked]
    public bool AChiTocca { get; set; }

    // Bool variable for singleplayer or multiplauer
    [Networked, Tooltip("Bool variable checking if singleplayer or multiplayer")]
    public bool isCollaborative { get; set; }

    [Networked, Tooltip("Level.")]
    public int level { get; set; }//numero tra 1 e 3 (compresi)

    [Networked, Tooltip("Timer used for the whole game.")]
    public TickTimer gameTimer { get; set; }

    [Networked, Tooltip("The length of the timer, which corresponds to the duration of the game.")]
    public float gameTimerLength { get; set; }

    [Networked, Tooltip("The time between stimuli.")]
    private float stimTimerLength { get; set; }

    // TODO modify this with the cycling speed
    [Networked, Tooltip("The target speed.")]
    public float targetSpeed { get; set; }

    [Tooltip("The index of the current stimulus.  -1 means no stimulus is being currently shown.")]
    [Networked, OnChangedRender(nameof(UpdateCurrentStimulus))]
    public int CurrentStimulus { get; set; } = -1;

    [Tooltip("The number of stimuli shown so far.")]
    [Networked, OnChangedRender(nameof(UpdateStimuliShownText))]
    public int StimuliShown { get; set; } = 0;

    [Networked, Tooltip("The current stimuli position.")]
    public int currStimuliPos { get; set; }
    [Networked, Tooltip("Random index for Prefabs")]
    public int randomIndex { get; set; }
    [Networked]
    public int selectedTargetPrefabIndex { get; set; }

    [Networked, Tooltip("Current target")]
    public string currentTargetType { get; set; }
    [Tooltip("The current state of the game.")]
    [Networked, OnChangedRender(nameof(OnASBGameStateChanged))]
    public ASBStateGame GameState { get; set; } = ASBStateGame.Intro;

    // We have to make it [Networked] to show to the players the objects
    //private int[,] ArrayStartEndLists = new int[2, 6];

    // Networked Array for Spawning prefabs
    [Networked, Capacity(7)]
    public NetworkArray<int> ArrayStartEndLists => default;

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void DealFeedbackRPC()
    {
        // The code inside here will run on the client which owns this object (has state and input authority).
        Debug.Log("Received DealFeedbackRPC on StateAuthority, modifying Networked variable Game state to feedback");
        GameState = ASBStateGame.ShowFeedback;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void ActivateStimulusRPC()
    {
        //Debug.LogError("Received ActivateStimulusRPC on StateAuthority, spawning network object");
        NetworkObject currTarget;
        Transform targetParent;

        if (currStimuliPos == 0)
        {
            targetParent = stimuliObjRight;
        }
        else
        {
            targetParent = stimuliObjLeft;
        }

        //targetPrefab.gameObject.SetActive(true);
        /*
        int selectedTargetPrefabIndex;
        Debug.LogError("I am " + ASBPlayer.LocalPlayer.PlayerName + "and randomIndex is " + randomIndex.ToString());
        if (randomIndex != -1)
        {
            selectedTargetPrefabIndex = TargetPrefabIdxList[randomIndex];
        }
        else
        {
            selectedTargetPrefabIndex = -1;
        }
        */
        string outputString = "";

        // Loop to spawn all prefabs into the scene
        for (int i = 0; i < TargetPrefabIdxList.Count; i++)
        {
            outputString = outputString + TargetPrefabIdxList[i].ToString() + ", ";
        }
        Debug.LogError("I am " + ASBPlayer.LocalPlayer.PlayerName + " and target prefab index is " + selectedTargetPrefabIndex + " target list " + outputString);
        NetworkObject selectedTargetPrefab = null;
        //for (int i = 0; i < TargetPrefabIdxList.Count; i++)
        //{
        
        // Use a Networked Array to spawn Prefabs in the Scene
        if (selectedTargetPrefabIndex >= ArrayStartEndLists[0] && selectedTargetPrefabIndex < ArrayStartEndLists[1])
        {
            selectedTargetPrefab = asbStimuliList1p[selectedTargetPrefabIndex];
        }
        else if (selectedTargetPrefabIndex >= ArrayStartEndLists[1] && selectedTargetPrefabIndex < ArrayStartEndLists[2])
        {
            selectedTargetPrefab = asbStimuliList2p[selectedTargetPrefabIndex - asbStimuliList1p.Count];
        }
        else if (selectedTargetPrefabIndex >= ArrayStartEndLists[2] && selectedTargetPrefabIndex < ArrayStartEndLists[3])
        {
            selectedTargetPrefab = asbStimuliList3p[selectedTargetPrefabIndex - (asbStimuliList1p.Count + asbStimuliList2p.Count)];
        }
        else if (selectedTargetPrefabIndex >= ArrayStartEndLists[3] && selectedTargetPrefabIndex < ArrayStartEndLists[4])
        {
            selectedTargetPrefab = asbStimuliList1c[selectedTargetPrefabIndex - (asbStimuliList1p.Count + asbStimuliList2p.Count + asbStimuliList3p.Count)];
        }
        else if (selectedTargetPrefabIndex >= ArrayStartEndLists[4] && selectedTargetPrefabIndex < ArrayStartEndLists[5])
        {
            selectedTargetPrefab = asbStimuliList2c[selectedTargetPrefabIndex - (asbStimuliList3p.Count + asbStimuliList2p.Count + asbStimuliList1p.Count + asbStimuliList1c.Count)];
        }
        else if (selectedTargetPrefabIndex >= ArrayStartEndLists[5] && selectedTargetPrefabIndex < ArrayStartEndLists[6])
        {
            selectedTargetPrefab = asbStimuliList3c[selectedTargetPrefabIndex - (asbStimuliList3p.Count + asbStimuliList2p.Count + asbStimuliList1p.Count + asbStimuliList1c.Count + asbStimuliList2c.Count)];
        }
        // Spawn the new object into the scene
        currTarget = Runner.Spawn(selectedTargetPrefab, targetParent.position);
        // Set the object visible to the player
        currTarget.gameObject.SetActive(true);
        
        // Set the tag to the new spawned object
        currentTargetType = currTarget.gameObject.tag;
        //Print a Debug Message (not visible in the build, although in the Developer Version)
        Debug.LogError("I am " + ASBPlayer.LocalPlayer.PlayerName + " and target prefab is " + currentTargetType + " " + currTarget.gameObject.name);
        // Print on the console the lenght of the array for Target Pregabs (actually 10)
        Debug.LogError("TargetPrefabIndexList.Count = " + TargetPrefabIdxList.Count.ToString());
        //}
    }

    #endregion

    #region UI ELEMENTS

    /// <summary>
    /// Question, answer, and answer highlights
    /// </summary>
    public TextMeshProUGUI stimulus;
    public TextMeshProUGUI gameTimerText, stimTimerText, turnTimerText;
    public Image[] answerHighlights;

    /// <summary>
    /// Text message shown when the game changes state.
    /// </summary>
    public TextMeshProUGUI asbMessage;

    public TextMeshProUGUI stimuliShownText;

    [Tooltip("Button displayed to leave the game after a round ends.")]
    public GameObject leaveGameBtn;

    [Tooltip("Button displayed, only to the master client, to start a new game.")]
    public GameObject startNewGameBtn;

    // TODO modify this to have an array (Done)
    [Networked, Capacity(100)]
    public NetworkLinkedList<int> TargetPrefabIdxList => default;

    #endregion

    [Header("Game Rules")]
    [Tooltip("The maximum number of stimuli.")]
    [Min(1)]
    public float maxStimuli; // this could be remove in the final version because the game stops when the duration expires

    [Tooltip("The amount of time the stimulus will be shown.")]
    public float stimulusLength = 30;

    [Tooltip("The minimum number of points earned for getting a question correct")]
    private int pointsClicked = 0;

    #region SFX
    [Header("SFX Audio Sources")]
    [SerializeField, Tooltip("AudioSource played when the local player gets an answer correct.")]
    private AudioSource _correctSFX;

    [SerializeField, Tooltip("AudioSource played when the local player gets an answer incorrect.")]
    private AudioSource _incorrectSFX;

    [SerializeField, Tooltip("AudioSource played when the local player selects an answer.")]
    private AudioSource _confirmSFX;

    [SerializeField, Tooltip("AudioSource played when the local player selects an answer.")]
    private AudioSource _errorSFX;
    #endregion

    /// <summary>
    /// Has a asb manager been made; set to true on spawn and false on despawn
    /// </summary>
    public static bool ASBManagerPresent { get; private set; } = false;
    /*void Start()
    {
        asbStimuliList.Add(new GameObject());
    }
    */
    /// <summary>
    /// The different states of the ABS game.  Made as a byte since there are not that many.
    /// </summary>
    public enum ASBStateGame : byte
    {
        Intro = 0,
        ShowStimulus = 1,
        ShowFeedback = 2,
        GameOver = 3,
        NewRound = 4,
    }

    public override void Spawned()
    {
        // Disallows players from joining once the game is started.
        if (Runner.IsSharedModeMasterClient)
        {
            Runner.SessionInfo.IsOpen = false;
            Runner.SessionInfo.IsVisible = false;
        }

        //// If we have state authority, we set an intro time and randomized the question list.
        if (HasStateAuthority)
        {
            // Sets an initial intro timer
            // The initial timer for the game is only 3 seconds.
            stimTimerLength = 3;
            stimTimer = TickTimer.CreateFromSeconds(Runner, stimTimerLength);
            if(isCollaborative)
                turnTimer = TickTimer.CreateFromSeconds(Runner, stimTimerLength);
            gameTimer = TickTimer.CreateFromSeconds(Runner, gameTimerLength);

            CreateASBIndexLists();

            ShuffleStimuli();
        }

        ASBManagerPresent = true;

        FusionConnector.Instance?.SetPregameMessage(string.Empty);

        OnASBGameStateChanged();
        UpdateCurrentStimulus();
        UpdateStimuliShownText();
    }

    /// <summary>
    /// Shuffles the questions around.
    /// </summary>
    
    private void CreateASBIndexLists()
    {
        int lunghezza = 0;

        // Clear all the lists
        asbStimuliList1cIds.Clear();
        asbStimuliList2cIds.Clear();
        asbStimuliList3cIds.Clear();
        asbStimuliList1pIds.Clear();
        asbStimuliList2pIds.Clear();
        asbStimuliList3pIds.Clear();

        ArrayStartEndLists.Set(0, lunghezza);

        for (int i = 0; i < asbStimuliList1p.Count; i++)
        {
            asbStimuliList1pIds.Add(i);
        }
        lunghezza += asbStimuliList1p.Count;
        ArrayStartEndLists.Set(1, lunghezza);

        for (int i = 0; i < asbStimuliList2p.Count; i++)
        {
            asbStimuliList2pIds.Add(i + lunghezza);
        }
        lunghezza += asbStimuliList2p.Count;
        ArrayStartEndLists.Set(2, lunghezza);
        for (int i = 0; i < asbStimuliList3p.Count; i++)
        {
            asbStimuliList3pIds.Add(i + lunghezza);
        }
        lunghezza += asbStimuliList3p.Count;
        ArrayStartEndLists.Set(3, lunghezza);
        for (int i = 0; i < asbStimuliList1c.Count; i++)
        {
            asbStimuliList1cIds.Add(i + lunghezza);
        }
        lunghezza += asbStimuliList1c.Count;
        ArrayStartEndLists.Set(4, lunghezza);
        for (int i = 0; i < asbStimuliList2c.Count; i++)
        {
            asbStimuliList2cIds.Add(i + lunghezza);
        }
        lunghezza += asbStimuliList2c.Count;
        ArrayStartEndLists.Set(5, lunghezza);
        for (int i = 0; i < asbStimuliList3c.Count; i++)
        {
            asbStimuliList3cIds.Add(i + lunghezza);
        }
        // We can also don't do that.
        lunghezza += asbStimuliList3c.Count;
        ArrayStartEndLists.Set(6, lunghezza);
    }

    private void ShuffleStimuli()
    {
        Debug.Log("SHUFFLING STIMULI!");
        // Creates a temp list, adding every index avaiable
        if (level == 1)
        {
            int index = Random.Range(0, 3);
            if (index == 0)
            {
                For(asbStimuliList1pIds);
            }
            else if (index == 1)
            {
                For(asbStimuliList2pIds);
            }
            else
            {
                For(asbStimuliList3pIds);
            }
        }
        else if (level == 2)
        {
            int index = Random.Range(0, 3);
            if (index == 0)
            {
                For(asbStimuliList1pIds);
                index = Random.Range(0, 2);
                if (index == 0)
                {
                    For(asbStimuliList3pIds);
                }
                else
                {
                    For(asbStimuliList2pIds);
                }
            }
            else if (index == 1)
            {
                For(asbStimuliList2pIds);
                index = Random.Range(0, 2);
                if (index == 0)
                {
                    For(asbStimuliList1pIds);
                }
                else
                {
                    For(asbStimuliList3pIds);
                }
            }
            else
            {
                For(asbStimuliList3pIds);
                index = Random.Range(0, 2);
                if (index == 0)
                {
                    For(asbStimuliList2pIds);
                }
                else
                {
                    For(asbStimuliList3pIds);
                }
            }
        }
        else if (level == 3)
        {
            For(asbStimuliList1pIds);
            For(asbStimuliList2pIds);
            For(asbStimuliList3pIds);
        } // First park, then city
        if (level == 1)
        {
            int index = Random.Range(3, 6);
            if (index == 3)
            {
                For(asbStimuliList1cIds);
            }
            else if (index == 4)
            {
                For(asbStimuliList2cIds);
            }
            else
            {
                For(asbStimuliList3cIds);
            }
        }
        else if (level == 2)
        {
            int index = Random.Range(3, 6);
            if (index == 3)
            {
                For(asbStimuliList1cIds);
                index = Random.Range(0, 2);
                if (index == 0)
                {
                    For(asbStimuliList2cIds);
                }
                else
                {
                    For(asbStimuliList3cIds);
                }
            }
            else if (index == 4)
            {
                For(asbStimuliList2cIds);
                index = Random.Range(0, 2);
                if (index == 0)
                {
                    For(asbStimuliList3cIds);
                }
                else
                {
                    For(asbStimuliList1cIds);
                }
            }
            else
            {
                For(asbStimuliList3cIds);
                index = Random.Range(0, 2);
                if (index == 0)
                {
                    For(asbStimuliList1cIds);
                }
                else
                {
                    For(asbStimuliList2cIds);
                }
            }
        }
        else if (level == 3)
        {
            For(asbStimuliList1cIds);
            For(asbStimuliList2cIds);
            For(asbStimuliList3cIds);
        }
        // Sets the fifty questions
        /*for (int i = 0; i < targetPrefab.Count; i++)
        {
            N c = randomizedStimuliList[Random.Range(0, randomizedStimuliList.Count)];
            randomizedStimuliList.Remove(c);
            randomizedStimuliList.Insert(i,c);
        }
        */
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        ASBManagerPresent = false;
    }

    /// <summary>
    /// Note, was a bit confused because only the shared mode master client seems to run this.
    /// Game worked fine, but unsure if every client should be running this in their simulation or not.
    /// </summary>
    public override void FixedUpdateNetwork()
    {
        // We check to see if any player has chosen answer, and if so, go to the show feedback state. // ACTIVE check if we are colliding a target


        if (!gameTimer.Expired(Runner))
        {
            if (stimTimer.Expired(Runner)) // if when the timer expires we are in the show stimulus state, it means that no answer has been chosen
            {

                if (StimuliShown < maxStimuli)
                {
                    currStimuliPos = Random.Range(0, 2);
                    if (TargetPrefabIdxList.Count > 0)
                    {
                        randomIndex = Random.Range(0, TargetPrefabIdxList.Count);
                    }
                    else
                    {
                        randomIndex = -1;
                    }
                    Debug.LogError("I am " + ASBPlayer.LocalPlayer.PlayerName + "and randomIndex is " + randomIndex.ToString());
                    if (randomIndex != -1)
                    {
                        selectedTargetPrefabIndex = TargetPrefabIdxList[randomIndex];
                    }
                    else
                    {
                        selectedTargetPrefabIndex = -1;
                    }
                    ASBPlayer.LocalPlayer.ChosenAnswer = -1;
                    CurrentStimulus++;

                    stimTimerLength = stimulusLength;
                    stimTimer = TickTimer.CreateFromSeconds(Runner, stimTimerLength);
                    GameState = ASBStateGame.ShowStimulus;
                    StimuliShown++;
                }
                else
                {
                    stimTimer = TickTimer.None;
                    gameTimer = TickTimer.None;
                    GameState = ASBStateGame.GameOver;
                }
            }
        }
        else // game over after the final duration has been reached
        {
            stimTimer = TickTimer.None;
            gameTimer = TickTimer.None;
            GameState = ASBStateGame.GameOver;
        }
        if (isCollaborative)
        {
            if (turnTimer.RemainingTime(Runner) == 0)
            {
                turnTimer = TickTimer.CreateFromSeconds(Runner, turnTimerLength);
                AChiTocca = !AChiTocca;
            }
        }
    }

    /// <summary>
    /// Called when a player picks an answer.
    /// </summary>
    /// <param name="index"></param>
    public void PickAnswer(int index)
    {

        // If we are in the question state and the local player has not picked an answer...
        if (GameState == ASBStateGame.ShowStimulus)
        {
            // TODO modify here by checking whether the answer is correct or not
            // For now, if Chosen Answer is less than 0, this means they haven't picked an answer.
            // We don't allow players to pick new answers at this time.
            if (ASBPlayer.LocalPlayer.ChosenAnswer < 0)
            {
                _confirmSFX.Play();

                ASBPlayer.LocalPlayer.ChosenAnswer = index;
                //Debug.LogError("I am " + ASBPlayer.LocalPlayer.PlayerName + " and my answer is " + ASBPlayer.LocalPlayer.ChosenAnswer.ToString());

                GameState = ASBStateGame.ShowFeedback;
            }
            //else
            //{
            //    _errorSFX.Play();
            //}
        }

    }

    /// <summary>
    /// Update function that updates the timer visual.
    /// </summary>
    public void Update()
    {
        //Debug.LogError("current state game " + GameState.ToString());
        //Debug.LogError("Update: I am " + ASBPlayer.LocalPlayer.PlayerName + " and has authority is " + HasStateAuthority.ToString());
        // Updates the timer visual
        float? stimRemainingTime = stimTimer.RemainingTime(Runner);
        float? turnRemainingTime = turnTimer.RemainingTime(Runner);

        string player;

        if (stimRemainingTime.HasValue)
        {
            stimTimerText.text = Mathf.Round(stimRemainingTime.Value).ToString();
        }
        else
        {
            stimTimerText.text = "0";
        }
        float? gameRemainingTime = gameTimer.RemainingTime(Runner);

        if (gameRemainingTime.HasValue)
        {
            int tempo = (int) Mathf.Round(gameRemainingTime.Value);
            gameTimerText.text = (tempo/60).ToString() + ':' + (tempo%60).ToString();
        }
        else
        {
            gameTimerText.text = "GAME";
            if(!stimRemainingTime.HasValue)
            {
                stimTimerText.text = "OVER";
            }
        }

        if (isCollaborative)
        {
            if (AChiTocca)
            {
                player = ReturnPlayerName(1);
            }
            else
            {
                player = ReturnPlayerName(2);
            }
            if (turnRemainingTime.HasValue)
            {
                turnTimerText.text = player + ": " + Mathf.Round(turnRemainingTime.Value).ToString();
            }
        }
    }

    public string ReturnPlayerName(int id)
    {
        foreach(ASBPlayer v in ASBPlayer.ASBPlayerRefs)
        {
            if(v.PlayerId == id)
            {
                return v.PlayerName.ToString();
            }
        }
        return null;
    }

    private void OnASBGameStateChanged()
    {
        // If showin an answer, we show which players got the question correct and increase their score.
        if (GameState == ASBStateGame.Intro || GameState == ASBStateGame.NewRound)
        {
            // Resets the score for the player
            ASBPlayer.LocalPlayer.Score = 0;

            //ASBPlayer.LocalPlayer.Expression = ASBPlayer.AvatarExpressions.Neutral;

            asbMessage.text = GameState == ASBStateGame.Intro ? "Select The Correct Answer\nStarting Game Soon" : "New Game Starting Soon!";

            targetSpeed = 20;
            stimuliContainerObj.GetComponent<StimuliMovement>().Move(targetSpeed);

            //endGameObject.Hide();
        }
        else if (GameState == ASBStateGame.ShowFeedback)
        {
            OnGameStateShowFeedback();

            //endGameObject.Hide();
        }
        else if (GameState == ASBStateGame.GameOver)
        {
            OnGameStateGameOver();
        }
        else if (GameState == ASBStateGame.ShowStimulus)
        {
            asbMessage.text = string.Empty;
            // TODO delete this if not appropriate
            //endGameObject.Hide();
        }
        leaveGameBtn.SetActive(GameState == ASBStateGame.GameOver);
        startNewGameBtn.SetActive(GameState == ASBStateGame.GameOver && Runner.IsSharedModeMasterClient == true);
    }
    private void OnGameStateGameOver()
    {
        Debug.Log("Game over");
    }
    private void OnGameStateShowFeedback()
    {
        asbMessage.text = string.Empty;
        // If the player picks the correct answer, their score increases
        if (isCollaborative)
        {
            if ((AChiTocca == true && ASBPlayer.LocalPlayer.PlayerId == 1) || (AChiTocca == false && ASBPlayer.LocalPlayer.PlayerId == 2))
            {
                if (ASBPlayer.LocalPlayer.ChosenAnswer == 0)
                {
                    //ASBPlayer.LocalPlayer.Expression = TriviaPlayer.AvatarExpressions.Happy_CorrectAnswer;
                    int scoreValue = new int();//da modificare if(tag() parco o città
                    if (currentTargetType == "Park")
                    {
                        scoreValue = -1;
                        pointsClicked++;
                    }
                    else if (currentTargetType == "City")
                    {
                        scoreValue = 1;
                        pointsClicked++;
                    }
                    // Gets the score pop up and toggles it.
                    var scorePopUp = ASBPlayer.LocalPlayer.ScorePopUp;
                    scorePopUp.Score = scoreValue;
                    scorePopUp.Toggle = !scorePopUp.Toggle;
                    ASBPlayer.LocalPlayer.ScorePopUp = scorePopUp;
                    ASBPlayer.LocalPlayer.Score += scoreValue;
                    _correctSFX.Play();
                }
                else
                {
                    // Gets score value and toggles it
                    var scorePopUp = ASBPlayer.LocalPlayer.ScorePopUp;
                    scorePopUp.Score = 0;
                    scorePopUp.Toggle = !scorePopUp.Toggle;
                    ASBPlayer.LocalPlayer.ScorePopUp = scorePopUp;
                    //TODO Modify here for feedback score
                    //ASBPlayer.LocalPlayer.Expression = ASBPlayer.AvatarExpressions.Angry_WrongAnswer;
                    _incorrectSFX.Play();
                }
            }
        }
        else
        {
            if (ASBPlayer.LocalPlayer.ChosenAnswer == 0)
            {
                //ASBPlayer.LocalPlayer.Expression = TriviaPlayer.AvatarExpressions.Happy_CorrectAnswer;
                int scoreValue = new int();//da modificare if(tag() parco o città
                if (currentTargetType == "Park")
                {
                    scoreValue = -1;
                    pointsClicked++;
                }
                else if (currentTargetType == "City")
                {
                    scoreValue = 1;
                    pointsClicked++;
                }
                // Gets the score pop up and toggles it.
                var scorePopUp = ASBPlayer.LocalPlayer.ScorePopUp;
                scorePopUp.Score = scoreValue;
                scorePopUp.Toggle = !scorePopUp.Toggle;
                ASBPlayer.LocalPlayer.ScorePopUp = scorePopUp;
                ASBPlayer.LocalPlayer.Score += scoreValue;
                _correctSFX.Play();
            }
            else
            {
                // Gets score value and toggles it
                var scorePopUp = ASBPlayer.LocalPlayer.ScorePopUp;
                scorePopUp.Score = 0;
                scorePopUp.Toggle = !scorePopUp.Toggle;
                ASBPlayer.LocalPlayer.ScorePopUp = scorePopUp;
                //TODO Modify here for feedback score
                //ASBPlayer.LocalPlayer.Expression = ASBPlayer.AvatarExpressions.Angry_WrongAnswer;
                _incorrectSFX.Play();
            }
        }
    }
        
    private void UpdateCurrentStimulus()
    {
        // If we are asking a question, we set the answers.
        if (CurrentStimulus >= 0)
        {
            //stimulusObj.SetActive(true);
            //stimulus.text = asbStimuliList[stimulusIndex].name;
            if (HasStateAuthority)
            {
                //Debug.LogError("ActivateStimulus: I am " + ASBPlayer.LocalPlayer.PlayerName + " and has authority is " + HasStateAuthority.ToString());
                ActivateStimulusRPC();
            }
            // Clears the trivia message
            asbMessage.text = string.Empty;
            // Deisgnate that the local player has not chosen an answer yet.
            ASBPlayer.LocalPlayer.ChosenAnswer = -1;
            //Change the game state
            if (HasStateAuthority)
            {
                GameState = ASBStateGame.ShowStimulus;
            }
        }
        // We hide the question element in case a player late joins at the end of the game.
        /*if (GameState != ASBStateGame.ShowFeedback && GameState != ASBStateGame.ShowStimulus)
        {
            //stimulusObj.SetActive(false);
            foreach (GameObject stim in asbStimuliList1p)
            {
                stim.SetActive(false);
            }
        }*/
    }
    private void UpdateStimuliShownText()
    {
        if (StimuliShown == 0)
            stimuliShownText.text = "";
        else
            stimuliShownText.text = "Stimuli shown: " + StimuliShown + "\nClick effettuati: " + pointsClicked;
    }
    public async void LeaveGame()
    {
        await Runner.Shutdown(true, ShutdownReason.Ok);
        FusionConnector fc = GameObject.FindObjectOfType<FusionConnector>();
        if (fc)
        {
            fc.mainMenuObject.SetActive(true);
            fc.mainGameObject.SetActive(false);
        }
    }
    public void StartNewGame()
    {
        if (HasStateAuthority == false)
            return;
        GameState = ASBStateGame.NewRound;
        StimuliShown = 0;
        // Sets an initial intro timer
        stimTimerLength = 3f;
        turnTimerLength = 30f;
        stimTimer = TickTimer.CreateFromSeconds(Runner, stimTimerLength);
        gameTimer = TickTimer.CreateFromSeconds(Runner, gameTimerLength);

        if(isCollaborative)
            turnTimer = TickTimer.CreateFromSeconds(Runner, turnTimerLength);

        if (ASBPlayer.ASBPlayerRefs.Count == 1)
        {
            isCollaborative = false;
        }
        else if(ASBPlayer.ASBPlayerRefs.Count > 1)
        {
            isCollaborative = true;
        }
    }

    public void StateAuthorityChanged()
    {
        if (GameState == ASBStateGame.GameOver)
        {
            startNewGameBtn.SetActive(Runner.IsSharedModeMasterClient);
        }
    }
    public void For(List<int> lista)
    {
        for (int i = 0; i < lista.Count; i++)
        {
            TargetPrefabIdxList.Add(lista[i]);
            // TODO add to the array target prefab the elements of lista
        }
    }
}