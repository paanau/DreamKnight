using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonScript : MonoBehaviour
{
    private string identity;
    private GameController gc;
    [SerializeField] private GameObject menuBackground;

    // Start is called before the first frame update
    void Start()
    {
        gc = GameObject.Find("GameController").GetComponent<GameController>();
        string s = gameObject.name;
        identity = s.IndexOf(" ") > 0 ? s.Substring(0, s.IndexOf(" ")) : s;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public string PressMe()
    {
        PressAction();
        Debug.Log("I am " + identity);
        return identity;
    }

    private void PressAction()
    {
        switch (identity)
        {
            case "Pause":
                TogglePause(true);
                break;
            case "MenuBackground":
                TogglePause(false);
                break;
            case "Play":
                TogglePause(true);
                break;
            case "Settings":
                break;
            default:
                break;
        }
    }

    private void TogglePause(bool b)
    {
        gc.TogglePause(b);
        Debug.Log(b);
    }
}
