namespace FrostweepGames.Plugins.GoogleCloud.TextToSpeech
{
    [UnityEditor.InitializeOnLoad]
    public class DefineProcessing : Plugins.DefineProcessing
    {
        internal static readonly string[] _Defines = new string[]
        {
            "FG_GCTTS"
        };

        static DefineProcessing()
        {
            AddOrRemoveDefines(true, true, _Defines);
        }
    }
}