using Microsoft.Data.DataView;
using Microsoft.ML;
using System;
using System.IO;

namespace Scalable.Model.Engine
{
    public class MLModelEngineThreadStatic<TData, TPrediction> : IMLModelEngine<TData, TPrediction>
                        where TData : class
                        where TPrediction : class, new()
    {
        private MLContext _mlContext;
        private ITransformer _mlModel;

        /// <summary>
        /// Exposing the ML model allowing additional ITransformer operations such as Bulk predictions', etc.
        /// </summary>
        public ITransformer MLModel
        {
            get => _mlModel;
        }

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
        public MLModelEngineThreadStatic(string modelFilePathName)
        {
            //Create the MLContext object to use under the scope of this class 
            _mlContext = new MLContext(seed: 1);

            //Load the ProductSalesForecast model from the .ZIP file
            using (var fileStream = File.OpenRead(modelFilePathName))
            {
                _mlModel = _mlContext.Model.Load(fileStream);
            }
        }

        public TPrediction Predict(TData dataSample)
        {
            TPrediction prediction = this.PredictionEngine.Predict(dataSample);
            return prediction;
        }
    }
}
