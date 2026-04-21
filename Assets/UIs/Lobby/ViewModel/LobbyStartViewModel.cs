using System;
using System.Runtime.CompilerServices;
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

            m_Session.PlayerJoined += UpdateCanStartGame;
            m_Session.PlayerHasLeft += UpdateCanStartGame;
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

        m_Session.PlayerJoined -= UpdateCanStartGame;
        m_Session.PlayerHasLeft -= UpdateCanStartGame;
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

    void UpdateCanStartGame(string playerId)
    {
        UpdateCanStartGame();
    }

    void UpdateCanStartGame()
    {
        bool isAllReady = true;
        foreach (IReadOnlyPlayer player in m_Session.Players)
        {
            if (player.Id.Equals(m_Session.CurrentPlayer.Id))
            {
                continue;
            }
            if (!player.Properties.TryGetValue(Define.Network.Ready, out PlayerProperty Ready) || !bool.Parse(Ready.Value))
            {
                isAllReady = false;
                break;
            }
        }

        if (isAllReady == false)
        {
            CanStartGame = false;
            return;
        }
        else
        {
            CanStartGame = true;
        }
    }

    public void StartGame()
    {
        if (NetworkManager.Singleton?.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(Define.SceneName.Main, LoadSceneMode.Single);
        }
    }

    public long GetViewHashCode() => m_IsHost ? m_CanStartGame ? 2 : 1 : 0;
    public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

    void Notify([CallerMemberName] string property = null)
    {
        propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
    }

}
