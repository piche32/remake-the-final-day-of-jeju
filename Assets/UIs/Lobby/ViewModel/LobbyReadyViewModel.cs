using System;
using System.Runtime.CompilerServices;
using Unity.Properties;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UIElements;
public class LobbyReadyViewModel : IDataSourceViewHashProvider, INotifyBindablePropertyChanged
{
    SessionObserver m_SessionObserver;
    ISession m_Session;
    long m_UpdateVersion;

    [CreateProperty, UxmlAttribute]
    public bool IsClient
    {
        get => m_IsClient;
        private set
        {
            if (m_IsClient == value)
            {
                return;
            }
            m_IsClient = value;
            ++m_UpdateVersion;
            Notify();
        }
    }
    bool m_IsClient;

    [CreateProperty, UxmlAttribute]
    public bool IsReady
    {
        get => m_IsReady;
        private set
        {
            if (m_IsReady == value)
            {
                return;
            }
            m_IsReady = value;
            ++m_UpdateVersion;
            Notify();
        }
    }
    bool m_IsReady;

    public LobbyReadyViewModel(string sessionType)
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
        IsReady = false;

        if (!newSession.IsHost && !newSession.IsServer)
        {
            IsClient = true;
            m_Session = newSession;
            m_Session.RemovedFromSession += OnSessionRemoved;
            m_Session.Deleted += OnSessionRemoved;
        }
        else
        {
            IsClient = false;
        }
    }

    void OnSessionRemoved()
    {
        CleanupSession();

        IsClient = false;
        IsReady = false;
    }
    void CleanupSession()
    {
        m_Session.RemovedFromSession -= OnSessionRemoved;
        m_Session.Deleted -= OnSessionRemoved;

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

    public async void SetReady()
    {
        if (m_Session == null)
        {
            Debug.LogError("Session is null");
            return;
        }
        if (m_Session.CurrentPlayer == null)
        {
            Debug.LogError("Player is null");
            return;
        }

        IsReady = !IsReady;
        var readyProperty = new PlayerProperty(IsReady.ToString());

        try
        {
            m_Session.CurrentPlayer.SetProperty(Define.Network.Ready, readyProperty);
            await m_Session.SaveCurrentPlayerDataAsync();

        }
        catch (Exception e)
        {
            Debug.LogError($"Error setting ready property: {e.Message}");
            IsReady = !IsReady;
        }
    }

    public long GetViewHashCode() => m_UpdateVersion;
    public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;
    void Notify([CallerMemberName] string property = null)
    {
        propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
    }
}
