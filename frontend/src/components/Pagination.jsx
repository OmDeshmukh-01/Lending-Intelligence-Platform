export default function Pagination({ page, totalPages, onPage }) {
  if (totalPages <= 1) return null;

  return (
    <nav className="flex items-center justify-between pt-4 border-t border-slate-100" aria-label="Pagination">
      <p className="text-sm text-slate-500">
        Page <span className="font-medium text-slate-900">{page}</span> of{" "}
        <span className="font-medium text-slate-900">{totalPages}</span>
      </p>
      <div className="flex gap-2">
        <button
          onClick={() => onPage(page - 1)}
          disabled={page <= 1}
          className="btn-secondary text-sm px-3 py-1.5 disabled:opacity-40"
          aria-label="Previous page"
        >
          ← Previous
        </button>
        <button
          onClick={() => onPage(page + 1)}
          disabled={page >= totalPages}
          className="btn-secondary text-sm px-3 py-1.5 disabled:opacity-40"
          aria-label="Next page"
        >
          Next →
        </button>
      </div>
    </nav>
  );
}
