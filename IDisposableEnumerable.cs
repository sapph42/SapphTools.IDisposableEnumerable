using System.Collections;

namespace SapphTools.IDisposableEnumerable; 
public interface IDisposableEnumerable<out T> : IEnumerable, IDisposable 
    where T : IDisposable {
    bool HasOwnership { get; init; }
}