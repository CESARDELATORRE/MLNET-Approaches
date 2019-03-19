using Microsoft.Data.DataView;
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

        //Constructor to be used if loading model from serialized .ZIP file
        public MLModelEngineSimple(MLContext mlContext, string modelFilePathName)
        {
            _mlContext = mlContext;

            //Load the model from the .ZIP file
            using (var fileStream = File.OpenRead(modelFilePathName))
            {
                _mlModel = _mlContext.Model.Load(fileStream);
            }
        }

        //Constructor to be used if model (ITransformer) and MLContext are already available
        public MLModelEngineSimple(MLContext mlContext, ITransformer mlModel)
        {
            //Use provided MLContext
            _mlContext = mlContext;

            //Use provided ITransformer (ML.NET Model)
            _mlModel = mlModel;
        }

        public TPrediction Predict(TData dataSample)
        {
            // The Prediction engine is not thread safe so we always create a new one
            var predictionEngine = _mlModel.CreatePredictionEngine<TData, TPrediction>(_mlContext);

            TPrediction prediction = predictionEngine.Predict(dataSample);
            return prediction;
        }

        public IDataView PredictMany(IDataView testDataView)
        {
            IDataView predictions = _mlModel.Transform(testDataView);
            return predictions;
        }
    }
}
