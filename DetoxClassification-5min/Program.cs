//*****************************************************************************************
//*                                                                                       *
//* This is an auto-generated file by Microsoft ML.NET CLI (Command-Line Interface) tool. *
//*                                                                                       *
//*****************************************************************************************

using System;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.Data.DataView;


namespace SampleBinaryClassification
{
    class Program
    {
        private static string TrainDataPath = @"D:\MVPDemo\customer-reviews-40kRows.tsv";
        private static string ModelPath = @"D:\MVPDemo\SampleBinaryClassification\model.zip";

        static void Main(string[] args)
        {
            // Create MLContext to be shared across the model creation workflow objects 
            var mlContext = new MLContext();

            var command = Command.Predict; // Your desired action here

            if (command == Command.Predict)
            {
                Predict(mlContext);
                ConsoleHelper.ConsoleWriteHeader("=============== If you also want to train a model use Command.TrainAndPredict  ===============");
            }

            if (command == Command.TrainAndPredict)
            {
                TrainEvaluateAndSaveModel(mlContext);
                Predict(mlContext);
            }

            Console.WriteLine("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();
        }

        private enum Command
        {
            Predict,
            TrainAndPredict
        }

        private static ITransformer TrainEvaluateAndSaveModel(MLContext mlContext)
        {
            // Load data
            Console.WriteLine("=============== Loading data ===============");
            IDataView trainingDataView = mlContext.Data.LoadFromTextFile<SampleObservation>(
                                            path: TrainDataPath,
                                            hasHeader: true,
                                            separatorChar: '\t',
                                            allowQuoting: true,
                                            allowSparse: false);

            // Common data process configuration with pipeline data transformations
            var dataProcessPipeline = mlContext.Transforms.Text.FeaturizeText("SentimentText_tf", "SentimentText")
                                      .Append(mlContext.Transforms.CopyColumns("Features", "SentimentText_tf"))
                                      .Append(mlContext.Transforms.Normalize("Features", "Features"));

            // Set the training algorithm, then create and config the modelBuilder  
            var trainer = mlContext.BinaryClassification.Trainers.LogisticRegression(labelColumnName: "Sentiment", featureColumnName: "Features");
            var trainingPipeline = dataProcessPipeline.Append(trainer);

            // Cross-Validate with single dataset (since we don't have two datasets, one for training and for evaluate)
            // in order to evaluate and get the model's accuracy metrics
            Console.WriteLine("=============== Cross-validating to get model's accuracy metrics ===============");
            var crossValidationResults = mlContext.BinaryClassification.CrossValidateNonCalibrated(trainingDataView, trainingPipeline, numFolds: 5, labelColumn: "Sentiment");
            ConsoleHelper.PrintBinaryClassificationFoldsAverageMetrics(trainer.ToString(), crossValidationResults);

            // Train the model fitting to the DataSet
            Console.WriteLine("=============== Training the model ===============");
            var trainedModel = trainingPipeline.Fit(trainingDataView);

            // Save/persist the trained model to a .ZIP file
            Console.WriteLine($"=============== Saving the model  ===============");
            using (var fs = new FileStream(ModelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
                mlContext.Model.Save(trainedModel, fs);

            Console.WriteLine("The model is saved to {0}", ModelPath);
            Console.WriteLine("=============== End of training process ===============");

            return trainedModel;
        }

        // Try/test a single prediction by loading the model from the file, first.
        private static void Predict(MLContext mlContext)
        {
            //Load data to test. Could be any test data. For demonstration purpose train data is used here.
            IDataView trainingDataView = mlContext.Data.LoadFromTextFile<SampleObservation>(
                                            path: TrainDataPath,
                                            hasHeader: true,
                                            separatorChar: '\t',
                                            allowQuoting: true,
                                            allowSparse: false);

            var sample = mlContext.Data.CreateEnumerable<SampleObservation>(trainingDataView, false).First();

            ITransformer trainedModel;
            using (var stream = new FileStream(ModelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                trainedModel = mlContext.Model.Load(stream);
            }

            // Create prediction engine related to the loaded trained model
            var predEngine = trainedModel.CreatePredictionEngine<SampleObservation, SamplePrediction>(mlContext);

            //Score
            var resultprediction = predEngine.Predict(sample);

            Console.WriteLine($"=============== Single Prediction  ===============");
            Console.WriteLine($"Actual value: {sample.Sentiment} | Predicted value: {resultprediction.Prediction}");
            Console.WriteLine($"==================================================");
        }

    }

    public class SampleObservation
    {
        [ColumnName("Sentiment"), LoadColumn(0)]
        public bool Sentiment { get; set; }


        [ColumnName("SentimentText"), LoadColumn(1)]
        public string SentimentText { get; set; }


    }

    public class SamplePrediction
    {
        // ColumnName attribute is used to change the column name from
        // its default value, which is the name of the field.
        [ColumnName("PredictedLabel")]
        public bool Prediction { get; set; }

        public float Score { get; set; }
    }

}
