using OrchardCore.ResourceManagement;

namespace OrchardCore.Themes.MatTheme
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(IResourceManifestBuilder builder)
        {
            var manifest = builder.Add();

            //manifest
            //    .DefineStyle("MatTheme-bs-oc")
            //    .SetUrl("~/MatTheme/css/bootstrap-oc.min.css", "~/MatTheme/css/bootstrap-oc.css")
            //    .SetVersion("1.0.0");

            manifest
                .DefineStyle("site-styles")
                .SetUrl("~/MatTheme/css/site-styles.min.css", "~/MatTheme/css/site-styles.css")
                .SetVersion("1.0.0");

        }
    }
}
