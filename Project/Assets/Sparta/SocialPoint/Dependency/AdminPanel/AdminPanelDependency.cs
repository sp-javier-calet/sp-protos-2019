#if ADMIN_PANEL 

using System.Text;
using System.Collections.Generic;
using SocialPoint.AdminPanel;
using SocialPoint.Dependency.Graph;

namespace SocialPoint.Dependency
{
    public class AdminPanelDependency : IAdminPanelConfigurer, IAdminPanelGUI
    {
        struct Filter
        {
            public string Name;
            public bool Instantiated;
            public bool Root;
            public bool Interface;
            public bool NullValue;
        }

        Filter _filter;

        readonly AdminPanelDependencyNode _nodePanel;

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            if(DependencyGraphBuilder.IsAvailable)
            {
                adminPanel.RegisterGUI("Dependency", this);
            }
        }

        public AdminPanelDependency()
        {
            _nodePanel = new AdminPanelDependencyNode();
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            var graph = DependencyGraphBuilder.Graph;

            CreateFilterOptions(layout);

            var itr = graph.GetEnumerator();
            while(itr.MoveNext())
            {
                var node = itr.Current;
                if(IsFiltered(node))
                {
                    layout.CreateButton(node.Name, GetColor(node), () => {
                        _nodePanel.Node = node;
                        layout.OpenPanel(_nodePanel);
                    });
                }
            }
            itr.Dispose();
        }

        void CreateFilterOptions(AdminPanelLayout layout)
        {
            var foldoutLayout = layout.CreateFoldoutLayout("Filter");
            var contentLayout = foldoutLayout.CreateVerticalLayout();
            contentLayout.CreateTextInput(string.IsNullOrEmpty(_filter.Name) ? "Filter" : _filter.Name, 
                value => {
                    _filter.Name = string.IsNullOrEmpty(value) ? null : value.ToLower();
                });

            var hlayout = contentLayout.CreateHorizontalLayout();
            hlayout.CreateToggleButton("Root", _filter.Root, ButtonColor.Green, value => {
                _filter.Root = value;
            });
            hlayout.CreateToggleButton("Instantiated", _filter.Instantiated, ButtonColor.Blue, value => {
                _filter.Instantiated = value;
            });
            hlayout.CreateToggleButton("Interface", _filter.Interface, ButtonColor.Yellow, value => {
                _filter.Interface = value;
            });
            hlayout.CreateToggleButton("Null", _filter.NullValue, ButtonColor.Red, value => {
                _filter.NullValue = value;
            });
            contentLayout.CreateButton("Apply", layout.Refresh);
            layout.CreateMargin(2);
        }

        bool IsFiltered(Node node)
        {
            bool included = true;

            if(_filter.Root)
            {
                included &= node.IsRoot;
            }
            if(_filter.NullValue)
            {
                included &= node.HasNullValue;
            }
            if(_filter.Instantiated)
            {
                included &= node.Instantiated;
            }
            if(_filter.Interface)
            {
                included &= node.IsInterface;
            }
            if(!string.IsNullOrEmpty(_filter.Name))
            {
                included &= node.Name.ToLower().Contains(_filter.Name);
            }
            return included;
        }

        static ButtonColor GetColor(Node node)
        {
            if(node.IsRoot)
            {
                return ButtonColor.Green;
            }
            if(node.HasNullValue)
            {
                return ButtonColor.Red;
            }
            if(node.Instantiated)
            {
                return ButtonColor.Blue;
            }
            return node.IsInterface ? ButtonColor.Yellow : ButtonColor.Gray;
        }


        class AdminPanelDependencyNode : IAdminPanelGUI
        {
            public Node Node;

            readonly StringBuilder _content;
            readonly StringBuilder _history;

            public AdminPanelDependencyNode()
            {
                _content = new StringBuilder();
                _history = new StringBuilder();
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

                layout.CreateLabel("History");
                layout.CreateVerticalScrollLayout().CreateTextArea(CollectNodeHistory());
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
                    .Append("Has Null Value: ").AppendLine(Node.HasNullValue.ToString())
                    .AppendLine();
                
                if(Node.IsRoot || Node.Instigator != null)
                {
                    _content
                        .Append("Root class: ").AppendLine(Node.IsRoot.ToString())
                        .Append("Instigator: ").AppendLine(Node.Instigator != null ? Node.Instigator.Class ?? "<none>" : "<none>")
                        .AppendLine();
                }

                return _content.ToString();
            }

            public string CollectNodeHistory()
            {
                _history.Length = 0;
                for(var i = 0; i < Node.History.Count; ++i)
                {
                    var action = Node.History[i];
                    _history.Append(" - ").AppendLine(action.ToString());
                }
                return _history.ToString();
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

#endif
