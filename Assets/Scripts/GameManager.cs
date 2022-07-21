// Bibliotecas do sistema raiz
using System;
using System.Collections.Generic;
using System.Collections;
// Bibliotecas de Serizalização
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Linq;
// Bibliotecas da Unity Engine
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour {

    // VARIAVEIS CONSTANTES
    private const string SAVE_TITLE_OBJECT_KEY = "Text_SaveTitle_";
    private const string SAVE_PHASE_OBJECT_KEY = "Text_PhaseName_";
    private const string SAVE_COLLETABLES_OBJECT_KEY = "Text_ColletablesQuant_";
    private const string SAVE_SCROLLBAR_OBJECT_KEY = "Scrollbar_GameProgress_";
    private const string SAVE_GAME_PROGRESS_OBJECT_KEY = "Text_gameProgressQuant_";

    private const string FILE_LOCATE_DIALOG_KEY = "/Dialogs/pastadeteste.csv";
    private const string FILE_LOCATE_SAVE_KEY = "/SaveData/save";

    private const string DIALOG_UI_NPC_OBJECT_KEY = "";
    private const string DIALOG_UI_PHRASE_OBJECT_KEY = "";
    private const string DIALOG_NPC_OBJECT_KEY = "Npc_name";
    private const string DIALOG_PHRASE_OBJECT_KEY = "fala";

    private const float DIALOG_SPEED_DISPLAY_VERYSLOW_KEY = 0.5f;
    private const float DIALOG_SPEED_DISPLAY_SLOW_KEY = 0.25f;
    private const float DIALOG_SPEED_DISPLAY_NORMAL_KEY = 0.15f;
    private const float DIALOG_SPEED_DISPLAY_FAST_KEY = 0.07f;
    private const float DIALOG_SPEED_DISPLAY_VERYFAST_KEY = 0.01f;

    private const char DIALOG_SPEED_CHAR_VERYSLOW_KEY = '%';
    private const char DIALOG_SPEED_CHAR_SLOW_KEY = '&';
    private const char DIALOG_SPEED_CHAR_NORMAL_KEY = '*';
    private const char DIALOG_SPEED_CHAR_FAST_KEY = '('; 
    private const char DIALOG_SPEED_CHAR_VERYFAST_KEY = ')';

    private const string SAVE_GAME_PROGRESS_NULL_PROGRESS_KEY = "0.0%";

    private const string INITIAL_PHASE_KEY = "Phase1";

    // VARIAVEIS DO PLAYER
    private const string PLAYER_KEY = "Player";
    private byte FoundPlayer = 0;

    private protected GameObject atualPlayer = null;
    private protected Rigidbody2D player_rb;

    private protected float atualSpeed;
    private protected const float minSpeed = 5, maxSpeed = 15, aceleration = 3, desaceleretion = 9;
    private float direction = 0;

    private protected byte playerFlipToLeft, playerFlipToRigh;

    private protected float jumpForce;
    private protected const float maxForce = 30, forceIncress = 50;

    [SerializeField] private protected Vector2 playerPosition;

    private Slider lifeBar;
    [SerializeField] private protected float life, maxLife;

    // Varivaies do save menu (pre load)
    private TMP_Text[] saveMenu_saveStats_text = new TMP_Text[3], saveMenu_lastPhase_text = new TMP_Text[3], saveMenu_colletablesFunds_text = new TMP_Text[3];
    private TMP_Text[] saveMenu_gamePorcent_text = new TMP_Text[3];
    private Scrollbar[] saveMenu_gamePorcent_slider = new Scrollbar[3];

    // Constroladores Gerais
    private protected bool pauseGame;

    [SerializeField] private protected string atualScene;

    // Variaveis do sistema de dialogo
    private protected string DialogFile;

    private protected string[] lines, cellsOfAtualLine;
    private protected string[] listOfPhrases;
    private protected int selectedLine;
    private protected int atualCell;

    private protected string phraseToDisplay = string.Empty, npcToDisplay = string.Empty;
    private Text npcText, phraseText;

    public float timerTextChar, timeToDispayChar;
    private bool startToWritePhrase = false;
    private char[] charToDisplay;
    private int charsCount = 0;
    // Coletaveis
    [SerializeField] private string[] collectables;
    [SerializeField] private bool[] unlockedCollectibles;

    // Sistema de Save 
    private protected string[] local_save_file = new string[3];
    [SerializeField] private string saveStats = "< EMPTY >";

    // Start is called before the scene starts
    private void Awake() {
        DialogFile = Application.dataPath + FILE_LOCATE_DIALOG_KEY;

        for(int i = 0; i < 3; i++) {
            local_save_file[i] = Application.dataPath + FILE_LOCATE_SAVE_KEY + (i + 1) + ".dat";
            if(!File.Exists(local_save_file[i])) File.Create(local_save_file[i]);
        }

        DontDestroyOnLoad(this.gameObject);
    }

    // Start is called before the first frame update
    private void Start() {
        for(int i = 0; i < local_save_file.Length; i++) {
            if(GameObject.Find(SAVE_TITLE_OBJECT_KEY + (i + 1).ToString()) != null) saveMenu_saveStats_text[i] = GameObject.Find(SAVE_TITLE_OBJECT_KEY + (i + 1).ToString()).GetComponent<TMP_Text>();
            if(GameObject.Find(SAVE_PHASE_OBJECT_KEY + (i + 1).ToString()) != null) saveMenu_lastPhase_text[i] = GameObject.Find(SAVE_PHASE_OBJECT_KEY + (i + 1).ToString()).GetComponent<TMP_Text>();
            if(GameObject.Find(SAVE_COLLETABLES_OBJECT_KEY + (i + 1).ToString()) != null) saveMenu_colletablesFunds_text[i] = GameObject.Find(SAVE_COLLETABLES_OBJECT_KEY + (i + 1).ToString()).GetComponent<TMP_Text>();
            if(GameObject.Find(SAVE_SCROLLBAR_OBJECT_KEY + (i + 1).ToString())) saveMenu_gamePorcent_slider[i] = GameObject.Find(SAVE_SCROLLBAR_OBJECT_KEY + (i + 1).ToString()).GetComponent<Scrollbar>();
            if(GameObject.Find(SAVE_GAME_PROGRESS_OBJECT_KEY + (i + 1).ToString()) != null) saveMenu_gamePorcent_text[i] = GameObject.Find(SAVE_GAME_PROGRESS_OBJECT_KEY + (i + 1).ToString()).GetComponent<TMP_Text>();
        }
        if(GameObject.Find(DIALOG_NPC_OBJECT_KEY).GetComponent<Text>() != null) npcText = GameObject.Find(DIALOG_NPC_OBJECT_KEY).GetComponent<Text>();
        if(GameObject.Find(DIALOG_PHRASE_OBJECT_KEY).GetComponent<Text>()) phraseText = GameObject.Find(DIALOG_PHRASE_OBJECT_KEY).GetComponent<Text>();

        timeToDispayChar = DIALOG_SPEED_DISPLAY_NORMAL_KEY;
    }

    // Update is called once per frame
    private void Update() {
        if(Input.GetKeyDown(KeyCode.L)) StartNewGame(); // linha de teste

        if(FoundPlayer == 0) {
            if(atualPlayer == null) atualPlayer = GameObject.Find(PLAYER_KEY);
            else FoundPlayer = 1;

            if(atualPlayer != null) player_rb = atualPlayer.GetComponent<Rigidbody2D>();
        }
        else {
            if(atualPlayer != null && pauseGame == false) 
                MovPlayer(); JumpPlayer();
        }

        if(startToWritePhrase == true) ShowDialog();
        else {

        }
    }

    // Start Menu
    private void MenuManager() {

    }

    private void PreLoadData() {
        for(int i = 0; i < local_save_file.Length; i++) {
            int colletablesFoundsCount = 0;

            XmlSerializer xml = new XmlSerializer(typeof(GameData));
            Stream reader = new FileStream(local_save_file[i],FileMode.Open);

            GameData dat = new GameData();
            dat = (GameData)xml.Deserialize(reader);

            saveMenu_saveStats_text[i].text = dat.SaveStats;

            if(dat.SaveStats != "< EMPTY >") {
                saveMenu_lastPhase_text[i].text = dat.LastScene.ToString();

                if(unlockedCollectibles.Length == dat.UnlockedCollectibles.Length) {
                    for(int a = 0; a < dat.UnlockedCollectibles.Length; a++)
                        if(dat.UnlockedCollectibles[a] == true) colletablesFoundsCount++;

                    saveMenu_colletablesFunds_text[i].text = colletablesFoundsCount + "/" + unlockedCollectibles.Length;

                    if(saveMenu_gamePorcent_slider[i].value == 1f) saveMenu_gamePorcent_slider[i].value = 100.0f;
                }
                else {
                    saveMenu_colletablesFunds_text[i].text = "0/" + unlockedCollectibles.Length;
                    saveMenu_gamePorcent_text[i].text = SAVE_GAME_PROGRESS_NULL_PROGRESS_KEY;

                    saveMenu_gamePorcent_slider[i].value = 0.0f;
                }
            }
            else {
                saveMenu_lastPhase_text[i].text = string.Empty;
                saveMenu_colletablesFunds_text[i].text = "0/0";
                saveMenu_gamePorcent_text[i].text = SAVE_GAME_PROGRESS_NULL_PROGRESS_KEY;

                saveMenu_gamePorcent_slider[i].value = 0;
            }
            reader.Close();
        }
    }
    
    private void StartNewGame() {
        LoadScene(INITIAL_PHASE_KEY);
        atualPlayer = null;
        FoundPlayer = 0;
    }

    // Scene Manager
    private protected void LoadScene(string scene) {
        UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
        atualScene = scene;
    }

    // Player Movment
    private protected void JumpPlayer() {
        if(Input.GetKey(KeyCode.Space)) {
            if(jumpForce < maxForce) jumpForce += forceIncress * Time.deltaTime;
        }
        else if(Input.GetKeyUp(KeyCode.Space)) {
            player_rb.AddForce(new Vector2(0,jumpForce),ForceMode2D.Impulse);
            jumpForce = 0;
        }
    }
    
    private protected void MovPlayer() {
        float inputAxisHorizontal = Input.GetAxisRaw("Horizontal");

        player_rb.velocity = new Vector2(atualSpeed * direction, atualPlayer.gameObject.GetComponent<Rigidbody2D>().velocity.y);

        // set speed over aceleration time
        if(inputAxisHorizontal != 0) {
            direction = inputAxisHorizontal;

            // set start speed
            if(atualSpeed < minSpeed) atualSpeed = minSpeed;

            // set acceleration
            if(atualSpeed > maxSpeed) atualSpeed = maxSpeed; 
            else atualSpeed += aceleration * Time.deltaTime;

            // flip sprite
            if(inputAxisHorizontal > 0) playerFlipToLeft = 0;
            else playerFlipToLeft = 1;
            Flip();
        }
        else if(atualSpeed > 0) atualSpeed -= desaceleretion * Time.deltaTime;
    }

    private void Flip() {
        if(playerFlipToLeft != playerFlipToRigh) {
            if(playerFlipToRigh == 0) playerFlipToRigh = 1; else playerFlipToRigh = 0;

            Vector3 theScale = atualPlayer.transform.localScale;
            theScale.z *= -1;
            atualPlayer.transform.localScale = theScale;
        }
    }

    // Player Life

    private protected void ChangeLifeValue(float value) {
        life += value;
        
        if(life > maxLife) life = maxLife;
        else if(life <= 0) {
            lifeBar.value = life;
            Dead();
        }
    }

    private protected void ChangeLifeMaxValue(float value) {
        maxLife += value;
        lifeBar.maxValue = maxLife;
    }

    private void Dead() {

    }

    // Events Controll
    public void UIButtonPressed(string ButtonName) {
        switch(ButtonName) {
            case "NextPhrase":
            if(charsCount < charToDisplay.Length) timeToDispayChar = DIALOG_SPEED_DISPLAY_VERYFAST_KEY;
            else {
                atualCell++;
                ReadCsv(selectedLine);
            }
            break;
        }
    }
    
    public void StartEvent(string eventName, int idOfAnyAction) {
        switch(eventName) {
            default: Debug.LogError(eventName + " is not a valid event"); break;
        }
    }

    // Dialog System
    private void ShowDialog() {
        npcText.text = npcToDisplay;

        if(timerTextChar <= 0) {
            if(charsCount < charToDisplay.Length) {

                switch(charToDisplay[charsCount]) {
                    case DIALOG_SPEED_CHAR_VERYSLOW_KEY: timeToDispayChar = DIALOG_SPEED_DISPLAY_VERYSLOW_KEY; break;
                    case DIALOG_SPEED_CHAR_SLOW_KEY: timeToDispayChar = DIALOG_SPEED_DISPLAY_SLOW_KEY; break;
                    case DIALOG_SPEED_CHAR_NORMAL_KEY: timeToDispayChar = DIALOG_SPEED_DISPLAY_NORMAL_KEY; break;
                    case DIALOG_SPEED_CHAR_FAST_KEY: timeToDispayChar = DIALOG_SPEED_DISPLAY_FAST_KEY; break;
                    case DIALOG_SPEED_CHAR_VERYFAST_KEY: timeToDispayChar = DIALOG_SPEED_DISPLAY_VERYFAST_KEY; break;

                    default:
                    phraseText.text += charToDisplay[charsCount];
                    break;
                }
                
                charsCount++;
            }
            else startToWritePhrase = false;

            timerTextChar = timeToDispayChar;
        }
        else timerTextChar -= Time.deltaTime;
    }

    public void ReadCsv(int line) {
        StreamReader stream = new StreamReader(DialogFile);

        lines = stream.ReadToEnd().Split('/');
        cellsOfAtualLine = lines[line].Split(';');

        listOfPhrases = new string[cellsOfAtualLine.Length - 1];
        for(int i = 0; i < cellsOfAtualLine.Length - 1; i++) listOfPhrases[i] = cellsOfAtualLine[i + 1];

        phraseText.text = String.Empty;
        charsCount = 0;

        if(listOfPhrases[atualCell + 1] != String.Empty ) {
            phraseToDisplay = listOfPhrases[atualCell + 1];
            npcToDisplay = listOfPhrases[0];

            charToDisplay = phraseToDisplay.ToCharArray();
            timeToDispayChar = DIALOG_SPEED_DISPLAY_NORMAL_KEY;

            startToWritePhrase = true;
        }
        else {
            npcText.text = String.Empty;
            // destativar gameobject e desprender prsonagem
            atualCell = 0;
        }
    }

    // Save System
    private protected void SaveData(int saveID) {
        XmlSerializer ser = new XmlSerializer(typeof(GameData));
        StreamWriter writer = new StreamWriter(local_save_file[saveID - 1]);

        GameData dat = new GameData();

        // insert here the data

        dat.SaveID = saveID;
        dat.SaveStats = saveStats;
        dat.LastScene = atualScene;
        dat.UnlockedCollectibles = unlockedCollectibles;
        dat.Life = life;
        dat.PlayerPosition = playerPosition;

        // ====================

        ser.Serialize(writer,dat);
        writer.Close();
    }
    
    private protected void LoadData(int saveID) {
        if(File.Exists(local_save_file[saveID - 1])) {
            XmlSerializer xml = new XmlSerializer(typeof(GameData));
            Stream reader = new FileStream(local_save_file[saveID - 1],FileMode.Open);

            GameData dat = new GameData();
            dat = (GameData)xml.Deserialize(reader);

            // insert here the data

            saveStats = dat.SaveStats;
            unlockedCollectibles = dat.UnlockedCollectibles;
            playerPosition = dat.PlayerPosition;

            // ====================

            reader.Close();
        }
    }
    
    [SerializeField]
    public class GameData {
        private int _saveID;
        private string _saveStats, _lastScene;
        private bool[] _unlockedCollectibles;
        private float _life, _maxLife;
        private Vector2 _playerPosition;

        public int SaveID {
            get { return _saveID; }
            set { _saveID = value; }
        }

        public string SaveStats {
            get { return _saveStats; }
            set { _saveStats = value; }
        }

        public string LastScene {
            get { return _lastScene; }
            set { _lastScene = value; }
        }

        public bool[] UnlockedCollectibles {
            get { return _unlockedCollectibles; }
            set { _unlockedCollectibles = value; }
        }

        public float Life {
            get { return _life; }
            set { _life = value; }
        }

        public float MaxLife {
            get { return _maxLife; }
            set { _maxLife = value; }
        }

        public Vector2 PlayerPosition {
            get { return _playerPosition; }
            set { _playerPosition = value; }
        }
    }
}