using SocialPoint.AdminPanel;
using SocialPoint.Base;
using SocialPoint.Network;
using SocialPoint.Utils;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;

public sealed class AdminPanelHttpClient : IAdminPanelConfigurer, IAdminPanelGUI
{
    readonly ICoroutineRunner _runner;
    IHttpStreamClient _client;
    AdminPanelConsole _console;

    struct StreamData
    {
        public string Name;
        public CurlHttpStream Stream; 
    }

    List<StreamData> _streams;

    public AdminPanelHttpClient(ICoroutineRunner runner)
    {
        _runner = runner;
        _streams = new List<StreamData>();
    }

    public void OnConfigure(AdminPanel adminPanel)
    {
        adminPanel.RegisterGUI("Http Stream", this);
        _console = adminPanel.Console;
    }

    public void OnCreateGUI(AdminPanelLayout layout)
    {  
        var enabled = _client != null;
        layout.CreateToggleButton("Enable Client", enabled, value => 
            {
                if(value)
                {
                    _client = new CurlHttpStreamClient(_runner);
                }
                else
                {
                    _client.Dispose();
                    _client = null;
                    _streams.Clear();
                }
                layout.Refresh();
            });
        
        layout.CreateButton("Open Clock stream", () => {
            var req = new HttpRequest("https://http2.golang.org/clockstream");
            req.Timeout = 10000000;
            req.Proxy = EditorProxy.GetProxy();
            var conn = _client.Connect(req, OnRequestFinished);
            conn.DataReceived += OnDataReceived;
            _streams.Add(new StreamData { Name = "Clock", Stream = conn as CurlHttpStream });
            layout.Refresh();
        }, enabled);

        layout.CreateButton("Open Echo stream", () => {
            var req = new HttpRequest("https://http2.golang.org/ECHO");
            req.Method = HttpRequest.MethodType.PUT;
            req.Body = System.Text.ASCIIEncoding.ASCII.GetBytes("hello");
            req.Timeout = 10000000;
            req.Proxy = EditorProxy.GetProxy();
            var conn = _client.Connect(req, OnRequestFinished);
            conn.DataReceived += OnDataReceived;
            _streams.Add(new StreamData { Name = "Echo", Stream = conn as CurlHttpStream });
            layout.Refresh();
        }, enabled);


        layout.CreateMargin();

        foreach(var data in _streams)
        {
            layout.CreateToggleButton(data.Name, data.Stream.Active, (value) => {
                if(!value)
                {
                    data.Stream.Cancel();
                }
            }, enabled);
        }
    }

    void OnDataReceived(byte[] data)
    {   
        var msg = System.Text.ASCIIEncoding.ASCII.GetString(data);
        Log.d("Received http message: " + msg);
        _console.Print(msg);

    }

    void OnRequestFinished(HttpResponse response)
    {
        var msg = System.Text.ASCIIEncoding.ASCII.GetString(response.Body);
        Log.d("Finished request with body: " + msg); 
        _console.Print(msg);
    }
}
