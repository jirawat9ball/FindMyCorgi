using UnityEngine;
using UnityEngine.Advertisements;

public class AdsManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    public static AdsManager Instance { get; private set; }

    [Header("ใส่ Game ID จากหน้าเว็บโฆษณา (ลบตัวเลขสมมติทิ้ง)")]
    [SerializeField] string _androidGameId = "1234567";
    [SerializeField] string _iOSGameId = "1234568";
    
    [Header("โหมดทดสอบ (ปิดก่อนออกเกมจริง)")]
    [SerializeField] bool _testMode = true;

    private string _gameId;
    
    // ตั้งชื่อเป้าหมายวิดีโอให้ตรงกับระบบ Unity Ads Dashboard ค่าตั้งต้นมักจะเป็นแบบนี้
    private string _androidAdUnitId = "Rewarded_Android";
    private string _iOSAdUnitId = "Rewarded_iOS";
    private string _adUnitId;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAds();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitializeAds()
    {
        #if UNITY_IOS
            _gameId = _iOSGameId;
            _adUnitId = _iOSAdUnitId;
        #elif UNITY_ANDROID
            _gameId = _androidGameId;
            _adUnitId = _androidAdUnitId;
        #elif UNITY_EDITOR
            _gameId = _androidGameId; // ให้ใช้ Android จำลองตอนเทสบนคอม
            _adUnitId = _androidAdUnitId;
        #else
            _gameId = _androidGameId;
            _adUnitId = _androidAdUnitId;
        #endif

        if (!Advertisement.isInitialized && Advertisement.isSupported)
        {
            Advertisement.Initialize(_gameId, _testMode, this);
        }
    }

    public void OnInitializationComplete()
    {
        Debug.Log("✅ [AdsManager] เชื่อมระบบ Unity Ads สำเร็จแล้ว.");
        LoadRewardedAd(); // สั่งโหลดวิดีโอเตรียมรอไว้ทันที
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"❌ [AdsManager] การเชื่อมโหลดโฆษณาล้มเหลว: {error} - {message}");
    }

    // ===================================
    // การเริ่มโหลด และเล่นโฆษณา
    // ===================================

    public void LoadRewardedAd()
    {
        // สั่งดึงวิดีโอมาแคชหลังฉากไว้ล่วงหน้า
        Debug.Log("📹 [AdsManager] กำลังโหลดวิดีโอรอ: " + _adUnitId);
        Advertisement.Load(_adUnitId, this);
    }

    // 🌟 เอาปุ่ม UI มาผูกกับคำสั่งนี้ เพื่อกดสั่งเล่นโฆษณา!
    public void ShowRewardedAd()
    {
        // ตรวจสอบว่าโฆษณาโหลดเสร็จพร้อมดูหรือไม่
        Debug.Log("▶️ [AdsManager] เริ่มเล่นโฆษณา: " + _adUnitId);
        Advertisement.Show(_adUnitId, this);
    }

    // ===================================
    // Callback สถานะการดาวน์โหลดวิดีโอ
    // ===================================
    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        // โฆษณาโหลดพร้อมแล้ว (อาจจะเอาไปสั่งเปิดปุ่มกดใน UI ได้)
        Debug.Log("✔️ [AdsManager] ดาวน์โหลดวิดีโอเสร็จสิ้น: " + adUnitId);
    }

    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.Log($"❗ [AdsManager] ดาวน์โหลดวิดีโอล้มเหลว {adUnitId}: {error} - {message}");
    }

    // ===================================
    // Callback สถานะขณะเล่นวิดีโอ
    // ===================================
    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.Log($"❗ [AdsManager] โชว์วิดีโอไม่ได้ {adUnitId}: {error} - {message}");
    }

    public void OnUnityAdsShowStart(string adUnitId) {}
    public void OnUnityAdsShowClick(string adUnitId) {}

    // 🌟 คืนค่ารางวัลให้ผู้เล่น ตรงนี้สำคัญสุด !! 🌟
    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        // เช็คว่าดูวิดีโอแจกรางวัล... และดูจนจบจริงๆ ไม่ได้ข้าม
        if (adUnitId.Equals(_adUnitId) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        {
            Debug.Log("🎁 [AdsManager] เยี่ยมมาก! ดูโฆษณาจบ จะได้คำใบ้ (Snack) +1 อัน");
            
            // ให้รางวัลทันที และระบบจะบันทึกอัตโนมัติ
            if (Gamemanager.Instance != null)
            {
                 Gamemanager.Instance.AddSnackFromAd(1);
            }
            
            // เตรียมดึงวิดีโออันใหม่มารอไว้สำหรับกดรอบต่อไป
            LoadRewardedAd();
        }
        else if (showCompletionState.Equals(UnityAdsShowCompletionState.SKIPPED))
        {
            Debug.Log("⚠️ [AdsManager] ฮั่นแน่! กดข้ามโฆษณานี่นา... ไม่ให้ขนมหรอกนะ");
        }
    }
}
