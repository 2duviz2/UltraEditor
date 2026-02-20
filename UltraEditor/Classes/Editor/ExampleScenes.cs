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

    public const string DefaultSceneWithCombat = """{"objects":[{"type":"CubeObject","common":{"id":"Wall(-10.00, 98.75, 20.00)(0.00, 0.00, 90.00)(20.00, 0.25, 40.00)","name":"Wall","layer":24,"tag":"Wall","position":[-10.0,98.75,20.0],"eulerAngles":[0.0,0.0,90.0],"scale":[20.0000267,0.250000417,40.0],"active":true},"data":{"matType":0,"matTiling":0.25,"isTrigger":false,"shape":0,"fixTiling":false,"customUrl":""}},{"type":"CubeObject","common":{"id":"Floor(0.00, 89.25, 20.00)(0.00, 0.00, 0.00)(20.00, 1.00, 40.00)","name":"Floor","layer":24,"tag":"Floor","position":[0.0,89.25,20.0],"eulerAngles":[0.0,0.0,0.0],"scale":[20.0,1.0,40.0],"active":true},"data":{"matType":0,"matTiling":0.25,"isTrigger":false,"shape":0,"fixTiling":false,"customUrl":""}},{"type":"PrefabObject","common":{"id":"Zombie(Clone)(0.00, 90.00, 20.00)(0.00, 0.00, 0.00)(0.23, 0.23, 0.23)","name":"Zombie(Clone)","layer":12,"tag":"Enemy","position":[0.0,90.0,20.0],"eulerAngles":[0.0,183.21257,0.0],"scale":[0.22999993,0.23,0.22999993],"active":false,"parentId":"(4)EnemyWave(0.00, 90.00, 20.00)(0.00, 0.00, 0.00)(1.00, 1.00, 1.00)"},"data":{"prefabAsset":"Assets/Prefabs/Enemies/Zombie.prefab"}},{"type":"PrefabObject","common":{"id":"(5)FinalRoom(0.00, 90.00, 40.00)(0.00, 0.00, 0.00)(1.00, 1.00, 1.00)","name":"FinalRoom","layer":0,"tag":"Untagged","position":[0.0,90.0,40.0],"eulerAngles":[0.0,0.0,0.0],"scale":[1.0,1.0,1.0],"active":true},"data":{"prefabAsset":"Assets/Prefabs/Levels/Special Rooms/FinalRoom.prefab"}},{"type":"ArenaObject","common":{"id":"(7)ActivateArena(0.00, 95.00, 0.00)(0.00, 0.00, 0.00)(10.00, 10.00, 1.00)","name":"ActivateArena","layer":16,"tag":"Untagged","position":[0.0,95.0,0.0],"eulerAngles":[0.0,0.0,0.0],"scale":[10.0,10.0,1.0],"active":true},"data":{"onlyWave":true,"enemyIds":["Zombie(Clone)(0.00, 90.00, 20.00)(0.00, 0.00, 0.00)(0.23, 0.23, 0.23)"]}},{"type":"NextArenaObject","common":{"id":"(4)EnemyWave(0.00, 90.00, 20.00)(0.00, 0.00, 0.00)(1.00, 1.00, 1.00)","name":"EnemyWave","layer":16,"tag":"Untagged","position":[0.0,90.0,20.0],"eulerAngles":[0.0,0.0,0.0],"scale":[1.0,1.0,1.0],"active":true},"data":{"lastWave":true,"enemyCount":1,"toActivateIds":["FinalDoorOpener(0.00, 90.00, 40.00)(0.00, 0.00, 0.00)(1.00, 1.00, 1.00)"]}}]}""";
}
