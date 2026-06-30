using UnityEngine;
using UnityEngine.Video;
using System.Threading.Tasks;

public class SplashScreen : MonoBehaviour
{
    [Header("UI & Video Settings")]
    public GameObject SplashUI;
    public VideoPlayer videoPlayer;

    private bool isVideoStarted = false;
    private bool isVideoPlaying = false;

    // 🌟 1. เพิ่มตัวแปรแม่กุญแจ ล็อคไม่ให้กดข้ามซ้อนกัน
    private bool isTransitioningToMenu = false;

    private void Start()
    {
        if (SplashUI != null) SplashUI.SetActive(true);
        if (videoPlayer != null) videoPlayer.gameObject.SetActive(false);

        if (videoPlayer != null)
        {
            videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.CameraNearPlane;
            videoPlayer.targetCamera = Camera.main;

            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.Prepare();
        }
    }

    private void Update()
    {
        if (Input.anyKeyDown && !Input.GetKeyDown(KeyCode.Escape) && !isVideoStarted)
        {
            isVideoStarted = true;
            TransitionToVideo();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 🌟 เช็คด้วยว่า ถ้ากำลังโหลดไปหน้าเมนูอยู่ จะไม่สนใจการกด ESC อีก
            if (isVideoPlaying && !isTransitioningToMenu)
            {
                TrySkipVideo();
            }
            else
            {
                Debug.Log("วิดีโอยังไม่พร้อม หรือ กำลังโหลดเข้าเมนูอยู่ กดซ้ำไม่ได้ครับ!");
            }
        }
    }

    private void TrySkipVideo()
    {
        SaveData currentData = SaveManager.Instance.LoadGame();

        if (currentData == null)
        {
            Debug.LogWarning("ไม่พบไฟล์เซฟเก่า: บังคับดูคัตซีนครั้งแรกให้จบก่อน");
            return;
        }

        if (currentData.hasWatchedIntro)
        {
            Debug.Log("กด ESC: ผู้เล่นเคยดูคัตซีนนี้แล้ว กดข้าม (Skip) ทันที!");
            isVideoPlaying = false;
            TransitionToMainMenu();
        }
        else
        {
            Debug.Log("กด ESC แต่ข้ามไม่ได้: ไฟล์เซฟบอกว่ายังไม่เคยดูคัตซีนนี้");
        }
    }

    private void TransitionToVideo()
    {
        LoadSceneManager.Instance.PlayLocalTransition(async () =>
        {
            if (videoPlayer != null)
            {
                videoPlayer.gameObject.SetActive(true);
                videoPlayer.Play();

                float timeout = 5f;
                while (!videoPlayer.isPlaying && timeout > 0)
                {
                    timeout -= Time.deltaTime;
                    await Task.Yield();
                }

                if (timeout <= 0)
                {
                    Debug.LogWarning("วิดีโอโหลดไม่ขึ้น สั่งเข้าเมนูหลัก");
                    if (SplashUI != null) SplashUI.SetActive(false);
                    TransitionToMainMenu();
                    return;
                }

                isVideoPlaying = true;

                if (SplashUI != null) SplashUI.SetActive(false);
            }
            else
            {
                if (SplashUI != null) SplashUI.SetActive(false);
                TransitionToMainMenu();
            }
        });
    }

    private void OnVideoFinished(VideoPlayer source)
    {
        if (!isVideoPlaying) return;
        isVideoPlaying = false;

        SaveData currentData = SaveManager.Instance.LoadGame();

        if (currentData == null)
        {
            currentData = new SaveData();
        }

        currentData.hasWatchedIntro = true;
        SaveManager.Instance.SaveGame(currentData);
        Debug.Log("💾 บันทึกสถานะการดูคัตซีนลงไฟล์ JSON สำเร็จ!");

        TransitionToMainMenu();
    }

    private void TransitionToMainMenu()
    {
        // 🌟 2. ดักหน้าประตู! ถ้ามีคนเผลอเรียกฟังก์ชันนี้ซ้ำตอนที่มันกำลังโหลดอยู่ ให้เตะกลับไปเลย
        if (isTransitioningToMenu) return;

        // ล็อคแม่กุญแจทันที!
        isTransitioningToMenu = true;

        if (videoPlayer != null)
        {
            videoPlayer.Pause();
        }

        LoadSceneManager.Instance.PlayLocalTransition(() =>
        {
            if (videoPlayer != null)
            {
                videoPlayer.Stop();
                videoPlayer.gameObject.SetActive(false);
            }
            Debug.Log("เข้าสู่หน้า Main Menu!");
        });
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}