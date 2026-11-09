using System.Collections;

namespace SapphTools.IDisposableEnumerable; 
public interface IDisposableEnumerable<out T> : IEnumerable, IDisposableEnumerable
    where T : IDisposable {
    bool HasOwnership { get; }
}
public interface IDisposableEnumerable : IDisposable {
    bool IsDisposed { get; }
    static ObjectDisposedException DisposedMutationEx(string objectName) =>
        new(nameof(objectName), "Attempted to mutate a disposed IDisposedEnumerable object.");
}