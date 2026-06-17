using UnityEngine;
using UnityEngine.InputSystem; 
using UnityEngine.Events;
using System.Collections;
using Assets.Scripts;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System.IO.Compression;

public enum GameState
{
    Playing,
    Won
}

public class GameManager : MonoBehaviour
{
    private GameState gameState = GameState.Playing;
    ItemArray matrix;
    public GameObject GO2, GO4, GO8, GO16, GO32, GO64, GO128, GO256, GO512, GO1024, GO2048, GO4096, GO8192, GO16384, blankGO;
    public Text ScoreText, DebugText, HighScoreText;
    public UnityEvent winEvent;
    public UnityEvent duplicatedEvent;
    public SoundEvent onPlayBubbleSound;
    public int maxTile = 2048;
    private float distance = 0.109f;

    public IInputDetector inputDetector;

    private int ZIndex = 0, score = 0;
    public int GetScore() => score;

    [SerializeField]
    private float swipeThreshold = 50f; // Минимальная длина свайпа в пикселях

    private Vector2 touchStartPos;
    private Vector2 touchEndPos;
    private bool isSwiping = false;
    private bool isMoving = false; 
    private bool isWin = false;
    private bool isPause = false;

    //will read a file from Resources folder
    //and create the matrix with the preloaded data
    void InitArrayWithPremadeData()
    {
        string[,] sampleArray = Utilities.GetMatrixFromResourcesData();
        for (int row = 0; row < Globals.Rows; row++)
        {
            for (int column = 0; column < Globals.Columns; column++)
            {
                int value;
                if (int.TryParse(sampleArray[Globals.Rows - 1 - row, column], out value))
                {
                    CreateNewItem(value, row, column);
                }
            }
        }
    }

    // Use this for initialization
    void Start()
    {
        matrix = new ItemArray(Globals.Rows, Globals.Columns);
        Initialize();
        InitialPositionBackgroundSprites();

        inputDetector = GetComponent<ArrowKeysDetector>(); 
        if (Keyboard.current != null)
        {
            InputSystem.EnableDevice(Keyboard.current);
        }

        string x = Utilities.ShowMatrixOnConsole(matrix);
        DebugDisplay(x);
    }

    public void Initialize() 
    {
        Globals.Apply();
        maxTile = PlayerPrefs.GetInt("MaxTile", 2048);

        // 1. ПОЛНАЯ ОЧИСТКА СЦЕНЫ ПЕРЕД СТАРТОМ
        
        // Удаляем все старые игровые плитки (2, 4, 8...), чтобы они не наслаивались
        GameObject[] oldTiles = GameObject.FindGameObjectsWithTag("Tile");
        foreach (GameObject tile in oldTiles)
        {
            Destroy(tile);
        }

        // Удаляем все старые ячейки подложки поля, чтобы старое поле исчезло
        GameObject[] oldCells = GameObject.FindGameObjectsWithTag("GridCell");
        foreach (GameObject cell in oldCells)
        {
            Destroy(cell);
        }

        // 2. Инициализируем новый массив под актуальные размеры
        matrix = new ItemArray(Globals.Rows, Globals.Columns); 

        // 3. Запускаем генерацию нового поля и логику игры
        // Здесь должен быть ваш вызов метода, который строит визуальную сетку (например, GenerateGrid() или аналогичный)
        
        CreateNewItem();
        CreateNewItem();

        score = 0;
        UpdateScore(0);
        gameState = GameState.Playing;
    }


    void DebugDisplay(string content)
    {
        DebugText.text = content;
    }

    private void CreateNewItem(int value = 2, int? row = null, int? column = null)
    {
        int randomRow, randomColumn;

        if (row == null && column == null)
        {
            matrix.GetRandomRowColumn(out randomRow, out randomColumn);
        }
        else
        {
            randomRow = row.Value;
            randomColumn = column.Value;
        }

        var newItem = new Item();
        newItem.Row = randomRow;
        newItem.Column = randomColumn;
        newItem.Value = value;

        GameObject newGo = GetGOBasedOnValue(value);
        newGo.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        newItem.GO = Instantiate(newGo, GetCellPosition(randomRow, randomColumn), Quaternion.identity) as GameObject;

        newItem.GO.transform.scaleTo(Globals.AnimationDuration, new Vector3(1.0f, 1.0f, 1.0f));

        matrix[randomRow, randomColumn] = newItem;
    }



    private void InitialPositionBackgroundSprites()
    {
        for (int row = 0; row < Globals.Rows; row++)
        {
            for (int column = 0; column < Globals.Columns; column++)
            {
                Instantiate(blankGO, GetCellPosition(row, column), Quaternion.identity);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (gameState == GameState.Playing)
        {
            if(isPause) return;
            if (isMoving) return; 
            // Проверяем, активен ли тачскрин в данный момент
            if (Touchscreen.current != null)
            {
                var touch = Touchscreen.current.primaryTouch;

                // Если касания нет (фаза None), прерываем выполнение
                if (!touch.press.isPressed && !isSwiping) return;

                // Фиксируем начало касания (фаза Began аналог)
                if (touch.press.wasPressedThisFrame)
                {
                    touchStartPos = touch.position.ReadValue();
                    isSwiping = true;
                }
                // Фиксируем конец касания (фаза Ended аналог)
                else if (touch.press.wasReleasedThisFrame && isSwiping)
                {
                    touchEndPos = touch.position.ReadValue();
                    DetectSwipe();
                    isSwiping = false;
                }
            }

            InputDirection? value = inputDetector.DetectInputDirection();

            if (value.HasValue)
            {
                List<ItemMovementDetails> movementDetails = new List<ItemMovementDetails>();

                if (value == InputDirection.Left)
                    movementDetails = matrix.MoveHorizontal(HorizontalMovement.Left);
                else if (value == InputDirection.Right)
                    movementDetails = matrix.MoveHorizontal(HorizontalMovement.Right);
                else if (value == InputDirection.Top)
                    movementDetails = matrix.MoveVertical(VerticalMovement.Top);
                else if (value == InputDirection.Bottom)
                    movementDetails = matrix.MoveVertical(VerticalMovement.Bottom);

                if (movementDetails.Count > 0)
                {
                    StartCoroutine(AnimateItemsRoutine(movementDetails));
                }
                
                string x = Utilities.ShowMatrixOnConsole(matrix);
                DebugDisplay(x);
            }
        } else if(gameState == GameState.Won)
        {
            if(isWin)
            {
                winEvent.Invoke();
                Debug.Log("Молодец!");
                isWin = false;
            }
        }
    }

    private IEnumerator AnimateItemsRoutine(List<ItemMovementDetails> details)
    {
        isMoving = true;

        yield return StartCoroutine(AnimateItems(details)); 
        
        isMoving = false;
    }

    public void Move(int dir)
    {
        if(isMoving) return;
        List<ItemMovementDetails> movementDetails = new List<ItemMovementDetails>();
        switch(dir)
        {
            case 1:
            movementDetails = matrix.MoveHorizontal(HorizontalMovement.Left);
                break;
            case 2:
            movementDetails = matrix.MoveHorizontal(HorizontalMovement.Right);
                break;
            case 3:
            movementDetails = matrix.MoveVertical(VerticalMovement.Top);
                break;
            case 4:
            movementDetails = matrix.MoveVertical(VerticalMovement.Bottom);
                break;
        }
        if (movementDetails.Count > 0)
                {
                    StartCoroutine(AnimateItemsRoutine(movementDetails));
                }
                string x = Utilities.ShowMatrixOnConsole(matrix);
                DebugDisplay(x);
    }

    private void DetectSwipe()
    {
        // Вычисляем вектор и длину свайпа
        Vector2 swipeVector = touchEndPos - touchStartPos;
        float swipeDistance = swipeVector.magnitude;

        // Если свайп слишком короткий, игнорируем его
        if (swipeDistance < swipeThreshold) return;

        // Определяем направление
        if (Mathf.Abs(swipeVector.x) > Mathf.Abs(swipeVector.y))
        {
            // Горизонтальный свайп
            if (swipeVector.x > 0) Move(2);
            else Move(1);
        }
        else
        {
            // Вертикальный свайп
            if (swipeVector.y > 0) Move(3);
            else Move(4);
        }
    }

    IEnumerator AnimateItems(IEnumerable<ItemMovementDetails> movementDetails)
    {
        List<GameObject> objectsToDestroy = new List<GameObject>();

        foreach (var item in movementDetails)
        {
            var newGoPosition = GetCellPosition(item.NewRow, item.NewColumn);

            var tween = item.GOToAnimatePosition.transform.positionTo(Globals.AnimationDuration, newGoPosition);
            tween.autoRemoveOnComplete = true;

            if (item.GOToAnimateScale != null)
            {
                var secondMoveTween = item.GOToAnimateScale.transform.positionTo(Globals.AnimationDuration, newGoPosition);
                secondMoveTween.autoRemoveOnComplete = true;

                objectsToDestroy.Add(item.GOToAnimateScale);
                objectsToDestroy.Add(item.GOToAnimatePosition);
                
                var duplicatedItem = matrix[item.NewRow, item.NewColumn];
                switch(duplicatedItem.Value)
                {
                    case 4:
                        onPlayBubbleSound.Invoke(TypeOfSound.def);
                        break;
                    case 8:
                        onPlayBubbleSound.Invoke(TypeOfSound.def);
                        break;
                    case 16:
                        onPlayBubbleSound.Invoke(TypeOfSound.def);
                        break;
                    case 32:
                        onPlayBubbleSound.Invoke(TypeOfSound.cool);
                        break;
                    case 64:
                        onPlayBubbleSound.Invoke(TypeOfSound.cool);
                        break;
                    case 128:
                        onPlayBubbleSound.Invoke(TypeOfSound.cooler);
                        break;
                    case 256:
                        onPlayBubbleSound.Invoke(TypeOfSound.cooler);
                        break;
                    case 512:
                        onPlayBubbleSound.Invoke(TypeOfSound.coolest);
                        break;
                    case 1024:
                        onPlayBubbleSound.Invoke(TypeOfSound.coolest);
                        break;
                    case 2048:
                        onPlayBubbleSound.Invoke(TypeOfSound.coolest);
                        break;
                }
                UpdateScore(duplicatedItem.Value);

                if (duplicatedItem.Value == maxTile)
                {
                    gameState = GameState.Won;
                    isWin = true;
                }
            }
        }

        yield return new WaitForSeconds(Globals.AnimationDuration);

        foreach (var go in objectsToDestroy)
        {
            Destroy(go);
        }

        foreach (var item in movementDetails)
        {
            if (item.GOToAnimateScale != null)
            {
                var newGoPosition = GetCellPosition(item.NewRow, item.NewColumn);
                var duplicatedItem = matrix[item.NewRow, item.NewColumn];

                var newGO = Instantiate(GetGOBasedOnValue(duplicatedItem.Value), newGoPosition, Quaternion.identity) as GameObject;

                newGO.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                var appearanceTween = newGO.transform.scaleTo(Globals.AnimationDuration * 0.5f, 1.0f);
                appearanceTween.autoRemoveOnComplete = true;

                matrix[item.NewRow, item.NewColumn].GO = newGO;
            }
        }

        yield return new WaitForSeconds(Globals.AnimationDuration * 0.5f);

        CreateNewItem(); 
    }


    private void UpdateScore(int toAdd)
    {
        score += toAdd;
        ScoreText.text = "Score: " + score;
        HighScoreText.text = "High Score: " + SaveManager.GetPlayerData().highScore;
    }

    private GameObject GetGOBasedOnValue(int value)
    {
        GameObject newGo = null;
        switch (value)
        {
            case 2: newGo = GO2; break;
            case 4: newGo = GO4; break;
            case 8: newGo = GO8; break;
            case 16: newGo = GO16; break;
            case 32: newGo = GO32; break;
            case 64: newGo = GO64; break;
            case 128: newGo = GO128; break;
            case 256: newGo = GO256; break;
            case 512: newGo = GO512; break;
            case 1024: newGo = GO1024; break;
            case 2048: newGo = GO2048; break;
            case 4096: newGo = GO4096; break;
            case 8192: newGo = GO8192; break;
            case 16384: newGo = GO16384; break;
            default:
                throw new System.Exception("Uknown value:" + value);
        }
        return newGo;
    }
    public void RestartGame()
    {
        isWin = false;
        matrix = new ItemArray(Globals.Rows, Globals.Columns);
        Initialize();
        InitialPositionBackgroundSprites();
    }
    public void Pause(bool pause)
    {
        isPause = pause;
    }
    private Vector3 GetCellPosition(int row, int column)
    {
        // Вычисляем общее смещение для центрирования сетки
        float offsetX = (Globals.Columns - 1) * (1f + distance) / 2f;
        float offsetY = (Globals.Rows - 1) * (1f + distance) / 2f;

        // Сдвигаем координаты на половину ширины/высоты поля
        float x = (column * (1f + distance)) - offsetX;
        float y = (row * (1f + distance)) - offsetY;

        // Применяем к позиции самого GameManager
        return this.transform.position + new Vector3(x, y, ZIndex);
    }
}

