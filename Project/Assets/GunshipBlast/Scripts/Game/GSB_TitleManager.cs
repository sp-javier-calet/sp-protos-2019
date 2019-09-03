
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using SocialPoint.Rendering.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;

public class GSB_TitleManager : MonoBehaviour
{
    public CanvasGroup LogoScreen = null;
    public CanvasGroup VersusScreen = null;
    public CanvasGroup BehaviourScreen = null;
    public CanvasGroup ClientScreen = null;
    public CanvasGroup ServerScreen = null;
    public Button ConnectButton = null;
    public Button ServerStartButton = null;

    public TextMeshProUGUI YourIpLabel;
    public Image QRCodeImage;
    public TMP_InputField InputField;

    public static int ServerHost = -1;

    void SetScreenEnabled(CanvasGroup canvas, bool enabled)
    {
        if(canvas != null)
        {
            canvas.alpha = enabled ? 1f : 0f;
            canvas.interactable = enabled;
            canvas.blocksRaycasts = enabled;
        }
    }



    void Start()
    {
        StopServerOrClients();

        SetScreenEnabled(LogoScreen, true);
        SetScreenEnabled(VersusScreen, false);

        StartCoroutine(UpdateIpAddress());
    }

    void StopServerOrClients()
    {
        if (ServerHost == 1)
        {
            GSB_GameManager.Instance.NetworkController.StopHost();
        }
        else if (ServerHost == 0)
        {
            GSB_GameManager.Instance.NetworkController.StopClient();
        }

        ServerHost = -1;
    }

    public void OnPressed1Player()
    {
        GSB_GameManager.Instance.SetGameState(GSB_GameManager.GameState.E_PLAYING_1_PLAYER);
    }

    public void OnPressed2Versus()
    {
        ConnectButton.interactable = true;
        ServerStartButton.interactable = false;

        SetScreenEnabled(LogoScreen, false);
        SetScreenEnabled(VersusScreen, true);

        SetScreenEnabled(BehaviourScreen, true);
        SetScreenEnabled(ServerScreen, false);
        SetScreenEnabled(ClientScreen, false);
    }

    public void OnPressed4Back()
    {
        StopServerOrClients();

        SetScreenEnabled(LogoScreen, true);
        SetScreenEnabled(VersusScreen, false);
    }

    public void OnPressedServer()
    {
        ServerHost = 1;

        GSB_GameManager.Instance.NetworkController.StartHost();

        SetScreenEnabled(BehaviourScreen, false);
        SetScreenEnabled(ServerScreen, true);
    }

    public void OnPressedClient()
    {
        SetScreenEnabled(BehaviourScreen, false);
        SetScreenEnabled(ClientScreen, true);
    }

    public void OnPressedConnect()
    {
        ServerHost = 0;
        ConnectButton.interactable = false;

        GSB_GameManager.Instance.NetworkController.networkAddress = InputField.text;
        GSB_GameManager.Instance.NetworkController.StartClient();
    }

    public void OnPressedStart()
    {

    }

    IEnumerator UpdateIpAddress()
    {
        yield return null;

        var ip = "No wifi";
        while (true)
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if(ni == null)
                {
                    continue;
                }

                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    foreach (var ipInfo in ni.GetIPProperties().UnicastAddresses)
                    {
                        if(ipInfo == null)
                        {
                            continue;
                        }

                        if (ipInfo.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            ip = ipInfo.Address.ToString();
                        }
                    }
                }
            }

            YourIpLabel.text = ip;

            var qrCode = GenerateQR(ip);
            QRCodeImage.sprite = Sprite.Create(qrCode, new Rect(0, 0, qrCode.width, qrCode.height), new Vector2(0.5f, 0.5f));

            yield return new WaitForSeconds(1);
        }
    }

    static Color32[] Encode(string textForEncoding, int width, int height)
    {
        var writer = new BarcodeWriter
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                Height = height,
                Width = width
            }
        };
        return writer.Write(textForEncoding);
    }

    Texture2D GenerateQR(string text)
    {
        var encoded = new Texture2D(256, 256);
        var color32 = Encode(text, encoded.width, encoded.height);
        encoded.SetPixels32(color32);
        encoded.Apply();
        return encoded;
    }

    void Update()
    {
        if(ServerStartButton != null)
        {
            ServerStartButton.interactable = (GSB_GameManager.Instance.NetworkGameState.NumPlayers > 1);
        }
    }
}
