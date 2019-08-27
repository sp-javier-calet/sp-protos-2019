
using System.Collections;
using System.Net.NetworkInformation;
using SocialPoint.Rendering.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;

public class CP_TitleManager : MonoBehaviour
{
    public CanvasGroup LogoScreen = null;
    public CanvasGroup VersusScreen = null;
    public CanvasGroup BehaviourScreen = null;
    public CanvasGroup ClientScreen = null;
    public CanvasGroup ServerScreen = null;
    public BCSHModifier TitleBCSH = null;

    public TextMeshProUGUI YourIpLabel;
    public Image QRCodeImage;
    public TMP_InputField InputField;

    CP_NetworkController _networkController;

    CP_NetworkController NetworkController
    {
        get
        {
            if(_networkController == null)
            {
                _networkController = FindObjectOfType<CP_NetworkController>();
            }

            return _networkController;
        }
    }

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
        SetScreenEnabled(LogoScreen, true);
        SetScreenEnabled(VersusScreen, false);

        StartCoroutine(UpdateIpAddress());
    }

    public void OnPressed1Player()
    {
        CP_GameManager.Instance.SetGameState(CP_GameManager.GameState.E_PLAYING_1_PLAYER);
    }

    public void OnPressed4Versus()
    {
        SetScreenEnabled(LogoScreen, false);
        SetScreenEnabled(VersusScreen, true);

        SetScreenEnabled(BehaviourScreen, true);
        SetScreenEnabled(ServerScreen, false);
        SetScreenEnabled(ClientScreen, false);
    }

    public void OnPressed4Back()
    {
        SetScreenEnabled(LogoScreen, true);
        SetScreenEnabled(VersusScreen, false);
    }

    public void OnPressedServer()
    {
        NetworkController.StartHost();

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

    }

    public void OnPressedStart()
    {

    }

    IEnumerator UpdateIpAddress()
    {
        var ip = "No wifi";
        while (true)
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    foreach (var ipInfo in ni.GetIPProperties().UnicastAddresses)
                    {
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
        if(TitleBCSH != null)
        {
            TitleBCSH.Hue += 0.01f;
            if(TitleBCSH.Hue > 1.0f)
            {
                TitleBCSH.Hue -= 1.0f;
            }
        }
    }
}
