using UnityEngine;
using UnityEngine.InputSystem;

public class RunnerPlayerController : MonoBehaviour
{
    [Header("Forward")]
    public float forwardSpeed = 8f;
    public float speedIncreasePerSecond = 0.15f;

    [Header("Lanes")]
    public float laneDistance = 2f;
    public float laneChangeSpeed = 12f;
    int currentLane = 1;

    [Header("Jump")]
    public float jumpHeight = 1.6f;
    public float gravity = -20f;

    [Header("Slide")]
    public float slideDuration = 0.75f;
    public float slideHeightMultiplier = 0.5f; // CC height düþürme
    bool isSliding;
    float slideTimer;

    [Header("Swipe")]
    public float swipeMinDistance = 60f;

    CharacterController controller;
    float verticalVelocity;

    float defaultHeight;
    Vector3 defaultCenter;

    // swipe state
    Vector2 swipeStartPos;
    bool swipeTracking;

    // durumlar
    bool isJumping; // havadaysa true

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        defaultHeight = controller.height;
        defaultCenter = controller.center;
    }

    void Update()
    {
        if (GameManagerRunner.Instance == null) return;
        if (!GameManagerRunner.Instance.isRunning || GameManagerRunner.Instance.isGameOver) return;

        forwardSpeed += speedIncreasePerSecond * Time.deltaTime;

        HandleInput();
        HandleSlideTimer();

        // Gravity & Jump state
        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
            isJumping = false;
        }
        else
        {
            // grounded deðilsek havadayýz (zýplama veya düþme)
            if (!controller.isGrounded) isJumping = true;
        }

        verticalVelocity += gravity * Time.deltaTime;

        // Lane hareketi: CharacterController ile
        float targetX = (currentLane - 1) * laneDistance;
        float newX = Mathf.Lerp(transform.position.x, targetX, Time.deltaTime * laneChangeSpeed);
        float deltaX = newX - transform.position.x;

        Vector3 move = new Vector3(deltaX, verticalVelocity, forwardSpeed) * Time.deltaTime;
        controller.Move(move);
    }

    void HandleInput()
    {
        var kb = Keyboard.current;

        // Keyboard lane
        if (kb != null)
        {
            if (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame) ChangeLane(-1);
            if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame) ChangeLane(+1);

            if (kb.spaceKey.wasPressedThisFrame) TryJump();
            if (kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame) TrySlide();
        }

        // Mouse swipe-ish (PC test)
        var mouse = Mouse.current;
        if (mouse != null)
        {
            if (mouse.leftButton.wasPressedThisFrame)
            {
                swipeStartPos = mouse.position.ReadValue();
                swipeTracking = true;
            }
            if (swipeTracking && mouse.leftButton.wasReleasedThisFrame)
            {
                Vector2 end = mouse.position.ReadValue();
                swipeTracking = false;
                HandleSwipe(end - swipeStartPos);
            }
        }

        // Touch swipe (mobile)
        var ts = Touchscreen.current;
        if (ts != null)
        {
            var touch = ts.primaryTouch;

            if (touch.press.wasPressedThisFrame)
            {
                swipeStartPos = touch.position.ReadValue();
                swipeTracking = true;
            }
            if (swipeTracking && touch.press.wasReleasedThisFrame)
            {
                Vector2 end = touch.position.ReadValue();
                swipeTracking = false;
                HandleSwipe(end - swipeStartPos);
            }
        }
    }

    void HandleSwipe(Vector2 delta)
    {
        if (delta.magnitude < swipeMinDistance) return;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            // left/right
            if (delta.x > 0) ChangeLane(+1);
            else ChangeLane(-1);
        }
        else
        {
            // up/down
            if (delta.y > 0) TryJump();
            else TrySlide();
        }
    }

    void TryJump()
    {
        if (isSliding) return; // slide içindeyken zýplamasýn
        if (!controller.isGrounded) return;

        verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        isJumping = true;
    }

    void TrySlide()
    {
        if (isSliding) return;
        if (!controller.isGrounded) return; // havadayken slide yok

        isSliding = true;
        slideTimer = slideDuration;

        // CharacterController küçült
        controller.height = defaultHeight * slideHeightMultiplier;
        controller.center = new Vector3(defaultCenter.x, defaultCenter.y * slideHeightMultiplier, defaultCenter.z);
    }

    void HandleSlideTimer()
    {
        if (!isSliding) return;

        slideTimer -= Time.deltaTime;
        if (slideTimer <= 0f)
        {
            isSliding = false;

            // eski haline dön
            controller.height = defaultHeight;
            controller.center = defaultCenter;
        }
    }

    void ChangeLane(int dir)
    {
        currentLane = Mathf.Clamp(currentLane + dir, 0, 2);
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        var obstacle = hit.collider.GetComponent<Obstacle>();
        if (obstacle == null) return;

        switch (obstacle.type)
        {
            case ObstacleType.Hit:
                GameManagerRunner.Instance.GameOver();
                break;

            case ObstacleType.JumpOver:
                // Zýplýyorsa geçsin, zýplamýyorsa öl
                if (!isJumping) GameManagerRunner.Instance.GameOver();
                break;

            case ObstacleType.SlideUnder:
                // Slide yapýyorsa geçsin, yapmýyorsa öl
                if (!isSliding) GameManagerRunner.Instance.GameOver();
                break;
        }
    }
}

