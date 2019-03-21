using Microsoft.Data.DataView;

namespace Scalable.Model.Engine
{
    public interface IMLModelEngine<TData, TPrediction>
                    where TData : class
                    where TPrediction : class

    {
        TPrediction Predict(TData dataSample);
    }
}
