using System;
using Microsoft.ML.Data;

namespace TaxiFare.Data
{
    public class Example
    {
        [ColumnName("vendor_id"), LoadColumn(0)]
        public string Vendor_id { get; set; }

        [LoadColumn(new int[] { 1, 2, 3, 4 })]
        [VectorType(4)]
        public float[] NumericFeatures { get; set; }

        public float Rate_code { get => NumericFeatures[1]; set => NumericFeatures[1] = value; }

        public float Passenger_count { get => NumericFeatures[2]; set => NumericFeatures[2] = value; }

        public float Trip_time_in_secs { get => NumericFeatures[3]; set => NumericFeatures[3] = value; }

        public float Trip_distance { get => NumericFeatures[4]; set => NumericFeatures[4] = value; }


        [ColumnName("payment_type"), LoadColumn(5)]
        public string Payment_type { get; set; }


        [ColumnName("fare_amount"), LoadColumn(6)]
        public float Label { get; set; }
    }
}
