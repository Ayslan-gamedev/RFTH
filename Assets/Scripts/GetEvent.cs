using UnityEngine;

public class GetEvent : MonoBehaviour {
    private const string EVENT_NAME_STARTDIALOG_KEY = "StartDialog";

    private protected GameManager manager;

    public int lineOfDialog;
    
    private bool inArea;

    private void Start() {
        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    private void Update() {
        if(inArea == true) {
            if(Input.GetKeyDown(KeyCode.F)) manager.StartEvent(EVENT_NAME_STARTDIALOG_KEY, lineOfDialog);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.CompareTag("Player")) inArea = true;
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if(collision.CompareTag("Player")) inArea = false;
    }
}