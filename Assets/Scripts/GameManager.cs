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

    private const string FILE_LOCATE_DIALOG_KEY = "/Dialog/pastadeteste.cxv";
    private const string FILE_LOCATE_SAVE_KEY = "/SaveData/save";

    private const string SAVE_GAME_PROGRESS_NULL_PROGRESS_KEY = "0.0%";

    private const string INITIAL_PHASE_KEY = "Phase1";

    // VARIAVEIS DO PLAYER
    private const string PLAYER_KEY = "Player";
    private bool FoundPlayer = true;

    private protected GameObject atualPlayer = null;
    private protected Rigidbody2D player_rb;

    private protected float atualSpeed;
    private protected const float minSpeed = 5, maxSpeed = 15, aceleration = 3, desaceleretion = 9;
    private float direction = 0;

    private protected bool playerFlipToLeft, playerFlipToRigh;

    private protected float jumpForce;
    private protected const float maxForce = 30, forceIncress = 50;

    [SerializeField] private protected Vector2 playerPosition;
    [SerializeField] private protected float life;

    // Varivaies do save menu (pre load)
    private TMP_Text[] saveMenu_saveStats_text = new TMP_Text[3], saveMenu_lastPhase_text = new TMP_Text[3], saveMenu_colletablesFunds_text = new TMP_Text[3];

    private TMP_Text[] saveMenu_gamePorcent_text = new TMP_Text[3];

    private Scrollbar[] saveMenu_gamePorcent_slider = new Scrollbar[3];

    // Constroladores Gerais
    private protected bool pauseGame;

    [SerializeField] private protected string atualScene;
    
    // Variaveis do sistema de dialogo
    public string DialogFile;
    private protected string[] lines, coluns;

    public int cell;
    public string[] atualFrases = new string[17];

    public Text npc, frase;
    public string atualFrase = string.Empty, atualNpc;
    private bool startEWrite = false;

    public float timerText, timeToWait;

    // Sistema de Save 
    private protected string[] local_save_file = new string[3];
    [SerializeField] private string saveStats = "< EMPTY >";

    // Coletaveis
    [SerializeField] private string[] collectables;
    [SerializeField] private bool[] unlockedCollectibles;

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
        unlockedCollectibles[2] = true;

        for(int i = 1; i < local_save_file.Length + 1; i++) {
            saveMenu_saveStats_text[i - 1] = GameObject.Find(SAVE_TITLE_OBJECT_KEY + i.ToString()).GetComponent<TMP_Text>();
            saveMenu_lastPhase_text[i - 1] = GameObject.Find(SAVE_PHASE_OBJECT_KEY + i.ToString()).GetComponent<TMP_Text>();
            saveMenu_colletablesFunds_text[i - 1] = GameObject.Find(SAVE_COLLETABLES_OBJECT_KEY + i.ToString()).GetComponent<TMP_Text>();
            saveMenu_gamePorcent_slider[i - 1] = GameObject.Find(SAVE_SCROLLBAR_OBJECT_KEY + i.ToString()).GetComponent<Scrollbar>();
            saveMenu_gamePorcent_text[i - 1] = GameObject.Find(SAVE_GAME_PROGRESS_OBJECT_KEY + i.ToString()).GetComponent<TMP_Text>();
        }

        timerText = timeToWait;
    }
    // Update is called once per frame
    private void Update() {
        if(Input.GetKeyDown(KeyCode.L)) ReadCsv(cell);

        if(FoundPlayer == false) {
            if(atualPlayer == null) atualPlayer = GameObject.Find(PLAYER_KEY);
            else FoundPlayer = true;

            player_rb = atualPlayer.GetComponent<Rigidbody2D>();
        }
        else {
            if(atualPlayer != null && pauseGame == false) {

                MovPlayer();
                JumpPlayer();
            }
        }

        if(startEWrite == true) {
            timerText -= Time.deltaTime;
            var chaR = atualFrases[cell].ToCharArray();
            if(timerText <= 0) {
                frase.text = string.Empty;
                timerText = timeToWait;
            }
        }
    }

    // Start Menu
    private protected void MenuManager() {

    }

    private protected void OptionSelected(string Button) {
        switch(Button) {

        }
    }
    
    private protected void PreLoadData() {
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
                    for(int a = 0; a < dat.UnlockedCollectibles.Length; a++) {
                        if(dat.UnlockedCollectibles[a] == true)
                            colletablesFoundsCount++;
                    }

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
    
    private protected void StartNewGame() {
        LoadScene(INITIAL_PHASE_KEY);
        atualPlayer = null;
        FoundPlayer = false;
    }

    // Scene Manager
    private protected void LoadScene(string scene) {
        UnityEngine.SceneManagement.SceneManager.LoadScene(scene);

        atualScene = scene;
    }

    // Player Movment
    private protected void JumpPlayer() {

        if(Input.GetKey(KeyCode.Space)) {
            if(jumpForce < maxForce)
                jumpForce += forceIncress * Time.deltaTime;
        }
        else if(Input.GetKeyUp(KeyCode.Space)) {
            player_rb.AddForce(new Vector2(0,jumpForce),ForceMode2D.Impulse);
            jumpForce = 0;
        }
    }
    
    private protected void MovPlayer() {
        player_rb.velocity = new Vector2(atualSpeed * direction, atualPlayer.gameObject.GetComponent<Rigidbody2D>().velocity.y);

        // set speed over aceleration time
        if(Input.GetAxisRaw("Horizontal") != 0) {
            direction = Input.GetAxisRaw("Horizontal");

            // set start speed
            if(atualSpeed < minSpeed)
                atualSpeed = minSpeed;

            // set acceleration
            if(atualSpeed > maxSpeed)
                atualSpeed = maxSpeed;
            else
                atualSpeed += aceleration * Time.deltaTime;

            // flip sprite
            if(Input.GetAxis("Horizontal") > 0) {
                playerFlipToLeft = true;
                Flip();
            }
            else {
                playerFlipToLeft = false;
                Flip();
            }
        }
        else {
            if(atualSpeed > 0)
                atualSpeed -= desaceleretion * Time.deltaTime;
        }
    }

    private protected void Flip() {
        if((playerFlipToLeft && !playerFlipToRigh)||(!playerFlipToLeft && playerFlipToRigh)) {
            playerFlipToRigh = !playerFlipToRigh;

            Vector3 theScale = atualPlayer.transform.localScale;
            theScale.z *= -1;
            atualPlayer.transform.localScale = theScale;
        }
    }

    // Dialog System
    private protected void ShowDialog() {

    }

    private protected void ReadCsv(int cell) {
        frase.text = string.Empty;

        StreamReader stream = new StreamReader(DialogFile);

        lines = stream.ReadToEnd().Split('/');
        coluns = lines[cell].Split(';');

        string[] preFrase = new string[atualFrases.Length];
        for(int i = 1; i < coluns.Length - 1; i++) {
            atualFrases[i - 1] = coluns[i];
        }

        startEWrite = true;
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
        private string _saveStats;
        private string _lastScene;
        private bool[] _unlockedCollectibles;
        private float _life;
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

        public Vector2 PlayerPosition {
            get { return _playerPosition; }
            set { _playerPosition = value; }
        }
    }
}