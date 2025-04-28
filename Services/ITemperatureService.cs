using System.Collections.Generic;
using System.Threading.Tasks;

public interface ITemperatureService
{
    Task<List<TemperatureData>> GetTemperatureDataAsync(double latitude, double longitude);

}
