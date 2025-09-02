using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Profiling.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;


public class GameManager : MonoBehaviour
{

    public static GameManager Instance;
    public GameObject ContentCanvas;
    
    public Image ImageUp;
    public Image ImageDown;
    public RawImage RawImage;
    public VideoPlayer VideoPlayer;
    
    public List<Sprite> Sprites = new List<Sprite>();

    public float TimeOut = 5.0f; //Время на переключение на видео
    public int CountEffectParticle = 7000; //Количество партиклов для обновления фото и видео
    
    private int _currentVideoIndex = 0;
    private int _currentImageIndex = 0;
    private float _currentTime = 0f;
    public bool _isPlaying = false;
    public bool _isPaused = false;
    public float MaxScale;
    public float MinScale;
    public float SpeedParticle;
    public GameObject Prefab;
    public Transform ParentPrefab;
    
    public Queue<ParticlePrefab> ParticlePrefabs = new Queue<ParticlePrefab>();
    
    private Sprite _currentSprite;

    [SerializeField] Transform mouseEffect;
    [SerializeField] ParticleSystem mouseEffectPartSystem;
    [SerializeField] ParticleSystem finishEffectPartSystem;
    [SerializeField] Vector3 mousepos;
    [SerializeField] int countParticle = 0;

    [SerializeField] RenderTexture rt;
    
    public Material materialDefault;
    public Material materialMultiply;
    
    private string _imagePath = "//Content//Image//";
    private string _videoPath = "//Content//Video//";
    private string[] _videoPaths;

    private float _x = 1400;
    private float _y = 1050;
    private float _timer;
    private Vector3 _lastPosition;
    public int CountParticle;
    public Transform FinishParticle;
    
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        FinishParticle.gameObject.SetActive(false);
        FinishParticle.position=Vector3.zero;
        _lastPosition = Vector3.one*100f;
        ContentCanvas.SetActive(true);
        _isPaused = false;
        _isPlaying = false;
        mouseEffectPartSystem = mouseEffect.GetComponent<ParticleSystem>();
        //finishEffectPartSystem.Clear();
        //finishEffectPartSystem.Pause();

        for (int i = 0; i < 10; i++)
        {
            ParticlePrefab prefab = Instantiate(Prefab, ParentPrefab).GetComponent<ParticlePrefab>();
            prefab.Init();
        }

        CountParticle = 0;
        CreateFolder();
        StartCoroutine(LoadImageVideo());
        StartCoroutine(StartInit());
    }

    private void CreateFolder()
    {
        if(!Directory.Exists(Directory.GetCurrentDirectory()+_imagePath))
            Directory.CreateDirectory(Directory.GetCurrentDirectory()+_imagePath);
        if(!Directory.Exists(Directory.GetCurrentDirectory()+_videoPath))
            Directory.CreateDirectory(Directory.GetCurrentDirectory()+_videoPath);
    }

    private IEnumerator LoadImageVideo()
    {
        _isPaused = true;
        string[] pngFiles = Directory.GetFiles(Directory.GetCurrentDirectory() + _imagePath, "*.png");
        foreach (string pngFile in pngFiles)
        {
            string url = "file://" + pngFile;

            using (WWW www = new WWW(url))
            {
                yield return www;
                //Debug.Log(www.texture);
                Texture2D texture2D = www.texture;
                Sprite sprite = Sprite.Create(texture2D, new Rect(0.0f, 0.0f, texture2D.width, texture2D.height),
                    new Vector2(0.5f, 0.5f), 100.0f);
                Sprites.Add(sprite);
            }
        }
        _videoPaths = Directory.GetFiles(Directory.GetCurrentDirectory() + _videoPath, "*.mp4");
        _currentVideoIndex = -1;
        ChangeVideo();
        _isPaused = false;
    }

    IEnumerator StartInit()
    {
        yield return new WaitForSeconds(1f);
        Init();
    }

    private void Update()
    {
        if (_isPlaying && Time.time - _currentTime > TimeOut)
        {
            _isPlaying = false;
            StartCoroutine(ShowVideo());
            Init();
        }
        countParticle = mouseEffectPartSystem.particleCount;
    }

    private void LateUpdate()
    {
        OffEffect();
    }

    IEnumerator ShowVideo()
    {
        _isPaused = true;
        
        RawImage.enabled = true;
        countParticle = 0;
        ImageUp.material = materialDefault;
        yield return new WaitForSeconds(1f);
        _isPlaying = false;
        _isPaused = false;
        yield break;
    }

    private void ChangeVideo()
    {
        _currentVideoIndex++;
        if(_currentVideoIndex >= _videoPaths.Length) _currentVideoIndex = 0;
        VideoPlayer.url = _videoPaths[_currentVideoIndex];
    }
    
    private void ChangeImage()
    {
        _currentImageIndex++;
        if(_currentImageIndex >= Sprites.Count) _currentImageIndex = 0;
        ImageUp.sprite = Sprites[_currentImageIndex];
        if (_currentImageIndex + 1 >= Sprites.Count)
        {
            ImageDown.sprite = Sprites[0];    
        }
        else
        {
            ImageDown.sprite = Sprites[_currentImageIndex + 1];
        }
        ImageUp.material.SetFloat("_HoleRadius", -0.1f);
        
        StartCoroutine(TimeOutCoroutine(1f));
    }

    private IEnumerator StartEffectAnimationImage()
    {
        Debug.Log("Start Effect");
        _isPaused = true;

        yield return StartCoroutine(StartFinish());
        //yield return new WaitForSeconds(0.2f);
        ChangeSpriteVideo();
        countParticle = 0;
        mouseEffectPartSystem.Clear();
        yield return new WaitForSeconds(1f);
        _isPaused = false;
    }
    
    private IEnumerator StartEffectAnimationVideo()
    {
        Debug.Log("Start Effect");
        _isPaused = true;
        
        yield return StartCoroutine(StartFinish());
        //yield return new WaitForSeconds(0.2f);
        ChangeVideo();
        ChangeSpriteVideo();
        RawImage.enabled = false;
        countParticle = 0;
        mouseEffectPartSystem.Clear();
        ImageUp.material = materialMultiply;
        ImageUp.sprite = ImageUp.sprite;
        yield return new WaitForSeconds(1f);
        _isPaused = false;
    }

    private void MoveImage(int x, int y, float radius)
    {
        float xx = (x / _x) * 10.5f - 5.25f;
        float yy = (1 - y / _y) * 10.5f - 5.25f;
        OnEffect(xx, yy);
        if (!_isPaused && CountParticle > CountEffectParticle)
            StartCoroutine(StartEffectAnimationImage());
    }

    private void MoveVideo(int x, int y, float radius)
    {
        float xx = (x / _x) * 10.5f - 5.25f;
        float yy = (1 - y / _y) * 10.5f - 5.25f;
        OnEffect(xx, yy);
        if (!_isPaused && CountParticle > CountEffectParticle)
            StartCoroutine(StartEffectAnimationVideo());
    }

    private void ChangeSpriteVideo()
    {
        if (_isPlaying)
        {
            //Меняем фото
            ChangeImage();
        }
        else
        {
            //Выключаем видео
            //RawImage.gameObject.SetActive(false);
            _isPlaying = true;
        }
    }

    IEnumerator TimeOutCoroutine(float timeOut)
    {
        _isPaused = true;
        yield return new WaitForSeconds(timeOut);
        _isPaused = false;
    }

    public void OnClick(float x, float y, float radius)
    {
        if(_isPaused) return;
        _currentTime = Time.time;
        int xx = (int)(x * _x);
        int yy = (int)(y * _y);
        if (_isPlaying)
        {
            MoveImage(xx, yy, radius);
        }
        else
        {
            MoveVideo(xx, yy, radius);
        }
    }

    private void Init()
    {
        RawImage.material.SetFloat("_HoleRadius", -0.1f);
        ImageUp.material.SetFloat("_HoleRadius", -0.1f);
    }
    
    public void OnEffect(float x, float y)
    {
        if(Time.time - _timer<0.1f) return;
        Vector3 vector3 = new Vector3(-x, -y, -1);
        if((vector3-_lastPosition).magnitude<2.5f) return;
        _lastPosition = vector3;
        _timer = Time.time;
        //Debug.Log(x+" " + y);
        ParticlePrefab pp = ParticlePrefabs.Dequeue();
        pp.transform.position = vector3;
        pp.Show();
        
        //mouseEffect.localPosition = new Vector3(-x, -y, 50);
        //mouseEffectPartSystem.enableEmission = true;
    }

    public void OffEffect()
    {
        //mouseEffectPartSystem.enableEmission = false;
    }

    IEnumerator StartFinish()
    {
        FinishParticle.gameObject.SetActive(true);
        float s = 0.1f;
        FinishParticle.localScale = Vector3.one * s;
        while (FinishParticle.localScale.x < 3f)
        {
            s += Time.deltaTime * 2f;
            FinishParticle.localScale = Vector3.one * s;
            yield return null;
        }

        List<ParticlePrefab> pp = ParentPrefab.GetComponentsInChildren<ParticlePrefab>().ToList();
        foreach (var prefab in pp)
        {
            prefab.Hide();
        }
        FinishParticle.gameObject.SetActive(false);
    }

}
