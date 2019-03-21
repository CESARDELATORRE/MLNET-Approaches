using Microsoft.Data.DataView;
using Microsoft.Extensions.Configuration;
using Microsoft.ML;
using System;
using System.IO;


namespace ScalableMLModelWebAPI.MLModel
{
    public class MLModelEngineThreadStatic<TData, TPrediction> : IMLModelEngine<TData, TPrediction>
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

        /// <summary>
        /// Constructor with IConfiguration and MLContext as dependency
        public MLModelEngineThreadStatic(MLContext mlContext, ITransformer mlModel, int maximumPredictionEngineObjectsRetained = -1)
        {
            //Use injected singleton MLContext 
            _mlContext = mlContext;

            //Use injected MLModel previously loaded from model's .zip file
            _mlModel = mlModel;
        }

        public TPrediction Predict(TData dataSample)
        {
            TPrediction prediction = this.PredictionEngine.Predict(dataSample);
            return prediction;
        }

        public IDataView PredictMany(IDataView testDataView)
        {
            IDataView predictions = _mlModel.Transform(testDataView);
            return predictions;
        }
    }
}
