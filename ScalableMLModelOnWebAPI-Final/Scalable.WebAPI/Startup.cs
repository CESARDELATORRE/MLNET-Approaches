using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scalable.Model.DataModels;
using Scalable.Model.Engine;
using System.IO;

namespace Scalable.WebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // Register Types in IoC container for DI

            // OPTION A:
            // Using MLModelEngine ObjPooling implementation
            //
            services.AddSingleton<IMLModelEngine<SampleObservation, SamplePrediction>,
                                  MLModelEngineObjPool<SampleObservation, SamplePrediction>>((ctx) =>
                                  {
                                      string modelFilePathName = GetAbsolutePath(Configuration["MLModel:MLModelFilePath"]);
                                      return new MLModelEngineObjPool<SampleObservation, SamplePrediction>(modelFilePathName);
                                  });


            // OPTION B:
            // Using MLModelEngine ThreadStatic implementation
            //
            //services.AddSingleton<IMLModelEngine<SampleObservation, SamplePrediction>,
            //                      MLModelEngineThreadStatic<SampleObservation, SamplePrediction>>((ctx) =>
            //                      {
            //                          string modelFilePathName = GetAbsolutePath(Configuration["MLModel:MLModelFilePath"]);
            //                          return new MLModelEngineThreadStatic<SampleObservation, SamplePrediction>(modelFilePathName);
            //                      });


            // OPTION C:
            // Using MLModelEngine Simple implementation (Create Prediction Engine for every call)
            //
            //services.AddSingleton<IMLModelEngine<SampleObservation, SamplePrediction>,
            //                      MLModelEngineSimple<SampleObservation, SamplePrediction>>((ctx) =>
            //                      {
            //                          string modelFilePathName = GetAbsolutePath(Configuration["MLModel:MLModelFilePath"]);
            //                          return new MLModelEngineSimple<SampleObservation, SamplePrediction>(modelFilePathName);
            //                      });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }
    }
}
