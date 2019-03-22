using Microsoft.Data.DataView;
using Microsoft.ML;
using System.IO;


namespace Scalable.Model.Engine
{
    public class MLModelEngineSimple<TData, TPrediction> : IMLModelEngine<TData, TPrediction>
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

        /// <summary>
        /// Constructor with IConfiguration and MLContext as dependency
        public MLModelEngineSimple(string modelFilePathName)
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
            // The Prediction engine is not thread safe so we always create a new one
            
            // Measuring CreatePredictionengine() time
            var watch = System.Diagnostics.Stopwatch.StartNew();

            var predictionEngine = _mlModel.CreatePredictionEngine<TData, TPrediction>(_mlContext);

            watch.Stop();
            long elapsedMs = watch.ElapsedMilliseconds;

            TPrediction prediction = predictionEngine.Predict(dataSample);
            return prediction;
        }
    }
}
