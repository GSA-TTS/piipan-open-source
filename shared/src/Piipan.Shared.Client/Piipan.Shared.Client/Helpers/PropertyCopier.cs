namespace Piipan.Shared.Client.Helpers
{
    /// <summary>
    /// The PropertyCopier class is a helper class that copies the values of one class to another different class.
    /// If a property doesn't exist in the target class that source property is skipped
    /// </summary>
    public static class PropertyCopier
    {
        /// <summary>
        /// Copies the properties from one class to a different class
        /// </summary>
        /// <typeparam name="TSource">The source type</typeparam>
        /// <typeparam name="TDest">The destination type</typeparam>
        /// <param name="source">The source object to copy properties from</param>
        /// <param name="dest">The destination object to copy properties to</param>
        public static void CopyPropertiesTo<TSource, TDest>(TSource source, TDest dest)
        {
            if (source == null || dest == null) return;
            var sourceProps = typeof(TSource).GetProperties().Where(x => x.CanRead).ToList();
            var destProps = typeof(TDest).GetProperties()
                    .Where(x => x.CanWrite)
                    .ToList();

            foreach (var sourceProp in sourceProps)
            {
                if (destProps.Any(x => x.Name == sourceProp.Name))
                {
                    var p = destProps.First(x => x.Name == sourceProp.Name);
                    if (p.CanWrite)
                    { // check if the property can be set or no.
                        p.SetValue(dest, sourceProp.GetValue(source, null), null);
                    }
                }

            }

        }
    }
}
