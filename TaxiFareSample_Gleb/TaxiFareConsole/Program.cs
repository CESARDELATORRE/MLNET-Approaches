// TODO: Add appropriate auto-generated header that shows the command used to generate it.

using System;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.Data.DataView;
using Microsoft.ML.Transforms.Categorical;
using System.Collections.Generic;
using TaxiFare.Data;

namespace TaxiFare
{
    static class Program
    {
        static MLContext _context;

        // TODO: Create a simple Console API:
        // [train --train-file file --model_file_out file -- model-name string]
        // [predict --model_file file [--test-file file --output-file file]]  -- either file-file or interactive
        // [evaluate --model_file file --test-file file]
        // [train_evaluate --train-file file --test-file file string]
        // [info --model_file file] - prints input schema, model info and such

        private enum Action
        {
            Train,
            Predict,
            Evaluate,
            Info
        }

        /// <summary>
        /// Runs the progam
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // TODO: ADD CLI parser
            // get vars to get from CLI:
            var modelFile = @"model.zip";
            var testFile = @"taxi-fare-test.csv";
            var predictionFile = @"taxi-fare-prediction.csv";
            var trainFile = @"taxi-fare-train.csv";
            Action action = Action.Evaluate; // some action

            _context = new MLContext(seed: 1, conc: 1);
            switch (action)
            {
                case Action.Train: 
                    Train(trainFile, modelFile);
                    break;

                case Action.Predict:
                    Predict(testFile, modelFile, predictionFile);
                    // Interactive prediction: 
                    PredictInteractive(modelFile);
                    break;

                case Action.Evaluate:
                    Evaluate(testFile, modelFile);
                    break;
            }
            ConsoleHelper.ConsoleWriteHeader("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();
        }

        private static void PredictInteractive(string modelFile)
        {
            // TODO implement:
            var model = new TaxiFare.Model(modelFile, _context);
            var engine = model.CreatePredictionEngine();
            foreach(Data.Example example in GetExamplesFromConsole())
            {
                var result = engine.Predict(example);
                ConsoleHelper.PrintPrediction(result.Score.ToString());
            }
        }

        private static IEnumerable<Example> GetExamplesFromConsole()
        {
            // TODO implement:
            throw new NotImplementedException();
        }

        private static void Evaluate(string testFile, string modelFile)
        {
            var model = new TaxiFare.Model(modelFile, _context);
            var metrics = model.Evaluate(testFile);
            ConsoleHelper.PrintRegressionMetrics("foo", metrics);
        }

        private static void Predict(string testFile, string modelFile, string outputFile)
        {
            var model = new TaxiFare.Model(modelFile, _context);
            model.Transform(testFile, outputFile);
        }

        private static void Train(string trainData, string modelFile)
        {
            var model = new TaxiFare.Model(_context);
            model.Train(trainData);
            model.Save(modelFile);
        }
    }

}
