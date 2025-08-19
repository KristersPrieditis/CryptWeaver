public static class SceneSpawnRouter
{
    public static string NextSpawnId;

    public static void SetNext(string id) => NextSpawnId = id;
    public static void Clear()            => NextSpawnId = null;
}
