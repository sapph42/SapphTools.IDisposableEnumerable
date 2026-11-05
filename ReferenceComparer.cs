using System.Diagnostics.CodeAnalysis;

namespace SapphTools.IDisposableEnumerable;
internal class ReferenceComparer<TItem> : IEqualityComparer<TItem> {
    public bool Equals(TItem? x, TItem? y) {
        return ReferenceEquals(x, y);
    }

    public int GetHashCode([DisallowNull] TItem obj) {
        return obj.GetHashCode();
    }
}