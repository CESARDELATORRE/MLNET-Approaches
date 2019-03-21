using Microsoft.ML;
using Microsoft.Extensions.ObjectPool;
using System.IO;
using Microsoft.Data.DataView;

namespace Scalable.Model.Engine
{   
    public class MLModelEngineObjPooling<TData, TPrediction> : IMLModelEngine<TData, TPrediction>
                    where TData : class
                    where TPrediction : class, new()
    {
        private MLContext _mlContext;
        private ITransformer _mlModel;
        private ObjectPool<PredictionEngine<TData, TPrediction>> _predictionEnginePool;

        private int _maxObjectsRetained;

        /// <summary>
        /// Constructor with modelFilePathName to load
        public MLModelEngineObjPooling(string modelFilePathName, int maxObjectsRetained = -1)
        {
            //Create the MLContext object to use under the scope of this class 
            _mlContext = new MLContext(seed: 1);

            //Load the ProductSalesForecast model from the .ZIP file
            using (var fileStream = File.OpenRead(modelFilePathName))
            {
                _mlModel = _mlContext.Model.Load(fileStream);
            }

            _maxObjectsRetained = maxObjectsRetained;

            //Create PredictionEngine Object Pool
            _predictionEnginePool = CreatePredictionEngineObjectPool();
        }

        private ObjectPool<PredictionEngine<TData, TPrediction>> CreatePredictionEngineObjectPool()
        {
            var predEnginePolicy = new PooledPredictionEnginePolicy<TData, TPrediction>(_mlContext, _mlModel);

            DefaultObjectPool<PredictionEngine<TData, TPrediction>> pool;

            if (_maxObjectsRetained != -1)
            {
                pool = new DefaultObjectPool<PredictionEngine<TData, TPrediction>>(predEnginePolicy, _maxObjectsRetained);
            }
            else
            {
                //default maximumRetained is Environment.ProcessorCount * 2, if not explicetely provided
                pool = new DefaultObjectPool<PredictionEngine<TData, TPrediction>>(predEnginePolicy);
            }

            return pool;
        }

        public TPrediction Predict(TData dataSample)
        {
            ////Get PredictionEngine object from the Object Pool
            PredictionEngine<TData, TPrediction> predictionEngine = _predictionEnginePool.Get();

            //Predict
            TPrediction prediction = predictionEngine.Predict(dataSample);

            //Release used PredictionEngine object into the Object Pool
            _predictionEnginePool.Return(predictionEngine);

            return prediction;
        }

        public IDataView PredictMany(IDataView testDataView)
        {
            IDataView predictions = _mlModel.Transform(testDataView);
            return predictions;
        }
    }
    
}


// Measuring  Time
////Measure Predict() execution time
//var watch = System.Diagnostics.Stopwatch.StartNew();

////Predict
//TPrediction prediction = predictionEngine.Predict(dataSample);

//////Stop measuring time
//watch.Stop();
//            long elapsedMs = watch.ElapsedMilliseconds;