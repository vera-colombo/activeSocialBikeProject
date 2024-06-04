using Fusion;
using Photon.Voice.Fusion;
using Photon.Voice.Unity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ASBPlayer : NetworkBehaviour
{
    [Tooltip("The sprite used for every player but the local one.")]
    [SerializeField]
    Sprite _mainBackdrop;

    [Tooltip("The sprite used for the local player.")]
    [SerializeField]
    Sprite _localPlayerBackdrop;

    #region Network Properties
    [Tooltip("The name of the player")]
    [Networked, OnChangedRender(nameof(OnPlayerNameChanged))]
    public NetworkString<_16> PlayerName { get; set; }

    [Tooltip("Which character has the player chosen.")]
    [Networked, OnChangedRender(nameof(OnAvatarChanged))]
    public int ChosenAvatar { get; set; } = -1;

    [Tooltip("Which expression should the avatar be displaying now.  Should probably be an enum.")]
    [Networked, OnChangedRender(nameof(OnAvatarChanged))]
    public AvatarExpressions Expression { get; set; } = AvatarExpressions.Neutral;

    [Tooltip("What is the player's score.")]
    [Networked, OnChangedRender(nameof(OnScoreChanged))]
    public int Score { get; set; }

    [Tooltip("What is the player's score.")]
    [Networked, OnChangedRender(nameof(OnScorePopupChanged))]
    public TriviaScorePopUp ScorePopUp { get; set; }

    [Tooltip("If true, this player will be registered as the master client and displayed visually.")]
    [Networked, OnChangedRender(nameof(OnMasterClientChanged))]
    public NetworkBool IsMasterClient { get; set; }

    [Tooltip("Which answer did the player choose.  0 is always the correct answer, but the answers are randomized locally.")]
    [Networked, OnChangedRender(nameof(OnAnswerChosen))]
    public int ChosenAnswer { get; set; } = -1;

    [Tooltip("If true, the local player is muted and will not transmit voice over the network.")]
    [Networked, OnChangedRender(nameof(OnMuteChanged))]
    public NetworkBool Muted { get; set; }

    public MeshRenderer playerMeshRenderer;

    [Networked, OnChangedRender(nameof(ColorChanged))]
    public Color NetworkedColor { get; set; }

    
    #endregion

    [Tooltip("Reference to the avatars a player can use.")]
    public Image avatarRenderer;

    [Tooltip("Reference to the avatar expressions.")]
    public GameObject[] facialExpressions;

    [Tooltip("The sprites used to render the character")]
    public Sprite[] avatarSprites;

    [Tooltip("The image used for the backdrop.")]
    public Image backdrop;

    [Tooltip("Reference to the name display object.")]
    public TextMeshProUGUI nameText;

    [Tooltip("Reference to the score display object.")]
    public TextMeshProUGUI scoreText;

    [Tooltip("Image that will turn on if the local player is the master client.")]
    public Image masterClientIcon;

    [Tooltip("Image toggled when the local player wants to mute their mic.")]
    public Image muteSpeakerIcon;

    [Tooltip("Image toggled when the a player is speaking or when the local player is recording.")]
    public Image speakingIcon;

    [Tooltip("The sprites used to render the character")]
    public GameObject avatarSelectableSpriteGameObject;

    [SerializeField, Tooltip("Audio source that players when selecting an avatar.")]
    private AudioSource _clickLocalPlayerAudio;

    [SerializeField, Tooltip("Audio source that plays when trying to select another player's avatar.")]
    private AudioSource _clickRemotePlayerAudio;

    [SerializeField, Tooltip("Audio source that players when a new avatar is selected.")]
    private AudioSource _onChangeAvatarAudio;

    [SerializeField, Tooltip("Text pop up for when an answer has been answered.")]
    private TextMeshProUGUI _scorePopUpText;

    [SerializeField, Tooltip("Animator triggered when score is updated or an incorrect answer is given.")]
    private Animator _scorePopUpAnimator;

    /// <summary>
    /// Unsure if this pattern is okay, but static references to the local player and a list of all players.
    /// </summary>
    public static ASBPlayer LocalPlayer;

    protected ASBManager asbManager;

    [SerializeField, Tooltip("Reference to the voice network object that will show if a player is speaking or not.")]
    private VoiceNetworkObject _voiceNetworkObject;

    [SerializeField, Tooltip("Reference to the recorder for this player.")]
    private Recorder _recorder;

    [SerializeField, Tooltip("Reference to the object containing canvas elements.")]
    protected GameObject _canvasObjs;



    public Camera Camera;
    /// <summary>
    /// A list of all players currently in the game.
    /// </summary>
    public static List<ASBPlayer> ASBPlayerRefs = new List<ASBPlayer>();
    public enum AvatarExpressions
    {
        Neutral = 0,
        AnswerSelected = 1,
        Angry_WrongAnswer = 2,
        Happy_CorrectAnswer = 3,
    }

    

    /// <summary>
    /// When a character is spawned, we have to do the checks that a user would do in case someone spawns late.
    /// </summary>
    public override void Spawned()
    {
        base.Spawned();

        // Adds this player to a list of player refs and then sorts the order by index
        ASBPlayerRefs.Add(this);
        ASBPlayerRefs.Sort((x, y) => x.Object.StateAuthority.AsIndex - y.Object.StateAuthority.AsIndex);

        // The OnRenderChanged functions are called during spawn to make sure they are set properly for players who have already joined the room.
        OnAnswerChosen();
        OnScoreChanged();
        OnPlayerNameChanged();
        OnAvatarChanged();

        // We assign the local test player a different sprite
        if (Object.HasStateAuthority == true)
        {
            backdrop.sprite = _localPlayerBackdrop;
            LocalPlayer = this;
        }
        else
        {
            backdrop.sprite = _mainBackdrop;
        }

        if (ASBPlayerRefs.Count == 1) 
        {
            transform.SetParent(FusionConnector.Instance.playerContainer[0], false);
            NetworkedColor = Color.yellow;
        }
        else 
        {
            transform.SetParent(FusionConnector.Instance.playerContainer[1], false);
            NetworkedColor = Color.cyan;
        }
       

        _canvasObjs.transform.SetParent(FusionConnector.Instance.playerCanvasContainer, false);

        // Sets the master client value on spawn
        if (HasStateAuthority)
        {
            IsMasterClient = Runner.IsSharedModeMasterClient;
        }
        masterClientIcon.enabled = IsMasterClient;

        OnMuteChanged();

        // Hides the avatar selector on spawn
        avatarSelectableSpriteGameObject.gameObject.SetActive(false);

        // We show the "Start Game Button" for the master client only, regardless of the number of players in the room.
        bool showGameButton = Runner.IsSharedModeMasterClient && ASBManager.ASBManagerPresent == false;
        FusionConnector.Instance.showGameButton.SetActive(showGameButton);

        if (HasStateAuthority)
        {
            Camera = Camera.main;
            Camera.GetComponent<FirstPersonCamera>().Target = transform;
        }
    }
    // TODO Verify if it's ok
    void ColorChanged()
    {
        playerMeshRenderer.material.color = NetworkedColor;
    }

    public void ShowDropdown()
    {
        if (HasStateAuthority)
        {
            avatarSelectableSpriteGameObject.SetActive(true);
            _clickLocalPlayerAudio.Play();
        }
        else
        {
            _clickRemotePlayerAudio.Play();
        }
    }

    public void MakeAvatarSelection(Transform t)
    {
        if (HasStateAuthority)
        {
            ChosenAvatar = t.GetSiblingIndex();
            avatarSelectableSpriteGameObject.SetActive(false);

            _onChangeAvatarAudio.Play();
        }
    }

    void OnAvatarChanged()
    {
        // Sets which avatar face and expression to choose
        if (ChosenAvatar >= 0)
            avatarRenderer.sprite = avatarSprites[ChosenAvatar];
        else
            avatarRenderer.sprite = null;

        for (int i = 0; i < facialExpressions.Length; i++)
        {
            facialExpressions[i].SetActive(ChosenAvatar >= 0 && (int)Expression == i);
        }
    }

    void OnAnswerChosen()
    {
        if (LocalPlayer != null) 
        {
            Debug.LogError("OnAnswerChosen: I am " + LocalPlayer.PlayerName + " and answer is " + LocalPlayer.ChosenAnswer);
        }
        if (HasStateAuthority)
        {
            if (ChosenAnswer >= 0)
            {
                Debug.Log("answer");
            }
            else 
            {
                Debug.Log("no answer");
            }
        }
      
    }

    void OnScoreChanged()
    {
        scoreText.text = Score.ToString();
    }

    void OnScorePopupChanged()
    {
        if (ScorePopUp.Score > 0)
        {
            _scorePopUpText.text = string.Format("+{0}", ScorePopUp.Score);
            _scorePopUpAnimator.SetTrigger("CorrectAnswer");
        }
        else
        {
            _scorePopUpText.text = "X";
            _scorePopUpAnimator.SetTrigger("WrongAnswer");
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        // Removes the player from the list
        ASBPlayerRefs.Remove(this);

        // Sets the local test play to null
        if (this == LocalPlayer)
            LocalPlayer = null;

        if (HasStateAuthority)
            IsMasterClient = runner.IsSharedModeMasterClient;

        bool showGameButton = Runner.IsSharedModeMasterClient && ASBManager.ASBManagerPresent == false;
        FusionConnector.Instance.showGameButton.SetActive(showGameButton);
    }

    void OnPlayerNameChanged()
    {
        nameText.text = PlayerName.Value;
    }

    void OnMasterClientChanged()
    {
        masterClientIcon.enabled = IsMasterClient;
    }

    public void ToggleVoiceTransmission()
    {
        if (HasStateAuthority)
        {
            Muted = !Muted;
            _recorder.TransmitEnabled = !Muted;
        }
    }

    public void OnMuteChanged()
    {
        muteSpeakerIcon.enabled = Muted;
    }

    private void Update()
    {
       
        speakingIcon.enabled = (_voiceNetworkObject.SpeakerInUse && _voiceNetworkObject.IsSpeaking) || (_voiceNetworkObject.RecorderInUse && _voiceNetworkObject.IsRecording);

        if (HasStateAuthority == false)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            ASBPlayer.LocalPlayer.ChosenAnswer = 0;
            Debug.LogError("I am " + ASBPlayer.LocalPlayer.PlayerName + " and my answer is " + ASBPlayer.LocalPlayer.ChosenAnswer.ToString());
            GameObject.FindGameObjectWithTag("ASBManager").GetComponent<ASBManager>().DealFeedbackRPC();
        }

        // TEST ONLY
        //if (Input.GetKeyDown(KeyCode.G))
        //{
        //    ASBPlayer.LocalPlayer.GetComponent<PlayerMovement>().isMoving = true;
        //    Debug.LogError("I am " + ASBPlayer.LocalPlayer.PlayerName + " and I am moving" + ASBPlayer.LocalPlayer.GetComponent<PlayerMovement>().isMoving.ToString());
        //}
        //if (Input.GetKeyDown(KeyCode.H))
        //{
        //    ASBPlayer.LocalPlayer.GetComponent<PlayerMovement>().isMoving = false;
        //    Debug.LogError("I am " + ASBPlayer.LocalPlayer.PlayerName + " and I am moving" + ASBPlayer.LocalPlayer.GetComponent<PlayerMovement>().isMoving.ToString());

        //}

    }

    public void SendAnswer() 
    {
        ASBPlayer.LocalPlayer.ChosenAnswer = 0;
        Debug.LogError("I am " + ASBPlayer.LocalPlayer.PlayerName + " and my answer is " + ASBPlayer.LocalPlayer.ChosenAnswer.ToString());
        GameObject.FindGameObjectWithTag("ASBManager").GetComponent<ASBManager>().DealFeedbackRPC();
    }
}
