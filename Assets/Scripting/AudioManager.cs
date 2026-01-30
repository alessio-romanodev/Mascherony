using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("BGM")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioClip bgm;

    [Header("SFX")]
    [SerializeField] private AudioClip jumpSfx;
    [SerializeField] private AudioClip attackSfx;
    [SerializeField] private AudioClip dashSfx;
    [SerializeField] private AudioClip objectSfx;

    [Header("Footsteps")]
    [SerializeField] private AudioClip[] footstepSfx;

    [Header("SFX Settings")]
    [SerializeField] private float sfxVolume = 1f;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        PlayBGM();
    }

    // ================= BGM =================
    public void PlayBGM()
    {
        if (bgm == null) return;

        bgmSource.clip = bgm;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    // ================= SFX =================
    public void PlayJump()     => PlaySFX(jumpSfx);
    public void PlayAttack()   => PlaySFX(attackSfx);
    public void PlayDash()     => PlaySFX(dashSfx);
    public void PlayObject()   => PlaySFX(objectSfx);

    // ================= Footstep =================
    public void PlayFootstep()
    {
        if (footstepSfx == null || footstepSfx.Length == 0) return;

        AudioClip clip = footstepSfx[Random.Range(0, footstepSfx.Length)];
        PlaySFX(clip);
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        GameObject sfxObj = new GameObject("SFX_" + clip.name);
        sfxObj.transform.parent = transform;

        AudioSource source = sfxObj.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = sfxVolume;
        source.pitch = Random.Range(0.95f, 1.05f);
        source.Play();

        Destroy(sfxObj, clip.length / source.pitch);
    }
}
