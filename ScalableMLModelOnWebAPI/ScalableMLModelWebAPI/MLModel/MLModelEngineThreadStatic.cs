using Microsoft.Data.DataView;
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

        //Constructor to be used if loading model from serialized .ZIP file
        public MLModelEngineThreadStatic(MLContext mlContext, string modelFilePathName)
        {
            _mlContext = mlContext;

            //Load the model from the .ZIP file
            using (var fileStream = File.OpenRead(modelFilePathName))
            {
                _mlModel = _mlContext.Model.Load(fileStream);
            }
        }

        //Constructor to be used if model (ITransformer) and MLContext are already available
        public MLModelEngineThreadStatic(MLContext mlContext, ITransformer mlModel)
        {
            //Use provided MLContext
            _mlContext = mlContext;

            //Use provided ITransformer (ML.NET Model)
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
