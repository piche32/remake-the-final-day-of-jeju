using System;
using System.Runtime.CompilerServices;
using System.Linq;
using Unity.Netcode;
using Unity.Properties;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
public class LobbyStartViewModel : IDataSourceViewHashProvider, INotifyBindablePropertyChanged
{
    SessionObserver m_SessionObserver;
    ISession m_Session;
    long m_UpdateVersion;
    [CreateProperty]
    public bool CanStartGame
    {
        get => m_CanStartGame;
        private set
        {
            if (m_CanStartGame == value)
            {
                return;
            }
            m_CanStartGame = value;
            ++m_UpdateVersion;
            Notify();
        }
    }
    bool m_CanStartGame;
    [CreateProperty]
    public bool IsHost
    {
        get => m_IsHost;
        private set
        {
            if (m_IsHost == value)
            {
                return;
            }
            m_IsHost = value;
            ++m_UpdateVersion;
            Notify();
        }
    }
    bool m_IsHost;

    public LobbyStartViewModel(string sessionType)
    {
        m_SessionObserver = new SessionObserver(sessionType);
        m_SessionObserver.SessionAdded += OnSessionAdded;

        if (m_SessionObserver.Session != null)
        {
            OnSessionAdded(m_SessionObserver.Session);
        }
    }

    void OnSessionAdded(ISession newSession)
    {
        if (newSession.IsHost)
        {
            IsHost = true;

            m_Session = newSession;
            m_Session.RemovedFromSession += OnSessionRemoved;
            m_Session.Deleted += OnSessionRemoved;

            m_Session.PlayerJoined += UpdateCanStartGameWithPlayerId;
            m_Session.PlayerHasLeft += UpdateCanStartGameWithPlayerId;
            m_Session.PlayerPropertiesChanged += UpdateCanStartGame;

            UpdateCanStartGame();
        }
        else
        {
            IsHost = false;
        }
    }

    void OnSessionRemoved()
    {
        CleanupSession();
    }
    void CleanupSession()
    {
        m_Session.RemovedFromSession -= OnSessionRemoved;
        m_Session.Deleted -= OnSessionRemoved;

        m_Session.PlayerJoined -= UpdateCanStartGameWithPlayerId;
        m_Session.PlayerHasLeft -= UpdateCanStartGameWithPlayerId;
        m_Session.PlayerPropertiesChanged -= UpdateCanStartGame;
        m_Session = null;
    }

    public void Dispose()
    {
        if (m_SessionObserver != null)
        {

            m_SessionObserver.SessionAdded -= OnSessionAdded;
            m_SessionObserver.Dispose();
            m_SessionObserver = null;
        }

        if (m_Session != null)
        {
            CleanupSession();
        }
    }

    void UpdateCanStartGameWithPlayerId(string playerId)
    {
        UpdateCanStartGame();
    }

    private bool m_IsUpdating = false;
    async void UpdateCanStartGame()
    {
        if (m_IsUpdating) return;
        m_IsUpdating = true;

        await Awaitable.NextFrameAsync();

        m_IsUpdating = false;

        if (m_Session == null)
        {
            CanStartGame = false;
            return;
        }
        if (m_Session.Players == null || m_Session.Players.Count <= 1)
        {
            CanStartGame = false;
            return;
        }

        bool allClientsReady = m_Session.Players
            .Where(p => p.Id != m_Session.CurrentPlayer.Id)
            .All(p =>
                p.Properties.TryGetValue(Define.Network.Ready, out PlayerProperty readyProperty) &&
                bool.TryParse(readyProperty.Value, out bool isReady) &&
                isReady);

        CanStartGame = allClientsReady;
    }

    public void StartGame()
    {
        if (NetworkManager.Singleton?.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(Define.SceneName.Main, LoadSceneMode.Single);
        }
    }

    public long GetViewHashCode() => m_UpdateVersion;
    public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

    void Notify([CallerMemberName] string property = null)
    {
        propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
    }

}
