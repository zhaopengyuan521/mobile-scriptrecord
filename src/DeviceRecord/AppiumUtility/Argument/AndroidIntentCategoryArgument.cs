
namespace AppiumUtility.Argument
{
    public sealed class AndroidIntentCategoryArgument : AppiumServerStringArgument
    {
        private const string _CommandSwitch = "--intent-category";

        public AndroidIntentCategoryArgument(string category)
        {
            Init(_CommandSwitch, category);
        }
    }
}
