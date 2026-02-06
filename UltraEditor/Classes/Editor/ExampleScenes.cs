namespace UltraEditor.Classes.Editor;
/// <summary> Static class that returns example scenes for the editor in a .uterus text type </summary>
public static class ExampleScenes
{
    public static string GetDefaultScene()
    {
        return @"
? CubeObject ?
Wall(-10.00, 98.75, 20.00)(0.00, 0.00, 90.00)(20.00, 0.25, 40.00)
Wall
24
Wall
(-10.00, 98.75, 20.00)
(0.00, 0.00, 90.00)
(20.00, 0.25, 40.00)
1

0

? END ?
? CubeObject ?
Floor(0.00, 89.25, 20.00)(0.00, 0.00, 0.00)(20.00, 1.00, 40.00)
Floor
24
Floor
(0.00, 89.25, 20.00)
(0.00, 0.00, 0.00)
(20.00, 1.00, 40.00)
1

0
";
    }
}
