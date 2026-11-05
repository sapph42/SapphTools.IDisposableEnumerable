using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace SapphTools.IDisposableEnumerable; 
public class DisposableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDisposableEnumerable<TValue>
        where TKey : notnull
        where TValue : IDisposable {
    private bool disposedValue;
    private readonly Dictionary<TKey, TValue> _dictionary;

    public bool HasOwnership { get; init; }
    public ICollection<TKey> Keys => _dictionary.Keys;
    public ICollection<TValue> Values => _dictionary.Values;
    public int Count => ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Count;
    public bool IsReadOnly => ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).IsReadOnly;
    public bool IsDisposed => disposedValue;

    public TValue this[TKey key] {
        get => _dictionary[key];
        set {
            if (TryGetValue(key, out TValue? val)) {
                if (ReferenceEquals(val, value)) {
                    return;
                }
                Remove(key);
            }
            Add(key, value);
        }
    }

    public DisposableDictionary() : this(0, null, true) { }
    public DisposableDictionary(bool hasOwnership) : this(0, null, hasOwnership) { }
    public DisposableDictionary(int capacity) : this(capacity, null, true) { }
    public DisposableDictionary(IEqualityComparer<TKey>? comparer) : this(0, comparer, true) { }
    public DisposableDictionary(int capacity, IEqualityComparer<TKey>? comparer) : 
        this(capacity, comparer, true) { }
    public DisposableDictionary(int capacity, IEqualityComparer<TKey>? comparer, bool hasOwnership) {
        _dictionary = new(capacity, comparer);
        HasOwnership = hasOwnership;
    }
    public DisposableDictionary(IDictionary<TKey, TValue> dictionary, bool hasOwnership) : this(dictionary, null, hasOwnership) { }
    public DisposableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey>? comparer, bool hasOwnership) {
        _dictionary = new(dictionary, comparer);
        HasOwnership = hasOwnership;
    }
    public DisposableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, bool hasOwnership) : 
        this(collection, null, hasOwnership) { }
    public DisposableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey>? comparer, bool hasOwnership) {
        _dictionary = new(collection, comparer);
        HasOwnership = hasOwnership;
    }

    public void Add(TKey key, TValue value) {
        if (disposedValue) {
            throw IDisposableEnumerable.DisposedMutationEx(GetType().Name);
        }
        _dictionary.Add(key, value);
    }
    public void Add(KeyValuePair<TKey, TValue> item) {
        if (disposedValue) {
            throw IDisposableEnumerable.DisposedMutationEx(GetType().Name);
        }
        ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Add(item);
    }
    public void Clear() {
        if (disposedValue) {
            throw IDisposableEnumerable.DisposedMutationEx(GetType().Name);
        }
        if (HasOwnership) {
            foreach (TKey key in Keys) {
                this[key].Dispose();
            }
        }
        _dictionary.Clear();
    }
    public bool Contains(KeyValuePair<TKey, TValue> item) =>
        ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Contains(item);
    public bool ContainsKey(TKey key) {
        return _dictionary.ContainsKey(key);
    }
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
        ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).CopyTo(array, arrayIndex);
    }
    public bool Remove(TKey key) {
        if (disposedValue) {
            throw IDisposableEnumerable.DisposedMutationEx(GetType().Name);
        }
        if (TryGetValue(key, out TValue? val)) {
            if (_dictionary.Remove(key)) {
                if (HasOwnership) {
                    val.Dispose();
                }
                return true;
            } else {
                return false;
            }
        } else {
            return false;
        }
    }
    public bool Remove(KeyValuePair<TKey, TValue> item) {
        if (disposedValue) {
            throw IDisposableEnumerable.DisposedMutationEx(GetType().Name);
        }
        if (!TryGetValue(item.Key, out TValue? val)) {
            return false;
        }
        if (_dictionary.Remove(item.Key)) {
            if (HasOwnership && val.Equals(item.Value)) {
                item.Value.Dispose();
            }
            return true;
        }
        return false;
    }
    public bool TryGetValue(TKey key, [NotNullWhen(true)] out TValue? value) {
        return _dictionary.TryGetValue(key, out value);
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _dictionary.GetEnumerator();

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
}
