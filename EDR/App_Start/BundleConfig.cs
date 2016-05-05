using System.Web;
using System.Web.Optimization;

namespace EDR
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                            // "~/Scripts/jquery.validate.unobtrusive.js",
                            "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css",
                      "~/Content/sidebar.bootstrap.css"));

            //  foolproof scripts
            bundles.Add(new ScriptBundle("~/bundles/foolproof").Include(
                        "~/Scripts/qunit.js",
                        "~/Scripts/mvcfoolproof.unobtrusive.js"));

            //  jquery UI
            bundles.Add(new ScriptBundle("~/bundles/jqueryUI").Include(
                        "~/Scripts/jquery-ui.js"));

            //  GeoComplete
            bundles.Add(new ScriptBundle("~/bundles/GeoComplete").Include(
                        //"~/Scripts/jquery.geocomplete.min.js",
                        "~/Scripts/jquery.geocomplete.js"));

            //  timepicker
            bundles.Add(new ScriptBundle("~/bundles/TimePicker").Include(
                        //"~/Scripts/timepicker/jquery.timepicker.min.js",                        
                        "~/Scripts/timepicker/jquery.timepicker.js",
                        "~/Scripts/timepicker/bootstrap-datepicker.js",
                        "~/Scripts/timepicker/site.js"));

            //  timepicker css
            bundles.Add(new StyleBundle("~/Content/TimePickercss").Include(
                      "~/Content/timepicker/jquery.timepicker.css",
                      "~/Content/timepicker/bootstrap-datepicker.css"));

            //  datepair
            bundles.Add(new ScriptBundle("~/bundles/DatePair").Include(
                        "~/Scripts/timepicker/datepair.js",
                        "~/Scripts/timepicker/jquery.datepair.js"));

            //  rate Yo
            bundles.Add(new StyleBundle("~/Content/RateYo").Include(
                      "~/Content/jquery.rateyo.css"));

            //  rate Yo
            bundles.Add(new ScriptBundle("~/bundles/RateYo").Include(
                        "~/Scripts/jquery.rateyo.js"));

            // Set EnableOptimizations to false for debugging. For more information,
            // visit http://go.microsoft.com/fwlink/?LinkId=301862
            BundleTable.EnableOptimizations = true;
        }
    }
}
