import { useState } from "react";
import { useApplications } from "../hooks/useApplications";
import ApplicationTable from "../components/ApplicationTable";
import FilterPanel from "../components/FilterPanel";
import Pagination from "../components/Pagination";
import LoadingState from "../components/LoadingState";
import ErrorState from "../components/ErrorState";

const DEFAULT_FILTERS = { page: 1, pageSize: 20 };

export default function Applications() {
  const [filters, setFilters] = useState(DEFAULT_FILTERS);
  const { data, loading, error, refresh } = useApplications(filters);

  const handleClear = () => setFilters(DEFAULT_FILTERS);

  return (
    <main className="flex-1 p-4 sm:p-6 lg:p-8 overflow-auto">
      <header className="mb-6">
        <h1 className="text-2xl font-bold text-slate-900">All Applications</h1>
        <p className="text-sm text-slate-500 mt-1">
          Browse and filter all loan applications.
        </p>
      </header>

      <div className="space-y-4">
        <FilterPanel
          filters={filters}
          onChange={setFilters}
          onClear={handleClear}
        />

        <div className="card p-4 sm:p-6">
          {loading && <LoadingState message="Loading applications..." />}
          {error && <ErrorState message={error} onRetry={refresh} />}

          {!loading && !error && data && (
            <>
              <div className="flex items-center justify-between mb-3">
                <p className="text-sm text-slate-500">
                  {data.totalCount === 0
                    ? "No applications found"
                    : `${data.totalCount} application${data.totalCount !== 1 ? "s" : ""}`}
                </p>
              </div>

              <ApplicationTable applications={data.items} />

              <Pagination
                page={data.page}
                totalPages={data.totalPages}
                onPage={p => setFilters(f => ({ ...f, page: p }))}
              />
            </>
          )}
        </div>
      </div>
    </main>
  );
}
