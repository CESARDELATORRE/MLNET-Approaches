//*****************************************************************************************
//*                                                                                       *
//* This is an auto-generated file by Microsoft ML.NET CLI (Command-Line Interface) tool. *
//*                                                                                       *
//*****************************************************************************************

using Microsoft.Data.DataView;
using Microsoft.ML;
using SampleRegression.Common.Validations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SampleRegression.Model
{
    public class MLModelScorer<TData, TPrediction> : IMLModelScorer<TData, TPrediction>
                        where TData : class
                        where TPrediction : class, new()
    {
        private MLContext _mlContext;
        private ITransformer _mlModel;

        // The Prediction engine is not thread safe so it needs to be [ThreadStatic]: Unique for each thread (https://docs.microsoft.com/en-us/dotnet/api/system.threadstaticattribute)
        [ThreadStatic]
        private static PredictionEngine<TData, TPrediction> _predictionEngine;

        private PredictionEngine<TData, TPrediction> PredictionEngine
        {
            get
            {
                // _predictionEngine can be null the first time or if coming from a different thread, since it is [ThreadStatic]
                if (_predictionEngine == null)
                    _predictionEngine = _mlModel.CreatePredictionEngine<TData, TPrediction>(_mlContext);

                return _predictionEngine;            
            }
        }

        //Constructor to be used if loading model from serialized .ZIP file
        public MLModelScorer(string modelFilePathName, MLContext mlContext = null)
        {
            Guard.Against.NullOrEmpty(modelFilePathName, nameof(modelFilePathName));

            //Create or use provided MLContext
            if (mlContext != null)
                _mlContext = mlContext;
            else 
                _mlContext = new MLContext(seed: 1);

            //Load the model from the .ZIP file
            using (var fileStream = File.OpenRead(modelFilePathName))
            {
                _mlModel = _mlContext.Model.Load(fileStream);
            }
        }

        //Constructor to be used if model (ITransformer) and MLContext are already available
        public MLModelScorer(ITransformer mlModel, MLContext mlContext)
        {
            Guard.Against.Null(mlModel, nameof(mlModel));
            Guard.Against.Null(mlContext, nameof(mlContext));

            //Use provided MLContext
            _mlContext = mlContext;

            //Use provided ITransformer (ML.NET Model)
            _mlModel = mlModel;
        }

        public TPrediction Predict(TData dataSample)
        {
            Guard.Against.Null(dataSample, nameof(dataSample));

            TPrediction prediction = this.PredictionEngine.Predict(dataSample);

            return prediction;
        }

        public IDataView PredictMany(IDataView testDataView)
        {
            Guard.Against.Null(testDataView, nameof(testDataView));

            IDataView predictions = _mlModel.Transform(testDataView);
            return predictions;
        }
    }

        /* DELETE 
        
        // FROM BLAZOR SAMPLE...
         
        public class Engine
        {
            static MLContext context = new MLContext();
            static ITransformer model
                = context.Model.Load(File.Open("model.zip", FileMode.Open));

            [ThreadStatic]
            static PredictionEngine<SourceData, Prediction> t_engine;

            public static PredictionEngine<SourceData, Prediction> GetPredictionEngine(string modelfile)  //(CDLTLL) Why modelfile path here?
            {
                if (t_engine != null)
                    return t_engine;

                return t_engine = model.CreatePredictionEngine<SourceData, Prediction>(context);
            }

            public static float CalculatePercentage(double value)
            {
                return 100 * (1.0f / (1.0f + (float)Math.Exp(-value)));
            }
        }
        */

    }
