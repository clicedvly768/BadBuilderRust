using System.Reflection;

namespace BadBuilder.Helpers
{
    internal static class ResourceHelper
    {
        internal static void ExtractEmbeddedBinary(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string fullResourceName = $"BadBuilder.Resources.{resourceName}";

            using (Stream resourceStream = assembly.GetManifestResourceStream(fullResourceName))
            {
                if (resourceStream == null) return;

                using (FileStream fileStream = new FileStream($@"{resourceName}", FileMode.Create, FileAccess.Write))
                    resourceStream.CopyTo(fileStream);
            }
        }
    }
}