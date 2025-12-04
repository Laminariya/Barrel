using System;
using System.IO;
using Kedr;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CircleGalleryWipe : MonoBehaviour
{

    public Transform Circle;
    public int CenterX;
    public int CenterY;
    public int Radius;
    
    public Transform Gallery;
    public Transform RawImage;
    public Transform Mask;
    public Transform RemoveImage;
    public Transform RemoveRawImage;
    public Button b_UpBlackPoint;
    public Button b_DownBlackPoint;
    public TMP_Text CountBlack;
    public int CountBlackPoint;

    public JsonData jsonData;
    private string _nameConfig = "//config.txt";

    private int _width = 752;
    private int _height = 480;
    private float _ratio;
    private float _scale;
    private GalleryWipeController _controller;

    public void Init(int width, int height, GalleryWipeController controller)
    {
        LoadJson();
        _controller = controller;
        _width = width;
        _height = height;
        _ratio = _width / (float)_height;
        CenterX = _width / 2;
        CenterY = _height / 2;
        Radius = _height / 2;
        _scale = 1f;
        CountBlackPoint = 2000;
        b_UpBlackPoint.onClick.AddListener(OnUpBlackPoint);
        b_DownBlackPoint.onClick.AddListener(OnDownBlackPoint);
        
        CenterX = jsonData.CenterX_Wipe;
        CenterY = jsonData.CenterY_Wipe;
        Radius = jsonData.Radius_Wipe;
        // if(PlayerPrefs.HasKey("Scale_Wipe"))
        //     _scale = PlayerPrefs.GetFloat("Scale_Wipe");
        CountBlackPoint = jsonData.BlackPoint_Wipe;
        CountBlack.text = CountBlackPoint.ToString();
        Vector3 scale = Vector3.one;
        scale.x = _scale;
        Gallery.localScale = scale;
        RawImage.localScale = scale;
        Mask.localScale = scale;
        RemoveImage.localScale = scale;
        RemoveRawImage.localScale = scale;
        SetCircleRadius();
        SetCirclePosition();
    }

    public void SaveSettings()
    {
        // PlayerPrefs.SetInt("CenterX_Wipe", CenterX);
        // PlayerPrefs.SetInt("CenterY_Wipe", CenterY);
        // PlayerPrefs.SetInt("Radius_Wipe", Radius);
        // //PlayerPrefs.SetFloat("Scale_Wipe", RawImage.localScale.x);
        // PlayerPrefs.SetInt("BlackPoint_Wipe", CountBlackPoint);
        // PlayerPrefs.Save();
        SaveJson();
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
        
        if (Input.GetKey(KeyCode.Alpha9))
        {
            OnRadiusDown();
        }
        if (Input.GetKey(KeyCode.Alpha0))
        {
            OnRadiusUp();
        }

       
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.U))
            OnUpScale();
        if(Input.GetKeyDown(KeyCode.I))
            OnDownScale();
        if (Input.GetKeyDown(KeyCode.L))
        {
            _controller.ToggleLeft.isOn = !_controller.ToggleLeft.isOn;
            _controller.OnLeft(_controller.ToggleLeft);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            _controller.ToggleRight.isOn = !_controller.ToggleRight.isOn;
            _controller.OnRight(_controller.ToggleRight);
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            _controller.ToggleRevX.isOn = !_controller.ToggleRevX.isOn;
            _controller.OnRevX(_controller.ToggleRevX);
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            _controller.ToggleRevY.isOn = !_controller.ToggleRevY.isOn;
            _controller.OnRevY(_controller.ToggleRevY);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            OnDownBlackPoint();
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            OnUpBlackPoint();
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            _controller.ThresholdSlider.value -= 0.01f;
            _controller.OnThresholdSlider(_controller.ThresholdSlider.value);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            _controller.ThresholdSlider.value += 0.01f;
            _controller.OnThresholdSlider(_controller.ThresholdSlider.value);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            _controller.FadeSlider.value -= 0.01f;
            _controller.OnFadeSlider(_controller.FadeSlider.value);
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            _controller.FadeSlider.value += 0.01f;
            _controller.OnFadeSlider(_controller.FadeSlider.value);
        }

        // if (Input.GetKeyDown(KeyCode.M))
        // {
        //     SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        // }
    }

    private void SetCirclePosition()
    {
        //((CenterX - (_width / 2f)) * _ratio * 5f) / (_width / 2f);
        float x = (CenterX * (_ratio * 2.23f * 2)) / _width - _ratio * 2.23f;
        //((CenterY - (_height / 2f)) * 5f) / (_height / 2f);
        float y = (CenterY * 2.23f * 2f) / _height - 2.23f;
        Circle.position = new Vector2(x, y);
    }

    private void SetCircleRadius()
    {
        float r = (Radius * 2f / _height) * 2.23f * 2f;
        Circle.localScale = Vector3.one * r;
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
    
    private void OnRadiusUp()
    {
        Radius++;
        SetCircleRadius();
    }

    private void OnRadiusDown()
    {
        Radius--;
        if(Radius<10f) Radius = 10;
        SetCircleRadius();
    }

    private void OnUpScale()
    {
        Vector2 scale = RawImage.localScale;
        scale.x += 0.005f;
        Gallery.localScale = scale;
        RawImage.localScale = scale;
        Mask.localScale = scale;
        RemoveImage.localScale = scale;
        RemoveRawImage.localScale = scale;
    }

    private void OnDownScale()
    {
        Vector2 scale = RawImage.localScale;
        scale.x -= 0.005f;
        Gallery.localScale = scale;
        RawImage.localScale = scale;
        Mask.localScale = scale;
        RemoveImage.localScale = scale;
        RemoveRawImage.localScale = scale;
    }

    private void OnUpBlackPoint()
    {
        CountBlackPoint += 10;
        CountBlack.text = CountBlackPoint.ToString();
    }

    private void OnDownBlackPoint()
    {
        CountBlackPoint -= 10;
        CountBlack.text = CountBlackPoint.ToString();
    }

    private void LoadJson()
    {
        jsonData = new JsonData();
        try
        {
            string text = File.ReadAllText(Directory.GetCurrentDirectory()+_nameConfig);
            jsonData = JsonUtility.FromJson<JsonData>(text);
        }
        catch (FileNotFoundException)
        {
            Debug.Log("Файл не найден!");
        }
        catch (Exception ex)
        {
            Debug.Log($"Ошибка: {ex.Message}");
        }
    }

    public void SaveJson()
    {
        string json = JsonUtility.ToJson(jsonData);
        
        using (StreamWriter writer = new StreamWriter(Directory.GetCurrentDirectory() + _nameConfig))
        {
            writer.Write(json);
        }
    }
}

[SerializeField]
public class JsonData
{
    public int CenterX_Wipe;
    public int CenterY_Wipe;
    public int Radius_Wipe;
    public int BlackPoint_Wipe;

    public bool IsLeft;
    public bool IsRight;
    public bool IsRevX;
    public bool IsRevY;

    public float Fade;
    public float Threshold;

}
