using System.Collections.Generic;
using UnityEngine.UIElements;
using Unity.Properties;
using Blocks.Common;
using System;

namespace Jeju
{
    [UxmlElement]
    public partial class LobbyReadyButton : Button
    {
        const string k_LobbyReadyButtonText = "Ready";
        List<DataBinding> m_Bindings = new();

        LobbyReadyViewModel m_ViewModel;

        [CreateProperty, UxmlAttribute]
        public string SessionType
        {
            get => m_SessionType;
            set
            {
                if (m_SessionType == value)
                {
                    return;
                }
                m_SessionType = value;
                if (panel != null)
                {
                    UpdateBindings();
                }
            }
        }
        string m_SessionType;

        [CreateProperty, UxmlAttribute]
        public bool IsSelected
        {
            get => m_IsSelected;
            set
            {
                if (m_IsSelected == value)
                {
                    return;
                }
                m_IsSelected = value;
                UpdateUI();

            }
        }

        bool m_IsSelected;

        [CreateProperty, UxmlAttribute]
        public bool Visibility
        {
            get => style.display == DisplayStyle.Flex;
            set => style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public LobbyReadyButton()
        {
            text = k_LobbyReadyButtonText;

            AddToClassList(BlocksTheme.Button);

            var seletedBinding = new DataBinding()
            {
                dataSourcePath = new PropertyPath(nameof(LobbyReadyViewModel.IsReady)),
                bindingMode = BindingMode.ToTarget
            };
            SetBinding(new BindingId(nameof(IsSelected)), seletedBinding);
            m_Bindings.Add(seletedBinding);

            var displayBinding = new DataBinding()
            {
                dataSourcePath = new PropertyPath(nameof(LobbyReadyViewModel.IsClient)),
                bindingMode = BindingMode.ToTarget
            };
            SetBinding(new BindingId(nameof(Visibility)), displayBinding);
            m_Bindings.Add(displayBinding);

            clicked += OnClick;

            focusable = false;

            RegisterCallback<AttachToPanelEvent>(_ => UpdateBindings());
            RegisterCallback<DetachFromPanelEvent>(_ => CleanupBindings());
        }

        void UpdateBindings()
        {
            CleanupBindings();

            m_ViewModel = new LobbyReadyViewModel(m_SessionType);

            foreach (var binding in m_Bindings)
            {
                binding.dataSource = m_ViewModel;
            }
        }

        void CleanupBindings()
        {
            m_ViewModel?.Dispose();
            m_ViewModel = null;

            foreach (var binding in m_Bindings)
            {
                if (binding.dataSource is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                binding.dataSource = null;
            }
        }

        void OnClick()
        {
            m_ViewModel.SetReady();
        }

        void UpdateUI()
        {
            EnableInClassList("is-on", IsSelected);
        }
    }

}