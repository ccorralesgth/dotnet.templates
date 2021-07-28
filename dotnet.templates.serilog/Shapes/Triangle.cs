using Microsoft.Extensions.Logging;
using Serilog;

namespace dotnet.templates.serilog.shapes.triangle
{
    internal interface ITriangle
    {
    }

    class Triangle: ITriangle
    {
        private readonly ILogger<Triangle> _log1;
        private readonly Serilog.ILogger _log2;

        public Triangle(ILogger<Triangle> log1)
        {
            this._log1 = log1;
            _log1.LogInformation("im a square");
        }

        public Triangle(Serilog.ILogger log2)
        {
            this._log2 = log2;
            _log2.ForContext<Triangle>().Information("im an square");
        }
    }
}
