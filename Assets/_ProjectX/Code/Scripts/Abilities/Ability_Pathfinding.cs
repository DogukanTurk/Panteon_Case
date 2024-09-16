using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// This class gives gameObjects to find the closest path with A* pathfinding algorithm
/// and if we want also triggers the ECS movement system.
/// </summary>
public class Ability_Pathfinding : MonoBehaviour
{
    /* ------------------------------------------ */

    public bool IsMovementControlledOnECS;

    /* ------------------------------------------ */

    private EntityManager _entityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

    private Entity _entity;

    private DynamicBuffer<Data_Buffer_Path> _pathBuffer;

    private bool _isECSEnabled;

    private float _movementSpeed;

    private CancellationTokenSource _cancellationTokenSource;

    /* ------------------------------------------ */

    private void Start()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        
        _entity = _entityManager.CreateEntity(typeof(Translation), typeof(LocalToWorld), typeof(Rotation));
        _pathBuffer = _entityManager.AddBuffer<Data_Buffer_Path>(_entity);

        if (!IsMovementControlledOnECS)
            ProcessMovement().Forget();
        
        // We run it here to make sure everything setup properly.
        HandleConversion();
    }

    private void Update()
    {
        // We make sure if it's changed in runtime, we'll rerun the process
        if (!_isECSEnabled)
        {
            if (IsMovementControlledOnECS)
                ProcessMovement().Forget();
        }
    }

    private void OnDestroy()
    {
        _cancellationTokenSource.Cancel();
        _entityManager.DestroyEntity(_entity);
    }

    /* ------------------------------------------ */

    public void Setup(float movementSpeed)
    {
        _movementSpeed = movementSpeed;
    }

    public void SetDestination(Vector3 destination)
    {
        if (_entityManager.HasComponent<Data_Request_Path_Finding>(_entity))
            return;
        
        int gridSize = Manager_Ingame_Settings.instance.Grid.Size;

        _pathBuffer = _entityManager.GetBuffer<Data_Buffer_Path>(_entity);
        _pathBuffer.Clear();

        // We have to round the numbers to correct grid size to get the actual grid in ECS side
        int2 startPosition = new int2(
            Mathf.RoundToInt(this.transform.position.x / gridSize) * gridSize,
            Mathf.RoundToInt(this.transform.position.z / gridSize) * gridSize
        );

        int2 targetPosition = new int2(
            Mathf.RoundToInt(destination.x / gridSize) * gridSize,
            Mathf.RoundToInt(destination.z / gridSize) * gridSize
        );

        // Creating a request here, this will trigger the path finding algorithm.
        _entityManager.AddComponentData(_entity, new Data_Request_Path_Finding()
        {
            StartPosition = startPosition,
            TargetPosition = targetPosition
        });
    }

    public void LookAt(Vector3 position)
    {
        Rotation tempRotation = _entityManager.GetComponentData<Rotation>(_entity);
        quaternion targetRotation =
            quaternion.LookRotationSafe(position - transform.position, new float3(0, 1, 0));
        tempRotation.Value = targetRotation;

        _entityManager.SetComponentData(_entity, tempRotation);
    }

    /* ------------------------------------------ */

    private async UniTask ProcessMovement()
    {
        while (isActiveAndEnabled && !IsMovementControlledOnECS)
        {
            _pathBuffer = _entityManager.GetBuffer<Data_Buffer_Path>(_entity);
            if (_pathBuffer.Length > 0)
            {
                int2 path = _pathBuffer[0].Path;
                Vector3 targetPosition = new Vector3(path.x, 0, path.y);
                
                // Rotating
                quaternion targetRotation =
                    quaternion.LookRotationSafe(targetPosition - transform.position, new float3(0, 1, 0));
                transform.rotation = math.slerp(transform.rotation, targetRotation, Time.deltaTime * 4);
                
                
                // Moving
                Vector3 moveDirection = Vector3.Normalize(targetPosition - transform.position);

                // We're moving the object here by its movement speed from Data_Unit
                transform.position += moveDirection * _movementSpeed * Time.deltaTime;

                // We use "distancesq" because it's faster than the "distance"
                if (math.distancesq(transform.position, targetPosition) < .005f)
                    _pathBuffer.RemoveAt(0);
            }

            // We make sure we'll wait, otherwise it can occur stackoverflow
            await UniTask.DelayFrame(1, cancellationToken: _cancellationTokenSource.Token);
        }

        HandleConversion();
    }

    private void HandleConversion()
    {
        if (IsMovementControlledOnECS)
        {
            _isECSEnabled = true;
            if (!_entityManager.HasComponent<Tag_ControllingOnECSSide>(_entity))
            {
                _entityManager.AddComponentData(_entity, new Data_Unit()
                {
                    Movement_Speed = _movementSpeed
                });
                _entityManager.AddComponentData(_entity, new Data_Transform()
                {
                    Value = this.transform
                });

                _entityManager.AddComponentData(_entity, new Tag_ControllingOnECSSide() { });

                // We'll get the component and update the location before passing it to the ECS movement system
                // otherwise, it'll move to the last position where was Translation last time.
                Translation tempTranslation = _entityManager.GetComponentData<Translation>(_entity);
                tempTranslation.Value = this.transform.position;

                _entityManager.SetComponentData(_entity, tempTranslation);
            }
        }
        else
        {
            _isECSEnabled = false;

            if (_entityManager.HasComponent<Tag_ControllingOnECSSide>(_entity))
                _entityManager.RemoveComponent<Tag_ControllingOnECSSide>(_entity);
        }
    }

    /* ------------------------------------------ */
}