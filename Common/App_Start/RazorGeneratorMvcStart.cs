using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using RazorGenerator.Mvc;
using System;

[assembly: WebActivatorEx.PostApplicationStartMethod(typeof(Common.App_Start.RazorGeneratorMvcStart), "Start")]

namespace Common.App_Start {
    public static class RazorGeneratorMvcStart {
        public static void Start()
        {
            object engine = null;
            try
            {
                engine = new PrecompiledMvcEngine(typeof(RazorGeneratorMvcStart).Assembly)
                {
                    UsePhysicalViewsIfNewer = HttpContext.Current.Request.IsLocal
                };
            }
            catch (System.Reflection.ReflectionTypeLoadException e)
            {
                string exceptions = "The following DLL load exceptions occurred:";
                foreach (var x in e.LoaderExceptions)
                {
                    exceptions += x.Message + ",\n\n";
                }
                throw new Exception("Error loading Razor Generator Stuff:\n" + exceptions);
            }

            ViewEngines.Engines.Insert(0, engine as PrecompiledMvcEngine);

            // StartPage lookups are done by WebPages. 
            VirtualPathFactoryManager.RegisterVirtualPathFactory(engine as PrecompiledMvcEngine);
        }
    }
}
