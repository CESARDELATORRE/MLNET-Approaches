using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;
using Microsoft.ML.Data;
using ScalableMLModelWebAPI.DataModels;
using ScalableMLModelWebAPI.MLModel;

namespace ScalableMLModelWebAPI
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

            //MLContext created as singleton for the whole ASP.NET Core app
            services.AddSingleton<MLContext>((ctx) =>
            {
                //Seed set to any number so you have a deterministic environment
                return new MLContext(seed: 1);
            });

            //ML Model (ITransformed) created as singleton for the whole ASP.NET Core app. Loads from .zip file here.
            services.AddSingleton<ITransformer,
                                  TransformerChain<ITransformer>> ((ctx) =>
                                {
                                    MLContext mlContext = ctx.GetRequiredService<MLContext>();
                                    string modelFilePathName = Configuration["MLModel:MLModelFilePath"];

                                    ITransformer mlModel;
                                    using (var fileStream = File.OpenRead(modelFilePathName))
                                        mlModel = mlContext.Model.Load(fileStream);

                                    return (TransformerChain<ITransformer>) mlModel;
                                });

            // PredictionEngine created as Transient since it is not thread safe 
            // This injected PredictionEngine is ONLY used on the MLModelEngineSimple implementation
            // This injected PredictionEngine is NOT used when using the Object Pooling or ThreadStatic approaches 
            services.AddTransient<PredictionEngine<SampleObservation, SamplePrediction>>((ctx) =>
                                  {
                                      MLContext mlContext = ctx.GetRequiredService<MLContext>();
                                      ITransformer mlmodel = ctx.GetRequiredService<ITransformer>();
                                      var predEngine = mlmodel.CreatePredictionEngine<SampleObservation, SamplePrediction>(mlContext);
                                      return predEngine;
                                  });

            // OPTION A:
            // Using MLModelEngine ObjPooling implementation
            //
            services.AddSingleton<IMLModelEngine<SampleObservation, SamplePrediction>,
                                  MLModelEngineObjPooling<SampleObservation, SamplePrediction>>();


            // OPTION B:
            // Using MLModelEngine ThreadStatic implementation
            //
            //services.AddSingleton<IMLModelEngine<SampleObservation, SamplePrediction>,
            //          MLModelEngineThreadStatic<SampleObservation, SamplePrediction>>();


            // OPTION C:
            // Using MLModelEngine Simple implementation (Create Prediction Engine for every call)
            //
            //services.AddSingleton<IMLModelEngine<SampleObservation, SamplePrediction>,
            //          MLModelEngineSimple<SampleObservation, SamplePrediction>>();



            // Using 'Factory code' when registering the engine. 
            // Not needed in current implementation
            //
            //services.AddSingleton<IMLModelEngine<SampleObservation, SamplePrediction>,
            //                      MLModelEngineObjPooling<SampleObservation, SamplePrediction>>((ctx) =>
            //{
            //    MLContext mlContext = ctx.GetRequiredService<MLContext>();
            //    string modelFilePathName = Configuration["MLModelFilePath"];

            //    return new MLModelEngineObjPooling<SampleObservation, SamplePrediction>(mlContext,
            //                                                                            modelFilePathName);
            //});

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
    }
}
