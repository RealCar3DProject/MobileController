using UnityEngine;
using System.Collections;

public class JoystickController : MonoBehaviour
{

    private bool sendForward;
    private bool sendBackwords;

    private void FixedUpdate()
    {
        ForwardComands();

        BackwardComands();

    }

    private void BackwardComands()
    {
        if (sendBackwords)
        {
            SendBackwardData();
        }
        else
        {
            SendBackwardCoolDown();
        }
    }
    private void ForwardComands()
    {
        if (sendForward)
        {
            SendForwardData();
        }
        else
        {
            ForwardCoolDOwn();
        }
    }
    public void SendForwardData()
    {
        sendForward = true;
        WebCom.carDataController.forwardCoolDown = !sendForward;

    }
    public void ForwardCoolDOwn()
    {
        sendForward = false;
        WebCom.carDataController.forwardCoolDown = !sendForward;

    }
    public void SendBackwardData()
    {
        sendBackwords = true;
        WebCom.carDataController.backwordCoolDown = !sendBackwords;

    }

    public void Break()
    {
        WebCom.carDataController.Break = true;
    }

    public void ReleaseBreak()
    {
        WebCom.carDataController.Break = false;

    }
    public void SendBackwardCoolDown()
    {
        sendBackwords = false;
        WebCom.carDataController.backwordCoolDown = !sendBackwords;
    }
}
