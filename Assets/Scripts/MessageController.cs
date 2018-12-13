using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class MessageController : MonoBehaviour {
    public Button yesBtn, noBtn, doneBtn;
    public static GameObject route;
    public static GameObject schedule;
    public static int globalConfirm;
    public static MessageController instance = null;
    public Vector2 fromLocation, toLocation;
    static int fromBldg, toBldg;
    public Sprite overlayTile;
    public bool scheduleOn = false; //controls click functionality in GameController & timeManagement
    public bool routeOn = false; //another flag to check in gamecontroller & timeManagement

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        route = GameObject.FindWithTag("chooseRoute");
        schedule = GameObject.FindWithTag("reviewRoute");
        enableDialog(false);
        showSchedule(false);
    }

    public void showRouteDialog(int from,int to,float pay, int day){
        routeOn = true;
        fromLocation = GameController.instance.getCenterTile(from);
        toLocation = GameController.instance.getCenterTile(to);
        enableDialog(true);
        route.transform.GetChild(1).GetComponent<Text>().text = "Mr. Vanderbilt wants you to complete the route below in " + day + " days for $" + pay + ". Accept?";
        //Debug.Log("Mr. Vanderbilt wants you to complete the route below in " + day + " days for $" + pay + ". Accept?");
        Background.instance.overlay.GetComponent<TilemapRenderer>().enabled = true;
        Background.instance.revealTiles(fromLocation,toLocation);
        //wait for click of button
    }

    void enableDialog(bool enable){
        //GameObject.FindWithTag("coverup").transform.GetChild(0).GetComponent<Image>().enabled = enable; //covers the exposed area to the left of the screen
        route.transform.GetChild(0).GetComponent<Image>().enabled = enable;
        route.transform.GetChild(1).GetComponent<Text>().enabled = enable;
        route.transform.GetChild(2).GetComponent<Image>().enabled = enable;
        route.transform.GetChild(2).GetComponent<Button>().enabled = enable;
        route.transform.GetChild(2).GetChild(0).GetComponent<Text>().enabled = enable;
        route.transform.GetChild(3).GetComponent<Image>().enabled = enable;
        route.transform.GetChild(2).GetChild(0).GetComponent<Text>().enabled = enable;
        route.transform.GetChild(3).GetChild(0).GetComponent<Text>().enabled = enable;

    }

    // Use this for initialization
    void Start () {
        //setup anything that relies on other scripts being Awake() in start, in this case overlay is set in Awake()
        yesBtn.onClick.AddListener(() => routeConfirmation(1)); //true
        noBtn.onClick.AddListener(() => routeConfirmation(0));  //flase
        doneBtn.onClick.AddListener(() => exitSchedule());
        Background.instance.overlay.GetComponent<Renderer>().enabled = false;
    }
   
    void routeConfirmation(int confirm){
        Background.instance.changeOverlayTileSprite(fromLocation, overlayTile);
        Background.instance.changeOverlayTileSprite(toLocation, overlayTile);
        globalConfirm = confirm;
        Background.instance.overlay.GetComponent<TilemapRenderer>().enabled = false;
        routeOn = false;
        enableDialog(false);
    }

    public void showSchedule(bool enable){

        scheduleOn = enable;
        schedule.transform.GetChild(0).GetComponent<Image>().enabled = enable;
        schedule.transform.GetChild(1).GetComponent<Text>().enabled = enable;
        schedule.transform.GetChild(2).GetComponent<Button>().enabled = enable;
        schedule.transform.GetChild(2).GetComponent<Image>().enabled = enable;
        schedule.transform.GetChild(2).GetChild(0).GetComponent<Text>().enabled = enable;
    }

    void exitSchedule(){
        //scheduleOn = false;
        GameController.instance.restoreGreenField();
        GameController.instance.destroyDeadlineText();
        showSchedule(false);

    }

	// Update is called once per frame
	void Update () {
		
	}
}
