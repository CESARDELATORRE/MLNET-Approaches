using Microsoft.ML.Data;

namespace ScalableMLModelWebAPI.DataModels
{
    public class SampleObservation
    {
        [ColumnName("Sentiment"), LoadColumn(0)]
        public bool IsToxic { get; set; }


        [ColumnName("SentimentText"), LoadColumn(1)]
        public string SentimentText { get; set; }

    }
}
