using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class RainerBootstrap : MonoBehaviour
{
    private const float MinSpawnRate = 0.01f;
    private const float MinLifetime = 2f;
    private const float MaxLifetime = 5f;

    [Header("Platforms")]
    [SerializeField] private Collider _spawnPlatform;
    [SerializeField] private List<Collider> _additionalPlatforms = new();
    [Tooltip("Offset will be calculated from any platform highest point")]
    [SerializeField, Min(0)] private int _MinOffsetFromPlatforms = 3;
    [SerializeField, Min(0)] private int _MaxOffsetFromPlatforms = 10;
    [Header("Cube pool")]
    [SerializeField] private RainingFigure _rainingFigurePrefab;
    [SerializeField, Min(1)] private int _poolSize = 100;
    [SerializeField, Min(MinSpawnRate)] private float _spawnRateInSeconds = 0.05f;

    private RainingFigure _rainingFigure;
    private Color _rainingFigureColor;
    private ObjectPool<RainingFigure> _pool;
    private float _platformsHighestPoint;

    private void Awake()
    {
        _rainingFigure = Instantiate(_rainingFigurePrefab);
        _rainingFigure.gameObject.SetActive(false);

        if (_rainingFigure.TryGetColor(out Color figureColor) == false)
            Debug.LogError("No color found for raining figure, fallback color will be used");

        _rainingFigureColor = figureColor;

        if (_rainingFigure.TryGetComponent<Rigidbody>(out Rigidbody _rainingRigidbody) == false)
        {
            _rainingRigidbody = _rainingFigure.gameObject.AddComponent<Rigidbody>();
            Debug.LogWarning("Raining Object has no collider to use gravity. Created default Rigidbody");
        }

        _rainingRigidbody.useGravity = true;

        _pool = new ObjectPool<RainingFigure>(
            createFunc: () => Instantiate(_rainingFigure, transform),
            actionOnGet: (figure) => ExecuteOnRainPoolGet(figure),
            actionOnRelease: (figure) => figure.gameObject.SetActive(false),
            actionOnDestroy: (figure) => Destroy(figure),
            collectionCheck: true,
            defaultCapacity: _poolSize,
            maxSize: _poolSize
        );

        _pool.Release(_rainingFigure);

        for (int i = 0; i < _poolSize - 1; i++)
        {
            RainingFigure objectForPool = Instantiate(_rainingFigure, transform);
            _pool.Release(objectForPool);
        }

        _platformsHighestPoint = _spawnPlatform.bounds.max.y;

        foreach (Collider platform in _additionalPlatforms)
            if (platform != null && platform.bounds.max.y > _platformsHighestPoint)
                _platformsHighestPoint = platform.bounds.max.y;

    }

    private void OnEnable()
    {
        InvokeRepeating(nameof(Rain), 0f, _spawnRateInSeconds);
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(Rain));
    }

    private void OnValidate()
    {
        UserUtilities.CatchNullArgumentException(
            (_spawnPlatform, nameof(_spawnPlatform)),
            (_rainingFigurePrefab, nameof(_rainingFigurePrefab))
        );
    }
    
    private void Rain()
    {
        if (_pool.CountInactive > 0)
            _pool.Get();
    }

    private void ExecuteOnRainPoolGet(RainingFigure rainingFigure)
    {
        rainingFigure.SetColor(_rainingFigureColor);
        rainingFigure.OnCollisionEntering += HandleFigureCollision;
        rainingFigure.transform.position = GetNewSpawnPosition();
        rainingFigure.gameObject.SetActive(true);
    }

    private void HandleFigureCollision(RainingFigure rainingFigure, Collision collision)
    {
        if (IsPlatformCollision(collision.collider))
        {
            rainingFigure.OnCollisionEntering -= HandleFigureCollision;
            StartCoroutine(ExecuteCollisionEvent(rainingFigure, UserUtilities.GenerateRandomFloat(MinLifetime, MaxLifetime)));
        }
    }

    private IEnumerator ExecuteCollisionEvent(RainingFigure rainingFigure, float delaySeconds)
    {
        rainingFigure.ChangeColor();
        yield return new WaitForSeconds(delaySeconds);
        _pool.Release(rainingFigure);
    }

    private bool IsPlatformCollision(Collider collider)
    {
        if (collider == _spawnPlatform)
            return true;

        foreach (var platform in _additionalPlatforms)
            if (collider == platform)
                return true;

        return false;
    }

    private Vector3 GetNewSpawnPosition()
    {
        Vector3 newSpawnPosition = new();
        Bounds platformBounds = _spawnPlatform.bounds;

        newSpawnPosition.x = UserUtilities.GenerateRandomFloat(platformBounds.min.x, platformBounds.max.x);
        newSpawnPosition.y = _platformsHighestPoint + UserUtilities.GenerateRandomFloat(_MinOffsetFromPlatforms, _MaxOffsetFromPlatforms);
        newSpawnPosition.z = UserUtilities.GenerateRandomFloat(platformBounds.min.z, platformBounds.max.z);

        return newSpawnPosition;
    }
}
