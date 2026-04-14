using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header ("Mouse")]
    public MouseState mouseState = MouseState.Locked;

    [Header("Time")]
    public float currentTimeScale = 1f;

    private float targetTimeScale = 1f;
    public float timeScaleLerpSpeed = 10f;


    private void Awake() // Singleton setup
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() // Apply initial settings
    {
        UpdateMouseLock(mouseState);


        
    }

    private void Update() // Keeps time scale synced
    {
        if (Time.timeScale != targetTimeScale)
        {
            Time.timeScale = Mathf.Lerp(Time.timeScale, targetTimeScale, Time.unscaledDeltaTime * timeScaleLerpSpeed);
        }

        currentTimeScale = Time.timeScale;
    }

    // MOUSE LOCK //

    public void UpdateMouseLock(MouseState state) // Sets mouse state
    {
        mouseState = state;

        switch (mouseState)
        {
            case MouseState.Locked:
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;

            case MouseState.Confined:
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
                break;

            case MouseState.Unlocked:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
        }
    }

    // TIME SCALE //

    public void SetTimeScale(float target)
    {
        targetTimeScale = target;
    }

    public void ResetTimeScale()
    {
        targetTimeScale = 1f;
    }
}

public enum MouseState
{
    Locked,
    Confined,
    Unlocked
}