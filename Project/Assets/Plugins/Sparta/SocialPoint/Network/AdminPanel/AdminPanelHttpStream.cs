#if ADMIN_PANEL 

using System.Text;
using SocialPoint.AdminPanel;
using SocialPoint.Base;
using SocialPoint.Network;
using System.Collections.Generic;

public sealed class AdminPanelHttpStream : IAdminPanelConfigurer, IAdminPanelGUI
{
    const string InfoUrl = "https://http2.golang.org/reqinfo";
    const string ClockUrl = "https://http2.golang.org/clockstream";
    const string EchoUrl = "https://http2.golang.org/ECHO";

    enum MessageEncoding
    {
        ASCII,
        UTF8,
        Binary
    }

    struct StreamData
    {
        public string Name;
        public IHttpStream Stream;
    }

    readonly CurlHttpStreamClient _client;
    AdminPanelConsole _console;
    MessageEncoding _encoding = MessageEncoding.ASCII;
    bool _verbose;
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
        layout.CreateButton("Open Info stream", () => {
            var req = new HttpRequest(InfoUrl);
            var conn = _client.Connect(req, OnRequestFinished);
            conn.DataReceived += OnDataReceived;
            _streams.Add(new StreamData { Name = "Info", Stream = conn });
            layout.Refresh();
        });

        layout.CreateButton("Open Clock stream", () => {
            var req = new HttpRequest(ClockUrl);
            var conn = _client.Connect(req, OnRequestFinished);
            conn.DataReceived += OnDataReceived;
            _streams.Add(new StreamData { Name = "Clock", Stream = conn });
            layout.Refresh();
        });

        layout.CreateButton("Open Echo stream", () => {
            var req = new HttpRequest(EchoUrl);
            req.Method = HttpRequest.MethodType.PUT;
            req.Body = Encode("hello");
            var conn = _client.Connect(req, OnRequestFinished);
            conn.DataReceived += OnDataReceived;
            _streams.Add(new StreamData { Name = "Echo", Stream = conn });
            layout.Refresh();
        });

        layout.CreateButton("Send message", () => {
            for(int i = 0, _streamsCount = _streams.Count; i < _streamsCount; i++)
            {
                var st = _streams[i];
                if(st.Stream.Active)
                {
                    st.Stream.SendData(Encode("Message"));
                }
            }
        });

        layout.CreateMargin();

        layout.CreateLabel("Client Info");

        layout.CreateTextArea(_client.Info);

        layout.CreateToggleButton("Verbose", _verbose, (value) => {
            _verbose = value;
            if(_client != null)
            {
                _client.Verbose = _verbose;
            }
        });

        using(var hlayout = layout.CreateHorizontalLayout())
        {
            hlayout.CreateToggleButton("ASCII", _encoding == MessageEncoding.ASCII,
                value => {
                    _encoding = MessageEncoding.ASCII;
                    layout.Refresh();
                });
            hlayout.CreateToggleButton("UTF8", _encoding == MessageEncoding.UTF8,
                value => {
                    _encoding = MessageEncoding.UTF8;
                    layout.Refresh();
                });
            hlayout.CreateToggleButton("Binary", _encoding == MessageEncoding.Binary,
                value => {
                    _encoding = MessageEncoding.Binary;
                    layout.Refresh();
                });
        }

        layout.CreateMargin();

        layout.CreateLabel("Active streams");

        for(int i = 0, _streamsCount = _streams.Count; i < _streamsCount; i++)
        {
            var data = _streams[i];
            layout.CreateToggleButton(data.Name, data.Stream.Active, value =>  {
                if(!value)
                {
                    data.Stream.Cancel();
                }
            });
        }
    }

    byte[] Encode(string data)
    {
        switch(_encoding)
        {
        case MessageEncoding.ASCII:
        default:
            return Encoding.ASCII.GetBytes(data);
        case MessageEncoding.UTF8:
            return Encoding.UTF8.GetBytes(data);
        case MessageEncoding.Binary:
            return Encoding.BigEndianUnicode.GetBytes(data);
        }
    }

    string Decode(byte[] data)
    {
        switch(_encoding)
        {
        case MessageEncoding.ASCII:
        default:
            return Encoding.ASCII.GetString(data);
        case MessageEncoding.UTF8:
            return Encoding.UTF8.GetString(data);
        case MessageEncoding.Binary:
            var content = new StringBuilder();
            for(int i = 0; i < data.Length; ++i)
            {
                content.AppendFormat("{0:x2}", data[i]);
                if(i % 4 == 0)
                {
                    content.Append(" ");
                }
                if(i % 16 == 0)
                {
                    content.AppendLine();
                }
            }
            return content.ToString();
        }
    }

    void OnDataReceived(byte[] data)
    {   
        var msg = Decode(data);
        _console.Print(msg);
    }

    void OnRequestFinished(HttpResponse response)
    {
        var msg = string.Empty;
        var er = response.Error;
        if(!Error.IsNullOrEmpty(er))
        {
            msg = er.ToString();
        }
        else
        {
            msg = Decode(response.Body);   
        }
        _console.Print(msg);
    }
}

#endif
