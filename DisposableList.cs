using System.Collections;

namespace SapphTools.IDisposableEnumerable; 
public class DisposableList<T> : IList<T>, IDisposableEnumerable<T> 
        where T : IDisposable {
    private bool disposedValue;
    private readonly List<T> _list;

    public int Count => _list.Count;
    public bool HasOwnership { get; init; }
    public bool IsReadOnly => false;

    public T this[int index] {
        get => _list[index];
        set {
            if (ReferenceEquals(_list[index], value)) {
                return;
            }
            if (_list[index] is not null) {
                Remove(_list[index]);
            }
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
    public void Add(T item) => _list.Add(item);
    public void Clear() {
        if (HasOwnership) {
            foreach (T item in _list) {
                item.Dispose();
            }
        }
        _list.Clear();
    }
    public bool Contains(T item) => _list.Contains(item);
    public void CopyTo(T[] array) => _list.CopyTo(array);
    public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);
    public void CopyTo(int index, T[] array, int arrayIndex, int count) => _list.CopyTo(index, array, arrayIndex, count);
    public DisposableList<T> GetRange(int index, int count, bool giveOwnership = false) =>
        new(_list.GetRange(index, count), giveOwnership);
    public int IndexOf(T item) => _list.IndexOf(item);
    public void Insert(int index, T item) => _list.Insert(index, item);
    public bool Remove(T item) {
        if (_list.Remove(item)) {
            if (HasOwnership) {
                item.Dispose();
            }
            return true;
        }
        return false;
    }
    public int RemoveAll(Predicate<T> match) {
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
            Remove(item);
        }
        return toRemove.Count;
    }
    public void RemoveAt(int index) {
        T item = _list[index];
        _list.RemoveAt(index);
        if (HasOwnership) {
            item.Dispose();
        }
    }
    public void RemoveRange(int index, int count) {
        if (!HasOwnership) {
            _list.RemoveRange(index, count);
            return;
        }
        List<T> toRemove = _list.GetRange(index, count);
        foreach (T item in toRemove) {
            Remove(item);
        }
    }

    protected virtual void Dispose(bool disposing) {
        if (!disposedValue) {
            if (disposing) {
                foreach (T item in this) {
                    try { item.Dispose(); } catch { }
                }
            }
            disposedValue = true;
        }
    }
    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    public IEnumerator<T> GetEnumerator() {
        throw new NotImplementedException();
    }
    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}
