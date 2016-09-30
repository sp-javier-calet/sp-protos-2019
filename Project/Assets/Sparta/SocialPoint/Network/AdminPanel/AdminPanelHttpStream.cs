using SocialPoint.AdminPanel;
using SocialPoint.Network;
using SocialPoint.Utils;
using System.Collections.Generic;

public sealed class AdminPanelHttpStream : IAdminPanelConfigurer, IAdminPanelGUI
{
    struct StreamData
    {
        public string Name;
        public IHttpStream Stream; 
    }

    readonly CurlHttpStreamClient _client;
    AdminPanelConsole _console;
    bool _verbose;
    IHttpStream _stream;
    List<StreamData> _streams;

    public AdminPanelHttpStream(CurlHttpStreamClient client)
    {
        _client = client;
        _streams = new List<StreamData>();
    }

    public void OnConfigure(AdminPanel adminPanel)
    {
        adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Http Stream", this));
        _console = adminPanel.Console;
    }

    public void OnCreateGUI(AdminPanelLayout layout)
    {
        layout.CreateToggleButton("Verbose", _verbose, (value) => 
            {
                _verbose = value;
                if(_client != null)
                {
                    _client.Verbose = _verbose;
                }
            });

        layout.CreateButton("Send message", () =>
            {
                foreach(var st in _streams)
                {
                    if(st.Stream.Active)
                    {
                        st.Stream.SendData(System.Text.ASCIIEncoding.ASCII.GetBytes("Message"));
                    }
                }
            });

        layout.CreateButton("Open Clock stream", () => {
            var req = new HttpRequest("https://http2.golang.org/clockstream");
            req.Timeout = 10000000;
            req.Proxy = EditorProxy.GetProxy();
            var conn = _client.Connect(req, OnRequestFinished);
            conn.DataReceived += OnDataReceived;
            _streams.Add(new StreamData { Name = "Clock", Stream = conn as CurlHttpStream });
            layout.Refresh();
        });

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
        });


        layout.CreateMargin();

        foreach(var data in _streams)
        {
            layout.CreateToggleButton(data.Name, data.Stream.Active, (value) => {
                if(!value)
                {
                    data.Stream.Cancel();
                }
            });
        }
    }

    void OnDataReceived(byte[] data)
    {   
        var msg = System.Text.ASCIIEncoding.ASCII.GetString(data);
        _console.Print(msg);

    }

    void OnRequestFinished(HttpResponse response)
    {
        var msg = System.Text.ASCIIEncoding.ASCII.GetString(response.Body);
        _console.Print(msg);
    }
}
