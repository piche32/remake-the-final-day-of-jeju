using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;
using Blocks.Common;

namespace Jeju
{
    [UxmlElement]
    public partial class LobbyStartButton : Button
    {
        const string k_LobbyStartButtonText = "Start Game";
        List<DataBinding> m_Bindings = new();
        LobbyStartViewModel m_ViewModel;

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

        public LobbyStartButton()
        {
            text = k_LobbyStartButtonText;

            AddToClassList(BlocksTheme.Button);
            var enableBinding = new DataBinding()
            {
                dataSourcePath = new PropertyPath(nameof(LobbyStartViewModel.CanStartGame)),
                bindingMode = BindingMode.ToTarget
            };
            SetBinding(new BindingId(nameof(enabledSelf)), enableBinding);
            m_Bindings.Add(enableBinding);

            var visibleBinding = new DataBinding()
            {
                dataSourcePath = new PropertyPath(nameof(LobbyStartViewModel.IsHost)),
                bindingMode = BindingMode.ToTarget
            };
            SetBinding(new BindingId(nameof(Visibility)), visibleBinding);
            m_Bindings.Add(visibleBinding);

            clicked += StartGame;

            RegisterCallback<AttachToPanelEvent>(_ => UpdateBindings());
            RegisterCallback<DetachFromPanelEvent>(_ => CleanupBindings());
        }

        void UpdateBindings()
        {
            CleanupBindings();

            m_ViewModel = new LobbyStartViewModel(m_SessionType);

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

        void StartGame()
        {
            m_ViewModel.StartGame();
        }

    }

}
