using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Kedr
{
    /// <summary>
    /// Контроллер галереи с эффектом стирания изображения рукой через веб-камеру.
    /// Позволяет пользователю "стирать" верхнее изображение, постепенно открывая следующее.
    /// </summary>
    public class GalleryWipeController : MonoBehaviour
    {
        // ---- Shader Property IDs ----
        private static readonly int NoiseTex = Shader.PropertyToID("_NoiseTex");
        private static readonly int Threshold = Shader.PropertyToID("_Threshold");
        private static readonly int Blur = Shader.PropertyToID("_Blur");
        private static readonly int NoiseAmount = Shader.PropertyToID("_NoiseAmount");
        private static readonly int BrushTex = Shader.PropertyToID("_BrushTex");
        private static readonly int MaskTex = Shader.PropertyToID("_MaskTex");
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int BgTex = Shader.PropertyToID("_BgTex");
        private static readonly int FadeT = Shader.PropertyToID("_FadeT");
        private static readonly int NoiseScale = Shader.PropertyToID("_NoiseScale");
        private static readonly int EdgeFeather = Shader.PropertyToID("_EdgeFeather");

        // ---- UI ----

        [Header("UI")]
        [Tooltip("RawImage для вывода финального результата (используется RevealWithMask.shader).")]
        [SerializeField]
        private RawImage outputImage;

        [Tooltip("MeshRenderer для вывода на 3D-меш (опционально).")] [SerializeField]
        private MeshRenderer meshRenderer;

        // ---- Media ----

        [Header("Media")]
        [Tooltip("Текстура-заставка, показывается при бездействии. Сюда можно поставить RT от видео плеера")]
        [SerializeField]
        private Texture2D screensaverImage;

        [Tooltip("Массив изображений галереи для последовательного открытия.")] [SerializeField]
        private List<Texture2D> galleryImages = new List<Texture2D>();

        [Tooltip("Шумовая текстура для эффекта 'замороженных' или 'дымовых' краёв при стирании.")] [SerializeField]
        private Texture2D noiseTexture;

        // ---- Настройки маски ----

        [Header("Настройки маски")] [Tooltip("Индекс веб-камеры (0 — по умолчанию).")] [SerializeField]
        private int webcamIndex;

        [Tooltip("Размер маски (и рендер-текстур) в пикселях.")] [SerializeField]
        private Vector2Int maskSize = new(512, 512);

        [Tooltip("Порог яркости для определения 'руки' на изображении с камеры.")] [SerializeField, Range(0, 1)]
        private float threshold = 0.15f;

        [Tooltip("Радиус блюра (размытия) по маске.")] [SerializeField, Range(0, 2)]
        private float blur = 2.0f;

        [Tooltip("Интенсивность шума по краям стирания.")] [SerializeField, Range(0, 1)]
        private float noiseAmount = 0.2f;

        [Tooltip("Доля стёртой области (0.0 - ничего, 1.0 - полностью), при которой начинается плавный переход.")]
        [SerializeField, Range(0.2f, 1f)]
        private float revealThreshold = 0.75f;

        // ---- Неактивность ----

        [Header("Настройки неактивности")]
        [Tooltip("Время (сек), после которого показывается заставка при отсутствии движения.")]
        [SerializeField]
        private float inactivityTimeout = 5.0f;

        // ---- Материалы шейдеров ----

        [Header("Материалы для шейдеров")]
        [Tooltip("Материал для генерации 'кисти' — MaskBrush.shader.")]
        [SerializeField]
        private Material brushMaskMaterial;

        [Tooltip("Материал для накопления маски — MaskAccumulate.shader.")] [SerializeField]
        private Material accumulateMaterial;

        [Tooltip("Материал для эффекта проявления — RevealWithMask.shader.")] [SerializeField]
        private Material revealMaterial;

        // ---- Runtime-поля (скрыты в инспекторе) ----

        private WebCamTexture _webcam; // Веб-камера
        private RenderTexture _brushRT, _wipeMaskRT; // RT для кисти и основной маски
        [SerializeField] private float _inactivityTimer; // Счётчик времени неактивности
        private bool _screensaverActive = true; // Активен ли режим заставки
        private int _fgIndex, _bgIndex; // Индексы текущего верхнего и нижнего изображения
        private float _wipeDelayTimer; // Таймер задержки после смены изображения

        [Tooltip("Масштаб шума для эффекта края.")] [SerializeField]
        private float noiseScale = 0.5f;

        [Tooltip("Ширина размытого края стирания.")] [SerializeField]
        private float edgeFeather = 0.5f;

        // ---- Параметры fade (плавного исчезновения fg) ----

        [Tooltip("Длительность плавного исчезновения верхнего слоя, сек.")] [SerializeField]
        private float fadeDuration = 0.4f;

        private bool _isFading; // Флаг: идёт ли сейчас плавный переход
        private float _fadeTimer; // Таймер анимации fade


        //----------------------------------------------
        
        public MeshRenderer MeshRenderer;
        public RawImage CameraRawImage;
        private string _imagePath = "//Content//Image//";
        private string _videoPath = "//Content//Video//";
        private string[] _videoPaths;
        private int _currentVideoIndex = 0;
        private int _currentImageIndex = 0;
        public RawImage RawImage;
        public VideoPlayer VideoPlayer;
        private bool _isPaused;
        private CircleGalleryWipe _circleGalleryWipe;
        
        public Slider FadeSlider;
        public Slider ThresholdSlider;
        public TMP_Text FadeText;
        public TMP_Text ThresholdText;

        public GameObject SettingsPanel;
        public GameObject Circle;

        public bool IsRight;
        public bool IsLeft;
        public bool IsRevX;
        public bool IsRevY;
        
        public Toggle ToggleLeft;
        public Toggle ToggleRight;
        public Toggle ToggleRevX;
        public Toggle ToggleRevY;

        public TMP_Text CountBlack;

        public SpriteRenderer TestSpriteRenderer;
        public Image RemoveImage;
        public RawImage RemoveRawImage;
        public float DeltaColor = 0.3f;
        [SerializeField] private float blackCount;
        public float RemoveTimer = 5f;
        private float _removeTimer;
        private List<Sprite> _removeSprites = new List<Sprite>();
        
        //----------------------------------------------


        private void Awake()
        {
            //PlayerPrefs.DeleteAll();
        }

        /// <summary>
        /// Инициализация: запуск веб-камеры, создание RenderTexture, подготовка материалов и запуск заставки.
        /// </summary>
        private void Start()
        {
            _circleGalleryWipe = FindObjectOfType<CircleGalleryWipe>();
            _webcam = new WebCamTexture(WebCamTexture.devices[webcamIndex].name, maskSize.x, maskSize.y, 30);
            MeshRenderer.material.mainTexture = _webcam;
            CameraRawImage.texture = _webcam;
            _webcam.Play();
            Debug.Log(_webcam.width + "x" + _webcam.height);
            maskSize.x = _webcam.width;
            maskSize.y = _webcam.height;
            _circleGalleryWipe.Init(_webcam.width, _webcam.height);
            
            if (PlayerPrefs.HasKey("Fade"))
            {
                fadeDuration = PlayerPrefs.GetFloat("Fade")/10f;
                revealThreshold = PlayerPrefs.GetFloat("Threshold");
            }
            SettingsPanel.SetActive(false);
            MeshRenderer.gameObject.SetActive(false);
            MeshRenderer.transform.localScale = new Vector3((float)_webcam.width/_webcam.height, 1, 1);
            Circle.SetActive(false);

            _isPaused = true;
            galleryImages.Clear();
            FadeSlider.onValueChanged.AddListener(OnFadeSlider);
            ThresholdSlider.onValueChanged.AddListener(OnThresholdSlider);
            StartCoroutine(LoadImageVideo());
            
            if(PlayerPrefs.HasKey("IsLeft"))
                IsLeft = PlayerPrefs.GetInt("IsLeft") == 1;
            if(PlayerPrefs.HasKey("IsRight"))
                IsRight = PlayerPrefs.GetInt("IsRight") == 1;
            if(PlayerPrefs.HasKey("IsRevX"))
                IsRevX = PlayerPrefs.GetInt("IsRevX") == 1;
            if(PlayerPrefs.HasKey("IsRevY"))
                IsRevY = PlayerPrefs.GetInt("IsRevY") == 1;
            
            ToggleLeft.isOn = IsLeft;
            ToggleRight.isOn = IsRight;
            ToggleRevX.isOn = IsRevX;
            ToggleRevY.isOn = IsRevY;
        }

        private void Init()
        {
            _inactivityTimer = Time.time;
            
            _brushRT = new RenderTexture(maskSize.x, maskSize.y, 0, RenderTextureFormat.R8);
            _wipeMaskRT = new RenderTexture(maskSize.x, maskSize.y, 0, RenderTextureFormat.R8);

            // Материалы для вывода: новые экземпляры на каждый UI/mesh
            if (outputImage)
            {
                outputImage.material = new Material(revealMaterial);
                outputImage.material.SetTexture(NoiseTex, noiseTexture);
            }

            if (meshRenderer)
            {
                meshRenderer.material = new Material(revealMaterial);
                meshRenderer.material.SetTexture(NoiseTex, noiseTexture);
            }

            ShowScreensaver();
            ResetMask();
          
        }

        

        /// <summary>
        /// Основной цикл работы: обработка маски, смена изображений, логика fade и inactivity.
        /// </summary>
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ActivateSettings();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetMask();
            }

            if(_isPaused) return;
            if (_wipeDelayTimer > 0f)
            {
                _wipeDelayTimer -= Time.deltaTime;
                return;
            }

            if (!_webcam.didUpdateThisFrame) return;

            // Блок плавного исчезновения (fade)
            if (_isFading)
            {
                _fadeTimer += Time.deltaTime;
                float t = Mathf.Clamp01(_fadeTimer / fadeDuration);

                // Во время fade: fg и bg те же, но верхний слой постепенно исчезает (fadeT)
                Texture fg = GetImageByIndex(_fgIndex);
                Texture bg = GetImageByIndex(_bgIndex);
                SetRevealPair(fg, bg, t);

                if (_fadeTimer >= fadeDuration) //После таймера меняем изображения на следующие!!!
                {
                    _inactivityTimer = Time.time;
                    _isFading = false;
                    _fgIndex = _bgIndex;
                    _bgIndex = (_bgIndex + 1) % galleryImages.Count;
                    ResetMask();
                    // Показать следующую пару, fadeT=0 (нет плавного исчезновения)
                    SetRevealPair(GetImageByIndex(_fgIndex), GetImageByIndex(_bgIndex), 0f);
                    RemoveImage.sprite = GetSpriteByIndex(_fgIndex);
                    if (_screensaverActive)
                    {
                        _screensaverActive = false;
                    }
                }
                Debug.Log("Fade");
                return;
            }

            
            //Вытаскиваем только пятно света!!!
            int lenght = _circleGalleryWipe.Radius * 2;
            int cordX = _circleGalleryWipe.CenterX;
            int cordY = _circleGalleryWipe.CenterY;
            
            var readTex = new Texture2D(lenght,lenght, TextureFormat.R8, false);
            Color[] pixels = _webcam.GetPixels(cordX - _circleGalleryWipe.Radius, cordY - _circleGalleryWipe.Radius, lenght, lenght);
            int x = 0;
            int y = 0;
            
            for (int i = 0; i < lenght; i++) 
            {
                for (int j = 0; j < lenght; j++)
                {
                    x = i;
                    y = j;
                    if (IsRight)
                    {
                        x = j;
                        y = lenght - 1 - i;
                    }
                    else if (IsLeft)
                    {
                        x = lenght - 1 - j;
                        y = i;
                    }

                    if (IsRevX)
                    {
                        x = lenght - 1 - x;
                        //y = y;
                    }
                    else if (IsRevY)
                    {
                        //x = x;
                        y = lenght - 1 - y;
                    }

                    readTex.SetPixel(x, y, pixels[j * lenght + i]); //Стандартно
                }
            }

            //readTex.SetPixels(_webcam.GetPixels(200, 200, 100, 100));
            readTex.Apply();
             TestSpriteRenderer.sprite = Sprite.Create(readTex, new Rect(0.0f, 0.0f, readTex.width, readTex.height),
                 new Vector2(0.5f, 0.5f), 100.0f); 
            
            // --- Генерация маски по веб-камере ---
            brushMaskMaterial.mainTexture = readTex;
            brushMaskMaterial.SetTexture(NoiseTex, noiseTexture);
            brushMaskMaterial.SetFloat(Threshold, threshold);
            brushMaskMaterial.SetFloat(Blur, blur);
            brushMaskMaterial.SetFloat(NoiseAmount, noiseAmount);
            Graphics.Blit(readTex, _brushRT, brushMaskMaterial);

            // --- Накопление результата кисти в общей маске ---
            var tempRT = RenderTexture.GetTemporary(_wipeMaskRT.width, _wipeMaskRT.height, 0, RenderTextureFormat.R8);
            Graphics.Blit(_wipeMaskRT, tempRT);
            accumulateMaterial.SetTexture(BrushTex, _brushRT);
            Graphics.Blit(tempRT, _wipeMaskRT, accumulateMaterial);
            RenderTexture.ReleaseTemporary(tempRT);

            // --- Передача маски в reveal-шейдер ---
            if (outputImage) outputImage.material.SetTexture(MaskTex, _wipeMaskRT);
            if (meshRenderer) meshRenderer.material.SetTexture(MaskTex, _wipeMaskRT);

            //var handNow = HandDetected();

            int _blackCount = 0;
            for (int i = 0; i < readTex.width; i += 2)
            {
                for (int j = 0; j < readTex.height; j += 2)
                {
                    if (GetTruePixel(pixels[j * readTex.width + i]))
                    {
                        _blackCount++;
                    }
                }
            }

            blackCount = _blackCount;
            CountBlack.text = blackCount.ToString();
            
            if (blackCount > _circleGalleryWipe.CountBlackPoint)
            {
                
                //Debug.Log("Hand Detected");
                // if (_screensaverActive)
                // {
                //     _screensaverActive = false;
                //     _fgIndex = -1;
                //     _bgIndex = 0;
                //     ResetMask();
                //     SetRevealPair(GetImageByIndex(_fgIndex), GetImageByIndex(_bgIndex), 0f);
                // }

                // Если стерто достаточно — запускаем fade
                if (CheckRevealedPercent(_wipeMaskRT) > revealThreshold && !_isFading)
                {
                    _isFading = true;
                    _fadeTimer = 0f;
                }

                if (Time.time - _removeTimer > RemoveTimer)
                {
                    ResetMask();
                }

                _removeTimer = Time.time;
                _inactivityTimer = Time.time;
                Color color = Color.white;
                color.a = 0;
                RemoveImage.color = color;
                RemoveRawImage.color = color;
            }
            else
            {
                //Debug.Log("No Hand Detected");
                //_inactivityTimer += Time.deltaTime;
                if (Time.time - _removeTimer > RemoveTimer)
                {
                    //Запускаем Ремув изображения
                    
                    if(_screensaverActive)
                        RemoveRawImage.color = Color.Lerp(RemoveRawImage.color, Color.white, Time.deltaTime * 20);
                    else
                    {
                        RemoveImage.color = Color.Lerp(RemoveImage.color, Color.white, Time.deltaTime * 20);
                    }
                }

                if (!_screensaverActive && Time.time - _inactivityTimer > inactivityTimeout)
                {
                    ShowScreensaver();
                    ResetMask();
                }
            }

           


        }

        /// <summary>
        /// Получить текстуру по индексу (gallery), либо screensaver, если индекс < 0.
        /// </summary>
        private Texture GetImageByIndex(int idx)
        {
            if (idx < 0) return screensaverImage;
            if (galleryImages.Count == 0) return screensaverImage;
            return galleryImages[idx % galleryImages.Count];
        }

        private Sprite GetSpriteByIndex(int idx)
        {
            return _removeSprites[idx % _removeSprites.Count];
        }

        /// <summary>
        /// Установить пары текстур для проявления (верхняя, нижняя) и значение fadeT (0-1).
        /// </summary>
        private void SetRevealPair(Texture fg, Texture bg, float fadeT)
        {
            var m = new Material(revealMaterial);
            m.SetTexture(MainTex, fg);
            m.SetTexture(BgTex, bg);
            m.SetTexture(MaskTex, _wipeMaskRT);
            m.SetTexture(NoiseTex, noiseTexture);
            m.SetFloat(NoiseScale, noiseScale);
            m.SetFloat(EdgeFeather, edgeFeather);
            m.SetFloat(FadeT, fadeT);

            if (outputImage) outputImage.material = m;
            if (meshRenderer) meshRenderer.material = m;
        }

        /// <summary>
        /// Показать заставку и подготовить к первому стиранию.
        /// </summary>
        private void ShowScreensaver()
        {
            Debug.Log("Screensaver");
            ChangeVideo();
            _screensaverActive = true;
            _fgIndex = -1;
            _bgIndex = 0;
            var bg = galleryImages.Count > 0 ? galleryImages[0] : null;
            SetRevealPair(RawImage.mainTexture, bg, 0f);
        }

        /// <summary>
        /// Сброс маски (очистка). Также ставит небольшую задержку перед повторным стиранием.
        /// </summary>
        private void ResetMask()
        {
            _wipeDelayTimer = 1.0f;
            RenderTexture.active = _wipeMaskRT;
            GL.Clear(false, true, Color.black);
            RenderTexture.active = null;
        }

        /// <summary>
        /// Детектирование наличия "руки" на маске (по количеству белых пикселей).
        /// </summary>
        private bool HandDetected()
        {
            var readTex = new Texture2D(_brushRT.width, _brushRT.height, TextureFormat.R8, false);
            RenderTexture.active = _brushRT;
            readTex.ReadPixels(new Rect(0, 0, _brushRT.width, _brushRT.height), 0, 0);
            readTex.Apply();
            RenderTexture.active = null;

            var pixels = readTex.GetPixels32();
            var white = 0;
            for (var i = 0; i < pixels.Length; i++)
                if (pixels[i].r > 127)
                    white++;
            Destroy(readTex);
            return white > pixels.Length * 0.01f; // >1% белых пикселей — рука есть
        }

        /// <summary>
        /// Подсчитать долю стертой области (по количеству белых пикселей в маске).
        /// </summary>
        private float CheckRevealedPercent(RenderTexture maskRT)
        {
            var readTex = new Texture2D(maskRT.width, maskRT.height, TextureFormat.R8, false);
            RenderTexture.active = maskRT;
            readTex.ReadPixels(new Rect(0, 0, maskRT.width, maskRT.height), 0, 0);
            readTex.Apply();
            RenderTexture.active = null;

            var pixels = readTex.GetPixels32();
            var erased = 0;
            for (var i = 0; i < pixels.Length; i++)
                if (pixels[i].r > 127)
                    erased++;
            Destroy(readTex);
            return (float)erased / pixels.Length;
        }
        
        //---------------------------------------------------------------------
        
        private void CreateFolder()
        {
            if(!Directory.Exists(Directory.GetCurrentDirectory()+_imagePath))
                Directory.CreateDirectory(Directory.GetCurrentDirectory()+_imagePath);
            if(!Directory.Exists(Directory.GetCurrentDirectory()+_videoPath))
                Directory.CreateDirectory(Directory.GetCurrentDirectory()+_videoPath);
        }
        
        private IEnumerator LoadImageVideo()
        {
            string[] pngFiles = Directory.GetFiles(Directory.GetCurrentDirectory() + _imagePath, "*.png");
            foreach (string pngFile in pngFiles)
            {
                string url = "file://" + pngFile;

                using (WWW www = new WWW(url))
                {
                    yield return www;
                    //Debug.Log(www.texture);
                    Texture2D texture2D = www.texture;
                    galleryImages.Add(texture2D);
                    Sprite sprite =  Sprite.Create(texture2D, new Rect(0.0f, 0.0f, texture2D.width, texture2D.height),
                        new Vector2(0.5f, 0.5f), 100.0f); 
                    _removeSprites.Add(sprite);
                }
            }

            _videoPaths = Directory.GetFiles(Directory.GetCurrentDirectory() + _videoPath, "*.mp4");
            _currentVideoIndex = -1;
            Init();
            ChangeVideo();
            _isPaused = false;
        }
        
        private void ChangeVideo()
        {
            _currentVideoIndex++;
            if(_currentVideoIndex >= _videoPaths.Length) _currentVideoIndex = 0;
            VideoPlayer.url = _videoPaths[_currentVideoIndex];
        }

        private void OnFadeSlider(float valume)
        {
            fadeDuration = valume/10f;
            FadeText.text = valume.ToString().Substring(0,4);
        }

        private void OnThresholdSlider(float valume)
        {
            revealThreshold = valume;
            ThresholdText.text = valume.ToString().Substring(0,4);
        }

        private void ActivateSettings()
        {
            if (SettingsPanel.activeSelf)
            {
                SettingsPanel.SetActive(false);
                MeshRenderer.gameObject.SetActive(false);
                Circle.SetActive(false);
                PlayerPrefs.SetFloat("Fade", FadeSlider.value);
                PlayerPrefs.SetFloat("Threshold", ThresholdSlider.value);
                PlayerPrefs.SetInt("IsLeft", IsLeft ? 1 : 0);
                PlayerPrefs.SetInt("IsRight", IsRight ? 1 : 0);
                PlayerPrefs.SetInt("IsRevX", IsRevX ? 1 : 0);
                PlayerPrefs.SetInt("IsRevY", IsRevY ? 1 : 0);
                PlayerPrefs.Save();
                _circleGalleryWipe.SaveSettings();
                Debug.Log("Save Settings " + FadeSlider.value + " " + ThresholdSlider.value);
                CameraRawImage.SetNativeSize();
            }
            else
            {
                SettingsPanel.SetActive(true);
                MeshRenderer.gameObject.SetActive(true);
                Circle.SetActive(true);
                fadeDuration = PlayerPrefs.GetFloat("Fade") / 10;
                revealThreshold = PlayerPrefs.GetFloat("Threshold");
                FadeText.text = (fadeDuration * 10).ToString().Substring(0, 4);
                ThresholdText.text = revealThreshold.ToString().Substring(0, 4);
                FadeSlider.value = fadeDuration * 10;
                ThresholdSlider.value = revealThreshold;
                Debug.Log("Load Settings");
            }
        }

        public void OnLeft(Toggle value)
        {
            IsLeft = value.isOn;
        }

        public void OnRight(Toggle value)
        {
            IsRight = value.isOn;
        }

        public void OnRevX(Toggle value)
        {
            IsRevX  = value.isOn;
        }

        public void OnRevY(Toggle value)
        {
            IsRevY = value.isOn;
        }
        
        private bool GetTruePixel(Color color)
        {
            if (color.r < DeltaColor &&
                color.g < DeltaColor &&
                color.b < DeltaColor)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }

}