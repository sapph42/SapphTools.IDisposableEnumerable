using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Security;

namespace SapphTools.IDisposableEnumerable; 
public class DisposableHashset<T> : ISet<T>, IDisposableEnumerable<T>
        where T: IDisposable {
    private bool disposedValue;
    private readonly HashSet<T> _set;

    public int Count => _set.Count;
    public bool HasOwnership { get; init; }
    public bool IsReadOnly => false;
    public bool IsDisposed => disposedValue;

    public DisposableHashset() : this(0, null, true) { }
    public DisposableHashset(bool hasOwnership) : this(0, null, hasOwnership) { }
    public DisposableHashset(IEqualityComparer<T>? comparer) : this(0, comparer, true) { }
    public DisposableHashset(IEqualityComparer<T>? comparer, bool hasOwnership) : this(0, comparer, hasOwnership) { }
    public DisposableHashset(int capacity) : this(capacity, null, true) { }
    public DisposableHashset(int capacity, bool hasOwnership) : this(capacity, null, hasOwnership) { }
    public DisposableHashset(int capacity, IEqualityComparer<T>? comparer, bool hasOwnership) {
        _set = new(capacity, comparer);
        HasOwnership = hasOwnership;
    }
    public DisposableHashset(IEnumerable<T> collection, bool hasOwnership) : this(collection, null, hasOwnership) { }
    public DisposableHashset(IEnumerable<T> collection, IEqualityComparer<T>? comparer, bool hasOwnership) : this(hasOwnership) {
        _set = new(collection, comparer);
    }
    public DisposableHashset(IDisposableEnumerable<T> collection, bool hasOwnership) : 
        this(collection, null, hasOwnership) { }
    public DisposableHashset(IDisposableEnumerable<T> collection, IEqualityComparer<T>? comparer, bool hasOwnership) {
        _set = new((IEnumerable<T>)collection, comparer);
        HasOwnership = hasOwnership;
    }

    public bool Add(T item) {
        if (disposedValue) {
            throw IDisposableEnumerable.DisposedMutationEx(GetType().Name);
        }
        return _set.Add(item);
    }
    public void Clear() {
        if (disposedValue) {
            throw IDisposableEnumerable.DisposedMutationEx(GetType().Name);
        }
        foreach (T item in _set) {
            DisposeOwnedItem(item);
        }
        _set.Clear();
    }
    public bool Contains(T item) => _set.Contains(item);
    public void CopyTo(T[] array, int arrayIndex) => _set.CopyTo(array, arrayIndex);
    public void ExceptWith(IEnumerable<T> other) {
        if (disposedValue) {
            throw IDisposableEnumerable.DisposedMutationEx(GetType().Name);
        }
        if (!HasOwnership) {
            _set.ExceptWith(other);
            return;
        }
        ArgumentNullException.ThrowIfNull(other, nameof(other));
        if (_set.Count == 0) {
            return;
        }
        if (other == this || other == _set) {
            Clear();
            return;
        }
        List<T> list = [];
        foreach (T item in other) {
            if (_set.Contains(item)) {
                list.Add(item);
            }
        }
        foreach (T item in list) {
            Remove(item);
        }
    }
    public void IntersectWith(IEnumerable<T> other) {
        if (disposedValue) {
            throw IDisposableEnumerable.DisposedMutationEx(GetType().Name);
        }
        if (!HasOwnership) {
            _set.IntersectWith(other);
            return;
        }
        ArgumentNullException.ThrowIfNull(other, nameof(other));
        if (other == this || other == _set || _set.Count == 0) {
            return;
        }
        HashSet<T> set = new(other, _set.Comparer);
        if (set.Count == 0) {
            Clear();
            return;
        }
        IntersectWithHashSetWithSameComparer(set);
    }
    public bool IsProperSubsetOf(IEnumerable<T> other) => _set.IsProperSubsetOf(other);
    public bool IsProperSupersetOf(IEnumerable<T> other) => _set.IsProperSupersetOf(other);
    public bool IsSubsetOf(IEnumerable<T> other) => _set.IsSubsetOf(other);
    public bool IsSupersetOf(IEnumerable<T> other) => _set.IsSupersetOf(other);
    public bool Overlaps(IEnumerable<T> other) => _set.Overlaps(other);
    public bool Remove(T item) {
        if (disposedValue) {
            throw IDisposableEnumerable.DisposedMutationEx(GetType().Name);
        }
        if (_set.TryGetValue(item, out T? setItem)) {
            _set.Remove(setItem);
            DisposeOwnedItem(setItem);
            return true;
        }
        return false;
    }
    public bool SetEquals(IEnumerable<T> other) => _set.SetEquals(other);
    public void SymmetricExceptWith(IEnumerable<T> other) {
        if (disposedValue) {
            throw IDisposableEnumerable.DisposedMutationEx(GetType().Name);
        }
        if (!HasOwnership) {
            _set.SymmetricExceptWith(other);
            return;
        }
        ArgumentNullException.ThrowIfNull(other,nameof(other));
        if (_set.Count == 0) {
            UnionWith(other);
            return;
        }
        if (other == this || other == _set) {
            Clear();
            return;
        }
        if (other is HashSet<T> otherAsset && EqualityComparersAreEqual(_set, otherAsset)) {
            SymmetricExceptWithUniqueHashSet(otherAsset);
        } else if (other is DisposableHashset<T> otherDAsset && EqualityComparersAreEqual(_set, otherDAsset._set)) {
            SymmetricExceptWithUniqueHashSet(otherDAsset._set);
        } else {
            SymmetricExceptWithEnumerable(other);
        }
    }
    public void UnionWith(IEnumerable<T> other) {
        if (disposedValue) {
            throw IDisposableEnumerable.DisposedMutationEx(GetType().Name);
        }
        _set.UnionWith(other);
    }

    internal static bool EqualityComparersAreEqual(HashSet<T> set1, HashSet<T> set2) => set1.Comparer.Equals(set2.Comparer);

    private void DisposeOwnedItem(T? item) {
        if (item is null || !HasOwnership) {
            return;
        }
        try { item.Dispose(); } catch { }
    }
    private void IntersectWithHashSetWithSameComparer(HashSet<T> other) {
        List<T> list = [.._set];
        foreach (T item in list) {
            if (!other.Contains(item, _set.Comparer)) {
                Remove(item);
            }
        }
    }
    private void SymmetricExceptWithEnumerable(IEnumerable<T> other) {
        HashSet<T> otherHash = new(other, _set.Comparer);
        SymmetricExceptWithUniqueHashSet(otherHash);
    }
    private void SymmetricExceptWithUniqueHashSet(HashSet<T> other) {
        foreach (T item in other) {
            if (!Remove(item)) {
                Add(item);
            }
        }
    }

    void ICollection<T>.Add(T item) => Add(item);

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
    public IEnumerator<T> GetEnumerator() => _set.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _set.GetEnumerator();
}