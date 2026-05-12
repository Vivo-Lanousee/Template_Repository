using UnityEngine;
using VContainer;
using R3;
using MessagePipe;
using VContainer.Unity;
using System;

public class GameModelScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
       

        //基本設定
        var options=builder.RegisterMessagePipe();
        // Globalに通知を送る設定（標準的な使い方）
        builder.RegisterMessageBroker<PlayerDamaged>(options);
        //Systemの登録
        builder.Register<PlayerSystem>(Lifetime.Singleton);

        // IInitializableとして登録することで、ゲーム開始時に自動で生成される
        builder.RegisterEntryPoint<UISystem>(Lifetime.Singleton);

        //便宜上TestMonoを登録することでPlayerSystemを探索させる為に使っている
        //PlayerSystemを探索できていれば問題ないっちゃない。
        builder.RegisterComponentInHierarchy<TestMono>();
    }
}


public class PlayerDamaged
{
    public int Damage;
}

public class PlayerSystem
{
    IPublisher<PlayerDamaged> publisher;
    public PlayerSystem(IPublisher<PlayerDamaged> publisher)
    {
        this.publisher = publisher;
    }

    public void Damage(int damage)
    {
        publisher.Publish(new PlayerDamaged
        {
            Damage = damage
        });
    }
}


public class UISystem : IInitializable, IDisposable
{
    private readonly ISubscriber<PlayerDamaged> _subscriber;
    private IDisposable _disposable;

    public UISystem(ISubscriber<PlayerDamaged> subscriber)
    {
        _subscriber = subscriber;
    }

    public void Initialize()
    {
        // Subscribeの戻り値（IDisposable）を保持し、不要になったら解除できるようにする
        var bag = MessagePipe.DisposableBag.CreateBuilder();
        _subscriber.Subscribe(x =>
        {
            Debug.Log($"UI表示: ダメージを{x.Damage}受けました！");
        }).AddTo(bag);

        _disposable = bag.Build();
    }

    public void Dispose()
    {
        _disposable?.Dispose();
    }
}
