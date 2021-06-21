using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    private BlurWall _blurWall;

    private void Awake()
    {
        Material material = (Material)Resources.Load("Materials/UIImage_Static_Blur_Advanced", typeof(Material));
        Sprite sprite = (Sprite)Resources.Load("Backgrounds/background 1", typeof(Sprite));
        
        if (material)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.localPosition = Camera.main.transform.position;
            go.transform.localPosition = new Vector3(go.transform.position.x, go.transform.position.y, go.transform.position.z + 1); //in front of camera
        
            Vector3 _cameraTopLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 10f));
            Vector3 _cameraBottomRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 10f));
        
            float actualWidth = _cameraBottomRight.x - _cameraTopLeft.x;
            float actualHeight = _cameraTopLeft.y - _cameraBottomRight.y;
        
            go.transform.localScale = new Vector3(actualWidth, actualHeight, 1);
        
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer)
            {
                renderer.material = material;
                _blurWall = go.AddComponent<BlurWall>();
            }
            else
            {
                Destroy(go);
                if (sprite)
                {
                    PauseMenu pauseMenu = FindObjectOfType<PauseMenu>();
                    if (pauseMenu)
                    {
                        Image image = pauseMenu.gameObject.GetComponent<Image>();
                        if (image)
                        {
                            image.sprite = sprite;
                            image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
                        }
                    }
                }
            }
        }
    }

    void Start()
    {
        gameObject.SetActive(false);       
    }

    private void OnEnable()
    {
        if (_blurWall == null)
            _blurWall = FindObjectOfType<BlurWall>();
    
        _blurWall.gameObject.SetActive(true);
    }
    
    private void OnDisable()
    {
        if (_blurWall)
            _blurWall.gameObject.SetActive(false);
    }


}
