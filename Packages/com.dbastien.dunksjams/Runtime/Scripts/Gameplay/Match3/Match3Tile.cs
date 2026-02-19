using System;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Match3Tile : MonoBehaviour
{
    public enum TileType
    {
        Normal,
        Blocker,
        Collectible,
        Empty,
        Hole
    }

    public enum ColorType
    {
        None = -1,
        Red = 0,
        Blue = 1,
        Green = 2,
        Yellow = 3,
        Purple = 4,
        Orange = 5
    }

    public enum BoosterType
    {
        None,
        Horizontal,
        Vertical,
        Bomb,
        ColorBomb
    }

    [SerializeField] private TileType _tileType = TileType.Normal;
    [SerializeField] private ColorType _colorType = ColorType.Red;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private GameObject _boosterIndicator;

    private BoosterType _booster = BoosterType.None;
    private Vector2Int _gridPosition;
    private bool _isSelected;
    private Vector3 _originalScale;

    public TileType Type => _tileType;
    public ColorType Color => _colorType;
    public BoosterType Booster => _booster;
    public Vector2Int GridPosition { get => _gridPosition; set => _gridPosition = value; }
    public bool IsMatchable => _tileType == TileType.Normal && _colorType != ColorType.None;

    public event Action<Match3Tile> OnTileClicked;

    private void Awake()
    {
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();

        _originalScale = transform.localScale;
    }

    public void Initialize(TileType type, ColorType color, Vector2Int gridPos)
    {
        _tileType = type;
        _colorType = color;
        _gridPosition = gridPos;
        _booster = BoosterType.None;
        _isSelected = false;

        if (_boosterIndicator != null)
            _boosterIndicator.SetActive(false);

        transform.localScale = _originalScale;

        UpdateVisuals();
    }

    public void SetBooster(BoosterType booster)
    {
        _booster = booster;

        if (_boosterIndicator != null)
            _boosterIndicator.SetActive(booster != BoosterType.None);
    }

    public void Select()
    {
        if (_isSelected) return;

        _isSelected = true;
        transform.localScale = _originalScale * 1.1f;
    }

    public void Deselect()
    {
        if (!_isSelected) return;

        _isSelected = false;
        transform.localScale = _originalScale;
    }

    public void PlayDestroyAnimation(Action onComplete = null)
    {
        // Simple scale-down animation
        TweenAPI.TweenTo(transform.localScale, Vector3.zero, 0.2f,
            v => transform.localScale = v, EaseType.QuadraticIn)
            .OnComplete(() =>
            {
                onComplete?.Invoke();
                gameObject.SetActive(false);
            });
    }

    public void AnimateToPosition(Vector3 targetPos, float duration = 0.2f, Action onComplete = null)
    {
        TweenAPI.TweenTo(transform.position, targetPos, duration,
            v => transform.position = v, EaseType.QuadraticInOut)
            .OnComplete(() => onComplete?.Invoke());
    }

    private void UpdateVisuals()
    {
        if (_spriteRenderer == null) return;

        _spriteRenderer.color = _colorType.ToColor();
    }

    private void OnMouseDown()
    {
        OnTileClicked?.Invoke(this);
    }

    private void OnEnable()
    {
        transform.localScale = _originalScale;
    }

    private void OnDisable()
    {
        _isSelected = false;
        // Tweens are automatically cleaned up by TweenManager
    }
}