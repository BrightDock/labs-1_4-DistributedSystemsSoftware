using System.Web;
using System.Web.Optimization;

namespace labs_1_4_DistributedSystemsSoftware
{
    public class BundleConfig
    {
        //Дополнительные сведения об объединении см. по адресу: http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
#if Release
            BundleTable.EnableOptimizations = true;
#endif

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery.signalR-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/Core").Include(
                        "~/Scripts/Core.js"));

            bundles.Add(new ScriptBundle("~/bundles/SK").Include(
                        "~/Scripts/jquery.sticky-kit.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/coolButtons").Include(
                        "~/Scripts/classie.js",
                        "~/Scripts/uiProgressButton.js"));

            // Используйте версию Modernizr для разработчиков, чтобы учиться работать. Когда вы будете готовы перейти к работе,
            // используйте средство сборки на сайте http://modernizr.com, чтобы выбрать только нужные тесты.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/mplayer").Include(
                      "~/Scripts/soundmanager2-jsmin.js",
                      "~/Scripts/bar-ui.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/font-awesome").Include(
                      "~/Content/font-awesome.min.css"));
        }
    }
}
