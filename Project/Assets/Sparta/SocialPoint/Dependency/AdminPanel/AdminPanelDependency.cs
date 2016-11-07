using System.Text;
using System.Collections.Generic;
using SocialPoint.AdminPanel;

namespace SocialPoint.Dependency
{
    public class AdminPanelDependency : IAdminPanelConfigurer, IAdminPanelGUI
    {
        DependencyGraph _graph;

        readonly AdminPanelDependencyNode _nodePanel;

        public AdminPanelDependency()
        {
            _nodePanel = new AdminPanelDependencyNode();
        }

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

            for(var i = 0; i < _graph.RootNodes.Count; ++i)
            {
                var node = _graph.RootNodes[i];
                layout.CreateButton(node.Class, () => {
                    _nodePanel.Node = node;
                    layout.OpenPanel(_nodePanel);
                });
            }

            layout.CreateMargin();
            layout.CreateButton("Refresh", layout.Refresh);
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
                layout.CreateLabel(Node.Class);
                layout.CreateMargin();
                layout.CreateTextArea(CollectNodeContent());
                if(!string.IsNullOrEmpty(Node.CreationStack))
                {
                    layout.CreateVerticalScrollLayout().CreateTextArea(Node.CreationStack);
                }
                layout.CreateMargin();
                CreateNodeLinkList(layout, "Aliases", Node.Aliases);
                CreateNodeLinkList(layout, "Definitions", Node.Definitions);
                CreateNodeLinkList(layout, "Dependencies", Node.Outcoming);
                CreateNodeLinkList(layout, "Needed for", Node.Incoming);
            }

            public string CollectNodeContent()
            {
                _content.Length = 0;

                _content
                    .Append("Class: ").AppendLine(Node.Class)
                    .Append("Namespace: ").AppendLine(Node.Namespace)
                    .Append("Tag: ").AppendLine(Node.Tag)
                    .Append("Binds: ").AppendLine(Node.Bind)
                    .Append("Is Single: ").AppendLine(Node.IsSingle.ToString())
                    .AppendLine();
                _content.AppendLine("History:");
                for(var i = 0; i < Node.History.Count; ++i)
                {
                    var action = Node.History[i];
                    _content.Append(" - ").AppendLine(action.ToString());
                }

                if(Node.IsRoot || Node.Instigator != null)
                {
                    _content
                        .AppendLine("Creation: ")
                        .Append("Root class: ").AppendLine(Node.IsRoot.ToString())
                        .Append("Instigator: ").AppendLine(Node.Instigator != null ? Node.Instigator.Class ?? "<none>" : "<none>")
                        .AppendLine();
                }

                return _content.ToString();
            }

            public void CreateNodeLinkList(AdminPanelLayout layout, string label, List<Node> list)
            {
                if(list.Count > 0)
                {
                    layout.CreateLabel(label);
                    for(var i = 0; i < list.Count; ++i)
                    {
                        var node = list[i];
                        layout.CreateButton(node.Class, () => {
                            Node = node;
                            layout.Refresh();
                        });
                    }
                    layout.CreateMargin();
                }
            }
        }
    }
}
