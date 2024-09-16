using System;
using DOT.Utilities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

public class Ability_Placeable : Ability_Interactable
{
    /* ------------------------------------------ */

    #region Definitions

    public enum State
    {
        None = 0,
        Built = 1,
        Moving = 2,
        Preview = 3
    }

    #endregion

    /* ------------------------------------------ */

    [Header("Rendering Related")] public SpriteRenderer SpR_Main;

    [Header("")] public State CurrentState;

    /* ------------------------------------------ */

    private Camera _mainCamera;

    private bool _isItInitialized, _isItEngaged;

    private int _defaultLayer, _gridSize;

    private float _mouseWheelRotation = 0f;

    private int2 _currentPositionAsInt2, _scaleAsInt2;

    private Vector3 _draggingOffset;

    /* ------------------------------------------ */

    private void Start()
    {
        // We should improve this section by moving it some sort of setup method
        // but it's okay for the prototype.
        _mainCamera = Camera.main;
        _gridSize = Manager_Ingame_Settings.instance.Grid.Size;

        _scaleAsInt2 = new int2((int)this.transform.lossyScale.x, (int)this.transform.lossyScale.z);
        _draggingOffset = new Vector3(.5f, 0, .5f);
    }

    private void Update()
    {
        if (_isItEngaged)
            Move();
    }

    /* ------------------------------------------ */

    public void SwitchState(State state)
    {
        switch (state)
        {
            case State.Built:
                CurrentState = State.Built;
                UpdateMaterials(Color.white);

                this.gameObject.SetLayer(_defaultLayer);

                _isItInitialized = true;
                break;

            case State.Moving:
                CurrentState = State.Moving;
                UpdateMaterials(Color.green);

                this.gameObject.SetLayer(2);

                break;
        }
    }

    public override bool Interact()
    {
        if (CurrentState != State.Built)
        {
            SwitchState(State.Moving);

            _isItEngaged = true;
            return true;
        }
        else
            return false;
    }

    public override bool Disengage()
    {
        if (_isItEngaged)
        {
            if (!_isItInitialized)
            {
                if (Built())
                {
                    _isItEngaged = false;
                    return true;
                }
            }
        }

        return false;
    }

    /* ------------------------------------------ */

    private void UpdateRenderer()
    {
        if (Manager_Ingame_Building.instance.CanBuild(_currentPositionAsInt2, _scaleAsInt2))
            UpdateMaterials(Color.green);
        else
            UpdateMaterials(Color.red);
    }

    private void UpdateMaterials(Color color)
    {
        SpR_Main.color = color;
    }

    private void Move()
    {
        // Fare pozisyonunu alıp dünya koordinatlarına çevirme
        Vector3 mouseWorldPosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);

        // Pozisyonu grid hücrelerine göre yuvarlama
        Vector3 gridPosition = new Vector3(
            Mathf.Round(mouseWorldPosition.x / _gridSize) * _gridSize,
            0f,
            Mathf.Round(mouseWorldPosition.z / _gridSize) * _gridSize);

        _currentPositionAsInt2.x = (int)gridPosition.x;
        _currentPositionAsInt2.y = (int)gridPosition.z;

        // Nesnenin pozisyonunu güncelle
        transform.position = gridPosition;
        
        // Bc of the pivot misplacement. We don't want to put it to the center of the grid.
        transform.position -= _draggingOffset;

        UpdateRenderer();
    }

    private bool Built()
    {
        // We're occupying the position and checking is it possible to built at the same time here.
        if (Manager_Ingame_Building.instance.AddBuilding(_currentPositionAsInt2, _scaleAsInt2))
        {
            // The position is available to built, so built
            SwitchState(State.Built);
            return true;
        }

        return false;
    }

    /* ------------------------------------------ */
}