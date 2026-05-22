namespace GameKit.App;

internal sealed class UpdateLoop
{
    private readonly List<IUpdatable?> _updatables = new();

    public void Register(IUpdatable updatable)
    {
        for (int i = 0; i < _updatables.Count; i++)
        {
            if (ReferenceEquals(_updatables[i], updatable))
            {
                return;
            }
        }

        _updatables.Add(updatable);
    }

    public void Unregister(IUpdatable updatable)
    {
        for (int i = 0; i < _updatables.Count; i++)
        {
            if (ReferenceEquals(_updatables[i], updatable))
            {
                _updatables[i] = null;
                return;
            }
        }
    }

    public void Update()
    {
        int count = _updatables.Count;
        bool needsCompaction = false;
        for (int i = 0; i < count; i++)
        {
            IUpdatable? updatable = _updatables[i];
            if (updatable == null)
            {
                needsCompaction = true;
                continue;
            }

            updatable.Update();
        }

        if (needsCompaction)
        {
            Compact();
        }
    }

    private void Compact()
    {
        for (int i = _updatables.Count - 1; i >= 0; i--)
        {
            if (_updatables[i] == null)
            {
                _updatables.RemoveAt(i);
            }
        }
    }
}
