using Rabbit.Go.Abstractions.Filters;

namespace Rabbit.Go.Internal
{
    public struct FilterCursor
    {
        private readonly IFilterMetadata[] _filters;
        private int _index;

        public FilterCursor(IFilterMetadata[] filters)
        {
            _filters = filters;
            _index = 0;
        }

        public void Reset()
        {
            _index = 0;
        }

        public FilterCursorItem<TFilter, TFilterAsync> GetNextFilter<TFilter, TFilterAsync>()
            where TFilter : class
            where TFilterAsync : class
        {
            while (_index < _filters.Length)
            {
                var filter = _filters[_index] as TFilter;
                var filterAsync = _filters[_index] as TFilterAsync;

                _index += 1;

                if (filter != null || filterAsync != null)
                {
                    return new FilterCursorItem<TFilter, TFilterAsync>(filter, filterAsync);
                }
            }

            return default(FilterCursorItem<TFilter, TFilterAsync>);
        }
    }
}