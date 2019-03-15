using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SampleRegression.Model
{
    public static class MLModelScorerStatic<TData, TPrediction>
                        where TData : class
                        where TPrediction : class, new()
    {
        private static MLContext _mlContext = new MLContext(seed:1);
        private static ITransformer _mlModel;

        [ThreadStatic]
        private static PredictionEngine<TData, TPrediction> _predictionEngine;

        public static void LoadMLModelFromFile(string mlModelFilePath)
        {
            _mlModel = _mlContext.Model.Load(File.Open(mlModelFilePath, FileMode.Open));
        }

        public static TPrediction Predict(TData dataSample)
        {
            //Get PredictionEngine object from the Object Pool
            PredictionEngine<TData, TPrediction> predictionEngine = GetPredictionEngine();

            //Predict
            TPrediction prediction = predictionEngine.Predict(dataSample);

            return prediction;
        }

        private static PredictionEngine<TData, TPrediction> GetPredictionEngine()
        {
            if (_predictionEngine != null)
                return _predictionEngine;

            return _predictionEngine = _mlModel.CreatePredictionEngine<TData, TPrediction>(_mlContext);
        }
    }

        /*
        public class Engine
        {
            static MLContext context = new MLContext();
            static ITransformer model
                = context.Model.Load(File.Open("model.zip", FileMode.Open));

            [ThreadStatic]
            static PredictionEngine<SourceData, Prediction> t_engine;

            public static PredictionEngine<SourceData, Prediction> GetPredictionEngine(string modelfile)
            {
                if (t_engine != null)
                    return t_engine;

                return t_engine = model.CreatePredictionEngine<SourceData, Prediction>(context);
            }

            public static float CalculatePercentage(double value)
            {
                return 100 * (1.0f / (1.0f + (float)Math.Exp(-value)));
            }
        }
        */


        /*
        public class MLModelEngine<TData, TPrediction>
                            where TData : class
                            where TPrediction : class, new()
        {
            private readonly MLContext _mlContext;
            private readonly ITransformer _model;


            //(CDLTLL) - Delete
            //private readonly ObjectPool<PredictionEngine<TData, TPrediction>> _predictionEnginePool;
            //private readonly int _minPredictionEngineObjectsInPool;
            //private readonly int _maxPredictionEngineObjectsInPool;
            //private readonly double _expirationTime;


            /// <summary>
            /// Constructor with modelFilePathName to load
            /// </summary>
            /// <param name="mlContext">MLContext to use</param>
            /// <param name="modelFilePathName">Model .ZIP file path name</param>
            public MLModelEngine(MLContext mlContext, string modelFilePathName)
            {
                _mlContext = mlContext;

                //Load the ProductSalesForecast model from the .ZIP file
                using (var fileStream = File.OpenRead(modelFilePathName))
                {
                    _model = mlContext.Model.Load(fileStream);
                }

                //Create PredictionEngine Object Pool
                _predictionEnginePool = CreatePredictionEngineObjectPool();
            }

            /// <summary>
            /// Constructor with ITransformer model already created
            /// </summary>
            /// <param name="mlContext">MLContext to use</param>
            /// <param name="model">Model/Transformer to use, already created</param>
            /// <param name="minPredictionEngineObjectsInPool">Minimum number of PredictionEngineObjects in pool, as goal. Could be less but eventually it'll tend to that number</param>
            /// <param name="maxPredictionEngineObjectsInPool">Maximum number of PredictionEngineObjects in pool</param>
            /// <param name="expirationTime">Expiration Time (mlSecs) of PredictionEngineObject since added to the pool</param>
            public MLModelEngine(MLContext mlContext, ITransformer model, int minPredictionEngineObjectsInPool = 5, int maxPredictionEngineObjectsInPool = 1000, double expirationTime = 30000)
            {
                _mlContext = mlContext;
                _model = model;
                _minPredictionEngineObjectsInPool = minPredictionEngineObjectsInPool;
                _maxPredictionEngineObjectsInPool = maxPredictionEngineObjectsInPool;
                _expirationTime = expirationTime;

                //Create PredictionEngine Object Pool
                _predictionEnginePool = CreatePredictionEngineObjectPool();
            }

            private ObjectPool<PredictionEngine<TData, TPrediction>> CreatePredictionEngineObjectPool()
            {
                return new ObjectPool<PredictionEngine<TData, TPrediction>>(objectGenerator: () =>
                {
                    //Measure PredictionEngine creation
                    var watch = System.Diagnostics.Stopwatch.StartNew();

                    //Make PredictionEngine
                    var predEngine = _model.CreatePredictionEngine<TData, TPrediction>(_mlContext);

                    //Stop measuring time
                    watch.Stop();
                    long elapsedMs = watch.ElapsedMilliseconds;

                    return predEngine;
                },
                                                                              minPoolSize: _minPredictionEngineObjectsInPool,
                                                                              maxPoolSize: _maxPredictionEngineObjectsInPool,
                                                                              expirationTime: _expirationTime);
            }

            public TPrediction Predict(TData dataSample)
            {
                //Get PredictionEngine object from the Object Pool
                PredictionEngine<TData, TPrediction> predictionEngine = _predictionEnginePool.GetObject();

                //Measure Predict() execution time
                var watch = System.Diagnostics.Stopwatch.StartNew();

                //Predict
                TPrediction prediction = predictionEngine.Predict(dataSample);

                //Stop measuring time
                watch.Stop();
                long elapsedMs = watch.ElapsedMilliseconds;

                //Release used PredictionEngine object into the Object Pool
                _predictionEnginePool.PutObject(predictionEngine);

                return prediction;
            }

        }
        */
    }
