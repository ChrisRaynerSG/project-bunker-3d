using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Centralized update loop for the entire project.
///
/// Instead of every component declaring its own Unity <c>Update()</c> message,
/// components implement <see cref="IUpdatable"/> and register themselves with
/// this manager. This keeps a single native <c>Update()</c> callback for the
/// whole project, which avoids the per-component Unity message overhead and
/// gives explicit control over update order and iteration safety.
/// </summary>
[DefaultExecutionOrder(-1000)]
public class UpdateManager : MonoBehaviour
{
    private static UpdateManager _instance;
    private static bool _isQuitting;

    /// <summary>
    /// Lazily created singleton instance. Accessing this before an instance
    /// exists (and while the application is running) creates a hidden
    /// <see cref="GameObject"/> that survives scene loads.
    /// </summary>
    public static UpdateManager Instance
    {
        get
        {
            if (_instance == null && !_isQuitting)
            {
                GameObject go = new GameObject(nameof(UpdateManager));
                _instance = go.AddComponent<UpdateManager>();
                DontDestroyOnLoad(go);
            }

            return _instance;
        }
    }

    private readonly List<IUpdatable> _updatables = new List<IUpdatable>();
    private readonly List<IUpdatable> _pendingAdd = new List<IUpdatable>();
    private readonly List<IUpdatable> _pendingRemove = new List<IUpdatable>();
    private bool _isIterating;

    /// <summary>
    /// Registers an updatable so it receives <see cref="IUpdatable.OnUpdate"/>
    /// calls from the single update loop.
    /// </summary>
    public static void Register(IUpdatable updatable)
    {
        if (_isQuitting || updatable == null) return;
        Instance.RegisterInternal(updatable);
    }

    /// <summary>
    /// Unregisters a previously registered updatable.
    /// </summary>
    public static void Unregister(IUpdatable updatable)
    {
        if (updatable == null || _instance == null) return;
        _instance.UnregisterInternal(updatable);
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void RegisterInternal(IUpdatable updatable)
    {
        if (_isIterating)
        {
            if (!_updatables.Contains(updatable) && !_pendingAdd.Contains(updatable))
            {
                _pendingAdd.Add(updatable);
            }

            _pendingRemove.Remove(updatable);
        }
        else if (!_updatables.Contains(updatable))
        {
            _updatables.Add(updatable);
        }
    }

    private void UnregisterInternal(IUpdatable updatable)
    {
        if (_isIterating)
        {
            if (!_pendingRemove.Contains(updatable))
            {
                _pendingRemove.Add(updatable);
            }

            _pendingAdd.Remove(updatable);
        }
        else
        {
            _updatables.Remove(updatable);
        }
    }

    /// <summary>
    /// The single update loop for the whole project. Dispatches
    /// <see cref="IUpdatable.OnUpdate"/> to every registered component.
    /// </summary>
    private void Update()
    {
        _isIterating = true;
        for (int i = 0; i < _updatables.Count; i++)
        {
            _updatables[i]?.OnUpdate();
        }
        _isIterating = false;

        FlushPendingChanges();
    }

    private void FlushPendingChanges()
    {
        if (_pendingRemove.Count > 0)
        {
            for (int i = 0; i < _pendingRemove.Count; i++)
            {
                _updatables.Remove(_pendingRemove[i]);
            }

            _pendingRemove.Clear();
        }

        if (_pendingAdd.Count > 0)
        {
            for (int i = 0; i < _pendingAdd.Count; i++)
            {
                if (!_updatables.Contains(_pendingAdd[i]))
                {
                    _updatables.Add(_pendingAdd[i]);
                }
            }

            _pendingAdd.Clear();
        }
    }

    private void OnApplicationQuit()
    {
        _isQuitting = true;
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
