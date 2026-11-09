using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace SapphTools.IDisposableEnumerable; 
public class DisposableList<T> : IList<T>, IDisposableEnumerable<T> 
        where T : IDisposable {
    private bool disposedValue;
    private readonly List<T> _list;
    private static readonly ReferenceComparer<T> ReferenceComparer = new();

    public int Count => _list.Count;
    public bool HasOwnership { get; init; }
    public bool IsReadOnly => false;
    public bool IsDisposed => disposedValue;

    public T this[int index] {
        get => _list[index];
        set {
            if (disposedValue) {
                throw IDisposableEnumerable.DisposedMutationEx(GetType().Name);
            }
            if (ReferenceEquals(_list[index], value)) {
                return;
            }
            DisposeOwnedItem(_list[index]);
            _list[index] = value;
        }
    }

    public DisposableList() : this(true) { }
    public DisposableList(bool hasOwnership) {
        _list = [];
        HasOwnership = hasOwnership;
    }
    public DisposableList(int capacity) : this(capacity, true) { }
    public DisposableList(int capacity, bool hasOwnership) {
        _list = new(capacity);
        HasOwnership = hasOwnership;
    }
    public DisposableList(IEnumerable<T> collection, bool hasOwnership) {
        HasOwnership = hasOwnership;
        _list = [.. collection];
    }
    public void Add(T item) {
        if (disposedValue) {
            throw IDisposableEnumerable.DisposedMutationEx(GetType().Name);
        }
        _list.Add(item);
    }
    public void Clear() {
        if (disposedValue) {
            throw IDisposableEnumerable.DisposedMutationEx(GetType().Name);
        }
        if (HasOwnership) {
            foreach (T item in _list) {
                DisposeOwnedItem(item);
            }
        }
        _list.Clear();
    }
    public bool Contains(T item) => _list.Contains(item);
    public void CopyTo(T[] array) => _list.CopyTo(array);
    public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);
    public void CopyTo(int index, T[] array, int arrayIndex, int count) => _list.CopyTo(index, array, arrayIndex, count);
    public DisposableList<T> GetRange(int index, int count) =>
        new(_list.GetRange(index, count), false);
    public int IndexOf(T item) => _list.IndexOf(item);
    public int IndexOf(T item, IEqualityComparer<T> comparer) {
        for (int i = 0; i < _list.Count; i++) {
            if (comparer.Equals(_list[i], item)) {
                return i;
            }
        }
        return -1;
    }
    public void Insert(int index, T item) {
        if (disposedValue) {
            throw IDisposableEnumerable.DisposedMutationEx(GetType().Name);
        }
        _list.Insert(index, item);
    }
    public bool Remove(T item) {
        if (disposedValue) {
            throw IDisposableEnumerable.DisposedMutationEx(GetType().Name);
        }
        int index = IndexOf(item);
        if (index < 0) {
            return false;
        }
        RemoveAt(index);
        return true;
    }
    public bool Remove(T item, IEqualityComparer<T> comparer) {
        if (disposedValue) {
            throw IDisposableEnumerable.DisposedMutationEx(GetType().Name);
        }
        int index = IndexOf(item, comparer);
        if (index < 0) {
            return false;
        }
        RemoveAt(index);
        return true;
    }
    public int RemoveAll(Predicate<T> match) {
        if (disposedValue) {
            throw IDisposableEnumerable.DisposedMutationEx(GetType().Name);
        }
        ArgumentNullException.ThrowIfNull(match, nameof(match));
        if (!HasOwnership) {
            return _list.RemoveAll(match);
        }
        List<T> toRemove = [];
        foreach (T item in _list) {
            if (match(item)) {
                toRemove.Add(item);
            }
        }
        foreach (T item in toRemove) {
            Remove(item, ReferenceComparer);
        }
        return toRemove.Count;
    }
    public void RemoveAt(int index) {
        if (disposedValue) {
            throw IDisposableEnumerable.DisposedMutationEx(GetType().Name);
        }
        T item = _list[index];
        _list.RemoveAt(index);
        DisposeOwnedItem(item);
    }
    public void RemoveRange(int index, int count) {
        if (disposedValue) {
            throw IDisposableEnumerable.DisposedMutationEx(GetType().Name);
        }
        if (!HasOwnership) {
            _list.RemoveRange(index, count);
            return;
        }
        List<T> toRemove = _list.GetRange(index, count);
        _list.RemoveRange(index, count);
        foreach (T item in toRemove) {
            DisposeOwnedItem(item);
        }
    }

    private void DisposeOwnedItem(T? item) {
        if (item is null || !HasOwnership) {
            return;
        }
        try { item.Dispose(); } catch { }
    }

    protected virtual void Dispose(bool disposing) {
        if (!disposedValue) {
            if (disposing) {
                Clear();
            }
            disposedValue = true;
        }
    }
    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
}
