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

            var itr = _graph.RootNodes.GetEnumerator();
            while(itr.MoveNext())
            {
                var node = itr.Current;
                layout.CreateButton(node.Name, () => {
                    _nodePanel.Node = node;
                    layout.OpenPanel(_nodePanel);
                });
            }
            itr.Dispose();
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
                layout.CreateLabel(Node.Name);
                layout.CreateMargin();
                layout.CreateTextArea(CollectNodeContent());
                if(!string.IsNullOrEmpty(Node.CreationStack))
                {
                    layout.CreateLabel("Creation Stack");
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
                    .Append("Class: ").Append(Node.Namespace).Append(".").AppendLine(Node.Class)
                    .Append("Tag: ").AppendLine(Node.Tag)
                    .Append("Is Single: ").AppendLine(Node.IsSingle.ToString())
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
