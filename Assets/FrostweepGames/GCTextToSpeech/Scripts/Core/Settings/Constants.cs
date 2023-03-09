namespace FrostweepGames.Plugins.GoogleCloud.TextToSpeech
{
    public class Constants
    {
        public const string API_VERSION_V1 = "v1";

        public const string API_VERSION_V1_BETA1 = "v1beta1";

        public static string POST_TEXT_SYNTHESIZE = "https://texttospeech.googleapis.com/" + (GeneralConfig.Config.betaAPI ? API_VERSION_V1_BETA1 : API_VERSION_V1) + "/text:synthesize";
        public static string GET_LIST_VOICES = "https://texttospeech.googleapis.com/" + (GeneralConfig.Config.betaAPI ? API_VERSION_V1_BETA1 : API_VERSION_V1) + "/voices";

        public const string API_KEY_PARAM = "?key=";


        public const Enumerators.AudioEncoding DEFAULT_AUDIO_ENCODING = Enumerators.AudioEncoding.LINEAR16;
        public const double DEFAULT_SAMPLE_RATE = 16000;
        public const double DEFAULT_VOLUME_GAIN_DB = 0.0;
    }
}