
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("플레이어 정보")]
    public string playerName = "하얀 토끼";
    public int hp = 100;
    public int lives = 3;

    [Header("이동 및 점프")]
    public float moveSpeed = 5f;
    public float jumpPower = 8f;
    public float fallLimit = -10f;

    [Header("캐릭터 외형")]
    public Transform visual;
    public Vector2 airScale = new Vector2(0.8f, 1.2f);

    [Header("시계 목숨 UI")]
    public GameObject[] lifeClocks = new GameObject[3];

    [Header("시작 메뉴")]
    public GameObject startMenuPanel;
    public Button startButton;

    [Header("인트로")]
    public GameObject introPanel;
    public float introDuration = 5f;

    [Header("게임 오버 스토리")]
    public GameObject gameOverStoryPanel;
    public float gameOverStoryDuration = 3f;

    [Header("게임 오버 화면")]
    public GameObject gameOverPanel;

    [Header("스테이지 설정")]
    public Vector3 startPosition;
    public string nextSceneName = "Stage2";

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private float facing = 1f;

    private bool isGrounded = false;
    private bool isGameOver = false;
    private bool isIntroPlaying = false;
    private bool isMenuOpen = false;
    private bool isLosingLife = false;

    // 재도전으로 씬을 다시 불러왔는지 확인
    private static bool retryFromGameOver = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        Time.timeScale = 1f;

        transform.position = startPosition;

        moveInput = Vector2.zero;

        isGrounded = false;
        isGameOver = false;
        isIntroPlaying = false;
        isMenuOpen = false;
        isLosingLife = false;

        UpdateLivesUI();

        // 게임 오버 UI 초기화
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (gameOverStoryPanel != null)
            gameOverStoryPanel.SetActive(false);

        if (introPanel != null)
            introPanel.SetActive(false);

        // 시작 버튼 연결
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(StartGame);
            startButton.onClick.AddListener(StartGame);
        }

        // 재도전으로 돌아온 경우
        if (retryFromGameOver)
        {
            retryFromGameOver = false;

            if (startMenuPanel != null)
                startMenuPanel.SetActive(false);

            isMenuOpen = false;
            isIntroPlaying = false;

            Time.timeScale = 1f;

            Debug.Log("재도전 - 현재 스테이지 바로 시작");
        }
        // 일반적으로 Stage1을 시작한 경우
        else if (startMenuPanel != null)
        {
            startMenuPanel.SetActive(true);
            startMenuPanel.transform.SetAsLastSibling();

            isMenuOpen = true;

            Time.timeScale = 0f;
        }
        // Stage2, Stage3 등 시작 메뉴가 없는 경우
        else
        {
            isMenuOpen = false;
            Time.timeScale = 1f;
        }
    }

    // 좌우 이동
    void OnMove(InputValue value)
    {
        if (isMenuOpen || isIntroPlaying || isGameOver)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput = value.Get<Vector2>();

        if (moveInput.x > 0f)
            facing = 1f;
        else if (moveInput.x < 0f)
            facing = -1f;
    }

    // 점프
    void OnJump(InputValue value)
    {
        if (!value.isPressed)
            return;

        if (isGrounded &&
            !isMenuOpen &&
            !isIntroPlaying &&
            !isGameOver)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpPower
            );
        }
    }

    void Update()
    {
        if (isMenuOpen || isIntroPlaying || isGameOver)
            return;

        MovePlayer();
        CheckFall();
        UpdateVisual();
    }

    // 이동 처리
    void MovePlayer()
    {
        transform.Translate(
            Vector3.right *
            moveInput.x *
            moveSpeed *
            Time.deltaTime
        );
    }

    // 낙하 판정
    void CheckFall()
    {
        if (transform.position.y < fallLimit)
        {
            LoseLife();
        }
    }

    // 캐릭터 방향 및 점프 모양
    void UpdateVisual()
    {
        if (visual == null)
            return;

        if (isGrounded)
        {
            visual.localScale = new Vector3(
                facing,
                1f,
                1f
            );
        }
        else
        {
            visual.localScale = new Vector3(
                facing * airScale.x,
                airScale.y,
                1f
            );
        }
    }

    // 바닥 충돌
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    // 트리거 충돌
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isMenuOpen || isIntroPlaying || isGameOver)
            return;

        // 다음 스테이지 또는 엔딩
        if (other.CompareTag("Goal"))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(nextSceneName);
        }
        // 함정
        else if (other.CompareTag("Trap"))
        {
            LoseLife();
        }
        // 닿으면 사라지는 오브젝트
        else if (other.CompareTag("Disappear"))
        {
            Destroy(other.gameObject);
        }
    }

    // 목숨 감소
    void LoseLife()
    {
        if (isGameOver || isLosingLife)
            return;

        isLosingLife = true;

        lives--;
        lives = Mathf.Max(lives, 0);

        Debug.Log("남은 목숨: " + lives);

        moveInput = Vector2.zero;
        rb.linearVelocity = Vector2.zero;

        isGrounded = false;

        UpdateLivesUI();

        if (lives <= 0)
        {
            StartGameOver();
        }
        else
        {
            // 목숨이 남아 있으면 시작 위치로 복귀
            transform.position = startPosition;

            isLosingLife = false;
        }
    }

    // 시계 목숨 UI
    void UpdateLivesUI()
    {
        for (int i = 0; i < lifeClocks.Length; i++)
        {
            if (lifeClocks[i] != null)
            {
                lifeClocks[i].SetActive(i < lives);
            }
        }
    }

    // 시작 버튼
    public void StartGame()
    {
        if (!isMenuOpen)
            return;

        isMenuOpen = false;
        isIntroPlaying = true;

        moveInput = Vector2.zero;

        if (startMenuPanel != null)
            startMenuPanel.SetActive(false);

        if (introPanel != null)
        {
            introPanel.SetActive(true);
            introPanel.transform.SetAsLastSibling();

            StartCoroutine(IntroSequence());
        }
        else
        {
            FinishIntro();
        }
    }

    // 인트로 표시
    IEnumerator IntroSequence()
    {
        yield return new WaitForSecondsRealtime(
            introDuration
        );

        FinishIntro();
    }

    // 인트로 종료
    public void FinishIntro()
    {
        if (introPanel != null)
            introPanel.SetActive(false);

        isIntroPlaying = false;

        moveInput = Vector2.zero;

        Time.timeScale = 1f;
    }

    // 게임 오버 시작
    void StartGameOver()
    {
        if (isGameOver)
            return;

        isGameOver = true;
        isLosingLife = false;

        moveInput = Vector2.zero;
        rb.linearVelocity = Vector2.zero;

        Debug.Log("게임 오버 시작");

        Time.timeScale = 0f;

        if (gameOverStoryPanel != null)
        {
            gameOverStoryPanel.SetActive(true);
            gameOverStoryPanel.transform.SetAsLastSibling();

            Debug.Log("게임 오버 스토리 표시");

            StartCoroutine(GameOverSequence());
        }
        else
        {
            ShowGameOver();
        }
    }

    // 게임 오버 스토리 후 게임 오버 화면
    IEnumerator GameOverSequence()
    {
        yield return new WaitForSecondsRealtime(
            gameOverStoryDuration
        );

        ShowGameOver();
    }

    // 게임 오버 화면
    public void ShowGameOver()
    {
        Debug.Log("게임 오버 화면 실행");

        if (gameOverStoryPanel != null)
            gameOverStoryPanel.SetActive(false);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            gameOverPanel.transform.SetAsLastSibling();

            Debug.Log("GameOverPanel 활성화 완료");
        }
        else
        {
            Debug.LogError(
                "GameOverPanel이 연결되지 않았어요"
            );
        }

        Time.timeScale = 0f;
    }

    // 재도전 버튼
    // 현재 스테이지를 처음부터 다시 시작
    public void RestartGame()
    {
        Debug.Log("재도전 버튼 실행");

        retryFromGameOver = true;

        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    // 종료하기 버튼
    // Stage1 타이틀 화면으로 복귀
    public void GoToTitle()
    {
        Debug.Log("타이틀로 이동");

        retryFromGameOver = false;

        Time.timeScale = 1f;

        SceneManager.LoadScene("Stage1");
    }

    // 기존 함수 이름 유지
    // QuitGame이 연결되어 있어도 타이틀로 이동
    public void QuitGame()
    {
        GoToTitle();
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}
