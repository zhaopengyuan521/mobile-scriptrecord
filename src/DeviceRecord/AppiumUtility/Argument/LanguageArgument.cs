
namespace AppiumUtility.Argument
{
    public sealed class LanguageArgument : AppiumServerStringArgument
    {
        private const string _CommandSwitch = "--language";

        public LanguageArgument(string language)
        {
            Init(_CommandSwitch, language);
        }
    }
}
