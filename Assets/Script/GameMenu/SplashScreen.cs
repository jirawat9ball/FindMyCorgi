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

            // 🌟 ให้เล่นเสียงจาก VDO โดยตรง
            videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
            videoPlayer.EnableAudioTrack(0, true);
            videoPlayer.SetDirectAudioVolume(0, 1f);

            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.Prepare();
        }
    }

    private void Update()
    {
        if (isTransitioningToMenu) return;

        // 🌟 ป้องกันการรับ input ระหว่างที่ LoadSceneManager กำลังทำ Transition (กันบั๊กกดข้ามตอนยังโหลดไม่เสร็จ)
        if (LoadSceneManager.Instance != null && !LoadSceneManager.Instance.isReady) return;

        if (Input.anyKeyDown)
        {
            if (!isVideoStarted)
            {
                isVideoStarted = true;
                TransitionToVideo();
            }
            else if (isVideoPlaying)
            {
                // 🌟 พยายามกดข้ามถ้าวิดีโอกำลังเล่นอยู่ (ใช้ได้ทุกปุ่ม)
                TrySkipVideo();
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
            Debug.Log("ผู้เล่นเคยดูคัตซีนนี้แล้ว กดข้าม (Skip) ทันที!");
            isVideoPlaying = false;
            TransitionToMainMenu();
        }
        else
        {
            Debug.Log("ข้ามไม่ได้: ต้องดูคัตซีนครั้งแรกให้จบก่อน");
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

                // 🌟 เปิดตัวบังคลิก (RaycastBlocker) ตอนวิดีโอเริ่มเล่น
                if (UIManager.Instance != null && UIManager.Instance.RaycastBlocker != null)
                {
                    UIManager.Instance.RaycastBlocker.SetActive(true);
                }
                
                // 🌟 ปิดเสียงดนตรีประกอบ (BG Music) ระหว่างที่เล่นวิดีโอ
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PauseBGSound(true);
                }

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
        
        // 🌟 ซิงค์ข้อมูลไปที่ Gamemanager ด้วย (ถ้ามี) ไม่งั้นเดี๋ยว Gamemanager จะเซฟทับกลับเป็น false!
        if (Gamemanager.Instance != null && Gamemanager.Instance.currentSaveData != null)
        {
            Gamemanager.Instance.currentSaveData.hasWatchedIntro = true;
        }

        Debug.Log("💾 บันทึกสถานะการดูคัตซีนลงไฟล์ JSON สำเร็จ!");

        TransitionToMainMenu();
    }

    private async void TransitionToMainMenu()
    {
        // 🌟 2. ดักหน้าประตู! ถ้ามีคนเผลอเรียกฟังก์ชันนี้ซ้ำตอนที่มันกำลังโหลดอยู่ ให้เตะกลับไปเลย
        if (isTransitioningToMenu) return;

        // ล็อคแม่กุญแจทันที!
        isTransitioningToMenu = true;

        if (videoPlayer != null)
        {
            videoPlayer.Pause();
        }

        // 🌟 ป้องกันกรณี LoadSceneManager กำลังทำงานอยู่ ให้รอจนกว่าจะพร้อมก่อนเริ่ม Transition ใหม่
        if (LoadSceneManager.Instance != null)
        {
            while (!LoadSceneManager.Instance.isReady)
            {
                await Task.Yield();
            }
        }

        // 🌟 ปิดตัวบังคลิก (RaycastBlocker) กลับคืนเมื่อออกจากวิดีโอ
        if (UIManager.Instance != null && UIManager.Instance.RaycastBlocker != null)
        {
            UIManager.Instance.RaycastBlocker.SetActive(false);
        }
        
        // 🌟 เปิดเสียงดนตรีประกอบ (BG Music) กลับคืนมา
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PauseBGSound(false);
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

        // เผื่อซีนถูกทำลายระหว่างทาง ให้แน่ใจว่าปิด Blocker แน่นอน
        if (UIManager.Instance != null && UIManager.Instance.RaycastBlocker != null)
        {
            UIManager.Instance.RaycastBlocker.SetActive(false);
        }
    }
}