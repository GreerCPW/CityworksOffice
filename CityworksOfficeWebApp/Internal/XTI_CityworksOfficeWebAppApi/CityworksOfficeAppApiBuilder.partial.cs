using XTI_Core;

namespace XTI_CityworksOfficeWebAppApi;

partial class CityworksOfficeAppApiBuilder
{
    partial void Configure()
    {
        source.SerializedDefaultOptions = XtiSerializer.Serialize(new CityworksOfficeAppOptions());
    }
}