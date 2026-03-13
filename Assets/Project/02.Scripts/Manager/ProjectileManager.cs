using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : SingletonBase<ProjectileManager>
{
    private readonly List<Projectile> _activeProjectiles = new List<Projectile>();

    public IReadOnlyList<Projectile> ActiveProjectiles => _activeProjectiles;

    protected override void Awake()
    {
        dontDestroy = false;
        base.Awake();
    }

    public void Register(Projectile proj)
    {
        if (!_activeProjectiles.Contains(proj))
        {
            _activeProjectiles.Add(proj);
        }
    }

    public void Unregister(Projectile proj)
    {
        _activeProjectiles.Remove(proj);
    }
}
