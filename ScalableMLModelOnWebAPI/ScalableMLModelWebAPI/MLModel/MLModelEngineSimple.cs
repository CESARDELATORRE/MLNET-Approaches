using Microsoft.Data.DataView;
using Microsoft.Extensions.Configuration;
using Microsoft.ML;
using System;
using System.IO;


namespace ScalableMLModelWebAPI.MLModel
{
    public class MLModelEngineSimple<TData, TPrediction> : IMLModelEngine<TData, TPrediction>
                        where TData : class
                        where TPrediction : class, new()
    {
        private MLContext _mlContext;
        private ITransformer _mlModel;
        private PredictionEngine<TData, TPrediction> _predictionEngine;

        /// <summary>
        /// Constructor with IConfiguration and MLContext as dependency
        public MLModelEngineSimple(MLContext mlContext, ITransformer mlModel, PredictionEngine<TData, TPrediction> predEngine, int maximumPredictionEngineObjectsRetained = -1)
        {
            //Use injected singleton MLContext 
            _mlContext = mlContext;

            //Use injected singleton MLModel previously loaded from model's .zip file
            _mlModel = mlModel;

            //Used injected transient PredictionEngine
            _predictionEngine = predEngine;
        }

        public TPrediction Predict(TData dataSample)
        {
            // The Prediction engine is not thread safe so we always create a new one
            // (NOT NEEDED) var predictionEngine = _mlModel.CreatePredictionEngine<TData, TPrediction>(_mlContext);

            TPrediction prediction = _predictionEngine.Predict(dataSample);
            return prediction;
        }

        public IDataView PredictMany(IDataView testDataView)
        {
            IDataView predictions = _mlModel.Transform(testDataView);
            return predictions;
        }
    }
}
