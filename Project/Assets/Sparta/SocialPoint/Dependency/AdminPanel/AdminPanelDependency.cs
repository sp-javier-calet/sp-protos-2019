using System.Text;
using System.Collections.Generic;
using SocialPoint.AdminPanel;

namespace SocialPoint.Dependency
{
    public class AdminPanelDependency : IAdminPanelConfigurer, IAdminPanelGUI
    {
        DependencyGraph _graph;

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            if(DependencyGraphBuilder.IsAvailable)
            {
                adminPanel.RegisterGUI("Dependency", this);
            }
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            if(_graph == null)
            {
                _graph = DependencyGraphBuilder.Graph;
            }

            layout.CreateOpenPanelButton("Root Nodes", new AdminPanelDependencyNodeList(_graph.RootNodes));
            layout.CreateOpenPanelButton("All Bindings", new AdminPanelDependencyNodeList(_graph));
        }

        class AdminPanelDependencyNodeList : IAdminPanelGUI
        {
            readonly IEnumerable<Node> _list;
            readonly AdminPanelDependencyNode _nodePanel;

            public AdminPanelDependencyNodeList(IEnumerable<Node> nodes)
            {
                _list = nodes;
                _nodePanel = new AdminPanelDependencyNode();
            }

            public void OnCreateGUI(AdminPanelLayout layout)
            {
                var itr = _list.GetEnumerator();
                while(itr.MoveNext())
                {
                    var node = itr.Current;

                    layout.CreateButton(node.Name, GetColor(node), () => {
                        _nodePanel.Node = node;
                        layout.OpenPanel(_nodePanel);
                    });
                }
                itr.Dispose();

                layout.CreateMargin();
                layout.CreateButton("Refresh", layout.Refresh);
            }

            ButtonColor GetColor(Node node)
            {
                return node.Instantiated ? ButtonColor.Blue : ButtonColor.Gray;
            }
        }

        class AdminPanelDependencyNode : IAdminPanelGUI
        {
            public Node Node;

            readonly StringBuilder _content;

            public AdminPanelDependencyNode()
            {
                _content = new StringBuilder();
            }

            public void OnCreateGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel(Node.Name);
                layout.CreateMargin();
                layout.CreateTextArea(CollectNodeContent());
                if(!string.IsNullOrEmpty(Node.CreationStack))
                {
                    layout.CreateLabel("Creation Stack");
                    layout.CreateVerticalScrollLayout().CreateTextArea(Node.CreationStack);
                }
                layout.CreateMargin();
                CreateNodeLinkList(layout, "Implements", Node.Implements);
                CreateNodeLinkList(layout, "Aliases", Node.Aliases);
                CreateNodeLinkList(layout, "Definitions", Node.Definitions);
                CreateNodeLinkList(layout, "Dependencies", Node.Outcoming);
                CreateNodeLinkList(layout, "Needed for", Node.Incoming);
            }

            public string CollectNodeContent()
            {
                _content.Length = 0;
                _content
                    .Append("Class: ").Append(Node.Namespace).Append(".").AppendLine(Node.Class)
                    .Append("Tag: ").AppendLine(Node.Tag)
                    .Append("Is Interface: ").AppendLine(Node.IsInterface.ToString())
                    .Append("Is Single: ").AppendLine(Node.IsSingle.ToString())
                    .Append("Instantiated: ").AppendLine(Node.Instantiated.ToString())
                    .AppendLine();
                
                if(Node.IsRoot || Node.Instigator != null)
                {
                    _content
                        .Append("Root class: ").AppendLine(Node.IsRoot.ToString())
                        .Append("Instigator: ").AppendLine(Node.Instigator != null ? Node.Instigator.Class ?? "<none>" : "<none>")
                        .AppendLine();
                }
                
                _content.AppendLine("History:");
                for(var i = 0; i < Node.History.Count; ++i)
                {
                    var action = Node.History[i];
                    _content.Append(" - ").AppendLine(action.ToString());
                }

                return _content.ToString();
            }

            public void CreateNodeLinkList(AdminPanelLayout layout, string label, HashSet<Node> nodes)
            {
                if(nodes.Count > 0)
                {
                    layout.CreateLabel(label);
                    var itr = nodes.GetEnumerator();
                    while(itr.MoveNext())
                    {
                        var node = itr.Current;
                        layout.CreateButton(node.Name, () => {
                            Node = node;
                            layout.Refresh();
                        });
                    }
                    itr.Dispose();
                    layout.CreateMargin();
                }
            }
        }
    }
}
