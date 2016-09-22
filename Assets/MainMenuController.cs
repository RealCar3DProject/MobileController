using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{

    public GameObject InputFieldPanel;
    public GameObject loadingScreeenPanel;
    public GameObject joysticPanel;

    public Transform centerPosition;
    public WebCom webCom;
    public InputField roomID;

    private BetweenTransform inputFieldBetweenTransform;
    private BetweenTransform loadingScreen;
    private BetweenTransform joystic;
    private bool isConnected;

    BetweenTransform LoadingScreen
    {
        get
        {
            if (loadingScreen == null)
            {
                return loadingScreen = loadingScreeenPanel.GetComponent<BetweenTransform>();
            }
            return loadingScreen;
        }

        set { loadingScreen = value; }
    }

    BetweenTransform InputFieldBetweenTransform
    {
        get
        {
            if (inputFieldBetweenTransform == null)
            {
                return inputFieldBetweenTransform = InputFieldPanel.GetComponent<BetweenTransform>();
            }

            return inputFieldBetweenTransform;
        }

        set { inputFieldBetweenTransform = value; }
    }

    BetweenTransform Joystic
    {
        get
        {
            if (joystic == null)
            {
                return joystic = joysticPanel.GetComponent<BetweenTransform>();
            }

            return joystic;
        }

        set { joystic = value; }
    }

    public void PlayGame()
    {
        InputFieldBetweenTransform.To = centerPosition;
        InputFieldBetweenTransform.PlayForward();
    }

    public void StartLoadingScreen()
    {
        LoadingScreen.To = centerPosition;
        LoadingScreen.OnFinish.AddListener(StartLoadingScreenAnimation);
        LoadingScreen.PlayForward();
        InputFieldBetweenTransform.ResetToBeginning();
    }

    private void StartLoadingScreenAnimation()
    {
        StartCoroutine(LoadingScreenAnimation());
    }

    private IEnumerator LoadingScreenAnimation()
    {
        webCom.JoinRoomButtonPressed(roomID.text);

        yield return new WaitUntil((() => WebCom.isConnceted));

        LoadingScreen.ResetToBeginning();
        Joystic.To = centerPosition;
        Joystic.PlayForward();
    }

    public void LeaveButton()
    {
        roomID.text = "";
        Joystic.PlayReverse();
    }
}
