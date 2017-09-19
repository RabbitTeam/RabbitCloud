using System;
using System.Collections.ObjectModel;

namespace Rabbit.Cloud.Facade.Abstractions.Formatters
{
    public class FormatterCollection<TFormatter> : Collection<TFormatter>
    {
        public void RemoveType<T>() where T : TFormatter
        {
            RemoveType(typeof(T));
        }

        public void RemoveType(Type formatterType)
        {
            for (var i = Count - 1; i >= 0; i--)
            {
                var formatter = this[i];
                if (formatter.GetType() == formatterType)
                {
                    RemoveAt(i);
                }
            }
        }
    }
}