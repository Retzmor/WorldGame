using UnityEngine;
using Zenject;

public class HotbarControllerInstaller : MonoInstaller
{
    [SerializeField] HotbarController hotbarController;
    public override void InstallBindings()
    {
        Container.Bind<HotbarController>().FromInstance(hotbarController).AsSingle();
    }
}