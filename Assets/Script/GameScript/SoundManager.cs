using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; } // Instance ของ Singleton
    AudioSource _audioEF;
    AudioSource _audioBG;

    public AudioClip OnClickSound;

    public AudioClip MainMenu;
    public AudioClip DefaultBGSound;

    private void Awake()
    {
        // ตรวจสอบ Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupAudioSources(); // แยกฟังก์ชันตั้งค่าออกมาให้เป็นระเบียบ
        }
        else
        {
            Destroy(gameObject);
            return; // 🌟 ต้องใส่ return เพื่อหยุดการทำงานของสคริปต์ตัวที่ถูกทำลายด้วยครับ
        }

    }
    private void SetupAudioSources()
    {
        // 🌟 ดึง Component มาใช้ก่อน (ป้องกันการสร้างเบิ้ลซ้ำซ้อน)
        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources.Length >= 2)
        {
            _audioEF = sources[0];
            _audioBG = sources[1];
        }
        else
        {
            _audioEF = gameObject.AddComponent<AudioSource>();
            _audioBG = gameObject.AddComponent<AudioSource>();
        }

        _audioBG.loop = true;
        _audioBG.playOnAwake = true;

        if (DefaultBGSound != null)
        {
            PlayBGSound(DefaultBGSound);
        }
    }
    public void OnSliderBGValueChange(float v) {
        _audioBG.volume = v;
    }
    public void OnSliderEFValueChange(float v)
    {
        _audioEF.volume = v;
    }
    public void PlayOnClickSound() {
        _audioEF.PlayOneShot(OnClickSound);
    }
    public void PlayBGSound(AudioClip BGSound) {
        if (BGSound == null) return;
        if (_audioBG.clip == BGSound && _audioBG.isPlaying) return; // 🌟 ถ้าเป็นเพลงเดิมที่กำลังเล่นอยู่ ให้ข้ามไปเลย ไม่ต้องเริ่มใหม่
        _audioBG.clip = BGSound;
        _audioBG.Play();
    }

    public void PlayBGSoundDefault() {         
        PlayBGSound(DefaultBGSound);
    }
    public void PlayBGSoundMainMenu()
    {
        PlayBGSound(MainMenu);
    }


}
