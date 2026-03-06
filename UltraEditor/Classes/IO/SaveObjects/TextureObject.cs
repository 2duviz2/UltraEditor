namespace UltraEditor.Classes.IO.SaveObjects;

using System.Linq;
using UltraEditor.Classes.Canvas;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[EditorComp("Creates a Texture with a unique URL you can copy and paste anywhere in a level with the object.")]
public class TextureObject : SavableObject
{
    [EditorVar("Open painter")]
    public UnityEvent onClick = new();

    [EditorVar("Texture ID")]
    public string TextureName = "";

    [EditorVar("Texture width")]
    public int imageSizeX = 32;

    [EditorVar("Texture height")]
    public int imageSizeY = 32;

    public Texture2D colonThree = new(32, 32);
    public RawImage imageContainer;
    public Image previewImage;

    public override void Tick()
    {
        if (new Vector2(colonThree.width, colonThree.height) != new Vector2(imageSizeX, imageSizeY))
        {
            colonThree = new(imageSizeX, imageSizeY)
            {
                filterMode = FilterMode.Point
            };

            EditorManager.Instance.SetAlert("Texture has been reset!");
        }

        if (TextureName == "")
            TextureName = Random.Range(0, int.MaxValue).ToString();
    }

    public void Awake()
    {
        DisableNavmesh();

        if (TextureName == "")
            TextureName = Random.Range(0, int.MaxValue).ToString();

        colonThree.filterMode = FilterMode.Point;

        onClick = new UnityEvent();
        onClick.AddListener(() =>
        {
            Transform PaintWindow = EditorManager.Instance.editorCanvas.transform.GetChild(0).Find("Paint");

            PaintWindow.SetActive(true);
            copyURL = TextureName;

            PaintWindow.Find("Button (1)").GetComponent<Button>().onClick = new();
            PaintWindow.Find("Button (1)").GetComponent<Button>().onClick.AddListener(CopyURL);

            previewImage = PaintWindow.Find("Preview").Find("Image").GetComponent<Image>();
            imageContainer = PaintWindow.Find("ImageContainer").Find("Image").GetComponent<RawImage>();

            imageContainer.texture = colonThree;
            imageContainer.GetOrAddComponent<PaintImage>().textureObj = this;
            imageContainer.GetOrAddComponent<PaintImage>().image = imageContainer;
            imageContainer.GetOrAddComponent<PaintImage>().brushSlider = PaintWindow.Find("Brush").Find("Slider").GetComponent<Slider>();
            imageContainer.GetOrAddComponent<PaintImage>().brushSlider.maxValue = (imageSizeX + imageSizeY) / 10;
        });
    }

    public static string copyURL = "Hai";

    public static void CopyURL() => GUIUtility.systemCopyBuffer = copyURL;
}
