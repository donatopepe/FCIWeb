using System;

namespace FCIWeb.Core.Utility
{
    /// <summary>
    /// Estensioni di confronto stringhe normalizzate (spazi rimossi, minuscole).
    /// Porting cross-platform di <c>Source/Areas/FCI/Uility/Extensions.cs</c>.
    /// </summary>
    /// <remarks>
    /// Differenza rispetto al legacy: <see cref="ContainsNormalized"/> e' resa null-safe
    /// (il legacy andava in <see cref="NullReferenceException"/> con <c>a == null</c>).
    /// </remarks>
    public static class Extensions
    {
        /// <summary>Confronto ignorando spazi e maiuscole/minuscole.</summary>
        public static bool EqualsNormalized(this string a, string b)
        {
            if (!String.IsNullOrWhiteSpace(a)) a = a.Replace(" ", "").ToLower();
            if (!String.IsNullOrWhiteSpace(b)) b = b.Replace(" ", "").ToLower();
            return a == b;
        }

        /// <summary>Verifica di contenimento ignorando spazi e maiuscole/minuscole.</summary>
        public static bool ContainsNormalized(this string a, string b)
        {
            if (String.IsNullOrWhiteSpace(a)) return String.IsNullOrWhiteSpace(b);
            a = a.Replace(" ", "").ToLower();
            if (String.IsNullOrWhiteSpace(b)) return a.Contains(String.Empty);
            b = b.Replace(" ", "").ToLower();
            return a.Contains(b);
        }
    }
}
