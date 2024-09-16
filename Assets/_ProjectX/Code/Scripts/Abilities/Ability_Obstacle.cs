using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// This classes purpose is to give gameObject to block grids 
/// </summary>
public class Ability_Obstacle : MonoBehaviour
{
    /* ------------------------------------------ */

    public bool IsThisDynamic;

    /* ------------------------------------------ */

    private EntityManager _entityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

    private Entity _entity;

    private CancellationTokenSource _cancellationTokenSource;

    /* ------------------------------------------ */

    private void Start()
    {
        _cancellationTokenSource = new CancellationTokenSource();

        Setup().Forget();
    }

    private void OnDestroy()
    {
        _cancellationTokenSource.Cancel();

        // Also, we need to clear the occupied positions
        if (Manager_Ingame_Building.instance != null)
            Manager_Ingame_Building.instance.RemoveBuilding(
                new int2((int)math.ceil(transform.position.x), (int)math.ceil(transform.position.z)),
                new int2((int)this.transform.lossyScale.x, (int)this.transform.lossyScale.z), this.gameObject);

        if (World.DefaultGameObjectInjectionWorld != null)
            _entityManager.DestroyEntity(_entity);
    }

    /* ------------------------------------------ */

    private async UniTask Setup()
    {
        // We make sure the object gets the place it needs to be initialy
        await UniTask.Delay(250, cancellationToken: _cancellationTokenSource.Token);

        _entity = _entityManager.CreateEntity(typeof(Translation), typeof(LocalToWorld), typeof(Data_Scale));

        // We'll get the component and update the location before passing it to the ECS movement system
        Translation tempTranslation = _entityManager.GetComponentData<Translation>(_entity);
        tempTranslation.Value = this.transform.position;

        _entityManager.SetComponentData(_entity, tempTranslation);
        _entityManager.SetComponentData(_entity, new Data_Scale()
        {
            Value = new int2((int)this.transform.localScale.x, (int)this.transform.localScale.z)
        });

        if (IsThisDynamic)
        {
            _entityManager.AddComponent<Tag_Sync_EntityToTransform>(_entity);
            _entityManager.AddComponent<Tag_Obstacle_Dynamic>(_entity);
        }
        else
            _entityManager.AddComponent<Tag_Obstacle_Static>(_entity);

        UpdatePositionLoop().Forget();
    }

    private async UniTask UpdatePositionLoop()
    {
        while (isActiveAndEnabled)
        {
            Translation tempTranslation = _entityManager.GetComponentData<Translation>(_entity);
            tempTranslation.Value = this.transform.position;

            _entityManager.SetComponentData(_entity, tempTranslation);

            // If it's tagged as dynamic, we will update the position quickly otherwise we will still update it slowly.
            // This can be improved by tagging the object as built and then update the position once bc It's static.
            if (IsThisDynamic)
                await UniTask.Delay(100, cancellationToken: _cancellationTokenSource.Token);
            else
                await UniTask.Delay(1000, cancellationToken: _cancellationTokenSource.Token);
        }
    }
}