using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.Extensions.ObjectPool;
using System.IO;
using System;
using Microsoft.Data.DataView;
using Microsoft.Extensions.Configuration;

namespace ScalableMLModelWebAPI.MLModel
{   
    public class MLModelEngineObjPooling<TData, TPrediction> : IMLModelEngine<TData, TPrediction>
                    where TData : class
                    where TPrediction : class, new()
    {
        private MLContext _mlContext;
        private ITransformer _mlModel;
        private ObjectPool<PredictionEngine<TData, TPrediction>> _predictionEnginePool;

        private int _maximumPredictionEngineObjectsRetained;

        /// <summary>
        /// Constructor with IConfiguration and MLContext as dependency
        public MLModelEngineObjPooling(MLContext mlContext, ITransformer mlModel, int maximumPredictionEngineObjectsRetained = -1)
        {
            //Use injected singleton MLContext 
            _mlContext = mlContext;

            //Use injected MLModel previously loaded from model's .zip file
            _mlModel = mlModel;

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

            var predEnginePolicy = new PooledPredictionEnginePolicy<TData, TPrediction>(_mlContext, _mlModel);

            //Create Object Pool with Factory
            var pool = objPoolProvider.Create<PredictionEngine<TData, TPrediction>>(predEnginePolicy);

            //Create Object Pool with 'new' --> QUESTION: Why two possible ways for creating a Pool?
            //var pool2 = new DefaultObjectPool<PredictionEngine<TData, TPrediction>>(policy: predEnginePolicy,
            //                                                            maximumRetained: 16);

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