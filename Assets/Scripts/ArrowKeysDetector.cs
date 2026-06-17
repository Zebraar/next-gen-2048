using Assets.Scripts;
using UnityEngine;
using UnityEngine.InputSystem;

public class ArrowKeysDetector : MonoBehaviour, IInputDetector
{
    // Создаем действия для направлений
    private InputAction moveUp;
    private InputAction moveDown;
    private InputAction moveLeft;
    private InputAction moveRight;

    void OnEnable()
    {
        // Инициализируем и привязываем клавиши (W/Стрелка вверх и т.д.)
        moveUp = new InputAction(binding: "<Keyboard>/w");
        moveUp.AddBinding("<Keyboard>/upArrow");

        moveDown = new InputAction(binding: "<Keyboard>/s");
        moveDown.AddBinding("<Keyboard>/downArrow");

        moveLeft = new InputAction(binding: "<Keyboard>/a");
        moveLeft.AddBinding("<Keyboard>/leftArrow");

        moveRight = new InputAction(binding: "<Keyboard>/d");
        moveRight.AddBinding("<Keyboard>/rightArrow");

        // Обязательно включаем их
        moveUp.Enable();
        moveDown.Enable();
        moveLeft.Enable();
        moveRight.Enable();
    }

    void OnDisable()
    {
        // Освобождаем память при отключении компонента
        moveUp.Disable();
        moveDown.Disable();
        moveLeft.Disable();
        moveRight.Disable();
    }

    public InputDirection? DetectInputDirection()
    {
        // Свойство .triggered срабатывает строго ОДИН РАЗ в момент нажатия (идеально для пошаговых игр/2048)
        // Если нужно постоянное удержание, замени .triggered на .IsPressed()
        if (moveUp.triggered) return InputDirection.Top;
        if (moveDown.triggered) return InputDirection.Bottom;
        if (moveLeft.triggered) return InputDirection.Left;
        if (moveRight.triggered) return InputDirection.Right;

        return null;
    }
}