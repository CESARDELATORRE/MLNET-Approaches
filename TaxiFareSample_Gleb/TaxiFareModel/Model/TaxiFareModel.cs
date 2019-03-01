// TODO: Add appropriate auto-generated header that shows the command used to generate it.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.DataView;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Categorical;

namespace TaxiFare
{
    public class Model
    {
        private readonly MLContext _context;
        private ITransformer _model;

        public Model()
            : this(new MLContext())
        {
        }

        public Model(MLContext context)
        {
            _context = context;
        }

        public Model(string modelPath, MLContext context)
        {
            _context = context;
            using (var fs = new FileStream(modelPath, FileMode.Create, FileAccess.Read, FileShare.Read))
                _model = _context.Model.Load(fs);
        }

        /// <summary>
        /// Get the trainig pipeline
        /// </summary>
        /// <returns></returns>
        public IEstimator<ITransformer> GetTrainingPipeline()
        {
            // Data transformation and featurization
            var dataProcessPipeline = _context.Transforms.CopyColumns("Label", "fare_amount")
                    .Append(_context.Transforms.Categorical.OneHotEncoding(
                        new OneHotEncodingEstimator.ColumnInfo("vendor_id", "vendor_id"),
                        new OneHotEncodingEstimator.ColumnInfo("payment_type", "payment_type")))

                    .Append(_context.Transforms.Concatenate("Features", new[] { "vendor_id", "payment_type", "rate_code", "passenger_count", "trip_time_in_secs", "trip_distance" }));

            // Add FastTree algorithm 
            var trainer = _context.Regression.Trainers.FastTree(
                    labelColumnName: "Label",
                    featureColumnName: "Features");
            return dataProcessPipeline.Append(trainer);
        }

        /// <summary>
        /// Returns a pipeline with SDCA
        /// Perhaps if we have several, we can add as many as we want:
        /// </summary>
        public IEstimator<ITransformer> GetTrainingPipeline_2()
        {
            // Data transformation and featurization
            var dataProcessPipeline = _context.Transforms.CopyColumns("Label", "fare_amount")
                    .Append(_context.Transforms.Categorical.OneHotEncoding(
                        new OneHotEncodingEstimator.ColumnInfo("vendor_id", "vendor_id"),
                        new OneHotEncodingEstimator.ColumnInfo("payment_type", "payment_type")))

                    .Append(_context.Transforms.Concatenate("Features", new[] { "vendor_id", "payment_type", "rate_code", "passenger_count", "trip_time_in_secs", "trip_distance" }));

            // Add FastTree algorithm 
            var trainer = _context.Regression.Trainers.StochasticDualCoordinateAscent(
                    labelColumnName: "Label",
                    featureColumnName: "Features");
            return dataProcessPipeline.Append(trainer);
        }

        /// <summary>
        /// Gets a reader of the Taxi file
        /// </summary>
        public IDataView GetDataReader(string dataPath)
        {
            // LoadData
            return _context.Data.ReadFromTextFile<Data.Example>(
                                            path: dataPath,
                                            hasHeader: true,
                                            separatorChar: ',',
                                            allowQuoting: true,
                                            trimWhitespace: false,
                                            allowSparse: true);
        }

        public void Train(string trainDataPath, IEstimator<ITransformer> pipeline = null)
        {
            var trainData = GetDataReader(trainDataPath);

            pipeline = pipeline ?? GetTrainingPipeline();
            // Train the model fitting to the DataSet
            _model = pipeline.Fit(trainData);
        }

        public RegressionMetrics Evaluate(string testDataPath)
        {
            CheckModelIsTrained();

            var testData = GetDataReader(testDataPath);
            var predictions = _model.Transform(testData);
            return  _context.Regression.Evaluate(predictions, "Label", "Score");
        }


        public void Save(string modelPath)
        {
            CheckModelIsTrained();

            using (var fs = new FileStream(modelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
                _context.Model.Save(_model, fs);
        }

        // TODO: Split out inference helper?
        public IEnumerable<Data.Prediction> Transform(IEnumerable<Data.Example> data)
        {
            CheckModelIsTrained();
            var view = _context.Data.ReadFromEnumerable<Data.Example>(data);
            var predictions = _model.Transform(view);
            return _context.CreateEnumerable<Data.Prediction>(predictions, false);
        }

        public IDataView Transform(IDataView data)
        {
            CheckModelIsTrained();
            return _model.Transform(data);
        }

        public void Transform(string testDataFile, string predictionFile)
        {
            CheckModelIsTrained();
            var view = GetDataReader(testDataFile);
            var predictions = _model.Transform(view);

            using (var fs = new FileStream(predictionFile, FileMode.Create, FileAccess.Write, FileShare.Write))
                _context.Data.SaveAsText(predictions, fs);
        }

        public PredictionEngine<Data.Example, Data.Prediction> CreatePredictionEngine()
        {
            CheckModelIsTrained();
            return _context.Model.CreatePredictionEngine<Data.Example, Data.Prediction>(_model);
        }

        private void CheckModelIsTrained()
        {
            if (_model == null)
                throw new InvalidOperationException("Model needs to be trained first");
        }
    }
}