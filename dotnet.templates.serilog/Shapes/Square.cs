using Microsoft.Extensions.Logging;
using Serilog;

namespace dotnet.templates.serilog.shapes.square
{
    public interface ISquareServices
    {

    }
    
    public class Square:ISquareServices
    {
        private readonly ILogger<Square> _log1;
        private readonly Serilog.ILogger _log2;

        public Square(ILogger<Square> log1)
        {
            this._log1 = log1;
            _log1.LogInformation("im a square");            
        }

        public Square(Serilog.ILogger log2)
        {
            this._log2 = log2;            
            _log2.ForContext<Square>().Information("im an square");
        }

    }
}
