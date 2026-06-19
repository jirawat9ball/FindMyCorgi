using UnityEngine;
using UnityEngine.Video;

public class SplashScreen : MonoBehaviour
{
    [Header("UI & Video Settings")]
    public GameObject SplashUI;
    public VideoPlayer videoPlayer;

    private bool isVideoStarted = false;
    private bool isVideoPlaying = false;

    private void Start()
    {
        if (SplashUI != null) SplashUI.SetActive(true);
        if (videoPlayer != null) videoPlayer.gameObject.SetActive(false);

        if (videoPlayer != null)
        {
            //ดึงกล้องจาก CoreManager
            videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.CameraNearPlane;
            videoPlayer.targetCamera = Camera.main;

            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.Prepare();
        }
    }

    private void Update()
    {
        // กดปุ่มอะไรก็ได้เพื่อเริ่ม (แต่ขอยกเว้นปุ่ม ESC ไว้ จะได้ไม่ชนกัน)
        if (Input.anyKeyDown && !Input.GetKeyDown(KeyCode.Escape) && !isVideoStarted)
        {
            isVideoStarted = true;
            TransitionToVideo();
            return;
        }

        // ถ้าวิดีโอกำลังฉายอยู่ และผู้เล่นกดปุ่ม ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // ดักเช็คก่อนว่าวิดีโอเริ่มเล่นจริงๆ หรือยัง
            if (isVideoPlaying)
            {
                TrySkipVideo();
            }
            else
            {
                Debug.Log("วิดีโอยังไม่พร้อม หรือกำลังทรานสิชันอยู่ กดข้ามไม่ได้นะ");
            }
        }
    }
    private void TrySkipVideo()
    {
        // ดึงข้อมูลเซฟ
        SaveData currentData = SaveManager.Instance.LoadGame();

        // ถ้าไม่เคยมีไฟล์เซฟมาก่อน เพิ่งเล่นครั้งแรก ห้ามข้าม!
        if (currentData == null)
        {
            Debug.LogWarning("ไม่พบไฟล์เซฟเก่า: บังคับดูคัตซีนครั้งแรกให้จบก่อน");
            return;
        }

        // ถ้ามีไฟล์เซฟ เช็คว่าเคยดูหรือยัง
        if (currentData.hasWatchedIntro)
        {
            Debug.Log("กด ESC: ผู้เล่นเคยดูคัตซีนนี้แล้ว กดข้าม (Skip) ทันที!");

            isVideoPlaying = false; // ล็อคไว้ไม่ให้กดข้ามซ้ำสองจนระบบรวน
            TransitionToMainMenu();
        }
        else
        {
            Debug.Log("กด ESC แต่ข้ามไม่ได้: ไฟล์เซฟบอกว่ายังไม่เคยดูคัตซีนนี้");
        }
    }

    private void TransitionToVideo()
    {
        LoadSceneManager.Instance.PlayLocalTransition(() =>
        {
            if (SplashUI != null) SplashUI.SetActive(false);

            if (videoPlayer != null)
            {
                videoPlayer.gameObject.SetActive(true);
                videoPlayer.Play();
                isVideoPlaying = true;
            }
            else
            {
                TransitionToMainMenu();
            }
        });
    }
    private void OnVideoFinished(VideoPlayer source)
    {
        if (!isVideoPlaying) return;
        isVideoPlaying = false;

        SaveData currentData = SaveManager.Instance.LoadGame();

        // ถ้าไฟล์เซฟเป็น null (เล่นครั้งแรก) ให้สร้างใหม่
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
        // สั่ง Pause วิดีโอทันที! 
        // ภาพบนจอจะหยุดนิ่ง "ปุ่มกดติดแล้ว" ระหว่างรอม่านดำปิดลงมา
        if (videoPlayer != null)
        {
            videoPlayer.Pause();
        }

        LoadSceneManager.Instance.PlayLocalTransition(() =>
        {
            // พอม่านดำปิดสนิท ค่อยสั่งปิดการทำงานของวิดีโอทิ้งไป
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