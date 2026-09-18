export default function FilterPanel({ filters, onChange, onClear }) {
  const hasFilters = filters.decision || filters.from || filters.to;

  return (
    <div className="card p-4">
      <div className="flex flex-col sm:flex-row sm:items-end gap-3 flex-wrap">
        {/* Decision filter */}
        <div className="min-w-[140px]">
          <label htmlFor="filter-decision" className="form-label">Decision</label>
          <select
            id="filter-decision"
            value={filters.decision || ""}
            onChange={e => onChange({ ...filters, decision: e.target.value || undefined, page: 1 })}
            className="form-input"
          >
            <option value="">All</option>
            <option value="Approved">Approved</option>
            <option value="Declined">Declined</option>
          </select>
        </div>

        {/* From date */}
        <div className="min-w-[150px]">
          <label htmlFor="filter-from" className="form-label">From date</label>
          <input
            id="filter-from"
            type="date"
            value={filters.from || ""}
            onChange={e => onChange({ ...filters, from: e.target.value || undefined, page: 1 })}
            className="form-input"
          />
        </div>

        {/* To date */}
        <div className="min-w-[150px]">
          <label htmlFor="filter-to" className="form-label">To date</label>
          <input
            id="filter-to"
            type="date"
            value={filters.to || ""}
            onChange={e => onChange({ ...filters, to: e.target.value || undefined, page: 1 })}
            className="form-input"
          />
        </div>

        {/* Clear */}
        {hasFilters && (
          <button onClick={onClear} className="btn-secondary text-sm whitespace-nowrap self-end">
            Clear Filters
          </button>
        )}
      </div>
    </div>
  );
}
