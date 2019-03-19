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
        private readonly IConfiguration _configuration;
        private MLContext _mlContext;
        private ITransformer _mlModel;

        /// <summary>
        /// Constructor with IConfiguration and MLContext as dependency
        public MLModelEngineSimple(IConfiguration config, MLContext mlContext)
        {
            _configuration = config;

            //Use injected singleton MLContext 
            _mlContext = mlContext;

            //Load the ProductSalesForecast model from the .ZIP file
            string modelFilePathName = _configuration["MLModel:MLModelFilePath"];
            using (var fileStream = File.OpenRead(modelFilePathName))
                _mlModel = _mlContext.Model.Load(fileStream);

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
