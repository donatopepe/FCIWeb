using System;

namespace FCIWeb.Core.Utility
{
    /// <summary>
    /// Copia per nome delle proprieta' da un oggetto dinamico (es. anonimo) a una entita' tipizzata.
    /// Porting cross-platform di <c>Source/Areas/FCI/Uility/MapObj.cs</c>
    /// (rimossa la dipendenza inutilizzata da Newtonsoft.Json).
    /// </summary>
    public class MapObj
    {
        /// <summary>
        /// Copia le proprieta' di <paramref name="expando"/> con nome uguale su <paramref name="entity"/>.
        /// Se <paramref name="entity"/> e' null ne crea una nuova istanza.
        /// </summary>
        public T ObjectToStatic<T>(object expando, T entity)
        {
            if (entity == null) entity = Activator.CreateInstance<T>();
            if (expando == null) return entity;

            var properties = expando.GetType().GetProperties();
            if (properties == null)
                return entity;

            foreach (var entry in properties)
            {
                var propertyExpando = expando.GetType().GetProperty(entry.Name);
                var propertyInfo = entity.GetType().GetProperty(entry.Name);
                if ((propertyExpando != null) && (propertyInfo != null))
                {
                    var value = propertyExpando.GetValue(expando);
                    if (propertyInfo.CanWrite)
                    {
                        propertyInfo.SetValue(entity, value, null);
                    }
                }
            }
            return entity;
        }
    }
}
