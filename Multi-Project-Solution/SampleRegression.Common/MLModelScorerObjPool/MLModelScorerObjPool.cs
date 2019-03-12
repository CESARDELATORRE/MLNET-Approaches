using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.Extensions.ObjectPool;
using System.IO;
using System;

namespace SampleRegression.Common.MLModelScorerObjPool
{
    
    public class MLModelScorerObjPool<TData, TPrediction>
                    where TData : class
                    where TPrediction : class, new()
    {
        private MLContext _mlContext;
        private ITransformer _model;
        private ObjectPool<PredictionEngine<TData, TPrediction>> _predictionEnginePool;

        private int _maximumPredictionEngineObjectsRetained;

        //private readonly int _minPredictionEngineObjectsInPool;
        //private readonly int _maxPredictionEngineObjectsInPool;
        //private readonly double _expirationTime;

        //public int CurrentPredictionEnginePoolSize
        //{
        //    get { return _predictionEnginePool.CurrentPoolSize; }
        //}

        /// <summary>
        /// Constructor with modelFilePathName to load
        public MLModelScorerObjPool(string modelFilePathName, int maximumPredictionEngineObjectsRetained = -1, MLContext mlContext = null)
        {
            //Create or use provided MLContext
            if (mlContext != null)
                _mlContext = mlContext;
            else
                _mlContext = new MLContext(seed: 1);

            ITransformer model;
            //Load the ProductSalesForecast model from the .ZIP file
            using (var fileStream = File.OpenRead(modelFilePathName))
            {
                model = _mlContext.Model.Load(fileStream);
            }

            Initialize(model, maximumPredictionEngineObjectsRetained);
        }

        /// <summary>
        /// Constructor with ITransformer model already created
        /// </summary>
        /// <param name="mlContext">MLContext to use</param>
        /// <param name="model">Model/Transformer to use, already created</param>
        public MLModelScorerObjPool(ITransformer model, int maximumPredictionEngineObjectsRetained = -1, MLContext mlContext = null)
        {
            //Create or use provided MLContext
            if (mlContext != null)
                _mlContext = mlContext;
            else
                _mlContext = new MLContext(seed: 1);

            Initialize(model, maximumPredictionEngineObjectsRetained);
        }

        private void Initialize(ITransformer model, int maximumPredictionEngineObjectsRetained = -1)
        {
            _model = model;
            _maximumPredictionEngineObjectsRetained = maximumPredictionEngineObjectsRetained;

            //Create PredictionEngine Object Pool
            _predictionEnginePool = CreatePredictionEngineObjectPool();
        }

        private ObjectPool<PredictionEngine<TData, TPrediction>> CreatePredictionEngineObjectPool()
        {
            DefaultObjectPoolProvider objPoolProvider = new DefaultObjectPoolProvider();

            //default maximumRetained is Environment.ProcessorCount * 2, if not explicetely provided
            if (_maximumPredictionEngineObjectsRetained != -1)
                objPoolProvider.MaximumRetained = _maximumPredictionEngineObjectsRetained;

            var predEnginePolicy = new PooledPredictionEnginePolicy<TData, TPrediction>(_mlContext, _model);

            //Measure Object Pool of pooled PredictionEngine objects
            var watch = System.Diagnostics.Stopwatch.StartNew();

            //Create Object Pool with Factory
            var pool = objPoolProvider.Create<PredictionEngine<TData, TPrediction>>(predEnginePolicy);

            //Create Object Pool with 'new'
            //var pool2 = new DefaultObjectPool<PredictionEngine<TData, TPrediction>>(policy: predEnginePolicy,
            //                                                            maximumRetained: 16);

            //Stop measuring time
            watch.Stop();
            long elapsedMs = watch.ElapsedMilliseconds;

            return pool;
        }

        public TPrediction Predict(TData dataSample)
        {
            ////Get PredictionEngine object from the Object Pool
            PredictionEngine<TData, TPrediction> predictionEngine = _predictionEnginePool.Get();

            //Measure Predict() execution time
            var watch = System.Diagnostics.Stopwatch.StartNew();

            //Predict
            TPrediction prediction = predictionEngine.Predict(dataSample);

            ////Stop measuring time
            watch.Stop();
            long elapsedMs = watch.ElapsedMilliseconds;

            //Release used PredictionEngine object into the Object Pool
            _predictionEnginePool.Return(predictionEngine);

            return prediction;
        }

    }
    
}
