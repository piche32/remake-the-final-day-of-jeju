using System;
using System.Runtime.CompilerServices;
using Unity.Properties;
using Unity.Services.Multiplayer;
using UnityEngine.UIElements;
public class LobbyReadyViewModel : IDataSourceViewHashProvider, INotifyBindablePropertyChanged
{
    SessionObserver m_SessionObserver;
    ISession m_Session;
    long m_UpdateVersion;

    [CreateProperty]
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

    [CreateProperty]
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

    public void SetReady()
    {
        IsReady = !IsReady;
        PlayerProperty ReadyProperty = new PlayerProperty(IsReady.ToString());

        m_Session?.CurrentPlayer?.SetProperty(Define.Network.Ready, ReadyProperty);
    }

    public long GetViewHashCode() => m_UpdateVersion;
    public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;
    void Notify([CallerMemberName] string property = null)
    {
        propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
    }
}
