using UnityEngine;
using Zenject;

public class PlayerAttackInstaller : MonoInstaller
{
    [SerializeField] AttackPlayer attackPlayer;
    public override void InstallBindings()
    {
        Container.Bind<AttackPlayer>().FromInstance(attackPlayer).AsSingle();
    }
}