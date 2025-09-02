using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WebCameraClass : MonoBehaviour
{
    public TMP_Text FPS;
    public TMP_Text BlackCount;
    public TMP_Text Sensitivity;
    public float DeltaColor;
    public int Radius;
    public int CenterX;
    public int CenterY;
    public Transform Circle;
    
    private WebCamTexture webcamTexture;
    private Color[] colors;
    private float _time;
    private int _fps;
    private int _blackCount;
    private int _width;
    private int _height;
    private GameManager _manager;

    public Button b_SensitivityUp;
    public Button b_SensitivityDown;
    public Button b_RadiusUp;
    public Button b_RadiusDown;
    public Button b_MoveUp;
    public Button b_MoveDown;
    public Button b_MoveLeft;
    public Button b_MoveRight;
    public TMP_InputField TimeOutInput;
    public TMP_InputField CountEffectParticleInput;

    public GameObject SettingsPanel;
    public MeshRenderer meshRenderer;
    public SpriteRenderer spriteRenderer;
    
    private Texture2D texture;
    
    private float _timeOutCreateParticle;

    void Start()
    {
        _manager = FindObjectOfType<GameManager>();
        
        b_SensitivityUp.onClick.AddListener(OnSensitivityUp);
        b_SensitivityDown.onClick.AddListener(OnSensitivityDown);
        b_RadiusUp.onClick.AddListener(OnRadiusUp);
        b_RadiusDown.onClick.AddListener(OnRadiusDown);
        b_MoveUp.onClick.AddListener(OnMoveUp);
        b_MoveDown.onClick.AddListener(OnMoveDown);
        b_MoveLeft.onClick.AddListener(OnMoveLeft);
        b_MoveRight.onClick.AddListener(OnMoveRight);

        if (PlayerPrefs.HasKey("Radius"))
            Radius = PlayerPrefs.GetInt("Radius");
        if (PlayerPrefs.HasKey("CenterX"))
            CenterX = PlayerPrefs.GetInt("CenterX");
        if (PlayerPrefs.HasKey("CenterY"))
            CenterY = PlayerPrefs.GetInt("CenterY");
        if (PlayerPrefs.HasKey("Sensitivity"))
            DeltaColor = PlayerPrefs.GetFloat("Sensitivity");

        if (PlayerPrefs.HasKey("Timeout"))
        {
            _manager.TimeOut = PlayerPrefs.GetFloat("Timeout");
            TimeOutInput.text = PlayerPrefs.GetFloat("Timeout").ToString();
        }
        else
        {
            TimeOutInput.text = _manager.TimeOut.ToString();
        }

        if (PlayerPrefs.HasKey("CountParticle"))
        {
            _manager.CountEffectParticle = PlayerPrefs.GetInt("CountParticle");
            CountEffectParticleInput.text = PlayerPrefs.GetInt("CountParticle").ToString();
        }
        else
        {
            CountEffectParticleInput.text = _manager.CountEffectParticle.ToString();
        }

        // 752/480 = 1.566
        webcamTexture = new WebCamTexture(256, 144);
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.mainTexture = webcamTexture;

        if (webcamTexture.deviceName != "no camera available.")
        {
            webcamTexture.Play();
        }
        
        _width = webcamTexture.width;
        _height = webcamTexture.height;
        Debug.Log(webcamTexture.width + " x " + webcamTexture.height);
        texture = new Texture2D(_width, _height, TextureFormat.RGBA32, false);
        Color[] colors = new Color[_width * _height];
        for (int y = 0; y < colors.Length; y++)
        {
            colors[y] = Color.clear;
        }

        texture.SetPixels(colors);
        texture.Apply();
        meshRenderer.material.mainTexture = texture;
        
        _time = 0;
        _fps = 0;
        SetCirclePosition();
        SetCircleRadius();
        GetComponent<MeshRenderer>().enabled = false;
        SettingsPanel.SetActive(false);
        Circle.gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            OnMoveLeft();
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            OnMoveRight();
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            OnMoveUp();
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            OnMoveDown();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time<2f) return;
        _blackCount =0;
        if (webcamTexture.deviceName != "no camera available.")
        {
            colors = webcamTexture.GetPixels();
        }
        
        int x = 0;
        int y = 0;
        
        for (int i = Mathf.Clamp(CenterX - Radius, 0, _width);
             i < Mathf.Clamp(CenterX + Radius, 0, _width);
             i += 5)
        {
            for (int j = Mathf.Clamp(CenterY - Radius, 0, _height);
                 j < Mathf.Clamp(CenterY + Radius, 0, _height);
                 j += 5)
            {
                if (GetTruePixel(colors[j * _width + i]))
                {
                    _blackCount++;
                    x += i;
                    y += j;
                }
            }
        }

        if (_blackCount > 15)
        {
            //SetImage();
            // Debug.Log("Count "+_blackCount);
            // int cordX = x / _blackCount - CenterX;
            // int cordY = y / _blackCount - CenterY;
            // _manager.OnClick((float)(Radius + cordX) / (2 * Radius), (float)(Radius + cordY) / (2 * Radius), Radius);
            CreateParticle(colors);
            //CreateParticle();
        }

        BlackCount.text = _blackCount.ToString();
        if (Time.time - _time > 1f)
        {
            _time = Time.time;
            FPS.text = "FPS: " + _fps;
            _fps = 0;
        }
        else
        {
            _fps++;
        }

        

        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            Radius--;
            SetCircleRadius();
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            Radius++;
            SetCircleRadius();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GetComponent<MeshRenderer>().enabled = !GetComponent<MeshRenderer>().enabled;
            SettingsPanel.SetActive(!SettingsPanel.activeSelf);
            Circle.gameObject.SetActive(SettingsPanel.activeSelf);
            PlayerPrefs.SetInt("Radius", Radius);
            PlayerPrefs.SetInt("CenterX", CenterX);
            PlayerPrefs.SetInt("CenterY", CenterY);
            PlayerPrefs.SetFloat("Sensitivity", DeltaColor);
            PlayerPrefs.SetFloat("Timeout", float.Parse(TimeOutInput.text));
            PlayerPrefs.SetInt("CountParticle", int.Parse(CountEffectParticleInput.text));
            PlayerPrefs.Save();
        }
    }

    private void SetImage()
    {
        //Debug.Log("All "+_width*_height);
        int k = 0;
        for (int i = 0; i < _width; i++)
        {
            for (int j = 0; j < _height; j++)
            {
                //colors[j * _width + i]
                if (colors[j * _width + i].r < DeltaColor &&
                    colors[j * _width + i].g < DeltaColor &&
                    colors[j * _width + i].b < DeltaColor)
                {
                    k++;
                    texture.SetPixel(i, j, Color.white);
                }
                
            }
        }
        //Debug.Log("KK "+k);
        texture.Apply();
        
    }

    private void CreateParticle(Color[] colors)
    {
        // (x - a)² + (y - b)² < R²
        
        if(Time.time - _timeOutCreateParticle<3f) return;

        _timeOutCreateParticle = Time.time;
        
        int x = 0;
        int y = 0;
        
        texture = new Texture2D(Radius*2, Radius*2, TextureFormat.RGBA32, false);
        //Color[] colors = new Color[Radius * 2 * Radius * 2];
        // for (int k = 0; k < colors.Length; k++)
        // {
        //     colors[k] = Color.clear;
        // }
        Color color = Color.white;
        //color.a = 1f;
        
        Debug.Log("center " + CenterX + " x " + CenterY);
        Debug.Log("whr " + _width + " x " + _height + " radius " + Radius);


        for (int i = Mathf.Clamp(CenterX - Radius, 0, _width);
             i < Mathf.Clamp(CenterX + Radius, 0, _width);
             i += 1)
        {
            for (int j = Mathf.Clamp(CenterY - Radius, 0, _height);
                 j < Mathf.Clamp(CenterY + Radius, 0, _height);
                 j += 1)
            {
                if (GetTruePixel(colors[j * _width + i]))
                {
                    texture.SetPixel(i - (CenterX - Radius), j - (CenterY - Radius), color);
                }
                else
                {
                    texture.SetPixel(i - (CenterX - Radius), j - (CenterY - Radius), Color.clear);
                }
            }
        }

        //texture.SetPixels(colors);
        texture.Apply();
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        spriteRenderer.sprite = sprite;
        //meshRenderer.material.mainTexture = texture;
    }

    private void SetCirclePosition()
    {
        float x = ((CenterX - (_width / 2f)) * 7.83f) / (_width / 2f);
        float y = ((CenterY - (_height / 2f)) * 5f) / (_height / 2f);
        Circle.position = new Vector2(-x, -y);
    }

    private void SetCircleRadius()
    {
        float r = (Radius*2f/_height)*10f*1.41f;
        Circle.localScale = Vector3.one * r;
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

    private void OnRadiusUp()
    {
        Radius++;
        SetCircleRadius();
    }

    private void OnRadiusDown()
    {
        Radius--;
        SetCircleRadius();
    }

    private void OnSensitivityUp()
    {
        DeltaColor += 0.01f;
        Sensitivity.text = DeltaColor.ToString();
    }
    
    private void OnSensitivityDown()
    {
        DeltaColor -= 0.01f;
        Sensitivity.text = DeltaColor.ToString();
    }

    private void OnMoveUp()
    {
        CenterY ++;
        SetCirclePosition();
    }

    private void OnMoveDown()
    {
        CenterY --;
        SetCirclePosition();
    }

    private void OnMoveLeft()
    {
        CenterX --;
        SetCirclePosition();
    }

    private void OnMoveRight()
    {
        CenterX ++;
        SetCirclePosition();
    }

}
